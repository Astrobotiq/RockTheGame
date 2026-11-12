#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// Arkadaşla denemek için Windows build'i: Builds/Windows/RockTheGame.exe, steam_appid.txt ve kısa bir OKU-BENI.txt.
    /// Build listesindeki sahneler kullanılır (MainMenu ilk, sonra NewScene ve lobi).
    /// </summary>
    public static class MultiplayerBuild
    {
        public const string OutputDirectory = "Builds/Windows";
        public const string ExeName = "RockTheGame.exe";

        [MenuItem("Rock The Game/Build/Windows (multiplayer testi)")]
        public static string BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(OutputDirectory, ExeName),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            string result = $"{summary.result}: {summary.totalErrors} hata, {summary.totalWarnings} uyarı, " +
                            $"{summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:0} sn";
            if (summary.result == BuildResult.Succeeded) WriteExtras();
            Debug.Log($"[Build] {result}");
            return result;
        }

        private static void WriteExtras()
        {
            // Eski build'lerin .bat'ları: oyun artık ana menüyle açılıyor.
            foreach (var bat in new[] { "Multiplayer.bat", "Host.bat" })
            {
                string path = Path.Combine(OutputDirectory, bat);
                if (File.Exists(path)) File.Delete(path);
            }
            // Steam (AppID 480, Spacewar): exe'nin yanındaki dosya Steam'e hangi uygulama olduğunu söyler.
            File.WriteAllText(Path.Combine(OutputDirectory, "steam_appid.txt"), "480");
            File.WriteAllText(Path.Combine(OutputDirectory, "OKU-BENI.txt"),
                "Rock The Game - multiplayer testi\r\n\r\n" +
                "RockTheGame.exe'yi açın; ana menüden Tek kişilik ya da Çok oyunculu seçin.\r\n\r\n" +
                "Steam ile: ikinizde de Steam açık olsun. Çok oyunculu ekranında 'Bağlantı: Steam' iken host\r\n" +
                "'Host ol' der, açılan Steam arayüzünden arkadaşını davet eder; arkadaşın daveti kabul edince oyun\r\n" +
                "kendiliğinden katılır (oyun kapalıysa Steam açar). Steam'in test uygulaması Spacewar olarak görünür.\r\n\r\n" +
                "IP ile: 'Bağlantı: IP' seçin. Biri 'Host ol' der, diğeri host'un IP adresini yazıp 'Katıl' der.\r\n" +
                "Aynı evde: host'un yerel IP'si (192.168.x.x). Farklı evlerde: ikiniz de Tailscale kurun,\r\n" +
                "host'un Tailscale IP'sini (100.x.x.x) yazın. Host'ta Windows Güvenlik Duvarı sorarsa izin verin.\r\n" +
                "Port: UDP 7777. İkinizin de aynı build'i kullanması gerekir.\r\n");
        }
    }
}
#endif
