using System;
using System.Collections.Generic;
using System.Linq;
using New_Scripts.Multiplayer.Flags;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Penalty;
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

        private bool _gameSceneLoaded;
        private string _playerName = "Oyuncu";
        private Guid _raceId = Guid.NewGuid();
        private bool _loadRequested;
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

        public static MultiplayerSession Current { get; private set; }

        public IRaceTransport Transport { get; private set; }
        public RaceLobbyFlow Flow { get; private set; }
        public RaceSession Session => Flow?.Session;
        public float RaceStartedAt { get; private set; } = -1f;

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
            return session;
        }

        public void Host(RaceMode mode)
        {
            NewFlow(isHost: true, mode);
            Transport.Host();
            Debug.Log($"[MP] Host dinliyor ({mode})");
        }

        public void Join(string address)
        {
            NewFlow(isHost: false, RaceMode.Race);
            Transport.Join(address);
            Debug.Log($"[MP] Bağlanılıyor → {address}");
        }

        private void NewFlow(bool isHost, RaceMode mode)
        {
            Flow = new RaceLobbyFlow(isHost, mode, _playerName, _raceId);
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
            if (Flow != null && Flow.Phase != LobbyPhase.Racing && Flow.Phase != LobbyPhase.Failed && reason != DisconnectReason.LocalRequest)
                Flow.Fail(DescribeDisconnect(reason));
            Disconnected?.Invoke(reason);
        }

        private static string DescribeDisconnect(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Timeout: return "Host'a ulaşılamadı";
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

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadSceneAsync(GameSceneName);
        }

        // sceneLoaded, sahnenin Awake/OnEnable'ından sonra, Start'ından önce gelir: yayıncı ilk odanın SetAsCurrent'ını yakalar.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameSceneName) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _gameSceneLoaded = true;
            SetUpWorldAndReady();
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
                rope != null ? rope.GetComponent<LineRenderer>() : null, GhostEyeSourceFactory.From(_player));
            DontDestroyOnLoad(_ghost.gameObject);

            _penalty = PenaltyHandler.Create(Session, _player);
            Session.PenaltyReceived += _penalty.Enqueue;

            SetUpFlags();

            _hud = RaceHud.Create(this);

            // Geri sayım boyunca oyuncu donuk; ceza da bu sırada bekler (PenaltyHandler.IsPlayerBusy).
            _player.OnStartRespawn();
            _playerFrozen = true;
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

        private void OnLocalFlagReached(int roomId)
        {
            var messages = Session.OnLocalFlagReached(roomId);
            Debug.Log($"[MP] Bayrak: oda {roomId}, sayaç {Session.LocalFlagCount}");
            Send(messages);
        }

        private void OnRaceStarted()
        {
            RaceStartedAt = Time.unscaledTime;
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
            foreach (var flag in _flags) DestroyOwned(flag);
            _flags.Clear();
            _penalty = null;
            _ghost = null;
            _broadcaster = null;
            _hud = null;

            if (_playerFrozen && _player != null) _player.OnEndRespawn();
            _playerFrozen = false;
            if (Current == this) Current = null;
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
