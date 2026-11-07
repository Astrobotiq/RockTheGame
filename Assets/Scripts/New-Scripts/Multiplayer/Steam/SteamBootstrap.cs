using System;
using New_Scripts.Multiplayer.Transport;
using Steamworks;
using UnityEngine;

namespace New_Scripts.Multiplayer.Steam
{
    /// <summary>
    /// Açılışta Steam'i başlatır (AppID 480) ve davet servisini kaydeder. Steam açık değilse ya da başlatılamazsa
    /// hiçbir şey kaydedilmez, lobi IP ile bağlanır. SteamClient.Shutdown yalnız uygulama kapanırken.
    /// Editörde varsayılan kapalı (menü: Rock The Game/Multiplayer/Steam (editör)); domain reload'da Steam'i iki kez
    /// başlatmamak için editörde bir kez başlatılır ve editör kapanana kadar açık kalır.
    /// </summary>
    public static class SteamBootstrap
    {
        public const string EditorPrefKey = "RockTheGame.Multiplayer.SteamInEditor";

        private static GameObject _pump;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.GetBool(EditorPrefKey, false)) return;
#endif
            if (!SteamClient.IsValid)
            {
                try
                {
                    SteamClient.Init(SteamInviteService.AppId, asyncCallbacks: false);
                }
                catch (Exception e)
                {
                    Debug.Log($"[MP] Steam başlatılamadı, IP ile bağlanılacak: {e.Message}");
                    return;
                }
            }

            InviteServices.Current = new SteamInviteService();
            Debug.Log($"[MP] Steam hazır: {SteamClient.Name}");

            _pump = new GameObject("[MP Steam]");
            UnityEngine.Object.DontDestroyOnLoad(_pump);
            _pump.AddComponent<SteamCallbackPump>();
#if !UNITY_EDITOR
            Application.quitting += () => { if (SteamClient.IsValid) SteamClient.Shutdown(); };
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Rock The Game/Multiplayer/Steam (editör)")]
        private static void ToggleInEditor()
        {
            bool on = !UnityEditor.EditorPrefs.GetBool(EditorPrefKey, false);
            UnityEditor.EditorPrefs.SetBool(EditorPrefKey, on);
            if (on && !System.IO.File.Exists("steam_appid.txt"))
                System.IO.File.WriteAllText("steam_appid.txt", SteamInviteService.AppId.ToString());
            Debug.Log($"[MP] Editörde Steam {(on ? "açık (Steam istemcisi çalışıyor olmalı; Play'de denenir)" : "kapalı")}");
        }

        [UnityEditor.MenuItem("Rock The Game/Multiplayer/Steam (editör)", true)]
        private static bool ToggleInEditorValidate()
        {
            UnityEditor.Menu.SetChecked("Rock The Game/Multiplayer/Steam (editör)", UnityEditor.EditorPrefs.GetBool(EditorPrefKey, false));
            return true;
        }
#endif

        /// <summary>Lobi beklerken de (taşıyıcı yokken) davet geri çağrıları işlensin.</summary>
        private sealed class SteamCallbackPump : MonoBehaviour
        {
            private void Update()
            {
                if (SteamClient.IsValid) SteamClient.RunCallbacks();
            }
        }
    }
}
