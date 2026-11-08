using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Flags;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Penalty;
using New_Scripts.Multiplayer.Save;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using New_Scripts.Player;
using New_Scripts.Player.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace New_Scripts.Multiplayer
{
    /// <summary>
    /// Bir multiplayer oturumunun kökü (DontDestroyOnLoad): taşıyıcıyı ve RaceLobbyFlow'u tutar. El sıkışma bitince oyun
    /// sahnesini yükler (geliştirici başlatıcısında sahne zaten yüklü), sahneye yayıncı, hayalet, bayraklar, ceza ve HUD'u
    /// ekler, oyuncuyu geri sayım boyunca dondurur. Tek kişilik koda yalnız okuyarak ve çağırarak dokunur.
    /// </summary>
    public sealed class MultiplayerSession : MonoBehaviour
    {
        public const string ObjectName = "[MP Session]";
        public const string GameSceneName = "NewScene";
        public const string LobbySceneName = "MultiplayerLobby";

        private bool _gameSceneLoaded;
        private string _playerName = "Oyuncu";
        private Guid _raceId = Guid.NewGuid();
        private bool _loadRequested;
        private bool _sceneLoadInProgress;
        private bool _worldReady;

        private PlayerController _player;
        private bool _playerFrozen;
        private LocalPlayerBroadcaster _broadcaster;
        private GhostView _ghost;
        private GhostInterpolator _interpolator = new GhostInterpolator();
        private bool _ghostVisible;
        private readonly List<RaceFlag> _flags = new List<RaceFlag>();
        private PenaltyHandler _penalty;
        private RaceHud _hud;

        // persistentDataPath kurucuda okunamaz (Unity); ilk kullanımda kurulur.
        private RaceSaveStore _storeInstance;
        private RaceSaveStore Store => _storeInstance ??= new RaceSaveStore();
        private readonly ConnectionWatch _watch = new ConnectionWatch();
        private RaceSaveData _resume;
        private int _resumeRoomId;
        private bool _pendingResumePlacement;
        private bool _isHost;
        private string _address;
        private double _reconnectAt;
        private float _elapsed;
        private bool _raceEverStarted;
        private (int room, int flags, int captured) _lastSaved = (-1, -1, -1);
        private RaceFinishLine _finishLine;
        private float _finishedAt = -1f;
        private const float ReturnHintDelaySeconds = 1.5f;

        public static MultiplayerSession Current { get; private set; }

        public IRaceTransport Transport { get; private set; }
        public RaceLobbyFlow Flow { get; private set; }
        public RaceSession Session => Flow?.Session;
        public float RaceStartedAt { get; private set; } = -1f;
        /// <summary>Yarış süresi: yalnız yarış sürerken ve kopma beklemesi yokken sayılır; kayda girer.</summary>
        public float ElapsedSeconds => _elapsed;
        public bool IsWaitingForReconnect => _watch.IsWaitingForReconnect;
        /// <summary>Yerel oyuncu bitirdi ve A / Enter ile lobiye dönebilir.</summary>
        public bool CanReturnToLobby => Session != null && Session.LocalFinished && _finishedAt >= 0f &&
                                        Time.unscaledTime - _finishedAt >= ReturnHintDelaySeconds;
        public int ReconnectSecondsLeft => _watch.SecondsLeft(Now);

        /// <summary>Oturum lobiye dönerek bittiyse nedeni; lobi bir kez gösterir ve temizler.</summary>
        public static string LastExitReason { get; set; }

        /// <summary>Bağlantı kapandı (zaman aşımı, reddedildi, karşı taraf ayrıldı, sürüm uyuşmadı).</summary>
        public event Action<DisconnectReason> Disconnected;

        private static double Now => Time.realtimeSinceStartupAsDouble;

        /// <summary>
        /// Oturumu kurar. <paramref name="host"/> verilirse bileşen ona eklenir (geliştirici başlatıcısı), yoksa kökte yeni bir
        /// nesne açılır. <paramref name="gameSceneAlreadyLoaded"/>: oyun sahnesi açıkken başlatıldıysa yeniden yüklenmez.
        /// </summary>
        public static MultiplayerSession Create(IRaceTransport transport, bool gameSceneAlreadyLoaded, string playerName,
            GameObject host = null, Guid? raceId = null)
        {
            if (Current != null && Current.gameObject != host) Current.Shutdown();

            var go = host != null ? host : new GameObject(ObjectName);
            if (Application.isPlaying && go.transform.parent == null) DontDestroyOnLoad(go);
            var session = go.AddComponent<MultiplayerSession>();
            session.Transport = transport;
            session._gameSceneLoaded = gameSceneAlreadyLoaded;
            session._playerName = string.IsNullOrEmpty(playerName) ? "Oyuncu" : playerName;
            if (raceId.HasValue) session._raceId = raceId.Value;
            transport.Connected += session.OnConnected;
            transport.Disconnected += session.OnDisconnected;
            transport.MessageReceived += session.OnMessage;
            Current = session;
            // Yarışta dünya durmaz (rakip hayalet akıyor); pause kapalı.
            New_Scripts.Pause.PauseManager.Block(session);
            return session;
        }

        /// <param name="resume">Lobide "Devam et" seçildiyse kayıt; null ise yeni yarış.</param>
        public void Host(RaceMode mode, RaceSaveData resume = null)
        {
            _isHost = true;
            UseResume(resume);
            NewFlow(isHost: true, mode, resume);
            Transport.Host();
            Debug.Log($"[MP] Host dinliyor ({Flow.Session.Mode}{(resume != null ? ", devam" : "")})");
        }

        public void Join(string address, RaceSaveData resume = null)
        {
            _isHost = false;
            _address = address;
            UseResume(resume);
            NewFlow(isHost: false, RaceMode.Race, resume);
            Transport.Join(address);
            Debug.Log($"[MP] Bağlanılıyor → {address}{(resume != null ? " (devam)" : "")}");
        }

        private void UseResume(RaceSaveData resume)
        {
            _resume = resume;
            _resumeRoomId = resume?.RoomId ?? 0;
            _elapsed = resume?.ElapsedSeconds ?? 0f;
        }

        private void NewFlow(bool isHost, RaceMode mode, RaceSaveData resume = null, RaceSession existing = null)
        {
            Guid raceId = existing != null && Flow != null ? Flow.RaceId : _raceId;
            Flow = new RaceLobbyFlow(isHost, mode, _playerName, raceId, resume, existing);
            Flow.LoadSceneRequested += OnLoadSceneRequested;
            Flow.RaceStarted += OnRaceStarted;
            Flow.Failed += reason => Debug.LogWarning($"[MP] {reason}");
            _loadRequested = false;
        }

        // ---------------- Taşıyıcı ----------------

        private void Update()
        {
            Transport?.Poll();
            Flow?.Tick(Now);
            UpdateGhost();
            TickRace();
            if (CanReturnToLobby && ReturnPressed()) ReturnToLobby();
        }

        private static bool ReturnPressed()
        {
            var pad = UnityEngine.InputSystem.Gamepad.current;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return (pad != null && pad.buttonSouth.wasPressedThisFrame) ||
                   (keyboard != null && keyboard.enterKey.wasPressedThisFrame);
        }

        private void ReturnToLobby()
        {
            Debug.Log("[MP] Yarış bitti; lobiye dönülüyor");
            LastExitReason = RaceHudText.Result(Session.Mode, true, _elapsed, Session.OpponentFinished,
                Session.OpponentElapsedSeconds, false).Replace("\n", " · ");
            Shutdown();
            SceneManager.LoadScene(LobbySceneName);
            Destroy(gameObject);
        }

        // ---------------- Süre, kayıt, kopma ----------------

        private void TickRace()
        {
            if (Flow == null || Session == null) return;
            if (_watch.IsWaitingForReconnect)
            {
                TickReconnect();
                return;
            }

            if (Flow.Phase == LobbyPhase.Racing)
            {
                if (!Session.LocalFinished) _elapsed += Time.unscaledDeltaTime;
                if (Transport != null && Transport.State == TransportState.Connected && _watch.IsSilent(Now) && !EitherFinished)
                    BeginReconnectWait("Rakipten 3 sn paket gelmedi");
            }

            if ((Flow.Phase == LobbyPhase.Countdown || Flow.Phase == LobbyPhase.Racing) && !_pendingResumePlacement &&
                !Session.LocalFinished) SaveIfChanged();
        }

        private void SaveIfChanged()
        {
            var state = (Session.LocalRoomId, Session.LocalFlagCount, Session.LocalCapturedRoomIds.Count);
            if (state == _lastSaved) return;
            _lastSaved = state;
            WriteSave();
        }

        /// <summary>Biri bitirdiyse kopma beklenmez: bitiren zaten donuk, kalan tek başına sürer.</summary>
        private bool EitherFinished => Session != null && (Session.LocalFinished || Session.OpponentFinished);

        private void WriteSave()
        {
            if (Session == null || Flow == null || Flow.RaceId == Guid.Empty || Session.LocalFinished) return;
            try
            {
                Store.Write(RaceSaveData.From(Session, Flow.RaceId, Flow.OpponentName, _elapsed));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MP] Yarış kaydı yazılamadı: {e.Message}");
            }
        }

        /// <summary>Yarış sürerken rakip koptu: kaydet, oyuncuyu dondur, hayaleti gizle, 30 sn yeniden bağlanmayı dene.</summary>
        private void BeginReconnectWait(string why)
        {
            if (_watch.IsWaitingForReconnect) return;
            Debug.LogWarning($"[MP] Rakip koptu ({why}); {ConnectionWatch.ReconnectWindowSeconds:0} sn bekleniyor");
            WriteSave();
            Freeze();
            _watch.BeginReconnectWait(Now);
            _ghostVisible = false;
            if (_ghost != null) _ghost.SetVisible(false);
            _reconnectAt = Now;
            if (Transport != null && Transport.State != TransportState.Disconnected) Transport.Disconnect();
        }

        private void TickReconnect()
        {
            if (_watch.HasExpired(Now))
            {
                GiveUp();
                return;
            }

            if (Transport == null || Transport.State != TransportState.Disconnected || Now < _reconnectAt) return;
            _reconnectAt = Now + 1.0;
            // Aynı oturum sürer; el sıkışma yeniden yapılır, sahne yüklü olduğu için hemen Ready ve 3-2-1.
            NewFlow(_isHost, Session.Mode, null, Session);
            if (_isHost) Transport.Host();
            else Transport.Join(_address);
        }

        private void GiveUp()
        {
            Debug.LogWarning("[MP] Rakip geri dönmedi; yarış kaydedildi, lobiye dönülüyor");
            WriteSave();
            LastExitReason = "Rakip geri dönmedi; yarış kaydedildi. İkiniz de Devam et ile sürdürebilirsiniz.";
            Shutdown();
            SceneManager.LoadScene(LobbySceneName);
            Destroy(gameObject);
        }

        private void Freeze()
        {
            if (_playerFrozen || _player == null) return;
            _player.OnStartRespawn();
            _playerFrozen = true;
        }

        private void OnConnected()
        {
            Debug.Log("[MP] Bağlandı");
            if (Flow != null) Send(Flow.OnConnected());
        }

        private void OnDisconnected(DisconnectReason reason)
        {
            Debug.Log($"[MP] Bağlantı kapandı: {reason}");
            _interpolator = new GhostInterpolator();

            if (_watch.IsWaitingForReconnect)
            {
                // Yeniden bağlanma denemesi tutmadı (zaman aşımı, host henüz dinlemiyor): saniyede bir tekrar.
                _reconnectAt = Math.Max(_reconnectAt, Now + 1.0);
            }
            else if (reason != DisconnectReason.LocalRequest && Flow != null && Flow.Phase != LobbyPhase.Failed)
            {
                // El sıkışma bittiyse (sahne yükleniyor ya da yarış sürüyor) beklenir; bitmediyse lobi hatayı gösterir.
                if (EitherFinished) Debug.Log("[MP] Bitişten sonra bağlantı kapandı; beklenmiyor");
                else if (Flow.Phase >= LobbyPhase.Loading) BeginReconnectWait(DescribeDisconnect(reason));
                else Flow.Fail(DescribeDisconnect(reason));
            }
            Disconnected?.Invoke(reason);
        }

        private string DescribeDisconnect(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Timeout: return _isHost ? "Rakibin bağlantısı zaman aşımına uğradı" : "Host'a ulaşılamadı";
                case DisconnectReason.Rejected: return "Bağlantı reddedildi (host dolu ya da adres yanlış)";
                case DisconnectReason.RemoteLeft: return "Rakip ayrıldı";
                default: return "Bağlantı koptu";
            }
        }

        private void OnMessage(byte[] bytes)
        {
            if (!RaceMessageCodec.TryDecode(bytes, out IRaceMessage message))
            {
                Debug.LogWarning($"[MP] Çözülemeyen paket ({bytes.Length} bayt)");
                return;
            }

            _watch.OnPacket(Now);
            if (message is SnapshotMessage snapshot)
            {
                _interpolator.Add(snapshot, Now);
                return;
            }

            Debug.Log($"[MP] ← {Describe(message)}");
            if (Flow == null) return;
            Send(Flow.Receive(message, Now));
            if (Flow.Phase == LobbyPhase.Failed) Transport.Disconnect();
        }

        private void Send(IReadOnlyList<IRaceMessage> messages)
        {
            foreach (var message in messages) Send(message);
        }

        private void Send(IRaceMessage message)
        {
            if (Transport != null && Transport.Send(RaceMessageCodec.Encode(message), reliable: message.Type != RaceMessageType.Snapshot))
                Debug.Log($"[MP] → {Describe(message)}");
        }

        // ---------------- Sahne ve dünya ----------------

        private void OnLoadSceneRequested()
        {
            if (_loadRequested) return;
            _loadRequested = true;

            if (_gameSceneLoaded)
            {
                SetUpWorldAndReady();
                return;
            }

            // Yükleme sürerken kopup yeniden bağlanılırsa sahne ikinci kez yüklenmez; sceneLoaded bekleneni yapar.
            if (_sceneLoadInProgress) return;
            _sceneLoadInProgress = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadSceneAsync(GameSceneName);
        }

        // sceneLoaded, sahnenin Awake/OnEnable'ından sonra, Start'ından önce gelir: yayıncı ilk odanın SetAsCurrent'ını yakalar.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameSceneName) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _sceneLoadInProgress = false;
            _gameSceneLoaded = true;
            if (_loadRequested) SetUpWorldAndReady();
        }

        private void SetUpWorldAndReady()
        {
            if (!_worldReady && Application.isPlaying) SetUpWorld();
            _worldReady = true;
            Send(Flow.OnSceneReady(Now));
        }

        private void SetUpWorld()
        {
            _player = FindFirstObjectByType<PlayerController>();
            var animation = _player != null ? _player.GetComponentInChildren<PlayerAnimationController>(true) : null;
            if (animation == null)
            {
                Debug.LogWarning("[MP] Oyuncu bulunamadı; dünya kurulmadı.");
                return;
            }

            _broadcaster = LocalPlayerBroadcaster.Create(_player, Session, Transport);
            DontDestroyOnLoad(_broadcaster.gameObject);

            var rope = _player.transform.Find("LeftRope");
            _ghost = GhostView.CreateFrom(animation.GetComponent<SpriteRenderer>(), animation.GetComponent<Animator>(),
                rope != null ? rope.GetComponent<LineRenderer>() : null, GhostEyeSourceFactory.From(_player),
                GhostEyeSourceFactory.ArmsFrom(_player), GhostEyeSourceFactory.DashFrom(_player));
            DontDestroyOnLoad(_ghost.gameObject);

            _penalty = PenaltyHandler.Create(Session, _player);
            Session.PenaltyReceived += _penalty.Enqueue;

            SetUpFlags();
            SetUpFinish();

            _hud = RaceHud.Create(this);

            // Geri sayım boyunca oyuncu donuk; ceza da bu sırada bekler (PenaltyHandler.IsPlayerBusy).
            Freeze();

            if (_resumeRoomId > 0) PlaceAtResumeRoomAsync(_resumeRoomId).Forget();
        }

        /// <summary>
        /// Kayıtlı yarışa devam: LevelBootstrapper.Start oyuncuyu başlangıç odasına koyduktan sonra kayıttaki odanın
        /// başlangıcına taşır. Oyuncu bu sırada geri sayım için donuk.
        /// </summary>
        private async UniTaskVoid PlaceAtResumeRoomAsync(int roomId)
        {
            _pendingResumePlacement = true;
            await UniTask.Yield(PlayerLoopTiming.Update);
            await UniTask.Yield(PlayerLoopTiming.Update);
            _pendingResumePlacement = false;
            if (this == null || _player == null) return;

            var room = RoomPlacement.FindRoom(roomId);
            if (room == null)
            {
                Debug.LogWarning($"[MP] Kayıttaki oda {roomId} sahnede yok; başlangıç odasında kalındı");
                return;
            }
            RoomPlacement.PlaceAt(_player, room, FindFirstObjectByType<RoomManager>());
            Debug.Log($"[MP] Kayıttan devam: oda {roomId}");
        }

        private void SetUpFlags()
        {
            if (!RaceFlag.ShouldPlaceFlags(Session.Mode)) return;

            var settings = RaceFlagSettingsSO.Load();
            foreach (var point in FindObjectsByType<RaceFlagPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                bool captured = Session.LocalCapturedRoomIds.Contains(point.RoomId);
                _flags.Add(RaceFlag.Create(point, settings, captured, OnLocalFlagReached));
            }
            Debug.Log($"[MP] {_flags.Count} bayrak kuruldu");
        }

        private void SetUpFinish()
        {
            var point = FindFirstObjectByType<RaceFinishPoint>(FindObjectsInactive.Include);
            if (point == null)
            {
                Debug.LogWarning("[MP] Sahnede RaceFinishPoint yok; yarış bitirilemez");
                return;
            }
            _finishLine = RaceFinishLine.Create(point, RaceFlagSettingsSO.Load(), OnLocalFinish);
        }

        private void OnLocalFinish()
        {
            if (Session == null || Session.LocalFinished) return;
            var messages = Session.OnLocalFinished(_elapsed);
            Debug.Log($"[MP] Bitirdin: {RaceHudText.FormatTime(_elapsed)}");
            Send(messages);
            Freeze();
            _finishedAt = Time.unscaledTime;
            try { Store.Delete(); } catch (Exception e) { Debug.LogWarning($"[MP] Kayıt silinemedi: {e.Message}"); }
        }

        private void OnLocalFlagReached(int roomId)
        {
            var messages = Session.OnLocalFlagReached(roomId);
            Debug.Log($"[MP] Bayrak: oda {roomId}, sayaç {Session.LocalFlagCount}");
            Send(messages);
        }

        private void OnRaceStarted()
        {
            RaceStartedAt = Time.unscaledTime;
            _raceEverStarted = true;
            if (_watch.IsWaitingForReconnect)
            {
                _watch.EndReconnectWait();
                Debug.Log("[MP] Rakip döndü; yarış sürüyor");
            }
            _watch.Reset();
            Debug.Log("[MP] Yarış başladı");
            if (_playerFrozen && _player != null) _player.OnEndRespawn();
            _playerFrozen = false;
        }

        private void UpdateGhost()
        {
            if (_ghost == null || _broadcaster == null || Session == null) return;

            bool hasPose = _interpolator.TrySample(Now, out GhostPose pose);
            if (hasPose) _ghost.Apply(pose);

            bool visible = hasPose && Transport != null && Transport.State == TransportState.Connected &&
                           GhostVisibility.IsVisible(Session.OpponentRoomId, _broadcaster.LocalRoomId, _broadcaster.LocalNeighborRoomIds);
            if (visible != _ghostVisible)
            {
                _ghostVisible = visible;
                _ghost.SetVisible(visible);
            }
        }

        // ---------------- Kapanış ----------------

        // Play Mode'da script yeniden derlenince OnDestroy gelmez ama OnDisable gelir: sürücü ve port burada bırakılır.
        private void OnDisable() => Shutdown();

        private void OnDestroy() => Shutdown();

        /// <summary>Taşıyıcıyı kapatır ve sahneye eklenen her şeyi kaldırır. Donuk oyuncu serbest bırakılır.</summary>
        public void Shutdown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_raceEverStarted && Transport != null) WriteSave();
            if (Transport != null)
            {
                Transport.Connected -= OnConnected;
                Transport.Disconnected -= OnDisconnected;
                Transport.MessageReceived -= OnMessage;
                Transport.Dispose();
                Transport = null;
            }

            if (_penalty != null && Session != null) Session.PenaltyReceived -= _penalty.Enqueue;
            DestroyOwned(_penalty);
            DestroyOwned(_ghost);
            DestroyOwned(_broadcaster);
            DestroyOwned(_hud);
            DestroyOwned(_finishLine);
            _finishLine = null;
            foreach (var flag in _flags) DestroyOwned(flag);
            _flags.Clear();
            _penalty = null;
            _ghost = null;
            _broadcaster = null;
            _hud = null;

            if (_playerFrozen && _player != null) _player.OnEndRespawn();
            _playerFrozen = false;
            if (Current == this) Current = null;
            New_Scripts.Pause.PauseManager.Unblock(this);
        }

        private static void DestroyOwned(Component component)
        {
            if (component == null) return;
            if (Application.isPlaying) Destroy(component.gameObject);
            else DestroyImmediate(component.gameObject);
        }

        private static string Describe(IRaceMessage message)
        {
            switch (message)
            {
                case HelloMessage h: return $"Hello v{h.ProtocolVersion} {h.Mode} '{h.PlayerName}'";
                case RoomEnteredMessage r: return $"RoomEntered {r.RoomId}";
                case FlagCapturedMessage f: return $"FlagCaptured oda {f.RoomId}, sayaç {f.NewCount}";
                case PenaltyMessage p: return $"Penalty → {p.TargetRoomId}";
                case ProgressSyncMessage s: return $"ProgressSync oda {s.RoomId}, sayaç {s.FlagCount}";
                case FinishedMessage f: return $"Finished {f.ElapsedSeconds:0.0} sn";
                default: return message.Type.ToString();
            }
        }
    }
}
