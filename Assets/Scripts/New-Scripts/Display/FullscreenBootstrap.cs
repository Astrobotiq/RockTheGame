#if !UNITY_EDITOR
using System;
using UnityEngine;

namespace New_Scripts.Display
{
    /// <summary>
    /// Build'de oyun her açılışta tam ekran (borderless) başlar. Unity son pencere durumunu PlayerPrefs'e yazdığı için
    /// bir kez pencereli açılan build sonraki açılışlarda da pencereli kalıyordu; ProjectSettings'teki fullscreenMode
    /// tek başına yetmiyor. Komut satırında -screen-fullscreen / -window-mode verilmişse dokunmaz.
    /// </summary>
    internal static class FullscreenBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg == "-screen-fullscreen" || arg == "-window-mode") return;
            }

            var desktop = Screen.currentResolution;
            Screen.SetResolution(desktop.width, desktop.height, FullScreenMode.FullScreenWindow);
        }
    }
}
#endif
