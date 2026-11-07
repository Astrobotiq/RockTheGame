using New_Scripts.Multiplayer.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// Ana menü (build'in ilk sahnesi, gamepad ile gezilir): Tek kişilik oyunu doğrudan açar, Çok oyunculu lobiyi
    /// (Steam açıksa arkadaş daveti) açar, Çıkış oyunu kapatır. Menüdeyken bir Steam daveti kabul edilirse (ya da oyun
    /// davetle açıldıysa) lobiye geçilir; menü daveti dinlemediği için servis onu bekletir, lobi alıp kendiliğinden katılır.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        public const string SceneName = "MainMenu";

        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button quitButton;

        private IInviteService _invites;
        private bool _loading;

        public void Wire(Button singlePlayer, Button multiplayer, Button quit)
        {
            singlePlayerButton = singlePlayer;
            multiplayerButton = multiplayer;
            quitButton = quit;
        }

        private void Awake()
        {
            singlePlayerButton.onClick.AddListener(() => Load(MultiplayerSession.GameSceneName));
            multiplayerButton.onClick.AddListener(() => Load(MultiplayerSession.LobbySceneName));
            quitButton.onClick.AddListener(Quit);

            _invites = InviteServices.Available ? InviteServices.Current : null;
        }

        private void Update()
        {
            if (_invites != null && _invites.HasPendingJoin) Load(MultiplayerSession.LobbySceneName);
        }

        private void Load(string sceneName)
        {
            if (_loading) return;
            _loading = true;
            SceneManager.LoadScene(sceneName);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
