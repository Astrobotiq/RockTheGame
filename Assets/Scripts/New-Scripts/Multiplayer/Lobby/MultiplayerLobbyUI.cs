using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Save;
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
    /// Steam açıksa (InviteServices) "Bağlantı: Steam" seçilebilir: Host ol Steam davet arayüzünü açar, davet kabul edilince
    /// kendiliğinden katılınır; adres alanı ve Katıl gizlenir.
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
        [SerializeField] private Button resumeButton;
        [SerializeField] private TMP_Text resumeLabel;
        [SerializeField] private Button connectionButton;
        [SerializeField] private TMP_Text connectionLabel;

        private RaceMode _mode = RaceMode.Race;
        private MultiplayerSession _session;
        private RaceSaveData _save;
        private bool _resumeOn;
        private string _exitReason;
        private bool _useInvite;
        private IInviteService _invites;

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

        public void WireResume(Button resume, TMP_Text label)
        {
            resumeButton = resume;
            resumeLabel = label;
        }

        public void WireConnection(Button connection, TMP_Text label)
        {
            connectionButton = connection;
            connectionLabel = label;
        }

        private void Awake()
        {
            modeButton.onClick.AddListener(ToggleMode);
            hostButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(StartJoin);
            cancelButton.onClick.AddListener(Cancel);
            Debug.Log($"[MP] Lobi açıldı (protokol v{RaceProtocol.Version})");
            _exitReason = MultiplayerSession.LastExitReason;
            MultiplayerSession.LastExitReason = null;
            if (resumeButton != null)
            {
                bool hasSave = new RaceSaveStore().TryRead(out _save);
                if (!hasSave) _save = null;
                resumeButton.gameObject.SetActive(hasSave);
                resumeButton.onClick.AddListener(ToggleResume);
                _resumeOn = hasSave; // kopup lobiye dönüldüyse devam en olası seçim
            }
            if (string.IsNullOrWhiteSpace(addressField.text)) addressField.text = DefaultAddress;

            _invites = InviteServices.Available ? InviteServices.Current : null;
            _useInvite = _invites != null;
            if (connectionButton != null)
            {
                connectionButton.gameObject.SetActive(_invites != null);
                connectionButton.onClick.AddListener(ToggleConnection);
            }
            if (_invites != null) _invites.JoinRequested += OnInviteAccepted;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_invites != null) _invites.JoinRequested -= OnInviteAccepted;
        }

        private void ToggleConnection()
        {
            _useInvite = !_useInvite && _invites != null;
            Refresh();
        }

        /// <summary>Steam daveti kabul edildi: host'a (SteamID) kendiliğinden katıl.</summary>
        private void OnInviteAccepted(string hostAddress)
        {
            if (_session != null && _session.Flow != null && _session.Flow.Phase != LobbyPhase.Failed) return;
            _useInvite = true;
            BeginSession().Join(hostAddress, Resume);
            Refresh();
        }

        // Build'de Host.bat / -mp-join ile kendiliğinden başla (yalnız ilk açılışta; lobiye dönülünce tekrar etmez).
        private static bool _autoStartUsed;

        private void Start()
        {
            string pendingInvite = _invites?.TakePendingJoin();
            if (pendingInvite != null)
            {
                OnInviteAccepted(pendingInvite);
                return;
            }
            if (_autoStartUsed || Application.isEditor) return;
            _autoStartUsed = true;
            var args = System.Environment.GetCommandLineArgs();
            string join = MultiplayerEntry.JoinAddress(args);
            if (MultiplayerEntry.HasHostArgument(args)) StartHost();
            else if (join != null)
            {
                _useInvite = false; // -mp-join her zaman IP
                addressField.text = join;
                StartJoin();
            }
        }

        private void ToggleMode()
        {
            _mode = _mode == RaceMode.Race ? RaceMode.Coop : RaceMode.Race;
            Refresh();
        }

        private void ToggleResume()
        {
            _resumeOn = !_resumeOn && _save != null;
            Refresh();
        }

        private RaceSaveData Resume => _resumeOn ? _save : null;

        private void StartHost()
        {
            BeginSession().Host(_mode, Resume);
            if (_useInvite) _invites.InviteFriends();
            Refresh();
        }

        private void StartJoin()
        {
            string address = string.IsNullOrWhiteSpace(addressField.text) ? DefaultAddress : addressField.text.Trim();
            BeginSession().Join(address, Resume);
            Refresh();
        }

        private MultiplayerSession BeginSession()
        {
            // İki pencere ya da iki bilgisayar: odakta olmayan oyun durursa bağlantı 5 sn'de kopar.
            Application.runInBackground = true;
            IRaceTransport transport = _useInvite
                ? _invites.CreateTransport()
                : new UnityTransportAdapter(UnityTransportSettings.Default);
            string name = _useInvite ? _invites.LocalPlayerName : System.Environment.UserName;
            _session = MultiplayerSession.Create(transport, gameSceneAlreadyLoaded: false, name);
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
            _invites?.CloseInvites();
            Refresh();
            EventSystem.current?.SetSelectedGameObject(hostButton.gameObject);
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            bool active = _session != null && _session.Flow != null && _session.Flow.Phase != LobbyPhase.Failed;
            RaceMode shownMode = Resume != null ? Resume.RaceMode : _mode;
            modeLabel.text = shownMode == RaceMode.Race ? "Mod: Yarış" : "Mod: Birlikte";
            modeButton.interactable = !active && Resume == null; // devam edilen yarışın modu kayıttan
            if (resumeButton != null && _save != null)
            {
                resumeButton.interactable = !active;
                resumeLabel.text = RaceHudText.ResumeLabel(_resumeOn, _save.RoomId, _save.FlagCount);
            }
            hostButton.interactable = !active;
            joinButton.interactable = !active;
            addressField.interactable = !active;
            cancelButton.interactable = active;
            if (connectionButton != null && _invites != null)
            {
                connectionButton.interactable = !active;
                connectionLabel.text = _useInvite ? $"Bağlantı: {_invites.DisplayName}" : "Bağlantı: IP";
            }
            // Davetle bağlanırken katılma davetten gelir: adres ve Katıl gizli.
            if (addressField.gameObject.activeSelf == _useInvite) addressField.gameObject.SetActive(!_useInvite);
            if (joinButton.gameObject.activeSelf == _useInvite) joinButton.gameObject.SetActive(!_useInvite);

            var flow = _session != null ? _session.Flow : null;
            if (flow == null && !string.IsNullOrEmpty(_exitReason))
            {
                statusText.text = _exitReason;
                return;
            }
            if (flow != null) _exitReason = null;
            statusText.text = _useInvite && flow == null
                ? RaceHudText.InviteIdle(_invites.DisplayName)
                : _useInvite && flow.IsHost && (flow.Phase == LobbyPhase.Idle || flow.Phase == LobbyPhase.Handshake)
                    ? RaceHudText.InviteWaiting(_invites.DisplayName)
                    : RaceHudText.LobbyStatus(flow?.Phase, flow != null && flow.IsHost, flow?.FailureReason,
                        UnityTransportSettings.Default.Port);

            // Düğme kapanınca gamepad seçimi boşa düşmesin.
            var es = EventSystem.current;
            if (es != null && (es.currentSelectedGameObject == null ||
                               es.currentSelectedGameObject.GetComponent<Selectable>() is Selectable s && !s.interactable))
                es.SetSelectedGameObject(active ? cancelButton.gameObject : hostButton.gameObject);
        }
    }
}
