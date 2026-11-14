using New_Scripts.Multiplayer.Session;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>Pist şeridinin her kare aldığı veri. Oda kimlikleri 1'den başlar; rakip odası 0 ise henüz bilinmiyor.</summary>
    public struct RaceTrackState
    {
        public int TotalRooms;
        public int LocalRoom;
        public int RivalRoom;
        public int LocalFlags;
        public int RivalFlags;
        /// <summary>Host mavi, istemci kırmızı (sonuç ekranıyla aynı kural).</summary>
        public bool LocalIsHost;
        /// <summary>Yalnız yarış modunda bayrak flamaları ve ceza vardır.</summary>
        public bool ShowFlags;
        /// <summary>Yarış sürüyor: rakibin alacağı sıradaki flama nabız atar.</summary>
        public bool Racing;
    }

    /// <summary>
    /// Yarış HUD'unun pist şeridi (docs/ui-research/race-hud-track.md): üstte yatay pist, yerel oyuncu çizginin üstünde aşağı bakan
    /// üçgen, rakip altında daire, her işaretçinin sağında 3 flama. Oda sayısı büyüyünce mercek
    /// (<see cref="RaceTrackWindow"/>): 14 odalık pencere, fark &gt; 10 ise iki parça + zikzak + "N rooms".
    /// <list type="bullet">
    /// <item>Renk kimlikten gelir (host mavi, istemci kırmızı); yerel/rakip ayrımı şekil ve konumla yapılır.</item>
    /// <item>Hareket (<see cref="UiSpring"/>, unscaled zaman): işaretçiler yaylanarak kayar, pencere yumuşakça kayar, bölünme zikzakla açılır,
    /// bayrak alınca flama ölçek+dönüşle oturur, 3/3'te 0.45 sn sonra Ink/Yellow şeritli ceza bandı gelir ve ceza alan işaretçi sarsılır.</item>
    /// <item>Rakibin alacağı sıradaki boş flama nabız atar (yerelde yok).</item>
    /// </list>
    /// Kodla kurulan overlay canvas (CRT dışı, <see cref="UiPixelScale"/>); <see cref="RaceHud"/> sürer. Değerler başlangıç önerisi, gamepad ile ayarlanır.
    /// </summary>
    public sealed class RaceTrackView : MonoBehaviour
    {
        public const string ObjectName = "[MP Track]";

        private const float Unit = UiPixelScale.UnitsPerArtPixel;
        private const float FrameW = UiPixelScale.ArtWidth;
        private const float FrameH = UiPixelScale.ArtHeight;

        // Yerleşim (sanat pikseli, çerçevenin sol-üstünden)
        private const float TrackX0 = 96f, TrackX1 = 384f;
        private const float BarY = 25f, BarH = 3f;
        private const float LocalY = 17f, RivalY = 36f;
        private const int Slots = RaceTrackWindow.DefaultWindowWidth;
        private const float Pitch = (TrackX1 - TrackX0) / Slots;
        private const int PieceSlots = RaceTrackWindow.SplitPieceWidth;
        private const float GapCenterSlot = PieceSlots + 1f;
        private const int TickPool = Slots + 3;

        private sealed class Tick
        {
            public RectTransform Rect;
            public Image Image;
            public int Kind = -1;
        }

        private sealed class Segment
        {
            public RectTransform Mask;
            public readonly Tick[] Ticks = new Tick[TickPool];
            public RectTransform Bar;
            public readonly UiSpring Lo = new UiSpring(200f, 22f);
            public readonly UiSpring Left = new UiSpring(200f, 22f);
            public readonly UiSpring Width = new UiSpring(200f, 22f);
        }

        private sealed class Flama
        {
            public RectTransform Rect;
            public UiPolygon Fill;
            public readonly UiSpring Pop = new UiSpring(320f, 16f) { Value = 1f, Target = 1f };
            public bool Filled;
        }

        private sealed class Marker
        {
            public RectTransform Rect;
            public RectTransform Body;
            public TMP_Text Number;
            public readonly UiSpring X = new UiSpring(240f, 20f);
            public readonly Flama[] Flags = new Flama[RaceRules.FlagsPerPenalty];
            public int LastActual = -1;
            public int Shown;
            public float ResetAt = -1f;
            public bool ResetIsPenalty;
            public bool ResetApplies;
            public float ShakeLeft;
            public bool Placed;
        }

        [Header("Hareket (başlangıç önerisi)")]
        [SerializeField, Range(1f, 2.5f)] private float flagPopScale = 1.6f;
        [SerializeField, Range(0f, 30f)] private float flagPopDegrees = 12f;
        [Tooltip("3. bayrak alındıktan sonra cezanın başlamasına kadar bekleme.")]
        [SerializeField, Range(0f, 1f)] private float penaltyHold = 0.45f;
        [SerializeField, Range(0.3f, 3f)] private float bandSeconds = 1.2f;
        [Tooltip("Ceza alan işaretçinin sarsıntı genliği (sanat pikseli; 3 = ±12 canvas birimi).")]
        [SerializeField, Range(0f, 6f)] private float shakeAmount = 3f;
        [SerializeField, Range(0.1f, 1f)] private float shakeSeconds = 0.45f;
        [Tooltip("Rakibin sıradaki flamasının nabız süresi.")]
        [SerializeField, Range(0.3f, 2f)] private float pulseSeconds = 0.7f;
        [Header("Boşta")]
        [SerializeField, Range(0f, 0.08f)] private float breathAmount = 0.03f;
        [SerializeField, Range(0.5f, 4f)] private float breathSeconds = 2.4f;

        private readonly Segment[] _segments = new Segment[2];
        private readonly Marker _local = new Marker();
        private readonly Marker _rival = new Marker();
        private readonly UiSpring _split = new UiSpring(160f, 20f);
        private readonly UiSpring _entrance = new UiSpring(240f, 22f);
        private readonly UiSpring _band = new UiSpring(300f, 22f);

        private UiThemeSO _theme;
        private GameObject _root;
        private RectTransform _frame;
        private RectTransform _stripRoot;
        private RectTransform _zigzag;
        private RectTransform _zigScaler;
        private TMP_Text _hiddenText;
        private RectTransform _bandRoot;
        private TMP_Text _bandText;
        private float _bandUntil = -1f;
        private bool _visible;
        private bool _hasLayout;
        private RaceTrackLayout _layout;
        private RaceTrackState _state;
        private float _border;
        private float _shadow;
        private float _time;

        public bool IsVisible => _visible;
        public RaceTrackLayout Layout => _layout;
        public string BandString => _bandText != null ? _bandText.text : "";
        public bool BandShown => _bandUntil > _time;
        public RectTransform LocalMarkerRect => _local.Rect;
        public RectTransform RivalMarkerRect => _rival.Rect;
        public bool RivalFlagFilled(int index) => _rival.Flags[index].Filled;
        public bool LocalFlagFilled(int index) => _local.Flags[index].Filled;

        public static RaceTrackView Create()
        {
            var go = new GameObject(ObjectName) { layer = LayerMask.NameToLayer("UI") };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            return go.AddComponent<RaceTrackView>();
        }

        // ---- API ----

        /// <summary>Her karede çağrılabilir; ilk çağrıda şerit yukarıdan yaylanarak gelir.</summary>
        public void Present(RaceTrackState state)
        {
            bool rivalKnown = state.RivalRoom > 0;
            int total = Mathf.Max(state.TotalRooms, state.LocalRoom, state.RivalRoom, 1);
            state.TotalRooms = total;
            int rivalRoom = rivalKnown ? state.RivalRoom : state.LocalRoom;
            var layout = RaceTrackWindow.Compute(total, state.LocalRoom, rivalRoom);

            if (!_visible)
            {
                _visible = true;
                _root.SetActive(true);
                _entrance.Value = -90f;
                _entrance.Velocity = 0f;
                _entrance.Target = 0f;
                _hasLayout = false;
                _local.LastActual = _rival.LastActual = -1;
            }

            _state = state;
            SetIdentity(state.LocalIsHost);
            ApplyLayout(layout, state.LocalRoom, rivalRoom, total, rivalKnown);
            SyncFlags(_local, state.LocalFlags, true, state);
            SyncFlags(_rival, state.RivalFlags, false, state);

            _local.Number.text = state.LocalRoom.ToString("00");
            _rival.Number.text = rivalKnown ? state.RivalRoom.ToString("00") : "??";
            _hiddenText.text = layout.IsSplit ? $"{layout.HiddenRooms} rooms" : _hiddenText.text;
            _rival.Rect.gameObject.SetActive(rivalKnown);
            SetFlagsActive(_local, state.ShowFlags);
            SetFlagsActive(_rival, state.ShowFlags && rivalKnown);
        }

        public void Hide()
        {
            if (!_visible) return;
            _visible = false;
            _root.SetActive(false);
            _bandUntil = -1f;
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
            _time = Time.unscaledTime;

            _entrance.Step(dt);
            _stripRoot.anchoredPosition = new Vector2(0f, -_entrance.Value * Unit);

            foreach (var s in _segments) StepSegment(s, dt);
            _split.Step(dt);
            StepZigzag();

            StepMarker(_local, dt, LocalY, true);
            StepMarker(_rival, dt, RivalY, false);

            _band.Target = _time < _bandUntil ? 1f : 0f;
            _band.Step(dt);
            float b = Mathf.Max(0f, _band.Value);
            _bandRoot.localScale = new Vector3(b, Mathf.Min(1f, b), 1f);
            _bandRoot.gameObject.SetActive(b > 0.01f);
        }

        // ---- yerleşim ----

        private void ApplyLayout(RaceTrackLayout layout, int localRoom, int rivalRoom, int total, bool rivalKnown)
        {
            var a = _segments[0];
            var b = _segments[1];
            bool snap = !_hasLayout;

            float aWidth = layout.IsSplit ? PieceSlots : layout.First.Count;
            SetTarget(a.Lo, layout.First.Lo, snap);
            SetTarget(a.Left, 0f, snap);
            SetTarget(a.Width, aWidth, snap);
            if (layout.IsSplit)
            {
                SetTarget(b.Lo, layout.Second.Lo, snap);
                SetTarget(b.Left, PieceSlots + 2f, snap);
                SetTarget(b.Width, PieceSlots, snap);
            }
            else
            {
                SetTarget(b.Left, aWidth, snap);
                SetTarget(b.Width, 0f, snap);
            }

            SetTarget(_split, layout.IsSplit ? 1f : 0f, snap);

            float lx = MarkerX(layout, localRoom);
            float rx = MarkerX(layout, rivalRoom);
            SetTarget(_local.X, lx, snap || !_local.Placed);
            SetTarget(_rival.X, rx, snap || !_rival.Placed);
            _local.Placed = _rival.Placed = true;

            _layout = layout;
            _hasLayout = true;
        }

        private static float MarkerX(RaceTrackLayout layout, int room)
        {
            bool inSecond = layout.IsSplit && room >= layout.Second.Lo;
            float left = inSecond ? PieceSlots + 2f : 0f;
            float lo = inSecond ? layout.Second.Lo : layout.First.Lo;
            return TrackX0 + (left + (room - lo) + 0.5f) * Pitch;
        }

        private static void SetTarget(UiSpring spring, float target, bool snap)
        {
            spring.Target = target;
            if (!snap) return;
            spring.Value = target;
            spring.Velocity = 0f;
        }

        private void StepSegment(Segment s, float dt)
        {
            s.Lo.Step(dt);
            s.Left.Step(dt);
            s.Width.Step(dt);

            float width = Mathf.Max(0f, s.Width.Value);
            s.Mask.gameObject.SetActive(width > 0.02f);
            s.Mask.anchoredPosition = new Vector2((TrackX0 + s.Left.Value * Pitch) * Unit, 0f);
            s.Mask.sizeDelta = new Vector2(width * Pitch * Unit, 80f * Unit);

            int firstRoom = Mathf.FloorToInt(s.Lo.Value) - 1;
            int total = Mathf.Max(1, _state.TotalRooms);
            for (int i = 0; i < s.Ticks.Length; i++)
            {
                var t = s.Ticks[i];
                int room = firstRoom + i;
                bool show = room >= 1 && room <= total;
                t.Rect.gameObject.SetActive(show);
                if (!show) continue;

                int kind = room == total ? 2 : room % 10 == 0 ? 1 : 0;
                if (kind != t.Kind) SetTickKind(t, kind);
                t.Rect.anchoredPosition = new Vector2((room - s.Lo.Value + 0.5f) * Pitch * Unit, t.Rect.anchoredPosition.y);
            }
        }

        private void SetTickKind(Tick t, int kind)
        {
            t.Kind = kind;
            float w = kind == 2 ? 4f : 1.5f;
            float h = kind == 2 ? 16f : kind == 1 ? 14f : 8f;
            t.Rect.sizeDelta = new Vector2(w, h) * Unit;
            t.Rect.anchoredPosition = new Vector2(0f, -(BarY + BarH * 0.5f - h * 0.5f) * Unit);
        }

        private void StepZigzag()
        {
            float amount = Mathf.Clamp01(_split.Value);
            _zigzag.gameObject.SetActive(amount > 0.02f);
            _zigzag.anchoredPosition = new Vector2((TrackX0 + GapCenterSlot * Pitch) * Unit, 0f);
            _zigScaler.localScale = new Vector3(1f, amount, 1f);
            var c = _hiddenText.color;
            c.a = amount;
            _hiddenText.color = c;
        }

        // ---- işaretçiler ve flamalar ----

        private void StepMarker(Marker m, float dt, float y, bool isLocal)
        {
            m.X.Step(dt);

            float shake = 0f;
            if (m.ShakeLeft > 0f)
            {
                m.ShakeLeft = Mathf.Max(0f, m.ShakeLeft - dt);
                shake = Mathf.Sin(_time * 70f) * shakeAmount * (m.ShakeLeft / shakeSeconds);
            }

            m.Rect.anchoredPosition = new Vector2((m.X.Value + shake) * Unit, -y * Unit);

            // Boşta: yerel işaretçi hafifçe nefes alır (yalnız ölçek).
            float breath = isLocal ? 1f + breathAmount * Mathf.Sin(_time * Mathf.PI * 2f / breathSeconds) : 1f;
            m.Body.localScale = new Vector3(breath, breath, 1f);

            if (m.ResetAt >= 0f && _time >= m.ResetAt)
            {
                m.ResetAt = -1f;
                m.Shown = m.LastActual;
                if (m.ResetIsPenalty && m.ResetApplies) StartPenalty(isLocal);
            }

            float pulse = 0f;
            if (!isLocal && _state.Racing && m.ResetAt < 0f)
                pulse = 0.5f - 0.5f * Mathf.Cos(_time * Mathf.PI * 2f / pulseSeconds);

            for (int i = 0; i < m.Flags.Length; i++)
            {
                var f = m.Flags[i];
                bool filled = i < m.Shown;
                f.Filled = filled;
                f.Pop.Step(dt);
                float v = f.Pop.Value;
                float rot = flagPopScale > 1f ? -flagPopDegrees * (v - 1f) / (flagPopScale - 1f) : 0f;
                f.Rect.localScale = new Vector3(v, v, 1f);
                f.Rect.localRotation = Quaternion.Euler(0f, 0f, rot);
                float blend = filled ? 1f : (!isLocal && i == m.Shown ? pulse : 0f);
                f.Fill.color = Color.Lerp(_theme.Paper, _theme.Yellow, blend);
            }
        }

        private void SyncFlags(Marker m, int actual, bool isLocal, RaceTrackState state)
        {
            if (!state.ShowFlags)
            {
                m.LastActual = actual;
                m.Shown = 0;
                m.ResetAt = -1f;
                return;
            }

            int max = RaceRules.FlagsPerPenalty;
            if (m.LastActual < 0)
            {
                m.Shown = actual;
            }
            else if (actual > m.LastActual)
            {
                if (m.ResetAt < 0f) m.Shown = actual;
                PopFlag(m, actual - 1);
            }
            else if (actual == 0 && m.LastActual == max - 1)
            {
                // Sayaç 3'ten sıfırlandı: 3. flama dolar, kısa bekleyişten sonra ceza gelir.
                m.Shown = max;
                PopFlag(m, max - 1);
                m.ResetAt = _time + penaltyHold;
                m.ResetIsPenalty = true;
                int victimRoom = isLocal ? state.RivalRoom : state.LocalRoom;
                m.ResetApplies = victimRoom > RaceRules.FirstRoomId;
            }
            else if (actual < m.LastActual && m.ResetAt < 0f)
            {
                m.Shown = actual;
            }

            m.LastActual = actual;
        }

        private void PopFlag(Marker m, int index)
        {
            if (index < 0 || index >= m.Flags.Length) return;
            var f = m.Flags[index];
            f.Pop.Value = flagPopScale;
            f.Pop.Velocity = 0f;
            f.Pop.Target = 1f;
        }

        private static void SetFlagsActive(Marker m, bool active)
        {
            foreach (var f in m.Flags)
                if (f.Rect.gameObject.activeSelf != active) f.Rect.gameObject.SetActive(active);
        }

        /// <param name="takerIsLocal">3. bayrağı alan yerel oyuncu mu; ceza alan karşı taraftır.</param>
        private void StartPenalty(bool takerIsLocal)
        {
            var victim = takerIsLocal ? _rival : _local;
            victim.ShakeLeft = shakeSeconds;
            int victimRoom = takerIsLocal ? _state.RivalRoom : _state.LocalRoom;
            _bandText.text = $"{(takerIsLocal ? "rival" : "you")} << room {Mathf.Max(1, victimRoom - 1):00}";
            _bandUntil = _time + bandSeconds;
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 105; // Ana HUD (100) üstünde, oda kartı (110) ve sonuç ekranının (120) altında
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<UiPixelScale>();

            _border = Unit;
            _shadow = Unit;

            _root = NewRect("Root", transform).gameObject;
            Stretch((RectTransform)_root.transform);

            _frame = NewRect("Frame", _root.transform);
            _frame.anchorMin = _frame.anchorMax = new Vector2(0.5f, 1f);
            _frame.pivot = new Vector2(0f, 1f);
            _frame.sizeDelta = new Vector2(FrameW, FrameH) * Unit;
            _frame.anchoredPosition = new Vector2(-FrameW * 0.5f * Unit, 0f);

            _stripRoot = NewTopLeft("Strip", _frame, FrameW * Unit, FrameH * Unit);

            // Ekranı kaplayan zemin yok: yalnız pistin arkasında ince ray, metinlerin arkasında küçük plakalar var.
            for (int i = 0; i < _segments.Length; i++) _segments[i] = NewSegment(i);

            _zigzag = NewTopLeft("Zigzag", _stripRoot, 0f, 0f);
            // Çizgi boşlukta da sürer, ortada kırık çizgi (zikzak) ile kopar.
            float barMid = BarY + BarH * 0.5f;
            float gapHalf = Pitch;
            _zigScaler = NewTopLeft("Zig Scaler", _zigzag, 0f, 0f);
            _zigScaler.anchoredPosition = new Vector2(0f, -barMid * Unit);
            // Çizgi boşlukta da sürer ve ortada /\/\ şeklinde kırılır (pist çizgisiyle aynı kalınlıkta).
            const float zigHalfWidth = 12f;
            const float amplitude = 5f;
            float half = BarH * 0.5f;
            Box(_zigScaler, "Stub L", -gapHalf, -half, -zigHalfWidth, half, _theme.Ink);
            Box(_zigScaler, "Stub R", zigHalfWidth, -half, gapHalf, half, _theme.Ink);
            var zig = new[]
            {
                new Vector2(-zigHalfWidth, 0f), new Vector2(-9f, -amplitude), new Vector2(-3f, amplitude),
                new Vector2(3f, -amplitude), new Vector2(9f, amplitude), new Vector2(zigHalfWidth, 0f)
            };
            for (int i = 0; i < zig.Length - 1; i++)
            {
                var d = (zig[i + 1] - zig[i]).normalized;
                var n = new Vector2(-d.y, d.x) * half;
                var a = zig[i] - d * half; // uç uzatması: bitişik parçalar arasında boşluk kalmasın
                var b = zig[i + 1] + d * half;
                AddShape(_zigScaler, "Zig " + i, new[] { a + n, b + n, b - n, a - n }, _theme.Ink, 0f);
            }

            Box(_zigScaler, "Hidden Plate", -19f, 37f - barMid, 19f, 45f - barMid, _theme.Ink);
            _hiddenText = AddText("Hidden", _stripRoot, 5f, _theme.Paper, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(60f, 8f));
            _hiddenText.rectTransform.SetParent(_zigzag, false);
            _hiddenText.rectTransform.anchoredPosition = new Vector2(0f, -41f * Unit);

            _local.Rect = NewTopLeft("Local", _stripRoot, 0f, 0f);
            _rival.Rect = NewTopLeft("Rival", _stripRoot, 0f, 0f);
            BuildMarker(_local, true);
            BuildMarker(_rival, false);

            BuildBand();
        }

        private Segment NewSegment(int index)
        {
            var s = new Segment { Mask = NewTopLeft("Segment " + index, _stripRoot, 0f, 0f) };
            s.Mask.gameObject.AddComponent<RectMask2D>();
            s.Bar = Box(s.Mask, "Bar", 0f, BarY, 1f, BarY + BarH, _theme.Ink).rectTransform;
            s.Bar.anchorMin = new Vector2(0f, 1f);
            s.Bar.anchorMax = new Vector2(1f, 1f);
            s.Bar.pivot = new Vector2(0.5f, 1f);
            s.Bar.sizeDelta = new Vector2(0f, BarH * Unit);
            s.Bar.anchoredPosition = new Vector2(0f, -BarY * Unit);
            for (int i = 0; i < s.Ticks.Length; i++)
            {
                var img = Box(s.Mask, "Tick " + i, 0f, 0f, 1f, 1f, _theme.Ink);
                var rt = img.rectTransform;
                rt.pivot = new Vector2(0.5f, 1f);
                s.Ticks[i] = new Tick { Rect = rt, Image = img };
            }

            return s;
        }

        private void BuildMarker(Marker m, bool isLocal)
        {
            Color color = isLocal ? _theme.Red : _theme.Blue; // yer tutucu; Present kimliğe göre boyar
            m.Body = NewTopLeft("Body", m.Rect, 0f, 0f);
            var shape = isLocal
                ? new[] { new Vector2(-7f, -7f), new Vector2(7f, -7f), new Vector2(0f, 7f) }
                : Circle(6.5f);
            AddShape(m.Body, "Shadow", shape, _theme.Ink, _border).rectTransform.anchoredPosition = new Vector2(_shadow, -_shadow);
            AddShape(m.Body, "Outline", shape, _theme.Ink, _border);
            var fill = AddShape(m.Body, "Fill", shape, color, 0f);
            fill.name = "Fill";
            m.Number = AddText("Number", m.Body, isLocal ? 5.5f : 5f, _theme.Paper, TextAlignmentOptions.Center,
                new Vector2(0f, isLocal ? -2f : 0f), new Vector2(16f, 8f));

            float startX = isLocal ? 10f : 9f;
            for (int i = 0; i < m.Flags.Length; i++)
            {
                var f = new Flama { Rect = NewTopLeft("Flag " + i, m.Rect, 0f, 0f) };
                f.Rect.anchoredPosition = new Vector2((startX + i * 6f) * Unit, 0f);
                var tri = new[] { new Vector2(-2.5f, -3f), new Vector2(-2.5f, 3f), new Vector2(3f, 0f) };
                AddShape(f.Rect, "Outline", tri, _theme.Ink, _border);
                f.Fill = AddShape(f.Rect, "Fill", tri, _theme.Paper, 0f);
                m.Flags[i] = f;
            }

            if (isLocal)
            {
                Box(m.Rect, "You Plate", -22f, -4f, -10f, 4f, _theme.Ink);
                var you = AddText("You", m.Rect, 5f, _theme.Paper, TextAlignmentOptions.Center, new Vector2(-16f, 0f), new Vector2(12f, 8f));
                you.text = "you";
            }

        }

        private bool _identityKnown;
        private bool _localIsHost;

        /// <summary>Kimlik rengini (host mavi, istemci kırmızı) işaretçilere uygular; yalnız kimlik değişince boyar.</summary>
        private void SetIdentity(bool localIsHost)
        {
            if (_identityKnown && _localIsHost == localIsHost) return;
            _identityKnown = true;
            _localIsHost = localIsHost;
            Paint(_local, localIsHost);
            Paint(_rival, !localIsHost);
        }

        private void Paint(Marker m, bool isHost)
        {
            Color c = isHost ? _theme.Blue : _theme.Red;
            var fill = m.Body.Find("Fill");
            if (fill != null) fill.GetComponent<UiPolygon>().color = c;
        }

        private void BuildBand()
        {
            _bandRoot = NewTopLeft("Penalty Band", _stripRoot, 0f, 0f);
            _bandRoot.anchoredPosition = new Vector2(FrameW * 0.5f * Unit, -56f * Unit);
            _bandRoot.localRotation = Quaternion.Euler(0f, 0f, -2f);
            var stripes = Box(_bandRoot, "Hazard", -86f, -7f, 86f, 7f, _theme.Yellow);
            var hazard = stripes.gameObject.AddComponent<UiHazardStripes>();
            hazard.Screen = UiScreen.MultiplayerResult;
            Box(_bandRoot, "Plate", -64f, -5f, 64f, 5f, _theme.Ink);
            _bandText = AddText("Text", _bandRoot, 6f, _theme.Paper, TextAlignmentOptions.Center, Vector2.zero, new Vector2(124f, 10f));
            _bandText.richText = false;
            _bandRoot.gameObject.SetActive(false);
        }

        // ---- yardımcılar ----

        private static Vector2[] Circle(float r)
        {
            const int seg = 20;
            var pts = new Vector2[seg];
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                pts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }

            return pts;
        }

        /// <summary>Merkezine göre sanat pikseli noktalar (y aşağı); <paramref name="grow"/> kadar (canvas birimi) dışa itilir.</summary>
        private static UiPolygon AddShape(RectTransform parent, string name, Vector2[] artPoints, Color color, float grow)
        {
            var local = new Vector2[artPoints.Length];
            for (int i = 0; i < local.Length; i++) local[i] = new Vector2(artPoints[i].x, -artPoints[i].y) * Unit;
            if (grow > 0f) local = PolygonMath.OffsetConvex(local, grow);

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            var poly = go.AddComponent<UiPolygon>();
            var rt = poly.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(24f, 24f) * Unit;
            rt.anchoredPosition = Vector2.zero;
            poly.raycastTarget = false;
            poly.SetPoints(local);
            poly.color = color;
            return poly;
        }

        /// <summary>Sanat pikseli koordinatlı dikdörtgen (sol-üst çerçeve köşesinden, y aşağı).</summary>
        private static Image Box(RectTransform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x0, -y0) * Unit;
            rt.sizeDelta = new Vector2(x1 - x0, y1 - y0) * Unit;
            return img;
        }

        private TMP_Text AddText(string name, RectTransform parent, float artSize, Color color, TextAlignmentOptions alignment,
            Vector2 positionArt, Vector2 sizeArt)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            var font = _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont;
            if (font != null) text.font = font;
            text.fontSize = artSize * Unit;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.color = color;
            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeArt * Unit;
            rt.anchoredPosition = new Vector2(positionArt.x, -positionArt.y) * Unit;
            return text;
        }

        private static RectTransform NewTopLeft(string name, Transform parent, float width, float height)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(width, height);
            return rt;
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
