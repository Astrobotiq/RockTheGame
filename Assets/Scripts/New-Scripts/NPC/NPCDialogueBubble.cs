using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Player;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.NPC
{
    /// <summary>
    /// NPC diyalog "levhası": ekranın alt köşesinde küçük, kesik köşeli beton levha (sarı rozet + tehlike şeridi + mono metin).
    /// Oyunu durdurmaz, ekranı kaplamaz (~%30 genişlik). Arayüz kodla kurulur (overlay canvas, <see cref="UiPixelScale"/>);
    /// sahnedeki eski balon görselleri (Image/TMP) kapatılır, API (<see cref="Instance"/>, <see cref="ShowDialogueAsync"/>,
    /// <see cref="HideDialogueAsync"/>) aynı kalır.
    /// <list type="bullet">
    /// <item>Hareket: <see cref="UiSpring"/> (levha kenardan kayar, rozet büyükten çakılır, gölge çakılır, rozet boşta nefes alır),
    /// unscaled zamanda. DOTween yok (docs/systems/ui-motion.md).</item>
    /// <item>Oyuncuyu kapatmama: oyuncu levhanın yarısına girince levha karşı köşeye kayar (<see cref="DialogueSlabMath.ResolveSide"/>,
    /// histerezis + bekleme); yine de üstüne biniyorsa arka planı soluklaşır, yazı solmaz.</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/npc-dialogue.md).
    /// </summary>
    [AddComponentMenu("RockTheGame/NPC/NPC Dialogue Bubble")]
    public class NPCDialogueBubble : MonoBehaviour
    {
        private const float Unit = UiPixelScale.UnitsPerArtPixel;
        private const UiScreen ThemeScreen = UiScreen.Hud;

        [Header("Boyut (sanat pikseli, 480x270 ızgara)")]
        [SerializeField, Range(60f, 160f)] private float textWidthArt = 104f;
        [SerializeField, Range(5f, 12f)] private float fontSizeArt = 8f;
        [SerializeField, Range(16f, 40f)] private float badgeWidthArt = 26f;
        [SerializeField, Range(0f, 24f)] private float edgeMarginArt = 8f;
        [SerializeField, Range(0f, 40f)] private float bottomMarginArt = 12f;

        [Header("Oyuncuyu kapatmama")]
        [Tooltip("Oyuncu orta çizgiyi bu kadar (viewport) levhanın yarısına geçince levha karşı köşeye kayar.")]
        [SerializeField, Range(0f, 0.3f)] private float sideHysteresis = 0.1f;
        [Tooltip("İki köşe değişimi arası en kısa süre (sn); oyuncu iki yarı arasında gidip gelirken levha titremesin.")]
        [SerializeField, Range(0f, 3f)] private float flipCooldown = 0.6f;
        [Tooltip("Oyuncunun etrafındaki pay (viewport): levhaya bu kadar yaklaşınca arka plan solar.")]
        [SerializeField, Range(0f, 0.15f)] private float playerClearance = 0.04f;
        [Tooltip("Oyuncu levhanın üstündeyken arka plan alfası (yazı solmaz).")]
        [SerializeField, Range(0.1f, 1f)] private float overlapAlpha = 0.4f;
        [SerializeField, Range(1f, 20f)] private float fadeSpeed = 8f;

        [Header("Hareket (başlangıç önerisi)")]
        [Tooltip("Rozetin çakılmadan önceki ölçeği (1 = çakma yok).")]
        [SerializeField, Range(1f, 3f)] private float badgeSlamScale = 1.7f;
        [Tooltip("Rozetin boşta nefes miktarı (ölçek oranı).")]
        [SerializeField, Range(0f, 0.1f)] private float breathAmount = 0.03f;
        [SerializeField, Range(0.5f, 4f)] private float breathSpeed = 2f;

        public static NPCDialogueBubble Instance { get; private set; }

        private readonly UiSpring _slide = new UiSpring(240f, 22f);
        private readonly UiSpring _side = new UiSpring(200f, 20f);
        private readonly UiSpring _slam = new UiSpring(320f, 16f);
        private readonly List<CanvasRenderer> _backdrop = new List<CanvasRenderer>();
        private readonly Vector3[] _corners = new Vector3[4];

        private UiThemeSO _theme;
        private GameObject _canvasRoot;
        private RectTransform _canvasRect;
        private RectTransform _content;
        private RectTransform _shadowRect, _iconRect;
        private UiPolygon _shadow, _outline, _fill, _badge, _divider, _strip, _icon;
        private UiConcreteSurface _fillSurface;
        private UiHazardStripes _stripStripes;
        private TMP_Text _text;

        private bool _visible;
        private SlabSide _sideState = SlabSide.Left;
        private float _lastFlipTime = -999f;
        private float _alpha = 1f;
        private float _widthArt = 100f;
        private float _heightArt = 30f;
        private float _shadowBase;
        private PlayerController _player;
        private Camera _camera;

        // ---- yaşam döngüsü ----

        private void Awake()
        {
            Instance = this;
            _theme = UiThemeSO.Load();

            // Sahnedeki eski balon görselleri (Image + TMP) yeni levhayla çakışmasın.
            foreach (var g in GetComponentsInChildren<Graphic>(true)) g.enabled = false;

            BuildUi();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_canvasRoot != null) Destroy(_canvasRoot);
        }

        /// <summary>Levhayı gösterir ve metni harf harf yazdırır.</summary>
        public async UniTask ShowDialogueAsync(string text, float delayBetweenChars, CancellationToken ct)
        {
            _text.text = text;
            _text.maxVisibleCharacters = 0;
            Layout(text);

            bool wasHidden = !_visible && _slide.Value < 0.02f;
            _visible = true;
            _content.gameObject.SetActive(true);

            if (wasHidden)
            {
                // Taraf kayarak değil, doğrudan seçilir; levha o kenardan gelir.
                if (TryGetPlayerViewport(out Vector2 pv)) _sideState = DialogueSlabMath.ResolveSide(pv.x, _sideState, sideHysteresis);
                _side.Value = _side.Target = _sideState == SlabSide.Right ? 1f : 0f;
                _side.Velocity = 0f;
                _slide.Value = 0f;
                _slide.Velocity = 0f;
                _slam.Value = badgeSlamScale;
                _slam.Velocity = 0f;
            }

            _slide.Target = 1f;
            _slam.Target = 1f;

            try
            {
                await UniTask.WaitUntil(() => _slide.Value > 0.8f, cancellationToken: ct);

                _text.ForceMeshUpdate();
                int total = _text.textInfo.characterCount;
                for (int i = 0; i <= total; i++)
                {
                    _text.maxVisibleCharacters = i;
                    await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenChars), cancellationToken: ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Oyuncu alandan çıktı ya da yeni satır geldi: yazma sessizce durur.
            }
        }

        /// <summary>Levhayı kenara geri çekerek kapatır.</summary>
        public async UniTask HideDialogueAsync(CancellationToken ct)
        {
            _visible = false;
            _slide.Target = 0f;

            try
            {
                await UniTask.WaitUntil(() => _slide.Value < 0.02f, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                // Oyuncu tekrar yaklaştı: Show zaten yeniden hedef verdi.
            }
        }

        // ---- hareket ----

        private void Update()
        {
            if (_content == null || !_content.gameObject.activeSelf) return;

            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;

            _slide.Step(dt);
            _side.Step(dt);
            _slam.Step(dt);

            bool hasPlayer = TryGetPlayerViewport(out Vector2 playerViewport);
            if (hasPlayer && _visible) UpdateSide(playerViewport.x, now);

            ApplyPosition();
            ApplyBadge(now);
            UpdateAlpha(dt, hasPlayer && _visible && Overlapping(playerViewport));

            if (!_visible && _slide.Value < 0.02f) _content.gameObject.SetActive(false);
        }

        private void UpdateSide(float playerX, float now)
        {
            var want = DialogueSlabMath.ResolveSide(playerX, _sideState, sideHysteresis);
            if (want == _sideState || now - _lastFlipTime < flipCooldown) return;

            _sideState = want;
            _side.Target = want == SlabSide.Right ? 1f : 0f;
            _lastFlipTime = now;
        }

        private void ApplyPosition()
        {
            float w = _widthArt * Unit;
            float margin = edgeMarginArt * Unit;
            float leftX = margin;
            float rightX = _canvasRect.rect.width - w - margin;
            float x = Mathf.LerpUnclamped(leftX, rightX, _side.Value);

            float dir = _sideState == SlabSide.Right ? 1f : -1f;
            x += dir * (w + margin) * (1f - _slide.Value);
            _content.anchoredPosition = new Vector2(x, bottomMarginArt * Unit);
        }

        private void ApplyBadge(float now)
        {
            float breath = 1f + breathAmount * Mathf.Sin(now * breathSpeed * Mathf.PI * 2f);
            _iconRect.localScale = Vector3.one * Mathf.Max(0.0001f, _slam.Value * breath);

            // Gölge çakılır: rozetle birlikte büyükten oturur.
            float k = Mathf.Max(0f, 1f + (_slam.Value - 1f));
            _shadowRect.anchoredPosition = new Vector2(_shadowBase * k, -_shadowBase * k);
        }

        private void UpdateAlpha(float dt, bool overlapping)
        {
            float target = overlapping ? overlapAlpha : 1f;
            if (Mathf.Approximately(_alpha, target)) return;

            _alpha = Mathf.MoveTowards(_alpha, target, fadeSpeed * dt);
            for (int i = 0; i < _backdrop.Count; i++) _backdrop[i].SetAlpha(_alpha);
        }

        // ---- oyuncu ----

        private bool TryGetPlayerViewport(out Vector2 viewport)
        {
            viewport = default;
            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            if (_camera == null) _camera = Camera.main;
            if (_player == null || _camera == null) return false;

            Vector3 v = _camera.WorldToViewportPoint(_player.transform.position);
            if (v.z < 0f) return false;
            viewport = new Vector2(v.x, v.y);
            return true;
        }

        private bool Overlapping(Vector2 playerViewport)
        {
            _content.GetWorldCorners(_corners); // overlay canvas: dünya = ekran pikseli
            float sw = UnityEngine.Screen.width;
            float sh = UnityEngine.Screen.height;
            var rect = Rect.MinMaxRect(_corners[0].x / sw, _corners[0].y / sh, _corners[2].x / sw, _corners[2].y / sh);
            return DialogueSlabMath.Overlaps(rect, playerViewport, playerClearance);
        }

        // ---- yerleşim ----

        /// <summary>Metnin yüksekliğine göre levhanın şeklini yeniden kurar (yazı bitmeden boyut oturur, harf harf büyümez).</summary>
        private void Layout(string text)
        {
            const float stripH = 6f;
            const float padTop = 4f;
            const float padBottom = 4f;
            const float padLeft = 5f;
            const float padRight = 6f;
            const float cut = 4f;

            float border = _theme.BorderWidthFor(ThemeScreen);
            float shadow = _theme.ShadowOffsetFor(ThemeScreen);
            float textH = _text.GetPreferredValues(text, textWidthArt * Unit, 0f).y / Unit;

            _widthArt = badgeWidthArt + padLeft + textWidthArt + padRight;
            _heightArt = Mathf.Max(badgeWidthArt, Mathf.Ceil(stripH + padTop + textH + padBottom));
            float w = _widthArt, h = _heightArt;
            _shadowBase = shadow * Unit;

            _content.sizeDelta = new Vector2(w, h) * Unit;

            var slab = new[] { P(0, 0), P(w, 0), P(w, h), P(cut, h), P(0, h - cut) };
            var outlinePoints = OffsetArt(slab, border);
            SetShape(_shadow, outlinePoints);
            SetShape(_outline, outlinePoints);
            SetShape(_fill, slab);
            _fillSurface.Apply();

            SetShape(_badge, new[] { P(0, stripH), P(badgeWidthArt, stripH), P(badgeWidthArt, h), P(cut, h), P(0, h - cut) });
            float b = Mathf.Max(1f, border);
            SetShape(_divider, new[] { P(badgeWidthArt, stripH), P(badgeWidthArt + b, stripH), P(badgeWidthArt + b, h), P(badgeWidthArt, h) });
            SetShape(_strip, new[] { P(0, 0), P(w, 0), P(w, stripH), P(0, stripH) });
            _stripStripes.Apply();

            // Rozet simgesi: rozetin ortasında ink daire (kendi merkezinden ölçeklenir).
            float cx = badgeWidthArt * 0.5f;
            float cy = stripH + (h - stripH) * 0.5f;
            _iconRect.sizeDelta = Vector2.zero;
            _iconRect.anchoredPosition = new Vector2(cx, -cy) * Unit;

            var textRt = _text.rectTransform;
            textRt.anchoredPosition = new Vector2(badgeWidthArt + b + padLeft, -(stripH + padTop)) * Unit;
            textRt.sizeDelta = new Vector2(textWidthArt, Mathf.Max(1f, textH)) * Unit;
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);

        /// <summary>Çokgeni her kenarından dışa iterek çerçeve kalınlığı verir (dışbükey çokgenler için).</summary>
        private static Vector2[] OffsetArt(Vector2[] artPoints, float amount)
        {
            var local = new Vector2[artPoints.Length];
            for (int i = 0; i < local.Length; i++) local[i] = new Vector2(artPoints[i].x, -artPoints[i].y);
            var offset = PolygonMath.OffsetConvex(local, amount);
            var result = new Vector2[offset.Length];
            for (int i = 0; i < offset.Length; i++) result[i] = new Vector2(offset[i].x, -offset[i].y);
            return result;
        }

        /// <summary>Sanat pikseli (y aşağı, sol-üstten) noktaları şekle yazar.</summary>
        private void SetShape(UiPolygon poly, Vector2[] artPoints)
        {
            var local = new Vector2[artPoints.Length];
            for (int i = 0; i < local.Length; i++) local[i] = new Vector2(artPoints[i].x, -artPoints[i].y) * Unit;
            poly.rectTransform.sizeDelta = new Vector2(_widthArt + 16f, _heightArt + 16f) * Unit;
            poly.SetPoints(local);
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            _canvasRoot = new GameObject("NPCDialogueCanvas", typeof(RectTransform));
            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 105; // HUD sayacının (100) üstünde, oda kartının (110) altında
            _canvasRoot.AddComponent<CanvasScaler>();
            _canvasRoot.AddComponent<UiPixelScale>();
            _canvasRect = (RectTransform)_canvasRoot.transform;

            _content = NewRect("Content", _canvasRoot.transform);
            _content.anchorMin = _content.anchorMax = Vector2.zero;
            _content.pivot = Vector2.zero;

            // Kaplar sol-üst köşeden (0,1) tutar; şekiller içeride sanat pikselinde.
            var shadowGo = NewRect("ShadowGroup", _content);
            ConfigureTopLeft(shadowGo);
            _shadowRect = shadowGo;
            _shadow = NewPolygon("Shadow", shadowGo, _theme.Ink);
            _outline = NewPolygon("Outline", _content, _theme.Ink);
            _fill = NewPolygon("Fill", _content, _theme.ConcreteSlab);
            _fillSurface = _fill.gameObject.AddComponent<UiConcreteSurface>();
            _fillSurface.Configure(ThemeScreen, ConcreteTone.Slab, asPaint: false);

            _badge = NewPolygon("Badge", _content, _theme.Yellow);
            var badgeSurface = _badge.gameObject.AddComponent<UiConcreteSurface>();
            badgeSurface.Configure(ThemeScreen, ConcreteTone.Wall, asPaint: true);
            badgeSurface.Apply();
            _badge.color = _theme.Yellow;

            _divider = NewPolygon("Divider", _content, _theme.Ink);

            _strip = NewPolygon("Strip", _content, _theme.Yellow);
            _stripStripes = _strip.gameObject.AddComponent<UiHazardStripes>();
            _stripStripes.Screen = ThemeScreen;

            // Simge: kendi merkezinden ölçeklenen ink daire.
            _iconRect = NewRect("Icon", _content);
            _iconRect.anchorMin = _iconRect.anchorMax = new Vector2(0f, 1f);
            _iconRect.pivot = new Vector2(0.5f, 0.5f);
            _icon = _iconRect.gameObject.AddComponent<UiPolygon>();
            _icon.raycastTarget = false;
            _icon.color = _theme.Ink;
            const float iconRadius = 6f;
            _icon.SetPoints(CirclePoints(iconRadius * Unit, 24));
            _iconRect.sizeDelta = new Vector2(iconRadius * 2f, iconRadius * 2f) * Unit;

            var textGo = NewRect("Text", _content);
            textGo.anchorMin = textGo.anchorMax = new Vector2(0f, 1f);
            textGo.pivot = new Vector2(0f, 1f);
            var tmp = textGo.gameObject.AddComponent<TextMeshProUGUI>();
            var font = _theme.MonoFont != null ? _theme.MonoFont : _theme.TitleFont;
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSizeArt * Unit;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            tmp.color = _theme.Ink;
            _text = tmp;

            // Solacak arka plan: tüm şekiller; yazı hariç.
            foreach (var g in new Graphic[] { _shadow, _outline, _fill, _badge, _divider, _strip, _icon })
                _backdrop.Add(g.canvasRenderer);

            _content.gameObject.SetActive(false);
        }

        private static void ConfigureTopLeft(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static UiPolygon NewPolygon(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var poly = go.AddComponent<UiPolygon>();
            var rt = poly.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            poly.raycastTarget = false;
            poly.color = color;
            return poly;
        }

        private static Vector2[] CirclePoints(float radius, int segments)
        {
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * radius;
            }

            return points;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
