using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// MultiplayerLobby sahnesinin ekranı (gamepad ile gezilir): mod (Yarış / Birlikte), Host ol, adres + Katıl, Vazgeç.
    /// Bağlantı kurulunca MultiplayerSession oyun sahnesini kendisi yükler. Modu host seçer; katılan host'un modunu alır.
    /// </summary>
    public sealed class MultiplayerLobbyUI : MonoBehaviour
    {
        public const string DefaultAddress = "127.0.0.1";

        [SerializeField] private Button modeButton;
        [SerializeField] private TMP_Text modeLabel;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_InputField addressField;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text statusText;

        private RaceMode _mode = RaceMode.Race;
        private MultiplayerSession _session;

        public void Wire(Button mode, TMP_Text modeText, Button host, TMP_InputField address, Button join, Button cancel, TMP_Text status)
        {
            modeButton = mode;
            modeLabel = modeText;
            hostButton = host;
            addressField = address;
            joinButton = join;
            cancelButton = cancel;
            statusText = status;
        }

        private void Awake()
        {
            modeButton.onClick.AddListener(ToggleMode);
            hostButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(StartJoin);
            cancelButton.onClick.AddListener(Cancel);
            if (string.IsNullOrWhiteSpace(addressField.text)) addressField.text = DefaultAddress;
            Refresh();
        }

        private void ToggleMode()
        {
            _mode = _mode == RaceMode.Race ? RaceMode.Coop : RaceMode.Race;
            Refresh();
        }

        private void StartHost()
        {
            BeginSession().Host(_mode);
            Refresh();
        }

        private void StartJoin()
        {
            string address = string.IsNullOrWhiteSpace(addressField.text) ? DefaultAddress : addressField.text.Trim();
            BeginSession().Join(address);
            Refresh();
        }

        private MultiplayerSession BeginSession()
        {
            // İki pencere ya da iki bilgisayar: odakta olmayan oyun durursa bağlantı 5 sn'de kopar.
            Application.runInBackground = true;
            _session = MultiplayerSession.Create(new UnityTransportAdapter(UnityTransportSettings.Default),
                gameSceneAlreadyLoaded: false, System.Environment.UserName);
            return _session;
        }

        private void Cancel()
        {
            if (_session != null)
            {
                _session.Shutdown();
                Destroy(_session.gameObject);
            }
            _session = null;
            Refresh();
            EventSystem.current?.SetSelectedGameObject(hostButton.gameObject);
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            bool active = _session != null && _session.Flow != null && _session.Flow.Phase != LobbyPhase.Failed;
            modeLabel.text = _mode == RaceMode.Race ? "Mod: Yarış" : "Mod: Birlikte";
            modeButton.interactable = !active;
            hostButton.interactable = !active;
            joinButton.interactable = !active;
            addressField.interactable = !active;
            cancelButton.interactable = active;

            var flow = _session != null ? _session.Flow : null;
            statusText.text = RaceHudText.LobbyStatus(flow?.Phase, flow != null && flow.IsHost, flow?.FailureReason,
                UnityTransportSettings.Default.Port);

            // Düğme kapanınca gamepad seçimi boşa düşmesin.
            var es = EventSystem.current;
            if (es != null && (es.currentSelectedGameObject == null ||
                               es.currentSelectedGameObject.GetComponent<Selectable>() is Selectable s && !s.interactable))
                es.SetSelectedGameObject(active ? cancelButton.gameObject : hostButton.gameObject);
        }
    }
}
