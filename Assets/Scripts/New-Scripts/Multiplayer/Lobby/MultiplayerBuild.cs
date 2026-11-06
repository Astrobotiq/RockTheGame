#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// Arkadaşla denemek için Windows build'i: Builds/Windows/RockTheGame.exe, yanına lobiden açan Multiplayer.bat ve kısa
    /// bir OKU-BENI.txt. Build listesindeki sahneler kullanılır (NewScene ilk, lobi ikinci).
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
            File.WriteAllText(Path.Combine(OutputDirectory, "Multiplayer.bat"),
                "@echo off\r\nstart \"\" \"%~dp0" + ExeName + "\" " + MultiplayerEntry.Argument + "\r\n");
            File.WriteAllText(Path.Combine(OutputDirectory, "Host.bat"),
                "@echo off\r\nstart \"\" \"%~dp0" + ExeName + "\" " + MultiplayerEntry.HostArgument + "\r\n");
            File.WriteAllText(Path.Combine(OutputDirectory, "OKU-BENI.txt"),
                "Rock The Game - multiplayer testi\r\n\r\n" +
                "Tek kişilik: RockTheGame.exe\r\n" +
                "Çok oyunculu: Multiplayer.bat (oyunu lobiden açar)\r\n" +
                "Hızlı yol: host Host.bat'ı açar (kendiliğinden host olur); katılan Multiplayer.bat ile girip\r\n" +
                "adresi yazar ya da kısayola şunu koyar: RockTheGame.exe -mp-join <host-ip>\r\n\r\n" +
                "Biri 'Host ol' der, diğeri host'un IP adresini yazıp 'Katıl' der.\r\n" +
                "Aynı evde: host'un yerel IP'si (192.168.x.x). Farklı evlerde: ikiniz de Tailscale kurun,\r\n" +
                "host'un Tailscale IP'sini (100.x.x.x) yazın. Host'ta Windows Güvenlik Duvarı sorarsa izin verin.\r\n" +
                "Port: UDP 7777. İkinizin de aynı build'i kullanması gerekir.\r\n");
        }
    }
}
#endif
