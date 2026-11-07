#if UNITY_EDITOR
using System;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace New_Scripts.Multiplayer.Dev
{
    /// <summary>
    /// Yalnız editörde, oyun sahnesinde doğrudan Play'e basınca iki kopyayı (Multiplayer Play Mode) birbirine bağlayan
    /// kısayol. NewScene'e konmaz: menüden açıkken Play Mode başında kökte kendi nesnesini oluşturur, kapalıyken hiçbir şey
    /// yapmaz. Lobi sahnesinden girildiyse (oturum zaten var) devreye girmez. Asıl iş MultiplayerSession'da.
    /// </summary>
    public sealed class MultiplayerDevLauncher : MonoBehaviour
    {
        public const string ObjectName = "[MP Dev Launcher]";
        private const float ClientRetryDelaySeconds = 1f;
        private static readonly Guid DevRaceId = new Guid("d3e0d3e0-0000-4000-8000-000000000001");

        private DevRole _role;
        private string _address;
        private float _retryAt = -1f;

        public MultiplayerSession Multiplayer { get; private set; }
        public RaceSession Session => Multiplayer != null ? Multiplayer.Session : null;
        public IRaceTransport Transport => Multiplayer != null ? Multiplayer.Transport : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSpawn()
        {
            if (MultiplayerSession.Current != null) return;
            if (SceneManager.GetActiveScene().name != MultiplayerSession.GameSceneName) return;
            SpawnIfEnabled();
        }

        public static MultiplayerDevLauncher SpawnIfEnabled() => Spawn(MultiplayerDevSettings.ResolveRole());

        public static MultiplayerDevLauncher Spawn(DevRole role)
        {
            if (role == DevRole.Off) return null;

            var go = new GameObject(ObjectName);
            if (Application.isPlaying)
            {
                // İki pencereden yalnız biri odakta olur; diğerinin oyun döngüsü durursa 5 sn'de bağlantı kopar.
                // Yalnız bu Play oturumu için: PlayerSettings.runInBackground değişmez.
                Application.runInBackground = true;
            }
            var launcher = go.AddComponent<MultiplayerDevLauncher>();
            launcher.Begin(role);
            return launcher;
        }

        private void Begin(DevRole role)
        {
            _role = role;
            _address = MultiplayerDevSettings.Address;
            Multiplayer = MultiplayerSession.Create(new UnityTransportAdapter(UnityTransportSettings.Default),
                gameSceneAlreadyLoaded: true, role.ToString(), host: gameObject, raceId: DevRaceId);
            Multiplayer.Disconnected += OnDisconnected;
            if (role == DevRole.Host) Multiplayer.Host(MultiplayerDevSettings.Mode);
            else Multiplayer.Join(_address);
        }

        private void OnDisconnected(DisconnectReason reason)
        {
            // Kopyalar aynı anda açılınca istemci host'tan önce davranabilir: host'a ulaşamazsa (Timeout) yeniden dener.
            // Reddedilince (host dolu, adres hatalı) denemez; yoksa saniyede bir sonsuz döngüye girer.
            if (_role == DevRole.Client && reason == DisconnectReason.Timeout && !Multiplayer.IsWaitingForReconnect &&
                Multiplayer.Flow.Phase < LobbyPhase.Loading)
                _retryAt = Time.unscaledTime + ClientRetryDelaySeconds;
        }

        private void Update()
        {
            if (_retryAt < 0f || Time.unscaledTime < _retryAt || Multiplayer == null || Multiplayer.Transport == null) return;
            _retryAt = -1f;
            Multiplayer.Join(_address);
        }

        // Play Mode'da script yeniden derlenince OnDestroy gelmez ama OnDisable gelir: sürücü ve port burada bırakılır.
        private void OnDisable()
        {
            if (Multiplayer != null) Multiplayer.Shutdown();
        }
    }
}
#endif
