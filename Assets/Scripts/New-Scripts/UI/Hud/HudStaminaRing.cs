using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.UI.Hud
{
    /// <summary>
    /// Kırılan stamina halkası (mockup): karakterin etrafında 6 sarı boyalı beton parça. Duvara tutunup enerji azaldıkça
    /// parçalar sırayla (sondan) kopar, dışarı fırlar, döner, düşer ve söner. Enerji dolunca parçalar yaylanarak yerine gelir.
    /// <list type="bullet">
    /// <item>Overlay canvas'ta çizilir (CRT dışında, keskin) ve <see cref="UiPixelScale"/> ile tam sayı sanat pikseline sabitlenir.
    /// Konum her kare karakterden <c>WorldToScreenPoint</c> ile alınır ve sanat pikseline yuvarlanır.</item>
    /// <item>Zaman: unscaled (hitstop/pause sırasında da kopan parça yarım kalmaz). Hareket: yay + basit balistik.</item>
    /// <item>Uyarı: enerji <c>warningSeconds</c> eşiğinin altına inince kalan parçalar sarı/kırmızı yanıp söner (GDD §5.2).</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/wall-movement.md, ui-motion.md).
    /// </summary>
    [AddComponentMenu("RockTheGame/UI/Hud Stamina Ring")]
    public sealed class HudStaminaRing : MonoBehaviour
    {
        public const int PieceCount = 6;
        private const float Unit = UiPixelScale.UnitsPerArtPixel;

        private sealed class Piece
        {
            public RectTransform Root;
            public UiArc Fill;
            public UiArc Outline;
            public Vector2 HomeOffset;
            public Vector2 Direction;
            public bool Broken;
            public bool Present;
            public Vector2 Velocity;
            public float Spin;
            public float Age;
            public float EnterWait;
            public readonly UiSpring Pop = new UiSpring(320f, 14f);
        }

        [Header("Boyut (sanat pikseli)")]
        [SerializeField, Range(8f, 48f)] private float outerRadius = 22f;
        [SerializeField, Range(2f, 12f)] private float thickness = 5f;
        [SerializeField, Range(0f, 40f)] private float gapDegrees = 14f;
        [SerializeField, Range(1f, 3f)] private float outlineArtPixels = 1f;

        [Header("Kopma (başlangıç önerisi)")]
        [Tooltip("Parçanın halkadan dışarı fırlama hızı (sanat piksel/sn).")]
        [SerializeField, Range(0f, 200f)] private float breakOutSpeed = 70f;
        [Tooltip("Yukarı doğru ek itme (sanat piksel/sn).")]
        [SerializeField, Range(0f, 200f)] private float breakUpSpeed = 50f;
        [SerializeField, Range(0f, 800f)] private float gravity = 300f;
        [SerializeField, Range(0f, 720f)] private float spinDegrees = 240f;
        [SerializeField, Range(0.2f, 2f)] private float fallSeconds = 0.7f;

        [Header("Uyarı (eşik PlayerStatsSO.StaminaWarningThreshold'dan gelir)")]
        [SerializeField, Range(2f, 20f)] private float warningBlinkHz = 7f;

        private readonly Piece[] _pieces = new Piece[PieceCount];
        private readonly UiSpring _visible = new UiSpring(300f, 20f);

        private UiThemeSO _theme;
        private Canvas _canvas;
        private RectTransform _ringRect;
        private Transform _target;
        private Collider2D _targetCollider;
        private Camera _camera;
        private int _intact = PieceCount;
        private float _warnBelowFraction = -1f;
        private bool _shown;
        private bool _warningOn;
        private bool _blinkPhase;
        private float _hideAt = -1f;

        // ---- dış API (PlayerUIController çağırır) ----

        /// <summary>Halkanın izleyeceği nesne (oyuncu). <paramref name="collider"/> verilirse halka onun merkezine oturur (pivot ayakta olabilir).</summary>
        public void Bind(Transform target, Collider2D collider = null)
        {
            _target = target;
            _targetCollider = collider;
        }

        /// <summary>Halkayı gösterir (duvara tutunma).</summary>
        public void Show()
        {
            _hideAt = -1f;
            if (_shown) return;
            _shown = true;
            _visible.Target = 1f;
        }

        /// <summary>
        /// Enerji değişti. <paramref name="current"/>/<paramref name="max"/> oranı sağlam parça sayısını belirler;
        /// azalan her parça kopar. <paramref name="warningThreshold"/> (sn) altına inince uyarı yanıp sönmesi başlar.
        /// </summary>
        public void SetStamina(float current, float max, float warningThreshold)
        {
            if (max <= 0f) return;
            _warnBelowFraction = warningThreshold / max;

            int intact = StaminaRingMath.IntactPieces(current / max, PieceCount);
            for (int i = _intact - 1; i >= intact; i--) Break(i);
            // Oran yukarı çıkarsa (nadir) eksik parçaları geri getir.
            for (int i = _intact; i < intact; i++) Restore(i, 0f);
            _intact = intact;

            _warningOn = current > 0f && current / max <= _warnBelowFraction;
            if (!_warningOn) SetBlink(false);
        }

        /// <summary>Enerji doldu: kopanlar yerine yaylanarak gelir, kısa süre sonra halka çekilir.</summary>
        public void RefillAndHide()
        {
            if (!_shown) return;
            _warningOn = false;
            SetBlink(false);
            _intact = PieceCount;
            for (int i = 0; i < PieceCount; i++)
                if (_pieces[i].Broken || !_pieces[i].Present) Restore(i, i * 0.03f);
            _hideAt = Time.unscaledTime + 0.45f;
        }

        // ---- yaşam döngüsü ----

        private void Awake()
        {
            _theme = UiThemeSO.Load();
            BuildUi();
        }

        private void OnDestroy()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (_hideAt >= 0f && Time.unscaledTime >= _hideAt)
            {
                _hideAt = -1f;
                _shown = false;
                _visible.Target = 0f;
            }

            _visible.Step(dt);
            float shown = Mathf.Max(0f, _visible.Value);
            bool active = shown > 0.002f || _visible.Target > 0f || AnyFalling();
            _ringRect.gameObject.SetActive(active);
            if (!active) return;

            FollowTarget();
            _ringRect.localScale = Vector3.one * Mathf.Max(0.0001f, shown);

            if (_warningOn) SetBlink(Mathf.FloorToInt(Time.unscaledTime * warningBlinkHz * 2f) % 2 == 0);

            for (int i = 0; i < PieceCount; i++) StepPiece(_pieces[i], dt);
        }

        private bool AnyFalling()
        {
            for (int i = 0; i < PieceCount; i++)
                if (_pieces[i].Broken && _pieces[i].Root.gameObject.activeSelf) return true;
            return false;
        }

        // ---- hareket ----

        private void FollowTarget()
        {
            if (_target == null) return;
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            Vector3 world = _targetCollider != null ? _targetCollider.bounds.center : _target.position;
            Vector3 screen = _camera.WorldToScreenPoint(world);
            float scale = Mathf.Max(0.0001f, _canvas.scaleFactor);
            Vector2 canvasPos = new Vector2(screen.x, screen.y) / scale;
            // Tam sayı sanat pikseline sabitle (1 sanat pikseli = 4 canvas birimi).
            canvasPos.x = Mathf.Round(canvasPos.x / Unit) * Unit;
            canvasPos.y = Mathf.Round(canvasPos.y / Unit) * Unit;
            _ringRect.anchoredPosition = canvasPos;
        }

        private void StepPiece(Piece p, float dt)
        {
            if (p.Broken)
            {
                if (!p.Root.gameObject.activeSelf) return;
                p.Age += dt;
                p.Velocity.y -= gravity * Unit * dt;
                p.Root.anchoredPosition += p.Velocity * dt;
                p.Root.localEulerAngles = new Vector3(0f, 0f, p.Root.localEulerAngles.z + p.Spin * dt);
                float life = Mathf.Clamp01(p.Age / fallSeconds);
                SetAlpha(p, 1f - life * life);
                if (life >= 1f) p.Root.gameObject.SetActive(false);
                return;
            }

            if (!p.Present) return;

            if (p.EnterWait > 0f)
            {
                p.EnterWait -= dt;
                if (p.EnterWait <= 0f) p.Pop.Target = 1f;
                return;
            }

            p.Pop.Step(dt);
            p.Root.localScale = Vector3.one * Mathf.Max(0.0001f, p.Pop.Value);
        }

        private void Break(int index)
        {
            var p = _pieces[index];
            if (p.Broken || !p.Present) return;
            p.Broken = true;
            p.Age = 0f;
            // Dışarı + yukarı; dönüş yönü parçanın yan tarafına göre (soldakiler sola, sağdakiler sağa döner): tutarlı, rastgele değil.
            p.Velocity = (p.Direction * breakOutSpeed + Vector2.up * breakUpSpeed) * Unit;
            p.Spin = -Mathf.Sign(p.Direction.x == 0f ? 1f : p.Direction.x) * spinDegrees;
            SetBlinkFor(p, false);
            SetAlpha(p, 1f);
        }

        private void Restore(int index, float wait)
        {
            var p = _pieces[index];
            p.Broken = false;
            p.Present = true;
            p.Age = 0f;
            p.Velocity = Vector2.zero;
            p.Root.gameObject.SetActive(true);
            p.Root.anchoredPosition = p.HomeOffset;
            p.Root.localEulerAngles = Vector3.zero;
            SetAlpha(p, 1f);
            SetBlinkFor(p, false);
            p.Pop.Value = 0f;
            p.Pop.Velocity = 0f;
            p.Pop.Target = wait > 0f ? 0f : 1f;
            p.EnterWait = wait;
            p.Root.localScale = Vector3.one * 0.0001f;
        }

        // ---- renk ----

        private void SetBlink(bool red)
        {
            if (red == _blinkPhase) return;
            _blinkPhase = red;
            for (int i = 0; i < PieceCount; i++)
                if (!_pieces[i].Broken) SetBlinkFor(_pieces[i], red);
        }

        private void SetBlinkFor(Piece p, bool red) => p.Fill.color = red ? _theme.Red : _theme.Yellow;

        private void SetAlpha(Piece p, float alpha)
        {
            var f = p.Fill.color; f.a = alpha; p.Fill.color = f;
            var o = p.Outline.color; o.a = alpha; p.Outline.color = o;
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            var canvasGo = new GameObject("HudStaminaRingCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 90; // sayaç (100) ve pause (200) altında
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UiPixelScale>();

            var ringGo = new GameObject("Ring", typeof(RectTransform));
            ringGo.transform.SetParent(canvasGo.transform, false);
            _ringRect = (RectTransform)ringGo.transform;
            _ringRect.anchorMin = _ringRect.anchorMax = Vector2.zero; // sol-alt: WorldToScreenPoint ile aynı eksen
            _ringRect.pivot = new Vector2(0.5f, 0.5f);
            _ringRect.sizeDelta = Vector2.zero;
            _ringRect.gameObject.SetActive(false);

            float outer = outerRadius * Unit;
            float inner = Mathf.Max(1f, (outerRadius - thickness)) * Unit;
            float border = outlineArtPixels * Unit;
            float mid = (outer + inner) * 0.5f;

            for (int i = 0; i < PieceCount; i++)
            {
                var piece = new Piece();
                float midDeg = StaminaRingMath.PieceMidDegrees(i, PieceCount);
                piece.Direction = StaminaRingMath.DirectionOf(midDeg);
                piece.HomeOffset = piece.Direction * mid;

                // Parça kendi ağırlık merkezinde döner: kök merkezde, yaylar kökten geri ofsetli.
                var rootGo = new GameObject("Piece " + i, typeof(RectTransform));
                rootGo.transform.SetParent(_ringRect, false);
                piece.Root = (RectTransform)rootGo.transform;
                piece.Root.anchorMin = piece.Root.anchorMax = piece.Root.pivot = new Vector2(0.5f, 0.5f);
                piece.Root.anchoredPosition = piece.HomeOffset;
                piece.Root.sizeDelta = Vector2.zero;

                float start = StaminaRingMath.PieceStartDegrees(i, PieceCount, gapDegrees);
                float sweep = StaminaRingMath.PieceSweepDegrees(PieceCount, gapDegrees);
                // Çerçeve yayı: yarıçap border kadar büyür; açı payı yay uzunluğunu korur.
                float pad = border / mid * Mathf.Rad2Deg;

                piece.Outline = NewArc("Outline", piece.Root, -piece.HomeOffset, outer + border, inner - border,
                    start - pad, sweep + pad * 2f, _theme.Ink);
                piece.Fill = NewArc("Fill", piece.Root, -piece.HomeOffset, outer, inner, start, sweep, _theme.Yellow);

                piece.Present = true;
                piece.Pop.Value = 1f;
                piece.Pop.Target = 1f;
                _pieces[i] = piece;
            }
        }

        private static UiArc NewArc(string name, Transform parent, Vector2 offset, float outer, float inner,
            float startDegrees, float sweepDegrees, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var arc = go.AddComponent<UiArc>();
            var rt = arc.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(outer * 2f, outer * 2f);
            arc.InnerRadiusRatio = Mathf.Clamp(inner / outer, 0f, 0.99f);
            arc.StartDegrees = startDegrees;
            arc.FillAmount = Mathf.Clamp01(sweepDegrees / 360f);
            arc.raycastTarget = false;
            arc.color = color;
            return arc;
        }
    }
}
