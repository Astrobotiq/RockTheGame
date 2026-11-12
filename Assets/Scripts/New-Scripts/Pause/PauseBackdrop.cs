using System;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Pause arka planı (mockup): dither + yarı saydam karartma, iki boyalı daire ve kesişim rengi (mercek), köşede sarı üçgen.
    /// Sahne öğeleri radyal menüde seçilen dilime göre sabit bir pozda yerleşir (<see cref="Pose"/>), boşta hafifçe sürüklenir,
    /// girişte dışarıdan kayarak gelir. Hepsi yaylarla (<see cref="UiSpring"/>) ve unscaled zamanla; yalnızca transform oynar
    /// (mesh yeniden üretilmez). Değerler başlangıç önerisi: gamepad ile ayarlanır.
    /// </summary>
    public sealed class PauseBackdrop
    {
        /// <summary>Bir dilim seçiliyken sahnenin duruşu. Konumlar sanat pikseli (1 = 4 canvas birimi).</summary>
        [Serializable]
        public struct Pose
        {
            public Vector2 PairOffset;
            public float PairRotation;
            public float PairScale;
            public Vector2 TriangleOffset;
        }

        /// <summary>resume = ev pozu; options sağa/aşağı, quit içeri, retry sola.</summary>
        public static Pose[] DefaultPoses() => new[]
        {
            new Pose { PairScale = 1f },
            new Pose { PairOffset = new Vector2(26f, -14f), PairRotation = -4f, PairScale = 1.05f, TriangleOffset = new Vector2(-14f, 10f) },
            new Pose { PairOffset = new Vector2(-12f, -34f), PairRotation = 5f, PairScale = 0.96f, TriangleOffset = new Vector2(18f, -12f) },
            new Pose { PairOffset = new Vector2(-34f, 16f), PairRotation = 3f, PairScale = 1.03f, TriangleOffset = new Vector2(8f, 16f) }
        };

        /// <summary>Dither'ın kayma hızı (sanat pikseli/sn). 0 = durur. Başlangıç önerisi, gamepad ile ayarlanır.</summary>
        public float DitherScrollPixelsPerSecond = 0f;

        private const float Unit = 4f;
        private static readonly int ArtPixelId = Shader.PropertyToID("_ArtPixel");
        private static readonly int ScrollId = Shader.PropertyToID("_Scroll");

        // Mockup ölçüleri (480x270, sol-üst orijin) → merkez orijinli canvas birimi.
        private static readonly Vector2 RedCenter = ToCanvas(104f, 100f);
        private static readonly Vector2 BlueCenter = ToCanvas(164f, 178f);
        private const float CircleRadius = 78f * Unit;
        // Üçgen sağ-alt köşeye göre: (330,262), (476,262), (476,84), köşe (480,270).
        private static readonly Vector2[] TriangleFromCorner =
        {
            new Vector2(-150f * Unit, 8f * Unit),
            new Vector2(-4f * Unit, 8f * Unit),
            new Vector2(-4f * Unit, 186f * Unit)
        };

        private readonly Pose[] _poses;
        private readonly UiSpring _px = new UiSpring(150f, 9f), _py = new UiSpring(150f, 9f);
        private readonly UiSpring _rot = new UiSpring(170f, 9f), _scale = new UiSpring(150f, 9f);
        private readonly UiSpring _tx = new UiSpring(160f, 9f), _ty = new UiSpring(160f, 9f);
        private readonly UiSpring _pairIn = new UiSpring(170f, 15f), _triIn = new UiSpring(170f, 15f), _dimIn = new UiSpring(420f, 40f);

        private RectTransform _root, _pair, _triangle;
        private Image _dither;
        private Material _ditherMaterial;
        private Vector2 _pairHome;
        private float _age, _pairWait, _triWait;
        private int _pose;

        private readonly UiSpring[] _poseSprings;
        private readonly UiSpring[] _allSprings;

        public PauseBackdrop(Pose[] poses = null)
        {
            _poses = poses != null && poses.Length > 0 ? poses : DefaultPoses();
            _poseSprings = new[] { _px, _py, _rot, _scale, _tx, _ty };
            _allSprings = new[] { _px, _py, _rot, _scale, _tx, _ty, _pairIn, _triIn, _dimIn };
        }

        public RectTransform Root => _root;

        public void Build(Transform parent, UiThemeSO theme)
        {
            _root = NewRect("Backdrop", parent);
            Stretch(_root);

            float balance = theme.BalanceFor(UiScreen.Pause);
            float border = theme.BorderWidthFor(UiScreen.Pause) * Unit;
            float shadow = UiBalance.ShadowOffset(5f, balance) * Unit;

            BuildDim(theme);

            // Daireler ve mercek tek grupta: grup merceğin ortasında döner/ölçeklenir, şekiller bozulmaz.
            var lens = PolygonMath.CircleLens(RedCenter, CircleRadius, BlueCenter, CircleRadius, 40);
            _pairHome = PolygonMath.Centroid(lens);
            _pair = NewRect("Pair", _root);
            Center(_pair, Vector2.zero);

            AddCircle("Red", _pair, RedCenter - _pairHome, CircleRadius, border, theme.Red, theme.Ink);
            AddCircle("Blue", _pair, BlueCenter - _pairHome, CircleRadius, border, theme.Blue, theme.Ink);

            // Mercek: önce çerçeveyi veren genişletilmiş mercek (ink), sonra kesişim rengi.
            var lensOuter = PolygonMath.CircleLens(RedCenter, CircleRadius + border, BlueCenter, CircleRadius + border, 40);
            AddPolygon("LensOutline", _pair, lensOuter, -_pairHome, theme.Ink);
            AddPolygon("LensFill", _pair, lens, -_pairHome, theme.Purple);

            // Üçgen sağ-alt köşeye bağlı: ekran oranı değişince köşede kalır.
            _triangle = NewRect("Triangle", _root);
            _triangle.anchorMin = _triangle.anchorMax = _triangle.pivot = new Vector2(1f, 0f);
            _triangle.anchoredPosition = Vector2.zero;
            _triangle.sizeDelta = Vector2.zero;
            var outline = PolygonMath.OffsetConvex(TriangleFromCorner, border);
            AddPolygon("TriangleShadow", _triangle, outline, new Vector2(shadow, -shadow), theme.Ink);
            AddPolygon("TriangleOutline", _triangle, outline, Vector2.zero, theme.Ink);
            AddPolygon("TriangleFill", _triangle, TriangleFromCorner, Vector2.zero, theme.Yellow);
        }

        public void Destroy()
        {
            if (_ditherMaterial != null) UnityEngine.Object.Destroy(_ditherMaterial);
        }

        /// <summary>Girişi baştan oynatır; <paramref name="pose"/> başlangıç duruşu.</summary>
        public void Show(int pose)
        {
            _age = 0f;
            _pose = Mathf.Clamp(pose, 0, _poses.Length - 1);
            ApplyPoseTargets();
            foreach (var s in _poseSprings)
            {
                s.Value = s.Target;
                s.Velocity = 0f;
            }

            // Karartma hemen başlar, daireler biraz sonra, üçgen en sonra; hepsi ekran dışından kayar.
            _dimIn.Value = 1f; _dimIn.Target = 0f; _dimIn.Velocity = 0f;
            _pairIn.Value = 1f; _pairIn.Target = 1f; _pairIn.Velocity = 0f; _pairWait = 0.04f;
            _triIn.Value = 1f; _triIn.Target = 1f; _triIn.Velocity = 0f; _triWait = 0.12f;
            Step(0f);
        }

        public void SetPose(int pose)
        {
            _pose = Mathf.Clamp(pose, 0, _poses.Length - 1);
            ApplyPoseTargets();
        }

        public void Step(float dt)
        {
            _age += dt;
            if (_pairWait > 0f) { _pairWait -= dt; if (_pairWait <= 0f) _pairIn.Target = 0f; }
            if (_triWait > 0f) { _triWait -= dt; if (_triWait <= 0f) _triIn.Target = 0f; }
            foreach (var s in _allSprings) s.Step(dt);

            // Karartma yumuşakça gelir.
            float dim = 1f - Mathf.Clamp01(_dimIn.Value);
            _dither.color = WithAlpha(_dither.color, dim);
            ScrollDither();

            float idle = Mathf.Clamp01((_age - 0.6f) / 1f);
            float t = _age;
            Vector2 drift = new Vector2(Mathf.Sin(t * 0.5f) * 10f, Mathf.Cos(t * 0.4f) * 8f) * idle;
            float tilt = Mathf.Sin(t * 0.35f) * 0.8f * idle;
            Vector2 triDrift = new Vector2(Mathf.Sin(t * 0.45f + 1f) * 6f, Mathf.Cos(t * 0.5f + 2f) * 6f) * idle;

            // Giriş: daireler sol-üstten, üçgen sağ-alttan kayarak gelir (kısa mesafe, ekranın 1/3'ünü geçmez).
            Vector2 pairIn = new Vector2(-1f, 0.55f) * (_pairIn.Value * 560f);
            Vector2 triIn = new Vector2(1f, -0.6f) * (_triIn.Value * 420f);

            _pair.anchoredPosition = _pairHome + new Vector2(_px.Value, _py.Value) * Unit + drift + pairIn;
            _pair.localRotation = Quaternion.Euler(0f, 0f, _rot.Value + tilt);
            _pair.localScale = Vector3.one * Mathf.Max(0.01f, _scale.Value);
            _triangle.anchoredPosition = new Vector2(_tx.Value, _ty.Value) * Unit + triDrift + triIn;
        }

        private void ApplyPoseTargets()
        {
            var p = _poses[_pose];
            _px.Target = p.PairOffset.x;
            _py.Target = p.PairOffset.y;
            _rot.Target = p.PairRotation;
            _scale.Target = p.PairScale > 0f ? p.PairScale : 1f;
            _tx.Target = p.TriangleOffset.x;
            _ty.Target = p.TriangleOffset.y;
        }

        // ---- kurulum ----

        /// <summary>
        /// Karartma tek tam ekran Image: desen, %50 örtü ve kayma <c>RockTheGame/UI/Dither</c> shader'ında (doku/mesh yok).
        /// Kayma yalnızca malzemeye bir vektör yazar, canvas'ı kirletmez.
        /// </summary>
        private void BuildDim(UiThemeSO theme)
        {
            var go = new GameObject("Dither", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(_root, false);
            _dither = go.AddComponent<Image>();
            _dither.raycastTarget = false;
            _dither.color = WithAlpha(theme.Ink, 0f);
            Stretch(_dither.rectTransform);

            if (theme.DitherMaterial == null)
            {
                Debug.LogWarning("[PauseBackdrop] UiTheme'de DitherMaterial atanmamış; karartma düz örtü olarak çizilir.");
                return;
            }

            _ditherMaterial = new Material(theme.DitherMaterial) { name = theme.DitherMaterial.name + " (instance)", hideFlags = HideFlags.HideAndDontSave };
            _ditherMaterial.SetFloat(ArtPixelId, Unit);
            _dither.material = _ditherMaterial;
        }

        private void ScrollDither()
        {
            if (_ditherMaterial == null) return;
            // Tam sanat pikseli adımları: Point örnekleme gibi keskin kalır, kesirli kayma desen titretir. Unscaled zaman.
            float steps = Mathf.Floor(_age * DitherScrollPixelsPerSecond);
            _ditherMaterial.SetVector(ScrollId, new Vector4(steps, steps, 0f, 0f));
        }

        private static void AddCircle(string name, Transform parent, Vector2 center, float radius, float border, Color fill, Color ink)
        {
            MakeArc(name + "Outline", parent, center, radius + border, ink);
            MakeArc(name, parent, center, radius, fill);
        }

        private static void MakeArc(string name, Transform parent, Vector2 center, float radius, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var arc = go.AddComponent<UiArc>();
            Center(arc.rectTransform, center);
            arc.rectTransform.sizeDelta = new Vector2(radius * 2f, radius * 2f);
            arc.raycastTarget = false;
            arc.color = color;
        }

        private static void AddPolygon(string name, Transform parent, Vector2[] points, Vector2 offset, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var poly = go.AddComponent<UiPolygon>();
            var shifted = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++) shifted[i] = points[i] + offset;
            var rt = poly.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = parent is RectTransform pr && pr.anchorMin == new Vector2(1f, 0f)
                ? new Vector2(1f, 0f)
                : new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            poly.SetPoints(shifted);
            poly.raycastTarget = false;
            poly.color = color;
        }

        private static Vector2 ToCanvas(float x, float y) => new Vector2((x - 240f) * Unit, (135f - y) * Unit);

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Center(RectTransform rt, Vector2 position)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
