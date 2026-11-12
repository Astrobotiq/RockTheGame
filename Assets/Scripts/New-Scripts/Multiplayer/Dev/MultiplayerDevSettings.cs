#if UNITY_EDITOR
using New_Scripts.Multiplayer.Messages;
using Unity.Multiplayer.PlayMode;
using UnityEditor;

namespace New_Scripts.Multiplayer.Dev
{
    public enum DevRole
    {
        Off,
        Host,
        Client
    }

    /// <summary>
    /// Geliştirici başlatıcısının editör ayarları. Yalnız "açık/kapalı" saklanır (EditorPrefs makine geneli olduğundan
    /// Multiplayer Play Mode kopyaları da aynı değeri görür); rol her pencerede ayrı belirlenir:
    /// ana editör Host, Multiplayer Play Mode'un sanal oyuncusu Client.
    /// </summary>
    public static class MultiplayerDevSettings
    {
        private const string EnabledKey = "RockTheGame.Multiplayer.DevLauncher.Enabled";
        private const string AddressKey = "RockTheGame.Multiplayer.DevLauncher.Address";
        private const string ModeKey = "RockTheGame.Multiplayer.DevLauncher.Mode";
        private const string MenuPath = "Rock The Game/Multiplayer/Geliştirici başlatıcısı";
        private const string SaveKey = "RockTheGame.Multiplayer.SaveEnabled";
        private const string SaveMenuPath = "Rock The Game/Multiplayer/Yarış kaydını yaz (editör)";

        /// <summary>
        /// Editörde yarış kaydı yazılsın mı (varsayılan açık). Test amaçlı deneme kaydı bırakıp lobide "Devam et"i açmasın diye
        /// kapatılabilir; kapalıyken yalnız varsayılan yola yazma atlanır (<see cref="Save.RaceSaveStore"/>), okuma ve silme aynen çalışır.
        /// Build'de bu ayar yok, kayıt hep yazılır.
        /// </summary>
        public static bool SaveEnabled
        {
            get => EditorPrefs.GetBool(SaveKey, true);
            set => EditorPrefs.SetBool(SaveKey, value);
        }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, false);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        public static string Address
        {
            get => EditorPrefs.GetString(AddressKey, "127.0.0.1");
            set => EditorPrefs.SetString(AddressKey, value);
        }

        public static RaceMode Mode
        {
            get => (RaceMode)EditorPrefs.GetInt(ModeKey, (int)RaceMode.Race);
            set => EditorPrefs.SetInt(ModeKey, (int)value);
        }

        public static DevRole ResolveRole()
        {
            if (!Enabled) return DevRole.Off;
            return CurrentPlayer.IsMainEditor ? DevRole.Host : DevRole.Client;
        }

        [MenuItem(MenuPath)]
        private static void ToggleEnabled() => Enabled = !Enabled;

        [MenuItem(SaveMenuPath)]
        private static void ToggleSave() => SaveEnabled = !SaveEnabled;

        [MenuItem(SaveMenuPath, true)]
        private static bool ToggleSaveValidate()
        {
            Menu.SetChecked(SaveMenuPath, SaveEnabled);
            return true;
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleEnabledValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return !EditorApplication.isPlaying;
        }
    }
}
#endif
