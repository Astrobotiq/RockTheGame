#if UNITY_EDITOR
using System.Linq;
using New_Scripts.Multiplayer.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// MainMenu.unity'yi bir kez kurar (lobiyle aynı görünüş): başlık, Tek kişilik, Çok oyunculu, Çıkış; MainMenuUI bağlı.
    /// Sahneyi build listesinin başına koyar, böylece oyun ana menüyle açılır. Var olan sahnenin üzerine yazmaz.
    /// </summary>
    public static class MainMenuBuilder
    {
        public const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Rock The Game/Ana menü sahnesini kur")]
        public static string Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                AddToBuildSettingsFirst();
                return $"{ScenePath} zaten var; build listesinde ilk sırada";
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return "iptal";
            string previous = SceneManager.GetActiveScene().path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildContents();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettingsFirst();

            if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            return $"{ScenePath} kuruldu";
        }

        private static void BuildContents()
        {
            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = MultiplayerLobbyBuilder.Background;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("MainMenuCanvas") { layer = LayerMask.NameToLayer("UI") };
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = MultiplayerUISettingsSO.Load().Font;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            panel.layer = canvasGo.layer;
            panel.transform.SetParent(canvasGo.transform, false);
            ((RectTransform)panel.transform).sizeDelta = new Vector2(900f, 700f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 32f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            MultiplayerLobbyBuilder.Label(panel.transform, "Title", "ROCK THE GAME", 120f, font, 200f);
            var single = MultiplayerLobbyBuilder.Button(panel.transform, "SinglePlayerButton", "Tek kişilik", font, out _);
            var multi = MultiplayerLobbyBuilder.Button(panel.transform, "MultiplayerButton", "Çok oyunculu", font, out _);
            var quit = MultiplayerLobbyBuilder.Button(panel.transform, "QuitButton", "Çıkış", font, out _);

            canvasGo.AddComponent<MainMenuUI>().Wire(single, multi, quit);
            eventSystem.firstSelectedGameObject = single.gameObject;
        }

        private static void AddToBuildSettingsFirst()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
