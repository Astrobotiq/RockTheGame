using System.Collections.Generic;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>
    /// Çok oyunculu sonuç ekranı (mockup <c>uiResult</c>, docs/ui-research/mashup-ui.html): beton duvar, çapraz siyah kiriş,
    /// eğik zemin + tehlike şeridi, sol üstte başlık levhası; 1. oyuncu (host) mavi silindir, 2. oyuncu (istemci) kırmızı küp.
    /// Sütun yüksekliği sıralamayı gösterir (<see cref="RaceResultMath"/>): kazanan tam boy, kaybeden süre oranında kısa.
    /// <list type="bullet">
    /// <item>Overlay canvas (CRT dışı), <see cref="UiPixelScale"/> ile tam sayı sanat pikseli. Kod ile kurulur; <see cref="RaceHud"/> sürer.</item>
    /// <item>Hareket (<see cref="UiSpring"/>, unscaled zaman): kiriş/zemin/levha kayarak gelir; sütunlar zeminden yükselir
    /// (kaybeden önce, kazanan son), oturunca gölge çakılır; boşta galip sütun hafifçe yüzer.</item>
    /// <item>Rakip henüz bitirmediyse yalnız yerel sütun yükselir; rakip bitirince onun sütunu da yükselir.</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/ui-motion.md).
    /// </summary>
    public sealed class RaceResultScreen : MonoBehaviour
    {
        public const string ObjectName = "[MP Result]";

        private const UiScreen Screen = UiScreen.MultiplayerResult;
        private const float Unit = UiPixelScale.UnitsPerArtPixel;
        private const float FrameW = UiPixelScale.ArtWidth;
        private const float FrameH = UiPixelScale.ArtHeight;
        private const float FloorY = 226f;
        private const float ClipY = 246f;
        private const float SideExtra = 100f;
        private const float MaxColumnHeight = 108f;
        private const float SinkDistance = MaxColumnHeight + 70f;

        private sealed class Group
        {
            public RectTransform Rect;
            public float Extent;
            public float Delay;
            public readonly UiSpring Slide = new UiSpring(240f, 22f);
            public bool Started;
        }

        private sealed class Column
        {
            public int Index;
            public float CenterX;
            public RectTransform Rect;
            public RectTransform Shape;
            public RectTransform ShadowRect;
            public RectTransform NumberRect;
            public TMP_Text Number;
            public UiExtrudedText NumberExtrusion;
            public TMP_Text Label;
            public RectTransform LabelRect;
            /// <summary>Etiketin arkasındaki plaka (yerel sarı, rakip siyah): kirişin üstüne gelse de okunur.</summary>
            public RectTransform PlateRect;
            public UiPolygon PlateFill;
            public readonly UiSpring Rise = new UiSpring(180f, 18f);
            public readonly UiSpring Slam = new UiSpring(320f, 16f);
            public bool Built;
            public bool BuiltPresent;
            public float BuiltHeight = -1f;
            public float StartAt = -1f;
            public bool Slammed;
            public bool Bobs;
            public float Phase;
        }

        [Header("Zamanlama (başlangıç önerisi)")]
        [SerializeField, Range(0f, 1f)] private float riseDelay = 0.35f;
        [Tooltip("Kazanan sütunun kaybedenden sonraki ek gecikmesi.")]
        [SerializeField, Range(0f, 0.6f)] private float winnerExtraDelay = 0.18f;
        [SerializeField, Range(1f, 4f)] private float slamScale = 2.4f;
        [Header("Boşta")]
        [Tooltip("Galip sütunun yüzme genliği (canvas birimi; ui-motion: sürüklenme ~9).")]
        [SerializeField, Range(0f, 16f)] private float bobAmount = 8f;
        [SerializeField, Range(0.2f, 3f)] private float bobSpeed = 1.1f;

        private readonly List<Group> _groups = new List<Group>();
        private readonly Column[] _columns = new Column[2];

        private UiThemeSO _theme;
        private GameObject _root;
        private RectTransform _frame;
        private Image _backdrop;
        private TMP_Text _headline;
        private TMP_Text _hint;
        private float _border;
        private float _shadow;
        private bool _visible;
        private float _entranceAt;
        private float _backdropAlpha;
        private float _hintAlpha;
        private bool _canReturn;

        public bool IsVisible => _visible;
        public string HeadlineText => _headline != null ? _headline.text : "";
        public string LabelText(int index) => _columns[index] != null ? _columns[index].Label.text : "";

        public static RaceResultScreen Create()
        {
            var go = new GameObject(ObjectName) { layer = LayerMask.NameToLayer("UI") };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            return go.AddComponent<RaceResultScreen>();
        }

        // ---- API ----

        /// <summary>Her karede çağrılabilir; ekran ilk çağrıda girişini oynatır, sonrasında yalnız değişen şeyi günceller.</summary>
        public void Present(RaceResultKind kind, RaceResultColumn first, RaceResultColumn second, bool canReturn)
        {
            float now = Time.unscaledTime;
            if (!_visible)
            {
                _visible = true;
                _root.SetActive(true);
                _entranceAt = now;
                _backdropAlpha = 0f;
                _hintAlpha = 0f;
                foreach (var g in _groups)
                {
                    g.Slide.Value = 0f;
                    g.Slide.Velocity = 0f;
                    g.Slide.Target = 0f;
                    g.Started = false;
                    ApplySlide(g);
                }

                foreach (var c in _columns) ResetColumn(c);
            }

            _headline.text = RaceResultMath.Headline(kind);
            _canReturn = canReturn;
            UpdateColumn(_columns[0], first, second.Present ? second.Height : 0f, now);
            UpdateColumn(_columns[1], second, first.Present ? first.Height : 0f, now);
        }

        public void Hide()
        {
            if (!_visible) return;
            _visible = false;
            _root.SetActive(false);
        }

        // ---- yaşam döngüsü ----

        private void Awake()
        {
            _theme = UiThemeSO.Load();
            BuildUi();
            _root.SetActive(false);
        }

        private void Update()
        {
            if (!_visible) return;
            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;
            float t = now - _entranceAt;

            _backdropAlpha = Mathf.MoveTowards(_backdropAlpha, 1f, dt / 0.18f);
            var bc = _backdrop.color;
            bc.a = _backdropAlpha;
            _backdrop.color = bc;

            foreach (var g in _groups)
            {
                if (!g.Started && t >= g.Delay)
                {
                    g.Started = true;
                    g.Slide.Target = 1f;
                }

                g.Slide.Step(dt);
                ApplySlide(g);
            }

            foreach (var c in _columns) StepColumn(c, dt, now);

            _hintAlpha = Mathf.MoveTowards(_hintAlpha, _canReturn ? 1f : 0f, dt / 0.2f);
            var hc = _hint.color;
            hc.a = _hintAlpha;
            _hint.color = hc;
        }

        // ---- sütunlar ----

        private static void ResetColumn(Column c)
        {
            c.Rise.Value = 0f;
            c.Rise.Velocity = 0f;
            c.Rise.Target = 0f;
            c.Slam.Value = 1f;
            c.Slam.Velocity = 0f;
            c.Slam.Target = 1f;
            c.Slammed = false;
            c.StartAt = -1f;
            c.Built = false;
            c.BuiltPresent = false;
            c.BuiltHeight = -1f;
        }

        private void UpdateColumn(Column c, RaceResultColumn data, float otherHeight, float now)
        {
            c.Label.text = RaceResultMath.Label(data);
            // Yerel sarı, bitirmiş rakip siyah, henüz bitirmemiş rakip kâğıt (kirişin üstüne gelse de okunur).
            bool darkPlate = !data.IsLocal && data.Present;
            c.Label.color = darkPlate ? _theme.Paper : _theme.Ink;
            c.PlateFill.color = data.IsLocal ? _theme.Yellow : data.Present ? _theme.Ink : _theme.Paper;
            bool heightChanged = Mathf.Abs(data.Height - c.BuiltHeight) > 0.001f;
            if (!c.Built || data.Present != c.BuiltPresent || (data.Present && heightChanged))
            {
                c.Built = true;
                c.BuiltPresent = data.Present;
                c.BuiltHeight = data.Height;
                RebuildColumn(c, data);
                c.Rise.Value = 0f;
                c.Rise.Velocity = 0f;
                c.Rise.Target = 0f;
                c.Slammed = false;
                float delay = riseDelay + (data.Height >= otherHeight ? winnerExtraDelay : 0f);
                // Rakip henüz bitirmediyse yalnız "RAKİP …" plakası yükselir; sütunu bitince gelir.
                c.StartAt = Mathf.Max(now, _entranceAt + delay);
            }

            c.Bobs = data.Present && data.Height >= otherHeight;
        }

        private void StepColumn(Column c, float dt, float now)
        {
            if (c.StartAt >= 0f && now >= c.StartAt)
            {
                c.StartAt = -1f;
                c.Rise.Target = 1f;
            }

            c.Rise.Step(dt);
            if (c.Rise.Target > 0f && !c.Slammed && c.Rise.Value >= 0.95f)
            {
                c.Slammed = true;
                c.Slam.Value = slamScale;
                c.Slam.Velocity = 0f;
                c.Slam.Target = 1f;
            }

            c.Slam.Step(dt);

            float bob = 0f;
            if (c.Bobs && c.Slammed)
            {
                c.Phase += dt * bobSpeed * Mathf.PI * 2f;
                bob = (0.5f + 0.5f * Mathf.Sin(c.Phase)) * bobAmount;
            }

            // Yükselirken zeminin altından gelir (RectMask2D kırpar); kısmen çıkmış sütun aşağıdan başlar.
            c.Rect.anchoredPosition = new Vector2(0f, -SinkDistance * Unit * (1f - c.Rise.Value) + bob);
            float s = _shadow * c.Slam.Value;
            c.ShadowRect.anchoredPosition = new Vector2(s, -s);
        }

        private void RebuildColumn(Column c, RaceResultColumn data)
        {
            for (int i = c.Shape.childCount - 1; i >= 0; i--) Destroy(c.Shape.GetChild(i).gameObject);
            c.ShadowRect = NewRect("Shadow", c.Shape);
            Stretch(c.ShadowRect);
            c.ShadowRect.SetAsFirstSibling();

            c.Number.gameObject.SetActive(data.Present);
            if (!data.Present)
            {
                PlaceLabel(c, FloorY - 14f);
                c.NumberExtrusion.Refresh();
                return;
            }

            float height = MaxColumnHeight * Mathf.Clamp01(data.Height);
            float top = FloorY - height;
            var faces = new List<Vector2[]>();
            var colors = new List<Color>();
            float labelY;
            float numberY;

            if (c.Index == 0)
            {
                const float rx = 46f, ry = 14f;
                faces.Add(CylinderBody(c.CenterX, rx, ry, top, FloorY));
                colors.Add(_theme.Blue);
                faces.Add(Ellipse(c.CenterX, top, rx, ry));
                colors.Add(_theme.Blue);
                labelY = top - ry - 14f;
                numberY = top + height * 0.5f + 4f;
            }
            else
            {
                const float w = 76f, d = 22f, dy = 11f;
                float x0 = c.CenterX - w * 0.5f, x1 = c.CenterX + w * 0.5f;
                faces.Add(new[] { new Vector2(x1, top), new Vector2(x1 + d, top - dy), new Vector2(x1 + d, FloorY - dy), new Vector2(x1, FloorY) });
                colors.Add(_theme.Red);
                faces.Add(new[] { new Vector2(x0, top), new Vector2(x0 + d, top - dy), new Vector2(x1 + d, top - dy), new Vector2(x1, top) });
                colors.Add(_theme.Red);
                faces.Add(new[] { new Vector2(x0, top), new Vector2(x1, top), new Vector2(x1, FloorY), new Vector2(x0, FloorY) });
                colors.Add(_theme.Red);
                labelY = top - dy - 14f;
                numberY = top + height * 0.5f;
            }

            for (int i = 0; i < faces.Count; i++) AddPolygon(c.ShadowRect, "Shadow " + i, faces[i], _theme.Ink, _border);
            for (int i = 0; i < faces.Count; i++)
            {
                AddPolygon(c.Shape, "Outline " + i, faces[i], _theme.Ink, _border);
                var fill = AddPolygon(c.Shape, "Fill " + i, faces[i], colors[i], 0f);
                var paint = fill.gameObject.AddComponent<UiConcreteSurface>();
                paint.Configure(Screen, ConcreteTone.Wall, asPaint: true);
                paint.Apply();
                fill.color = colors[i];
            }

            c.NumberRect.anchoredPosition = new Vector2(c.CenterX, -numberY) * Unit;
            PlaceLabel(c, labelY);
            c.NumberExtrusion.Refresh();
        }

        private static void PlaceLabel(Column c, float yArt)
        {
            var pos = new Vector2(c.CenterX, -yArt) * Unit;
            c.LabelRect.anchoredPosition = pos;
            c.PlateRect.anchoredPosition = pos;
        }

        internal static Vector2[] CylinderBody(float cx, float rx, float ry, float top, float bot)
        {
            const int seg = 20;
            var pts = new List<Vector2> { new Vector2(cx - rx, top) };
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.PI - i / (float)seg * Mathf.PI;
                pts.Add(new Vector2(cx + rx * Mathf.Cos(a), bot + ry * Mathf.Sin(a)));
            }

            pts.Add(new Vector2(cx + rx, top));
            return pts.ToArray();
        }

        internal static Vector2[] Ellipse(float cx, float cy, float rx, float ry)
        {
            const int seg = 28;
            var pts = new Vector2[seg];
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                pts[i] = new Vector2(cx + rx * Mathf.Cos(a), cy + ry * Mathf.Sin(a));
            }

            return pts;
        }

        // ---- arayüz ----

        private void ApplySlide(Group g) =>
            g.Rect.anchoredPosition = new Vector2(g.Extent * Unit * (1f - g.Slide.Value), 0f);

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120; // HUD (100) ve oda kartının (110) üstünde, pause'un (200) altında
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<UiPixelScale>();

            _border = _theme.BorderWidthFor(Screen) * Unit;
            _shadow = _theme.ShadowOffsetFor(Screen) * Unit;

            _root = NewRect("Root", transform).gameObject;
            Stretch((RectTransform)_root.transform);

            var backdropRect = NewRect("Backdrop", _root.transform);
            Stretch(backdropRect);
            _backdrop = backdropRect.gameObject.AddComponent<Image>();
            _backdrop.raycastTarget = false;
            var wall = backdropRect.gameObject.AddComponent<UiConcreteSurface>();
            wall.Configure(Screen, ConcreteTone.Wall, asPaint: false);
            wall.Apply();

            _frame = NewRect("Frame", _root.transform);
            _frame.anchorMin = _frame.anchorMax = new Vector2(0.5f, 0.5f);
            _frame.pivot = new Vector2(0f, 1f);
            _frame.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            _frame.anchoredPosition = new Vector2(-FrameW * 0.5f, FrameH * 0.5f) * Unit;

            float x0 = -SideExtra, x1 = FrameW + SideExtra;
            float wide = FrameW + SideExtra * 2f;

            // çapraz siyah kiriş (kalınlık 32, merkez (0,223) → (480,79))
            var beam = NewGroup("Beam", -(wide + 40f), 0f);
            var beamPts = new[]
            {
                new Vector2(x0, Line(223f, 79f, x0) - 16f), new Vector2(x1, Line(223f, 79f, x1) - 16f),
                new Vector2(x1, Line(223f, 79f, x1) + 16f), new Vector2(x0, Line(223f, 79f, x0) + 16f)
            };
            AddPolygon(beam.Rect, "Beam", beamPts, _theme.Ink, 0f);

            // eğik zemin: koyu levha + tehlike şeridi
            var floor = NewGroup("Floor", wide + 40f, 0.06f);
            var slabPts = new[] { new Vector2(x0, Line(244f, 232f, x0)), new Vector2(x1, Line(244f, 232f, x1)), new Vector2(x1, 420f), new Vector2(x0, 420f) };
            AddPolygon(floor.Rect, "Slab Outline", slabPts, _theme.Ink, _border);
            var slab = AddPolygon(floor.Rect, "Slab", slabPts, _theme.ConcreteMid, 0f);
            var slabPaint = slab.gameObject.AddComponent<UiConcreteSurface>();
            slabPaint.Configure(Screen, ConcreteTone.Mid, asPaint: false);
            slabPaint.Apply();
            var stripPts = new[] { new Vector2(x0, Line(238f, 226f, x0)), new Vector2(x1, Line(238f, 226f, x1)), new Vector2(x1, Line(252f, 240f, x1)), new Vector2(x0, Line(252f, 240f, x0)) };
            AddPolygon(floor.Rect, "Hazard Outline", stripPts, _theme.Ink, _border);
            var strip = AddPolygon(floor.Rect, "Hazard", stripPts, _theme.Yellow, 0f);
            var stripes = strip.gameObject.AddComponent<UiHazardStripes>();
            stripes.Screen = Screen;

            // sütunlar: zeminden yükselir, kırpma çizgisinin altı görünmez
            var stage = NewRect("Stage", _frame);
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(0f, 1f);
            stage.anchoredPosition = Vector2.zero;
            stage.sizeDelta = new Vector2(FrameW, ClipY) * Unit;
            stage.gameObject.AddComponent<RectMask2D>();
            _columns[0] = NewColumn(stage, 0, 150f);
            _columns[1] = NewColumn(stage, 1, 314f);

            // sol üst başlık levhası
            var plaque = NewGroup("Plaque", -(60f + 240f), 0.12f);
            var plaquePts = new[] { new Vector2(14f, 14f), new Vector2(236f, 14f), new Vector2(222f, 50f), new Vector2(14f, 50f) };
            AddSolid(plaque.Rect, "Plaque", plaquePts, _theme.Yellow, true);
            _headline = AddText("Headline", plaque.Rect, _theme.TitleFont, 19f, _theme.Ink, TextAlignmentOptions.MidlineLeft,
                new Vector2(28f, 32f), new Vector2(200f, 28f), new Vector2(0f, 0.5f));

            // süs: sarı üçgen
            var decor = NewGroup("Decor", 140f, 0.2f);
            AddSolid(decor.Rect, "Triangle", new[] { new Vector2(392f, 26f), new Vector2(452f, 26f), new Vector2(422f, 76f) }, _theme.Yellow, true);

            // alt ipucu (koyu zemin üstünde)
            _hint = AddText("Hint", _frame, _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont, 9f, _theme.Paper,
                TextAlignmentOptions.MidlineLeft, new Vector2(18f, 262f), new Vector2(200f, 12f), new Vector2(0f, 0.5f));
            _hint.text = "A: Lobiye dön";
            var hc = _hint.color;
            hc.a = 0f;
            _hint.color = hc;

            foreach (var g in _groups) ApplySlide(g);
        }

        private static float Line(float yAt0, float yAt480, float x) => yAt0 + (yAt480 - yAt0) * (x / FrameW);

        private Column NewColumn(RectTransform parent, int index, float centerX)
        {
            var c = new Column { Index = index, CenterX = centerX };
            c.Rect = NewRect(index == 0 ? "Cylinder" : "Cube", parent);
            c.Rect.anchorMin = c.Rect.anchorMax = c.Rect.pivot = new Vector2(0f, 1f);
            c.Rect.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            c.Rect.anchoredPosition = Vector2.zero;
            c.Shape = NewRect("Shape", c.Rect);
            Stretch(c.Shape);
            c.ShadowRect = NewRect("Shadow", c.Shape);
            Stretch(c.ShadowRect);

            var numberFont = _theme.TitleFont;
            var numberText = AddText("Number", c.Rect, numberFont, 40f, _theme.Paper, TextAlignmentOptions.Center,
                Vector2.zero, new Vector2(60f, 50f), new Vector2(0.5f, 0.5f));
            numberText.text = (index + 1).ToString();
            c.Number = numberText;
            c.NumberRect = numberText.rectTransform;
            c.NumberExtrusion = numberText.gameObject.AddComponent<UiExtrudedText>();
            c.NumberExtrusion.Screen = Screen;

            c.PlateRect = NewRect("LabelPlate", c.Rect);
            c.PlateRect.anchorMin = c.PlateRect.anchorMax = c.PlateRect.pivot = new Vector2(0f, 1f);
            c.PlateRect.sizeDelta = Vector2.zero;
            var plate = new[] { new Vector2(-47f, -8f), new Vector2(47f, -8f), new Vector2(47f, 8f), new Vector2(-47f, 8f) };
            AddPolygon(c.PlateRect, "Outline", plate, _theme.Ink, _border);
            c.PlateFill = AddPolygon(c.PlateRect, "Fill", plate, _theme.Ink, 0f);

            c.Label = AddText("Label", c.Rect, _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont, 10f, _theme.Ink,
                TextAlignmentOptions.Center, Vector2.zero, new Vector2(120f, 14f), new Vector2(0.5f, 0.5f));
            c.LabelRect = c.Label.rectTransform;
            return c;
        }

        private Group NewGroup(string name, float extentArt, float delay)
        {
            var rect = NewRect(name, _frame);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            var g = new Group { Rect = rect, Extent = extentArt, Delay = delay };
            _groups.Add(g);
            return g;
        }

        /// <summary>Sert gölge + çerçeve + dolgu (boyalı beton).</summary>
        private void AddSolid(RectTransform parent, string name, Vector2[] artPoints, Color fill, bool paint)
        {
            var shadow = AddPolygon(parent, name + " Shadow", artPoints, _theme.Ink, _border);
            shadow.rectTransform.anchoredPosition = new Vector2(_shadow, -_shadow);
            AddPolygon(parent, name + " Outline", artPoints, _theme.Ink, _border);
            var f = AddPolygon(parent, name + " Fill", artPoints, fill, 0f);
            if (!paint) return;
            var p = f.gameObject.AddComponent<UiConcreteSurface>();
            p.Configure(Screen, ConcreteTone.Wall, asPaint: true);
            p.Apply();
            f.color = fill;
        }

        /// <summary>Noktalar sanat pikseli, çerçevenin sol-üstünden (y aşağı); <paramref name="grow"/> kadar (canvas birimi) dışa itilir.</summary>
        private static UiPolygon AddPolygon(RectTransform parent, string name, Vector2[] artPoints, Color color, float grow)
        {
            var local = new Vector2[artPoints.Length];
            for (int i = 0; i < local.Length; i++) local[i] = new Vector2(artPoints[i].x, -artPoints[i].y) * Unit;
            if (grow > 0f) local = PolygonMath.OffsetConvex(local, grow);

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            var poly = go.AddComponent<UiPolygon>();
            var rt = poly.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            rt.anchoredPosition = Vector2.zero;
            poly.raycastTarget = false;
            poly.SetPoints(local);
            poly.color = color;
            return poly;
        }

        private TMP_Text AddText(string name, RectTransform parent, TMP_FontAsset font, float artSize, Color color,
            TextAlignmentOptions alignment, Vector2 positionArt, Vector2 sizeArt, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = artSize * Unit;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.color = color;
            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = pivot;
            rt.sizeDelta = sizeArt * Unit;
            rt.anchoredPosition = new Vector2(positionArt.x, -positionArt.y) * Unit;
            return text;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
