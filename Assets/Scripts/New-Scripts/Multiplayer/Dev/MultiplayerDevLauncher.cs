#if UNITY_EDITOR
using System;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using New_Scripts.Player;
using New_Scripts.Player.Visual;
using UnityEngine;

namespace New_Scripts.Multiplayer.Dev
{
    /// <summary>
    /// Yalnız editörde, lobi gelene kadar (MP 7) iki kopyayı birbirine bağlayan geçici başlatıcı.
    /// NewScene'e konmaz: menüden açıkken Play Mode başında kökte kendi nesnesini oluşturur, kapalıyken hiçbir şey yapmaz.
    /// Bağlanınca Hello + ProgressSync yollar, gelen mesajları "[MP]" önekiyle loglar ve RaceSession'a verir.
    /// </summary>
    public sealed class MultiplayerDevLauncher : MonoBehaviour
    {
        public const string ObjectName = "[MP Dev Launcher]";
        private const float ClientRetryDelaySeconds = 1f;
        private static readonly Guid DevRaceId = new Guid("d3e0d3e0-0000-4000-8000-000000000001");

        private DevRole _role;
        private string _address;
        private float _retryAt = -1f;
        private LocalPlayerBroadcaster _broadcaster;
        private GhostView _ghost;
        private GhostInterpolator _interpolator = new GhostInterpolator();
        private bool _ghostVisible;

        public RaceSession Session { get; private set; }
        public IRaceTransport Transport { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSpawn() => SpawnIfEnabled();

        public static MultiplayerDevLauncher SpawnIfEnabled() => Spawn(MultiplayerDevSettings.ResolveRole());

        public static MultiplayerDevLauncher Spawn(DevRole role)
        {
            if (role == DevRole.Off) return null;

            var go = new GameObject(ObjectName);
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
                // İki pencereden yalnız biri odakta olur; diğerinin oyun döngüsü durursa 5 sn'de bağlantı kopar.
                // Yalnız bu Play oturumu için: PlayerSettings.runInBackground değişmez.
                Application.runInBackground = true;
            }
            var launcher = go.AddComponent<MultiplayerDevLauncher>();
            launcher.Initialize(role, MultiplayerDevSettings.Address, MultiplayerDevSettings.Mode);
            return launcher;
        }

        private void Initialize(DevRole role, string address, RaceMode mode)
        {
            _role = role;
            _address = address;
            Session = new RaceSession(mode);
            Transport = new UnityTransportAdapter(UnityTransportSettings.Default);
            Transport.Connected += OnConnected;
            Transport.Disconnected += OnDisconnected;
            Transport.MessageReceived += OnMessage;
            if (Application.isPlaying) SetUpGhost();
            Begin(role);
        }

        private void Begin(DevRole role)
        {
            if (role == DevRole.Host)
            {
                Transport.Host();
                Debug.Log($"[MP] Host dinliyor :{UnityTransportSettings.Default.Port} ({Session.Mode})");
            }
            else
            {
                Transport.Join(_address);
                Debug.Log($"[MP] Client bağlanıyor → {_address}");
            }
        }

        /// <summary>Yerel oyuncunun yayıncısını ve rakibin hayaletini kurar (yalnız Play Mode, sahne kökünde).</summary>
        private void SetUpGhost()
        {
            var player = FindFirstObjectByType<PlayerController>();
            var animation = player != null ? player.GetComponentInChildren<PlayerAnimationController>(true) : null;
            if (animation == null)
            {
                Debug.LogWarning("[MP] Oyuncu bulunamadı; hayalet kurulmadı.");
                return;
            }

            _broadcaster = LocalPlayerBroadcaster.Create(player, Session, Transport);
            DontDestroyOnLoad(_broadcaster.gameObject);

            var rope = player.transform.Find("LeftRope");
            _ghost = GhostView.CreateFrom(animation.GetComponent<SpriteRenderer>(), animation.GetComponent<Animator>(),
                rope != null ? rope.GetComponent<LineRenderer>() : null);
            DontDestroyOnLoad(_ghost.gameObject);
        }

        private void UpdateGhost()
        {
            if (_ghost == null || _broadcaster == null) return;

            bool hasPose = _interpolator.TrySample(Time.realtimeSinceStartupAsDouble, out GhostPose pose);
            if (hasPose) _ghost.Apply(pose);

            bool visible = hasPose && Transport != null && Transport.State == TransportState.Connected &&
                           GhostVisibility.IsVisible(Session.OpponentRoomId, _broadcaster.LocalRoomId, _broadcaster.LocalNeighborRoomIds);
            if (visible != _ghostVisible)
            {
                _ghostVisible = visible;
                _ghost.SetVisible(visible);
            }
        }

        private void Update()
        {
            Transport?.Poll();
            UpdateGhost();

            if (_retryAt > 0f && Time.unscaledTime >= _retryAt)
            {
                _retryAt = -1f;
                Begin(_role);
            }
        }

        // Play Mode'da script yeniden derlenince OnDestroy gelmez ama OnDisable gelir: sürücü ve 7777 portu burada bırakılır.
        private void OnDisable() => Shutdown();

        private void OnDestroy() => Shutdown();

        private void Shutdown()
        {
            Transport?.Dispose();
            Transport = null;
            if (_ghost != null) Destroy(_ghost.gameObject);
            if (_broadcaster != null) Destroy(_broadcaster.gameObject);
            _ghost = null;
            _broadcaster = null;
        }

        private void OnConnected()
        {
            Debug.Log($"[MP] Bağlandı ({_role})");
            Send(new HelloMessage
            {
                ProtocolVersion = RaceProtocol.Version,
                Mode = Session.Mode,
                RaceId = DevRaceId,
                PlayerName = _role.ToString()
            });
            Send(Session.CreateProgressSync(0f));
        }

        private void OnDisconnected(DisconnectReason reason)
        {
            Debug.Log($"[MP] Bağlantı kapandı: {reason}");
            _interpolator = new GhostInterpolator();
            // Kopyalar aynı anda açılınca istemci host'tan önce davranabilir: host'a ulaşamazsa (Timeout) yeniden dener.
            // Reddedilince (host dolu, adres hatalı) denemez; yoksa saniyede bir sonsuz döngüye girer.
            if (_role == DevRole.Client && reason == DisconnectReason.Timeout)
                _retryAt = Time.unscaledTime + ClientRetryDelaySeconds;
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
                _interpolator.Add(snapshot, Time.realtimeSinceStartupAsDouble);
                return;
            }

            Debug.Log($"[MP] ← {Describe(message)}");
            Session.Receive(message);
        }

        private void Send(IRaceMessage message)
        {
            if (Transport.Send(RaceMessageCodec.Encode(message), reliable: message.Type != RaceMessageType.Snapshot))
                Debug.Log($"[MP] → {Describe(message)}");
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
#endif
