#if UNITY_EDITOR
using System;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
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

        public RaceSession Session { get; private set; }
        public IRaceTransport Transport { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSpawn() => SpawnIfEnabled();

        public static MultiplayerDevLauncher SpawnIfEnabled() => Spawn(MultiplayerDevSettings.ResolveRole());

        public static MultiplayerDevLauncher Spawn(DevRole role)
        {
            if (role == DevRole.Off) return null;

            var go = new GameObject(ObjectName);
            if (Application.isPlaying) DontDestroyOnLoad(go);
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

        private void Update()
        {
            Transport?.Poll();

            if (_retryAt > 0f && Time.unscaledTime >= _retryAt)
            {
                _retryAt = -1f;
                Begin(_role);
            }
        }

        private void OnDestroy()
        {
            Transport?.Dispose();
            Transport = null;
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
            // Kopyalar aynı anda açılınca istemci host'tan önce davranabilir: istemci kısa aralıkla yeniden dener.
            if (_role == DevRole.Client && reason != DisconnectReason.LocalRequest)
                _retryAt = Time.unscaledTime + ClientRetryDelaySeconds;
        }

        private void OnMessage(byte[] bytes)
        {
            if (!RaceMessageCodec.TryDecode(bytes, out IRaceMessage message))
            {
                Debug.LogWarning($"[MP] Çözülemeyen paket ({bytes.Length} bayt)");
                return;
            }

            if (message.Type != RaceMessageType.Snapshot) Debug.Log($"[MP] ← {Describe(message)}");
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
