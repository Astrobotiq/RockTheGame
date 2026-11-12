using System.Collections.Generic;
using New_Scripts.Multiplayer.Hud;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Lobby
{
    /// <summary>
    /// Çok oyunculu lobi ekranı, düzen B (docs/ui-research/lobby-v1.html "iki yarı"): ekran çapraz kirişle ikiye bölünür,
    /// solda host (mavi silindir, P1), sağda join (kırmızı küp, P2). Sol çubuk / d-pad yarıyı seçer, A onaylar, B geri/vazgeç,
    /// LB/RB mod, Y bağlantı (Steam / IP), X devam et. Mantık <see cref="MultiplayerLobbyUI"/>'da kalır; bu sınıf yalnızca çizer
    /// ve girdiyi ona iletir (kurallar <see cref="LobbyScreenMath"/>).
    /// <list type="bullet">
    /// <item>Overlay canvas (CRT dışı), <see cref="UiPixelScale"/> ile tam sayı sanat pikseli. Kod ile kurulur.</item>
    /// <item>Hareket (<see cref="UiSpring"/>, unscaled zaman): girişte parçalar sırayla kayarak gelir (gezinme ≤ 0,5 sn kilitli);
    /// seçim değişince kiriş seçilmeyen yana kayar, seçilmeyen yarı betona doğru soluklaşır, seçili yarının şekli yükselir;
    /// boşta şekiller hafifçe yüzer; mod değişince sekme sıçrar.</item>
    /// <item>Rakip bekleniyor ve geri sayım durumları bu görevde yalın (sağ yarı beton, "?" küp); kesik çizgi, bant ve 3-2-1
    /// ayrı görevde (TASKS.md "bekleme ve geri sayım").</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/ui-motion.md).
    /// </summary>
    public sealed class LobbyScreen : MonoBehaviour
    {
        public const string ObjectName = "[MP Lobby Screen]";

        private const UiScreen Screen = UiScreen.MultiplayerLobby;
        private const float Unit = UiPixelScale.UnitsPerArtPixel;
        private const float FrameW = UiPixelScale.ArtWidth;
        private const float FrameH = UiPixelScale.ArtHeight;
        private const float LockSeconds = 0.5f;
        private const float Pad = 200f; // çerçeve dışına taşan boya: tam sayı ölçekten artan kenarları örter

        private sealed class Group
        {
            public RectTransform Rect;
            public Vector2 Dir;
            public float Extent;
            public float Delay;
            public Vector2 Extra;
            public bool Started;
            public readonly UiSpring Slide = new UiSpring(240f, 22f);
        }

        [Header("Hareket (başlangıç önerisi)")]
        [Tooltip("Kirişin seçilmeyen yana kayma miktarı (sanat pikseli). Kiriş yarı genişliğinden (7) küçük kalmalı.")]
        [SerializeField, Range(0f, 6f)] private float beamShift = 5f;
        [Tooltip("Seçili yarının şeklinin yükselmesi (sanat pikseli).")]
        [SerializeField, Range(0f, 12f)] private float lift = 6f;
        [Tooltip("Seçilmeyen yarının betona doğru soluklaşması (0 = hiç, 1 = tam beton).")]
        [SerializeField, Range(0f, 1f)] private float fade = 0.55f;
        [Tooltip("Boşta şekillerin yüzme genliği (sanat pikseli).")]
        [SerializeField, Range(0f, 4f)] private float bob = 1.5f;
        [SerializeField, Range(0.2f, 3f)] private float bobSpeed = 0.8f;

        private readonly List<Group> _groups = new List<Group>();
        private readonly UiSpring _beam = new UiSpring(260f, 20f);
        private readonly UiSpring _hostEmph = new UiSpring(260f, 22f);
        private readonly UiSpring _joinEmph = new UiSpring(260f, 22f);
        private readonly UiSpring _tabPop = new UiSpring(320f, 16f);

        private MultiplayerLobbyUI _ui;
        private UiThemeSO _theme;
        private float _border;
        private float _shadow;
        private float _bornAt;
        private RectTransform _frame;
        private Group _left, _right, _beamGroup, _tab, _strip;
        private UiPolygon _leftFill, _rightFill;
        private RectTransform _p1, _p2;
        private readonly List<UiPolygon> _p1Faces = new List<UiPolygon>();
        private readonly List<UiPolygon> _p2Faces = new List<UiPolygon>();
        private TMP_Text _hostTitle, _hostCaption, _joinTitle, _joinCaption, _joinAddress, _p1Label, _p2Label;
        private TMP_Text _tabText, _connectionHint, _resumeHint, _confirmHint, _backHint, _errorText;
        private RectTransform _errorPlate;

        private LobbySide _side = LobbySide.Host;
        private InputAction _stick, _dpad, _submit, _back, _previous, _next, _connection, _resume;

        public LobbySide Side => _side;

        public static LobbyScreen Create(MultiplayerLobbyUI ui)
        {
            var go = new GameObject(ObjectName) { layer = LayerMask.NameToLayer("UI") };
            var screen = go.AddComponent<LobbyScreen>();
            screen._ui = ui;
            screen.Begin();
            return screen;
        }

        // ---- yaşam döngüsü ----

        private void Begin()
        {
            _theme = UiThemeSO.Load();
            _bornAt = Time.unscaledTime;
            BuildUi();
            BuildInputs();
            Update();
        }

        private void OnEnable() => EnableInputs();

        private void OnDisable() => DisableInputs();

        private void OnDestroy()
        {
            _stick?.Dispose(); _dpad?.Dispose(); _submit?.Dispose(); _back?.Dispose();
            _previous?.Dispose(); _next?.Dispose(); _connection?.Dispose(); _resume?.Dispose();
        }

        private void BuildInputs()
        {
            _stick = new InputAction("LobbyStick", InputActionType.Value, "<Gamepad>/leftStick");
            _dpad = new InputAction("LobbyDpad", InputActionType.Value, "<Gamepad>/dpad");
            _submit = new InputAction("LobbySubmit", InputActionType.Button, "<Gamepad>/buttonSouth");
            _back = new InputAction("LobbyBack", InputActionType.Button, "<Gamepad>/buttonEast");
            _previous = new InputAction("LobbyPrevious", InputActionType.Button, "<Gamepad>/leftShoulder");
            _next = new InputAction("LobbyNext", InputActionType.Button, "<Gamepad>/rightShoulder");
            _connection = new InputAction("LobbyConnection", InputActionType.Button, "<Gamepad>/buttonNorth");
            _resume = new InputAction("LobbyResume", InputActionType.Button, "<Gamepad>/buttonWest");
            EnableInputs();
        }

        private void EnableInputs()
        {
            if (_stick == null) return;
            _stick.Enable(); _dpad.Enable(); _submit.Enable(); _back.Enable();
            _previous.Enable(); _next.Enable(); _connection.Enable(); _resume.Enable();
        }

        private void DisableInputs()
        {
            if (_stick == null) return;
            _stick.Disable(); _dpad.Disable(); _submit.Disable(); _back.Disable();
            _previous.Disable(); _next.Disable(); _connection.Disable(); _resume.Disable();
        }

        // ---- girdi ----

        private void ReadInput(bool active)
        {
            float x = _stick.ReadValue<Vector2>().x;
            float dx = _dpad.ReadValue<Vector2>().x;
            if (Mathf.Abs(dx) > Mathf.Abs(x)) x = dx;
            if (!active) _side = LobbyScreenMath.Move(_side, x);

            if (_submit.WasPressedThisFrame() && LobbyScreenMath.CanConfirm(active, _side, _ui.UseInvite))
            {
                _beam.Kick(_side == LobbySide.Host ? -90f : 90f);
                if (_side == LobbySide.Host) _ui.StartHost();
                else _ui.StartJoin();
            }

            if (_back.WasPressedThisFrame())
            {
                if (active) _ui.Cancel();
                else _ui.BackToMainMenu();
            }

            if (_ui.CanChangeMode && (_previous.WasPressedThisFrame() || _next.WasPressedThisFrame()))
            {
                _ui.ToggleMode();
                _tabPop.Value = 1.3f;
                _tabPop.Velocity = 0f;
            }

            if (!active && _ui.InviteAvailable && _connection.WasPressedThisFrame()) _ui.ToggleConnection();
            if (!active && _ui.ResumeAvailable && _resume.WasPressedThisFrame()) _ui.ToggleResume();
        }

        // ---- hareket ----

        private void Update()
        {
            if (_ui == null) return;
            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;
            float t = now - _bornAt;

            foreach (var g in _groups)
            {
                if (!g.Started && t >= g.Delay)
                {
                    g.Started = true;
                    g.Slide.Target = 1f;
                }

                g.Slide.Step(dt);
            }

            bool active = _ui.Active;
            bool host = _ui.IsHost;
            bool opponent = _ui.OpponentPresent;
            if (t >= LockSeconds) ReadInput(active);
            if (active) _side = host ? LobbySide.Host : LobbySide.Join;

            _hostEmph.Target = _side == LobbySide.Host ? 1f : 0f;
            _joinEmph.Target = _side == LobbySide.Join ? 1f : 0f;
            _beam.Target = _side == LobbySide.Host ? beamShift : -beamShift;
            _tabPop.Target = 1f;
            _hostEmph.Step(dt);
            _joinEmph.Step(dt);
            _beam.Step(dt);
            _tabPop.Step(dt);

            _beamGroup.Extra = new Vector2(_beam.Value * Unit, 0f);
            foreach (var g in _groups) ApplySlide(g);

            Refresh(active, host, opponent, now);
        }

        private void Refresh(bool active, bool host, bool opponent, float now)
        {
            bool useInvite = _ui.UseInvite;
            bool waiting = active && host && !opponent;

            // yarılar: seçilmeyen betona doğru soluk; bekleyen sağ yarı düz beton
            float hostE = Mathf.Clamp01(_hostEmph.Value);
            float joinE = Mathf.Clamp01(_joinEmph.Value);
            SetColor(_leftFill, Color.Lerp(_theme.Blue, _theme.ConcreteWall, fade * (1f - hostE)));
            SetColor(_rightFill, waiting ? _theme.ConcreteWall : Color.Lerp(_theme.Red, _theme.ConcreteWall, fade * (1f - joinE)));

            SetColor(_hostTitle, Color.Lerp(_theme.Paper, _theme.Yellow, hostE));
            SetColor(_joinTitle, Color.Lerp(_theme.Paper, _theme.Yellow, joinE));
            SetColor(_joinCaption, waiting ? _theme.Ink : _theme.Paper);
            SetText(_hostCaption, LobbyScreenMath.HostCaption(active, host, useInvite));
            SetText(_joinCaption, LobbyScreenMath.JoinCaption(active, host, opponent, useInvite));
            if (_joinTitle.gameObject.activeSelf == waiting) _joinTitle.gameObject.SetActive(!waiting);

            bool showAddress = !active && !useInvite;
            if (_joinAddress.gameObject.activeSelf != showAddress) _joinAddress.gameObject.SetActive(showAddress);
            if (showAddress) SetText(_joinAddress, _ui.AddressText);

            // P2: rakip varsa kırmızı küp, yoksa soluk beton "?"
            Color cube = waiting ? _theme.ConcreteSlab : _theme.Red;
            foreach (var face in _p2Faces) SetColor(face, cube);
            SetText(_p2Label, waiting ? "?" : "P2");
            SetColor(_p2Label, waiting ? _theme.Ink : _theme.Paper);

            SetText(_tabText, "< " + LobbyScreenMath.ModeName(_ui.ShownMode) + " >");
            SetColor(_tabText, _ui.CanChangeMode ? _theme.Ink : _theme.Muted);
            _tabText.rectTransform.localScale = Vector3.one * Mathf.Max(0.5f, _tabPop.Value);

            bool showConnection = !active && _ui.InviteAvailable;
            _connectionHint.gameObject.SetActive(showConnection);
            if (showConnection) SetText(_connectionHint, LobbyScreenMath.ConnectionHint(useInvite, _ui.InviteName));
            bool showResume = !active && _ui.ResumeAvailable;
            _resumeHint.gameObject.SetActive(showResume);
            if (showResume) SetText(_resumeHint, LobbyScreenMath.ResumeHint(_ui.ResumeOn, _ui.ResumeRoomId, _ui.ResumeFlagCount));
            _confirmHint.gameObject.SetActive(LobbyScreenMath.CanConfirm(active, _side, useInvite));
            SetText(_backHint, active ? "B  CANCEL" : "B  BACK");

            string error = _ui.ErrorLine;
            SetText(_errorText, error);
            _errorPlate.gameObject.SetActive(error.Length > 0);

            // şekiller: seçili yarıda yükselir, boşta hafifçe yüzer
            float sway = Mathf.Sin(now * bobSpeed * Mathf.PI * 2f) * bob;
            _p1.anchoredPosition = new Vector2(0f, (lift * hostE + sway) * Unit);
            _p2.anchoredPosition = new Vector2(0f, (lift * joinE + (waiting ? 0f : -sway)) * Unit);
        }

        private static void SetColor(Graphic g, Color c)
        {
            if (g.color != c) g.color = c;
        }

        private static void SetText(TMP_Text t, string s)
        {
            if (t.text != s) t.text = s;
        }

        private static void ApplySlide(Group g)
        {
            g.Rect.anchoredPosition = g.Dir * (g.Extent * Unit * (1f - g.Slide.Value)) + g.Extra;
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<UiPixelScale>();

            _border = _theme.BorderWidthFor(Screen) * Unit;
            _shadow = _theme.ShadowOffsetFor(Screen) * Unit;

            _frame = NewRect("Frame", transform);
            _frame.anchorMin = _frame.anchorMax = new Vector2(0.5f, 0.5f);
            _frame.pivot = new Vector2(0f, 1f);
            _frame.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            _frame.anchoredPosition = new Vector2(-FrameW * 0.5f, FrameH * 0.5f) * Unit;

            // Çapraz sınır: (296,0) → (196,270); çerçeve dışına uzatılır.
            float top = -Pad, bottom = FrameH + Pad;
            float xTop = Boundary(top), xBottom = Boundary(bottom);

            _left = NewGroup("Left", new Vector2(-1f, 0f), FrameW * 0.7f, 0.06f);
            _leftFill = AddPaintedFill(_left.Rect, "Left", new[]
                { new Vector2(-Pad, top), new Vector2(xTop, top), new Vector2(xBottom, bottom), new Vector2(-Pad, bottom) }, _theme.Blue);

            _right = NewGroup("Right", new Vector2(1f, 0f), FrameW * 0.7f, 0.06f);
            _rightFill = AddPaintedFill(_right.Rect, "Right", new[]
                { new Vector2(xTop, top), new Vector2(FrameW + Pad, top), new Vector2(FrameW + Pad, bottom), new Vector2(xBottom, bottom) }, _theme.Red);

            // kiriş: sınırın üstünde, ink
            _beamGroup = NewGroup("Beam", new Vector2(0f, 1f), FrameH + Pad * 2f, 0f);
            AddPolygon(_beamGroup.Rect, "Beam", new[]
            {
                new Vector2(Boundary(top) - 7f, top), new Vector2(Boundary(top) + 7f, top),
                new Vector2(Boundary(bottom) + 7f, bottom), new Vector2(Boundary(bottom) - 7f, bottom)
            }, _theme.Ink, 0f);

            // host yarısı
            var monoBold = _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont;
            _hostTitle = AddExtrudedTitle(_left.Rect, "HostTitle", "host", 40f, new Vector2(30f, 52f), TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f));
            _hostCaption = AddText("HostCaption", _left.Rect, monoBold, 9f, _theme.Paper, TextAlignmentOptions.MidlineLeft,
                new Vector2(32f, 82f), new Vector2(200f, 12f), new Vector2(0f, 0.5f));
            _p1 = NewShape("P1", _left.Rect);
            BuildCylinder(_p1, 110f, 130f, 206f, 34f, 11f);
            _p1Label = AddText("P1Label", _p1, _theme.TitleFont, 28f, _theme.Paper, TextAlignmentOptions.Center,
                new Vector2(110f, 166f), new Vector2(80f, 34f), new Vector2(0.5f, 0.5f));
            _p1Label.text = "P1";

            // join yarısı
            _joinTitle = AddExtrudedTitle(_right.Rect, "JoinTitle", "join", 40f, new Vector2(446f, 52f), TextAlignmentOptions.MidlineRight, new Vector2(1f, 0.5f));
            _joinCaption = AddText("JoinCaption", _right.Rect, monoBold, 9f, _theme.Paper, TextAlignmentOptions.MidlineRight,
                new Vector2(446f, 82f), new Vector2(220f, 12f), new Vector2(1f, 0.5f));
            _joinAddress = AddText("JoinAddress", _right.Rect, monoBold, 8f, _theme.Paper, TextAlignmentOptions.MidlineRight,
                new Vector2(446f, 96f), new Vector2(220f, 11f), new Vector2(1f, 0.5f));
            _p2 = NewShape("P2", _right.Rect);
            BuildCube(_p2, 352f, 138f, 64f, 64f);
            _p2Label = AddText("P2Label", _p2, _theme.TitleFont, 28f, _theme.Paper, TextAlignmentOptions.Center,
                new Vector2(384f, 170f), new Vector2(80f, 34f), new Vector2(0.5f, 0.5f));
            _p2Label.text = "P2";

            // mod sekmesi (üst orta, sarı)
            _tab = NewGroup("Tab", new Vector2(0f, 1f), 60f, 0.14f);
            AddSolid(_tab.Rect, "Tab", new[] { new Vector2(176f, -20f), new Vector2(304f, -20f), new Vector2(292f, 26f), new Vector2(188f, 26f) }, _theme.Yellow, true);
            _tabText = AddText("TabText", _tab.Rect, _theme.TitleFont, 15f, _theme.Ink, TextAlignmentOptions.Center,
                new Vector2(240f, 12f), new Vector2(120f, 22f), new Vector2(0.5f, 0.5f));

            // alt şerit: ipuçları
            _strip = NewGroup("Strip", new Vector2(0f, -1f), 60f, 0.18f);
            var stripPts = new[] { new Vector2(-Pad, 236f), new Vector2(FrameW + Pad, 236f), new Vector2(FrameW + Pad, bottom), new Vector2(-Pad, bottom) };
            AddPolygon(_strip.Rect, "Strip Outline", stripPts, _theme.Ink, _border);
            var stripFill = AddPolygon(_strip.Rect, "Strip", stripPts, _theme.ConcreteSlab, 0f);
            var surface = stripFill.gameObject.AddComponent<UiConcreteSurface>();
            surface.Configure(Screen, ConcreteTone.Slab, asPaint: false);
            surface.Apply();

            _connectionHint = Hint("ConnectionHint", 18f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f), 150f);
            _resumeHint = Hint("ResumeHint", 130f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f), 170f);
            _confirmHint = Hint("ConfirmHint", 330f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f), 70f);
            _confirmHint.text = "A  CONFIRM";
            _backHint = Hint("BackHint", 410f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f), 70f);

            // hata / kopma nedeni: ink plaka üstünde sarı yazı, şeridin hemen üstünde
            _errorPlate = NewRect("ErrorPlate", _frame);
            _errorPlate.anchorMin = _errorPlate.anchorMax = _errorPlate.pivot = new Vector2(0f, 1f);
            _errorPlate.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            _errorPlate.anchoredPosition = Vector2.zero;
            AddPolygon(_errorPlate, "ErrorPlate", new[] { new Vector2(60f, 212f), new Vector2(420f, 212f), new Vector2(420f, 230f), new Vector2(60f, 230f) }, _theme.Ink, 0f);
            _errorText = AddText("ErrorText", _errorPlate, monoBold, 8f, _theme.Yellow, TextAlignmentOptions.Center,
                new Vector2(240f, 221f), new Vector2(350f, 12f), new Vector2(0.5f, 0.5f));
            _errorPlate.gameObject.SetActive(false);

            foreach (var g in _groups) ApplySlide(g);
        }

        private static float Boundary(float y) => 296f - 100f * y / FrameH;

        private TMP_Text Hint(string name, float x, TextAlignmentOptions alignment, Vector2 pivot, float width)
        {
            var monoBold = _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont;
            return AddText(name, _strip.Rect, monoBold, 8f, _theme.Ink, alignment, new Vector2(x, 253f), new Vector2(width, 11f), pivot);
        }

        private void BuildCylinder(RectTransform parent, float cx, float topY, float bottomY, float rx, float ry)
        {
            var body = RaceResultScreen.CylinderBody(cx, rx, ry, topY, bottomY);
            var cap = RaceResultScreen.Ellipse(cx, topY, rx, ry);
            var capColor = Color.Lerp(_theme.Blue, _theme.Paper, 0.25f);
            AddShapeFace(parent, "Body", body, _theme.Blue, _p1Faces, true);
            AddShapeFace(parent, "Cap", cap, capColor, _p1Faces, false);
        }

        private void BuildCube(RectTransform parent, float x, float y, float w, float h)
        {
            // Üst yüz sivri bir paralelkenar (~27°): ona çerçeve çekmek (OffsetConvex gönyesi) köşelerde çıkıntı yapar.
            // Bu yüzden tek dış çerçeve (dışbükey altıgen siluet) + yüz dolguları + iç kenar çizgileri çizilir.
            const float d = 20f, dy = 10f;
            var silhouette = new[]
            {
                new Vector2(x, y), new Vector2(x + d, y - dy), new Vector2(x + w + d, y - dy),
                new Vector2(x + w + d, y + h - dy), new Vector2(x + w, y + h), new Vector2(x, y + h)
            };
            var shadow = AddPolygon(parent, "Cube Shadow", silhouette, _theme.Ink, _border);
            shadow.rectTransform.anchoredPosition = new Vector2(_shadow, -_shadow);
            AddPolygon(parent, "Cube Outline", silhouette, _theme.Ink, _border);

            AddFace(parent, "Side", new[] { new Vector2(x + w, y), new Vector2(x + w + d, y - dy), new Vector2(x + w + d, y + h - dy), new Vector2(x + w, y + h) }, _theme.Red, _p2Faces);
            AddFace(parent, "Top", new[] { new Vector2(x, y), new Vector2(x + d, y - dy), new Vector2(x + w + d, y - dy), new Vector2(x + w, y) }, _theme.Red, _p2Faces);
            AddFace(parent, "Front", new[] { new Vector2(x, y), new Vector2(x + w, y), new Vector2(x + w, y + h), new Vector2(x, y + h) }, _theme.Red, _p2Faces);

            AddEdge(parent, "Edge Top-Front", new Vector2(x, y), new Vector2(x + w, y));
            AddEdge(parent, "Edge Front-Side", new Vector2(x + w, y), new Vector2(x + w, y + h));
            AddEdge(parent, "Edge Top-Side", new Vector2(x + w, y), new Vector2(x + w + d, y - dy));
        }

        /// <summary>Yalnız boyalı dolgu (çerçeve ve gölge çağıran tarafta); renk güncellemesi için listeye girer.</summary>
        private void AddFace(RectTransform parent, string name, Vector2[] pts, Color color, List<UiPolygon> fills)
        {
            var fill = AddPolygon(parent, name + " Fill", pts, color, 0f);
            var paint = fill.gameObject.AddComponent<UiConcreteSurface>();
            paint.Configure(Screen, ConcreteTone.Wall, asPaint: true);
            paint.Apply();
            fill.color = color;
            fills.Add(fill);
        }

        /// <summary>İki yüzün arasındaki ink çizgisi: dış çerçeveyle aynı kalınlıkta ince dörtgen (sivri köşe çıkıntısı yok).</summary>
        private void AddEdge(RectTransform parent, string name, Vector2 a, Vector2 b)
        {
            Vector2 dir = (b - a).normalized;
            Vector2 half = new Vector2(-dir.y, dir.x) * (_border / Unit * 0.5f);
            AddPolygon(parent, name, new[] { a + half, b + half, b - half, a - half }, _theme.Ink, 0f);
        }

        /// <summary>Çerçeve + dolgu (boyalı beton); <paramref name="shadow"/> ise önce sert gölge. Dolgu renk güncellemesi için listeye girer.</summary>
        private void AddShapeFace(RectTransform parent, string name, Vector2[] pts, Color color, List<UiPolygon> fills, bool shadow)
        {
            if (shadow)
            {
                var s = AddPolygon(parent, name + " Shadow", pts, _theme.Ink, _border);
                s.rectTransform.anchoredPosition = new Vector2(_shadow, -_shadow);
            }

            AddPolygon(parent, name + " Outline", pts, _theme.Ink, _border);
            var fill = AddPolygon(parent, name + " Fill", pts, color, 0f);
            var paint = fill.gameObject.AddComponent<UiConcreteSurface>();
            paint.Configure(Screen, ConcreteTone.Wall, asPaint: true);
            paint.Apply();
            fill.color = color;
            fills.Add(fill);
        }

        private UiPolygon AddPaintedFill(RectTransform parent, string name, Vector2[] pts, Color color)
        {
            var fill = AddPolygon(parent, name, pts, color, 0f);
            var paint = fill.gameObject.AddComponent<UiConcreteSurface>();
            paint.Configure(Screen, ConcreteTone.Wall, asPaint: true);
            paint.Apply();
            fill.color = color;
            return fill;
        }

        private TMP_Text AddExtrudedTitle(RectTransform parent, string name, string text, float artSize, Vector2 pos, TextAlignmentOptions alignment, Vector2 pivot)
        {
            var t = AddText(name, parent, _theme.TitleFont, artSize, _theme.Paper, alignment, pos, new Vector2(220f, 50f), pivot);
            t.text = text;
            var ext = t.gameObject.AddComponent<UiExtrudedText>();
            ext.Screen = Screen;
            return t;
        }

        private RectTransform NewShape(string name, RectTransform parent)
        {
            var rect = NewRect(name, parent);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private Group NewGroup(string name, Vector2 dir, float extentArt, float delay)
        {
            var rect = NewRect(name, _frame);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            var g = new Group { Rect = rect, Dir = dir, Extent = extentArt, Delay = delay };
            _groups.Add(g);
            return g;
        }

        /// <summary>Sert gölge + çerçeve + dolgu.</summary>
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

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
