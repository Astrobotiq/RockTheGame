using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// Build'de multiplayer'a giriş: oyun "-multiplayer" argümanıyla açılırsa (build klasöründeki Multiplayer.bat) ilk sahne
    /// yerine lobi yüklenir. Argümansız açılış tek kişilik oyundur. Ana menü gelince oraya bir düğme konup bu kalkabilir.
    /// Editörde devre dışı (orada lobi sahnesi elle açılır).
    /// </summary>
    public static class MultiplayerEntry
    {
        public const string Argument = "-multiplayer";
        /// <summary>Lobide kendiliğinden Host ol (Host.bat).</summary>
        public const string HostArgument = "-mp-host";
        /// <summary>Lobide kendiliğinden verilen adrese katıl: "-mp-join 100.64.0.1".</summary>
        public const string JoinArgument = "-mp-join";

        /// <summary>"-mp-join adres" verildiyse adres; yoksa null.</summary>
        public static string JoinAddress(string[] args)
        {
            if (args == null) return null;
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], JoinArgument, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        public static bool HasHostArgument(string[] args) => Contains(args, HostArgument);

        private static bool Contains(string[] args, string value)
        {
            if (args == null) return false;
            foreach (var arg in args)
                if (string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Lobiye girilecek mi: -multiplayer, -mp-host ya da -mp-join verildiyse.</summary>
        public static bool HasArgument(string[] args) =>
            Contains(args, Argument) || HasHostArgument(args) || JoinAddress(args) != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadLobbyIfRequested()
        {
            if (Application.isEditor || !HasArgument(Environment.GetCommandLineArgs())) return;
            Debug.Log($"[MP] {Argument}: lobi açılıyor");
            SceneManager.LoadScene(MultiplayerSession.LobbySceneName);
        }
    }
}
