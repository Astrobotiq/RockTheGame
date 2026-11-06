#if UNITY_EDITOR
using System.Linq;
using New_Scripts.Multiplayer.Hud;
using TMPro;
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
    /// MultiplayerLobby.unity'yi bir kez kurar: kamera, EventSystem (Input System UI modülü, gamepad), canvas ve düğmeler,
    /// MultiplayerLobbyUI bağlı. Sahneyi build listesine NewScene'den sonra ekler (tek kişilik açılış index 0'da kalır).
    /// Kurulduktan sonra sahne elle düzenlenebilir; komut var olan sahnenin üzerine yazmaz.
    /// </summary>
    public static class MultiplayerLobbyBuilder
    {
        public const string ScenePath = "Assets/Scenes/MultiplayerLobby.unity";
        private static readonly Color Background = new Color(0.36f, 0.66f, 0.85f);
        private static readonly Color ButtonColor = new Color(0.95f, 0.45f, 0.62f);

        [MenuItem("Rock The Game/Multiplayer/Lobi sahnesini kur")]
        public static string Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return $"{ScenePath} zaten var";
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return "iptal";
            string previous = SceneManager.GetActiveScene().path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildContents();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();

            if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            return $"{ScenePath} kuruldu";
        }

        private static void BuildContents()
        {
            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("LobbyCanvas") { layer = LayerMask.NameToLayer("UI") };
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
            var panelRect = (RectTransform)panel.transform;
            panelRect.sizeDelta = new Vector2(900f, 800f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Label(panel.transform, "Title", "ÇOK OYUNCULU", 96f, font, 130f);
            var mode = Button(panel.transform, "ModeButton", "Mod: Yarış", font, out var modeLabel);
            var host = Button(panel.transform, "HostButton", "Host ol", font, out _);
            var address = AddressField(panel.transform, font);
            var join = Button(panel.transform, "JoinButton", "Katıl", font, out _);
            var cancel = Button(panel.transform, "CancelButton", "Vazgeç", font, out _);
            var status = Label(panel.transform, "Status", "", 40f, font, 60f);

            var ui = canvasGo.AddComponent<MultiplayerLobbyUI>();
            ui.Wire(mode, modeLabel, host, address, join, cancel, status);
            eventSystem.firstSelectedGameObject = host.gameObject;
        }

        private static TMP_Text Label(Transform parent, string name, string text, float size, TMP_FontAsset font, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var label = go.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static Button Button(Transform parent, string name, string text, TMP_FontAsset font, out TMP_Text label)
        {
            var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            go.name = name;
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 90f;
            go.GetComponent<Image>().color = ButtonColor;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.selectedColor = colors.highlightedColor = new Color(1f, 0.85f, 0.3f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            button.colors = colors;
            label = go.GetComponentInChildren<TMP_Text>();
            label.text = text;
            label.fontSize = 48f;
            label.color = Color.white;
            if (font != null) label.font = font;
            return button;
        }

        private static TMP_InputField AddressField(Transform parent, TMP_FontAsset font)
        {
            var go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            go.name = "AddressField";
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 80f;
            var field = go.GetComponent<TMP_InputField>();
            field.text = MultiplayerLobbyUI.DefaultAddress;
            field.contentType = TMP_InputField.ContentType.Standard;
            field.pointSize = 40f;
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
            {
                t.fontSize = 40f;
                t.alignment = TextAlignmentOptions.Center;
                if (font != null) t.font = font;
            }
            if (field.placeholder is TMP_Text placeholder) placeholder.text = "Host adresi";
            return field;
        }

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
