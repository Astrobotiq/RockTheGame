using New_Scripts.Death;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Radyal pause menüsü: dört beton dilim (resume, options, quit, retry), sol stick yönü (<see cref="RadialMenuSelection"/>)
    /// ya da D-pad dilimi seçer, LB/RB döndürür, A onaylar, B devam eder. Seçili dilim sarı boyanır, dışarı çıkar, gölgesi çakılır.
    /// Hepsi yaylarla (<see cref="UiSpring"/>) ve unscaled zamanla çalışır (<c>timeScale 0</c> iken de).
    /// <c>PauseManager.PauseChanged</c> açar/kapatır; arayüzü kodla kurar (Overlay canvas). Pause arka planı ayrı iş.
    /// Süre/miktar değerleri başlangıç önerisi: gamepad ile ayarlanır (docs/systems/pause.md).
    /// </summary>
    [AddComponentMenu("RockTheGame/Pause/Pause Menu Panel")]
    public sealed class PauseMenuPanel : MonoBehaviour
    {
        private enum Action { Resume, Options, Quit, Retry }

        private sealed class Slice
        {
            public Action Action;
            public bool Enabled;
            public RectTransform Root;
            public CanvasGroup Group;
            public RectTransform Shadow;
            public UiArc Fill;
            public TMP_Text Label;
            public Vector2 Direction;
            public UiSpring Pop = new UiSpring(260f, 16f);
            public UiSpring Kick = new UiSpring(260f, 14f);
            // ζ ≈ 0,6: ease-out'a yakın, hafif aşımla oturur (skill önerileri: stiffness 300-500).
            public UiSpring Enter = new UiSpring(340f, 22f);
            public float EnterWait;
            public Vector2 ShadowBase;
        }

        public const string MainMenuSceneName = "MainMenu";

        /// <summary>Sanat pikseli (480x270) → canvas birimi (1920x1080).</summary>
        private const float Unit = 4f;

        [Header("Ölçü (sanat pikseli; mockup'tan)")]
        [SerializeField] private float innerRadius = 54f;
        [SerializeField] private float outerRadius = 106f;
        [Tooltip("Seçili dilimin dış yarıçapı (büyür).")]
        [SerializeField] private float selectedOuterRadius = 118f;
        [Tooltip("Seçili dilimin dışarı kayması.")]
        [SerializeField] private float selectedOffset = 6f;
        [Tooltip("Dilimler arası boşluk (derece, her iki yanda toplam).")]
        [SerializeField] private float gapDegrees = 8f;
        [SerializeField] private float labelRadius = 80f;
        [SerializeField] private float labelSize = 17f;

        [Header("Giriş")]
        [Tooltip("Açılışta onayın kilitli kaldığı süre (sn). Devam etmek (B/Start) hiç kilitlenmez.")]
        [SerializeField, Range(0f, 1f)] private float entranceLock = 0.4f;
        [Tooltip("Dilimler arası giriş gecikmesi (sn). Toplam giriş ~0,5 sn'yi geçmesin.")]
        [SerializeField, Range(0f, 0.1f)] private float enterStagger = 0.05f;
        [Tooltip("Dilimin merkeze doğru başladığı uzaklık (sanat pikseli).")]
        [SerializeField, Range(0f, 40f)] private float enterDistance = 20f;
        [Tooltip("Dilimin giriş başlangıç ölçeği.")]
        [SerializeField, Range(0.4f, 1f)] private float enterScale = 0.7f;
        [Tooltip("Halkanın girişte dönerek oturduğu açı (derece); 0 = dönmez.")]
        [SerializeField, Range(0f, 40f)] private float enterTwist = 14f;
        [Tooltip("Sarı boyanın seçili dilime vurduğu an (sn); gri gelir, sonra boyanır.")]
        [SerializeField, Range(0f, 0.6f)] private float paintDelay = 0.28f;
        [Tooltip("Stick'in dilim değiştirmesi için gereken uzunluk.")]
        [SerializeField, Range(0.2f, 0.9f)] private float stickDeadzone = RadialMenuSelection.DefaultDeadzone;

        private readonly Slice[] _slices =
        {
            new Slice { Action = Action.Resume, Enabled = true },
            // options: ayar ekranı yok; soluk ve seçilemez (ana menüdeki options gibi).
            new Slice { Action = Action.Options, Enabled = false },
            new Slice { Action = Action.Quit, Enabled = true },
            new Slice { Action = Action.Retry, Enabled = true }
        };

        // Yalnızca kapanış: girişten kısa ve aşımsız (~0,15 sn).
        private readonly UiSpring _open = new UiSpring(500f, 40f);
        private readonly UiSpring _twist = new UiSpring(210f, 18f);
        private bool _paintPending;
        private PauseBackdrop _backdrop;
        private bool[] _enabled;
        private UiThemeSO _theme;
        private GameObject _canvasRoot;
        private RectTransform _ring;
        private CanvasGroup _group;
        private InputAction _stick, _dpad, _submit, _back, _previous, _next;
        private int _selected;
        private bool _visible, _closing;
        private float _age;

        public bool IsOpen => _visible && !_closing;

        private void Awake()
        {
            _theme = UiThemeSO.Load();
            _enabled = new bool[_slices.Length];
            for (int i = 0; i < _slices.Length; i++) _enabled[i] = _slices[i].Enabled;
            BuildInputs();
            BuildUi();
            _canvasRoot.SetActive(false);
        }

        private void OnEnable() => PauseManager.PauseChanged += OnPauseChanged;

        private void OnDisable()
        {
            PauseManager.PauseChanged -= OnPauseChanged;
            DisableInputs();
            if (_canvasRoot != null) _canvasRoot.SetActive(false);
            _visible = false;
        }

        private void OnDestroy()
        {
            foreach (var a in new[] { _stick, _dpad, _submit, _back, _previous, _next }) a?.Dispose();
            _backdrop?.Destroy();
        }

        private void OnPauseChanged(bool paused)
        {
            if (paused) Show();
            else if (_visible) Hide();
        }

        private void Show()
        {
            _canvasRoot.SetActive(true);
            _visible = true;
            _closing = false;
            _age = 0f;
            _selected = 0;

            _open.Value = _open.Target = 1f;
            _open.Velocity = 0f;

            // Halka hafifçe dönerek oturur; dilimler sırayla, her biri kendi konum + ölçek + görünürlüğüyle gelir
            // (yalnızca solma yok), merkezde yığılıp beklemez. Seçili dilim önce gri gelir, sonra sarıya boyanır.
            _twist.Value = enterTwist;
            _twist.Target = 0f;
            _twist.Velocity = 0f;
            _paintPending = true;
            _backdrop.Show(0);

            for (int i = 0; i < _slices.Length; i++)
            {
                var s = _slices[i];
                s.Pop.Value = s.Pop.Target = 0f;
                s.Pop.Velocity = 0f;
                s.Kick.Value = s.Kick.Velocity = 0f;
                s.Enter.Value = 1f;
                s.Enter.Velocity = 0f;
                s.EnterWait = i * enterStagger;
                // İlk dilim beklemeden başlar (merkezde yığılı bekleyen dilim olmaz).
                s.Enter.Target = s.EnterWait <= 0f ? 0f : 1f;
            }

            EnableInputs();
            Apply(0f);
        }

        private void Hide()
        {
            _closing = true;
            _open.Target = 0f;
            DisableInputs();
        }

        private void Update()
        {
            if (!_visible) return;

            float dt = Time.unscaledDeltaTime;
            _age += dt;

            if (!_closing) HandleInput();
            Apply(dt);

            if (_closing && _open.Value < 0.03f)
            {
                _canvasRoot.SetActive(false);
                _visible = false;
            }
        }

        // ---- girdi ----

        private void HandleInput()
        {
            if (_back.WasPressedThisFrame())
            {
                Activate(Action.Resume);
                return;
            }

            int next = _selected;
            next = RadialMenuSelection.Pick(_stick.ReadValue<Vector2>(), next, _enabled, stickDeadzone);
            if (_dpad.WasPressedThisFrame()) next = RadialMenuSelection.Pick(_dpad.ReadValue<Vector2>(), next, _enabled, 0.3f);
            if (_next.WasPressedThisFrame()) next = RadialMenuSelection.Rotate(next, +1, _enabled);
            if (_previous.WasPressedThisFrame()) next = RadialMenuSelection.Rotate(next, -1, _enabled);
            if (next != _selected) Select(next);

            if (_submit.WasPressedThisFrame() && _age >= entranceLock) Activate(_slices[_selected].Action);
        }

        private void Select(int index)
        {
            _selected = index;
            _paintPending = false;
            _backdrop.SetPose(index);
            for (int i = 0; i < _slices.Length; i++) _slices[i].Pop.Target = i == index ? 1f : 0f;
            _slices[index].Kick.Value = 1.2f; // gölge çakılır
            // Seçili dilimin üstüne dönerken hafif bir tepme.
            _slices[index].Pop.Kick(2.5f);
        }

        private void Activate(Action action)
        {
            if (!_slices[(int)action].Enabled) return;

            var pause = PauseManager.Instance;
            if (pause != null) pause.Resume();

            switch (action)
            {
                case Action.Quit:
                    SceneManager.LoadScene(MainMenuSceneName);
                    break;
                case Action.Retry:
                    // Mevcut ölüm/respawn akışı: son checkpoint'te doğar, ölüm sayılır.
                    var health = FindFirstObjectByType<PlayerHealth>();
                    if (health != null) health.Kill();
                    break;
            }
        }

        private void BuildInputs()
        {
            _stick = new InputAction("PauseStick", InputActionType.Value, "<Gamepad>/leftStick");
            _dpad = new InputAction("PauseDpad", InputActionType.Value, "<Gamepad>/dpad");
            _submit = new InputAction("PauseSubmit", InputActionType.Button, "<Gamepad>/buttonSouth");
            _back = new InputAction("PauseBack", InputActionType.Button, "<Gamepad>/buttonEast");
            _previous = new InputAction("PausePrevious", InputActionType.Button, "<Gamepad>/leftShoulder");
            _next = new InputAction("PauseNext", InputActionType.Button, "<Gamepad>/rightShoulder");
        }

        private void EnableInputs()
        {
            _stick.Enable(); _dpad.Enable(); _submit.Enable(); _back.Enable(); _previous.Enable(); _next.Enable();
        }

        private void DisableInputs()
        {
            if (_stick == null) return;
            _stick.Disable(); _dpad.Disable(); _submit.Disable(); _back.Disable(); _previous.Disable(); _next.Disable();
        }

        // ---- hareket ----

        private void Apply(float dt)
        {
            _open.Step(dt);
            _twist.Step(dt);
            _backdrop.Step(dt);
            float open = Mathf.Clamp01(_open.Value);
            // Giriş solmayla değil dilimlerin kendi hareketiyle olur; yalnızca kapanış topluca solar.
            _group.alpha = _closing ? open : 1f;
            _ring.localScale = Vector3.one * (_closing ? Mathf.Lerp(0.92f, 1f, open) : 1f);
            _ring.localRotation = Quaternion.Euler(0f, 0f, _twist.Value);

            if (_paintPending && _age >= paintDelay)
            {
                _paintPending = false;
                var painted = _slices[_selected];
                painted.Pop.Target = 1f;
                painted.Kick.Value = 1.2f; // boya vurunca gölge çakılır
                painted.Pop.Kick(2.5f);
            }

            float growth = selectedOuterRadius / outerRadius - 1f;
            for (int i = 0; i < _slices.Length; i++)
            {
                var s = _slices[i];
                if (s.EnterWait > 0f)
                {
                    s.EnterWait -= dt;
                    if (s.EnterWait <= 0f) s.Enter.Target = 0f;
                }

                s.Pop.Step(dt);
                s.Kick.Step(dt);
                s.Enter.Step(dt);

                float pop = s.Pop.Value;
                float enter = s.Enter.Value; // 1 = girişin başı, 0 = oturdu (hafif aşım negatife gider)
                float enterClamped = Mathf.Clamp01(enter);
                // Dilim kendi yönünde içeriden gelir: konum + ölçek + görünürlük birlikte.
                s.Root.anchoredPosition = s.Direction * (selectedOffset * Unit * pop - enterDistance * Unit * enter);
                s.Root.localScale = Vector3.one * ((1f + growth * pop) * (1f - (1f - enterScale) * enter));
                s.Group.alpha = 1f - enterClamped;
                s.Shadow.anchoredPosition = s.ShadowBase * (1f + 0.8f * Mathf.Clamp01(pop) + s.Kick.Value);

                Color fill = !s.Enabled ? Color.Lerp(_theme.Muted, _theme.Ink, 0.35f)
                    : Color.Lerp(_theme.Muted, _theme.Yellow, Mathf.Clamp01(pop));
                if (s.Fill.color != fill) s.Fill.color = fill;

                Color label = !s.Enabled ? new Color(_theme.ConcreteSlab.r, _theme.ConcreteSlab.g, _theme.ConcreteSlab.b, 0.35f)
                    : Color.Lerp(_theme.ConcreteSlab, _theme.Ink, Mathf.Clamp01(pop));
                if (s.Label.color != label) s.Label.color = label;
                s.Label.rectTransform.localScale = Vector3.one * (1f + 0.08f * Mathf.Clamp01(pop));
            }
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            _canvasRoot = new GameObject("PauseMenuCanvas", typeof(RectTransform));
            _canvasRoot.transform.SetParent(transform, false);
            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            _group = _canvasRoot.AddComponent<CanvasGroup>();
            _group.interactable = false;
            _group.blocksRaycasts = false;

            _backdrop = new PauseBackdrop();
            _backdrop.Build(_canvasRoot.transform, _theme);

            _ring = NewRect("Ring", _canvasRoot.transform);
            _ring.anchorMin = _ring.anchorMax = _ring.pivot = new Vector2(0.5f, 0.5f);
            _ring.anchoredPosition = Vector2.zero;

            float border = _theme.BorderWidthFor(UiScreen.Pause) * Unit;
            float shadow = _theme.ShadowOffsetFor(UiScreen.Pause) * Unit;
            float inner = innerRadius * Unit, outer = outerRadius * Unit;
            float sweep = 90f - gapDegrees;

            for (int i = 0; i < _slices.Length; i++)
            {
                var s = _slices[i];
                s.Direction = RadialMenuSelection.SliceDirection(i, _slices.Length);
                s.Root = NewRect("Slice " + s.Action, _ring);
                Center(s.Root, outer * 2f);
                s.Group = s.Root.gameObject.AddComponent<CanvasGroup>();

                float start = i * 90f - 45f + gapDegrees * 0.5f;
                // Sert gölge ve dış çizgi: aynı şekil, ink; çizgi her yönde `border` kadar geniş.
                float midRadius = (inner + outer) * 0.5f;
                float borderDeg = border / midRadius * Mathf.Rad2Deg;
                s.Shadow = NewArc("Shadow", s.Root, outer + border, inner - border, start - borderDeg, sweep + 2f * borderDeg, _theme.Ink).rectTransform;
                s.ShadowBase = new Vector2(shadow, -shadow);
                NewArc("Outline", s.Root, outer + border, inner - border, start - borderDeg, sweep + 2f * borderDeg, _theme.Ink);
                s.Fill = NewArc("Fill", s.Root, outer, inner, start, sweep, _theme.Muted);

                s.Label = NewLabel(s, i);
            }
        }

        private TMP_Text NewLabel(Slice s, int index)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(s.Root, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (_theme.TitleFont != null) text.font = _theme.TitleFont;
            text.text = s.Action.ToString().ToLowerInvariant();
            text.fontSize = labelSize * Unit;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.color = _theme.ConcreteSlab;

            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 120f);
            rt.anchoredPosition = s.Direction * (labelRadius * Unit);
            // Yanlardaki yazılar dilimin boyunca okunur (mockup): sağ saat yönünde, sol saatin tersine döner.
            rt.localRotation = Quaternion.Euler(0f, 0f, index == 1 ? -90f : index == 3 ? 90f : 0f);
            return text;
        }

        private static UiArc NewArc(string name, Transform parent, float outer, float inner, float startDegrees, float sweepDegrees, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var arc = go.AddComponent<UiArc>();
            Center(arc.rectTransform, outer * 2f);
            arc.InnerRadiusRatio = Mathf.Clamp(inner / outer, 0f, 0.99f);
            arc.StartDegrees = startDegrees;
            arc.FillAmount = sweepDegrees / 360f;
            arc.raycastTarget = false;
            arc.color = color;
            return arc;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Center(RectTransform rt, float size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(size, size);
        }
    }
}
