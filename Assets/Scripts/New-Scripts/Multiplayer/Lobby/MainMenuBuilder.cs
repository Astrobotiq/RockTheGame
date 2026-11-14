#if UNITY_EDITOR
using System.Linq;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
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
    /// MainMenu.unity'yi kurar. Düzen docs/ui-research/mashup-ui.html'deki "ana menü" ekranının 4 katı (480x270 sanat ızgarası →
    /// 1920x1080 canvas): solda eğik başlık ve liste (play / multiplayer / options / quit), seçili satır sarı levha + halka imleci,
    /// sağda duvara açılmış delik (mavi boyalı halka, içinden kanca halkası). Renkler, fontlar ve denge <see cref="UiThemeSO"/>'dan.
    /// Çerçeve kalınlığı ve gölge kurulum anında temadan okunup sahneye yazılır: tema/denge değişince menüyü yeniden kur.
    /// <c>options</c> henüz bir ayar ekranı olmadığı için etkileşimsiz (gezinmede atlanır).
    /// Sahneyi build listesinin başına koyar, böylece oyun ana menüyle açılır.
    /// </summary>
    public static class MainMenuBuilder
    {
        public const string ScenePath = "Assets/Scenes/MainMenu.unity";

        private const float Px = 4f; // 480x270 sanat pikseli → 1920x1080 canvas birimi
        private const UiScreen Screen = UiScreen.MainMenu;

        // Kanca halkasının bantları, dıştan içe (mockup BANDS2); oyundaki node sprite'ının renkleri.
        private static readonly Color32[] HookBands =
        {
            new Color32(0xE5, 0x8A, 0x8A, 255), new Color32(0x25, 0x9E, 0xA0, 255), new Color32(0xF5, 0xEF, 0xCB, 255),
            new Color32(0x8A, 0x8A, 0x1A, 255), new Color32(0xFF, 0xB1, 0x1A, 255), new Color32(0xFF, 0x43, 0x00, 255),
            new Color32(0xFF, 0xE5, 0x5A, 255)
        };

        [MenuItem("Rock The Game/Ana menü sahnesini kur")]
        public static string Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                AddToBuildSettingsFirst();
                return $"{ScenePath} zaten var; build listesinde ilk sırada (yeniden kurmak için: Ana menüyü yeniden kur)";
            }

            return BuildScene();
        }

        [MenuItem("Rock The Game/Ana menüyü yeniden kur (üzerine yazar)")]
        public static string Rebuild() => BuildScene();

        private static string BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return "iptal";
            string previous = SceneManager.GetActiveScene().path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildContents();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettingsFirst();

            if (!string.IsNullOrEmpty(previous) && previous != ScenePath) EditorSceneManager.OpenScene(previous);
            return $"{ScenePath} kuruldu";
        }

        private static void BuildContents()
        {
            var theme = UiThemeSO.Load();
            float border = theme.BorderWidthFor(Screen) * Px;
            float shadow = theme.ShadowOffsetFor(Screen) * Px;

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = theme.Ink;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("MainMenuCanvas") { layer = LayerMask.NameToLayer("UI") };
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Bilerek UiPixelScale değil (UR-3, 2026-11-14): menü tam ekran kompozisyon; ölçek 2'de (1366x768, 1280x800) tam sayı
            // ölçek menüyü ekranın ~2/3'üne küçültüp altını boş bırakıyordu. Pause ve HUD UiPixelScale kullanır.
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var group = canvasGo.AddComponent<CanvasGroup>(); // açılışta gezinmeyi kilitler (UiMenuMotion)
            Transform root = canvasGo.transform;

            // Duvar: tam ekran beton.
            var wall = Rect("Wall", root, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(wall, theme.ConcreteWall).gameObject.AddComponent<UiConcreteSurface>().Configure(Screen, ConcreteTone.Wall, false);

            var hole = BuildHole(root, theme, border, out var bands);

            // Siyah kirişler (mockup: yatay 0..250 x 30..36, dikey x 28..34).
            var beamH = Rect("BeamH", root, TopLeft, TopLeft, TopLeft, new Vector2(0f, -30f * Px), new Vector2(250f * Px, 6f * Px));
            AddImage(beamH, theme.Ink);
            var beamV = Rect("BeamV", root, new Vector2(0f, 0f), new Vector2(0f, 1f), TopLeft, new Vector2(28f * Px, 0f), new Vector2(6f * Px, 0f));
            AddImage(beamV, theme.Ink);

            // Başlık: eğik, kırmızı, ekstrüde "rock"; altında "the game".
            // Başlık grubu: hareket (UiMenuMotion) ekstrüzyon kopyalarıyla birlikte dönsün diye ortak üst nesne; merkezi başlığın ortası.
            var titleCenter = new Vector2(300f, -380f);
            var title = Rect("Title", root, TopLeft, TopLeft, new Vector2(0.5f, 0.5f), titleCenter, Vector2.zero);
            var rock = Text(title, "TitleRock", "rock", theme.TitleFont, 64f * 0.95f * Px, theme.Red, new Vector2(0f, 0.5f), new Vector2(46f * Px, -76f * Px) - titleCenter, new Vector2(900f, 260f));
            rock.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * 0.05f);
            rock.gameObject.AddComponent<UiExtrudedText>().Screen = Screen;
            var game = Text(title, "TitleGame", "the game", theme.TitleFont, 22f * 0.95f * Px, theme.Ink, new Vector2(0f, 0.5f), new Vector2(50f * Px, -118f * Px) - titleCenter, new Vector2(700f, 110f));
            game.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * 0.04f);

            // Seçim imleci (tek, satırlar arasında kayar); etiketlerin arkasında kalsın diye önce eklenir.
            var cursor = BuildCursor(root, theme, border, shadow, out var cursorShadow, out var cursorRing);

            // Liste.
            var single = MenuItem(root, theme, "SinglePlayerButton", "play", 0, true, out var l0);
            var multi = MenuItem(root, theme, "MultiplayerButton", "multiplayer", 1, true, out var l1);
            var options = MenuItem(root, theme, "OptionsButton", "options", 2, false, out var l2);
            var quit = MenuItem(root, theme, "QuitButton", "quit", 3, true, out var l3);

            canvasGo.AddComponent<MainMenuUI>().Wire(single, multi, quit);
            canvasGo.AddComponent<UiMenuMotion>().Wire(new Selectable[] { single, multi, options, quit }, new TMP_Text[] { l0, l1, l2, l3 },
                hole, bands, title, beamH, beamV, cursor, cursorShadow, cursorRing, group);
            eventSystem.firstSelectedGameObject = single.gameObject;
        }

        /// <summary>Duvardaki delik: gökyüzü diski, kanca halkası, ink çerçeve, mavi boyalı halka.</summary>
        private static RectTransform BuildHole(Transform root, UiThemeSO theme, float border, out RectTransform[] bands)
        {
            // Mockup: merkez (364,122), halka 68..88 sanat pikseli. Sağ kenara göre konumlanır (farklı en-boy oranında kaymasın).
            float outer = 88f * Px;
            float inner = 68f * Px;
            var center = new Vector2(-(480f - 364f) * Px, -122f * Px);
            var holeRoot = Rect("Hole", root, TopRight, TopRight, new Vector2(0.5f, 0.5f), center, Vector2.zero);

            Arc(holeRoot, "Sky", inner, 0f, theme.Sky, false);
            float hook = 50f * Px;
            bands = new RectTransform[HookBands.Length];
            for (int i = 0; i < HookBands.Length; i++)
                bands[i] = Arc(holeRoot, $"Hook{i}", hook * (1f - i / (float)HookBands.Length), 0f, HookBands[i], false).rectTransform;

            // Çerçeve: ink halka halkanın iki kenarına yarım çizgi taşar, mavi boyalı halka içinde kalır.
            Arc(holeRoot, "RingInk", outer + border * 0.5f, inner - border * 0.5f, theme.Ink, false);
            Arc(holeRoot, "RingPaint", outer - border * 0.5f, inner + border * 0.5f, theme.Blue, true);
            return holeRoot;
        }

        /// <summary>Sarı levha, ink çerçeve ve gölge, açık halka imleci; ilk satırda başlar, UiMenuMotion kaydırır.</summary>
        private static RectTransform BuildCursor(Transform root, UiThemeSO theme, float border, float shadow, out RectTransform shadowRect, out RectTransform ringHolder)
        {
            var size = new Vector2(192f * Px, 26f * Px);
            var cursor = Rect("Cursor", root, TopLeft, TopLeft, new Vector2(0f, 0.5f), new Vector2(44f * Px, -160f * Px), size);
            var stretch0 = new Vector2(0f, 0f);
            var stretch1 = new Vector2(1f, 1f);
            var middle = new Vector2(0.5f, 0.5f);
            Vector2 half = new Vector2(border * 0.5f, border * 0.5f);
            shadowRect = Rect("Shadow", cursor, stretch0, stretch1, middle, new Vector2(shadow, -shadow), half * 2f);
            AddImage(shadowRect, theme.Ink).enabled = shadow > 0f;
            AddImage(Rect("Border", cursor, stretch0, stretch1, middle, Vector2.zero, half * 2f), theme.Ink);
            var slab = Rect("Slab", cursor, stretch0, stretch1, middle, Vector2.zero, -half * 2f);
            var slabImage = AddImage(slab, theme.Yellow);
            slabImage.gameObject.AddComponent<UiConcreteSurface>().Configure(Screen, ConcreteTone.Wall, true);
            slabImage.color = theme.Yellow; // bileşen eklenirken OnValidate rengi beton tonuna çeker; boyayı sonra ver
            ringHolder = Rect("RingCursor", cursor, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), middle, new Vector2(14f * Px, 0f), Vector2.zero);
            var ring = Arc(ringHolder, "Ring", 7f * Px, 3.5f * Px, theme.Ink, false);
            ring.FillAmount = 0.78f; // açık halka: dönünce görünür
            return cursor;
        }

        private static Button MenuItem(Transform root, UiThemeSO theme, string name, string text, int index, bool interactable, out TMP_Text label)
        {
            // Mockup: satır merkezi y = 160 + 26*i, levha x 44..236 (yükseklik 26).
            var size = new Vector2(192f * Px, 26f * Px);
            var button = Rect(name, root, TopLeft, TopLeft, new Vector2(0f, 0.5f), new Vector2(44f * Px, -(160f + 26f * index) * Px), size);
            var hit = AddImage(button, Color.clear);
            hit.raycastTarget = true;
            var control = button.gameObject.AddComponent<Button>();
            control.targetGraphic = hit;
            control.transition = Selectable.Transition.None;
            control.interactable = interactable;

            var text_ = Text(button, "Label", text, theme.TitleFont, 17f * 0.95f * Px, interactable ? theme.Ink : theme.Muted,
                new Vector2(0f, 0.5f), new Vector2(32f * Px, 0f), new Vector2(600f, size.y));
            text_.rectTransform.anchorMin = text_.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            label = text_;
            return control;
        }

        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);

        private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image AddImage(RectTransform rt, Color color)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Merkezden <paramref name="outerRadius"/> yarıçaplı daire/halka (iç yarıçap 0 = dolu). Boya = benekli yüzey.</summary>
        private static UiArc Arc(Transform parent, string name, float outerRadius, float innerRadius, Color color, bool paint)
        {
            var rt = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * outerRadius * 2f);
            var arc = rt.gameObject.AddComponent<UiArc>();
            arc.InnerRadiusRatio = innerRadius / outerRadius;
            if (paint) arc.gameObject.AddComponent<UiConcreteSurface>().Configure(Screen, ConcreteTone.Wall, true);
            arc.color = color; // yüzey bileşeninden sonra: OnValidate eklerken rengi ezer
            return arc;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color,
            Vector2 pivot, Vector2 position, Vector2 rectSize)
        {
            var rt = Rect(name, parent, TopLeft, TopLeft, pivot, position, rectSize);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
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
