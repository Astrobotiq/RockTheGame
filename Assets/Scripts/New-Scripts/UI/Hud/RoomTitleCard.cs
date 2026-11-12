using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using New_Scripts.Death;
using New_Scripts.LevelChange;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Hud
{
    /// <summary>
    /// Oda başlık kartı (mockup <c>uiCard</c>): odaya girince ekranın solundan eğik beton levha, levhadan taşan sarı kiriş
    /// (başlık yazısı kirişin eğimini izler), ekstrüde kırmızı oda numarası, kırpılmış mavi disk ve tehlike şeridi gelir;
    /// ~1 sn kalıp geri çekilir. <b>Oyunu durdurmaz</b>: hiçbir girişi/zamanı dondurmaz, hareket unscaled zamanda (hitstop'ta yarım kalmaz).
    /// <list type="bullet">
    /// <item>Overlay canvas'ta (CRT dışında, keskin), <see cref="UiPixelScale"/> ile tam sayı sanat pikseline sabit. Kod ile kurulur.</item>
    /// <item>Tetik: <b>oyuna girişte bir kez</b> (oda geçişlerinde değil). Oyun sahnesi başlayınca oyuncunun içinde durduğu oda
    /// gösterilir; kayıtlı yarışa devamda oyuncu önce kayıtlı odaya taşındığı için o oda gösterilir. İleride tek kişilik kayıt
    /// yüklenince de aynı yol: <see cref="ShowRoom"/>. Numarası olmayan oda (<c>RoomId ≤ 0</c>) kart göstermez.
    /// Başlık <see cref="Room.RoomTitle"/>'dan gelir; boşsa kirişte yazı olmaz.</item>
    /// <item>Hareket: <see cref="UiSpring"/> (levha, kiriş, şerit sırayla kayar; numara büyükten çakılır). DOTween kullanılmaz:
    /// yay unscaled zamanda yeniden hedeflenebilir (ui-motion.md).</item>
    /// <item>İris geçişiyle çakışma: <see cref="startDelay"/> ile kart geciktirilir (ayrı #test task'ı).</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/room-title-card.md, ui-motion.md).
    /// </summary>
    [AddComponentMenu("RockTheGame/UI/Room Title Card")]
    public sealed class RoomTitleCard : MonoBehaviour
    {
        private sealed class Group
        {
            public RectTransform Rect;
            /// <summary>Ekran dışından uzaklık (sanat pikseli): 0 değerinde grup bu kadar solda durur.</summary>
            public float Extent;
            public float Delay;
            public float Wait;
            public float PendingTarget;
            public bool HasPending;
            public readonly UiSpring Slide = new UiSpring(240f, 22f);
        }

        private const float Unit = UiPixelScale.UnitsPerArtPixel;
        // Mockup ölçüleri (480x270 ızgarada tam ekran kart); cardScale ile küçültülür.
        private const float MockupHeight = 270f;
        private static readonly Vector2 BeamFrom = new Vector2(0f, 206f);
        private static readonly Vector2 BeamTo = new Vector2(480f, 122f);
        private const float BeamLeftX = -70f;
        private const float BeamHalfThickness = 30f;
        private const float SettleSeconds = 0.35f;

        [Header("Zamanlama (başlangıç önerisi)")]
        [Tooltip("Odaya girişten karta kadar bekleme (sn). İris geçişi bitene kadar geciktirmek için (RoomTransitionCoordinator).")]
        [SerializeField, Range(0f, 2f)] private float startDelay;
        [Tooltip("Oyuna girişte oyuncunun yerleşmesini bekleyen kare sayısı (kayıtlı yarışa devamda oyuncu 2 kare sonra taşınır).")]
        [SerializeField, Range(1, 10)] private int entryWaitFrames = 4;
        [Tooltip("Kart yerine oturduktan sonra ekranda kalma süresi (sn). Toplam ≈ 0,35 + bu + ~0,3 çıkış.")]
        [SerializeField, Range(0.1f, 3f)] private float holdSeconds = 0.45f;
        [SerializeField, Range(0f, 0.3f)] private float beamDelay = 0.08f;
        [SerializeField, Range(0f, 0.5f)] private float numberDelay = 0.2f;
        [SerializeField, Range(0f, 0.5f)] private float hazardDelay = 0.26f;
        [Tooltip("Numaranın çakılmadan önceki ölçeği (1 = çakma yok).")]
        [SerializeField, Range(1f, 3f)] private float numberSlamScale = 1.7f;

        [Header("Boyut")]
        [Tooltip("Mockup kartının (tam ekran yüksekliği) ölçeği. 0,5 ≈ ekranın yarı yüksekliği (oyuncuyu az kapatsın).")]
        [SerializeField, Range(0.4f, 1f)] private float cardScale = 0.5f;

        private readonly List<Group> _groups = new List<Group>();
        private readonly UiSpring _numberSlam = new UiSpring(320f, 16f);

        private UiThemeSO _theme;
        private GameObject _canvasRoot;
        private RectTransform _content;
        private Group _slab, _beam, _hazard;
        private RectTransform _numberRect;
        private TMP_Text _numberText;
        private UiExtrudedText _numberExtrusion;
        private TMP_Text _titleText;
        private bool _numberVisible;
        private float _startAt = -1f;
        private float _hideAt = -1f;
        private float _numberAt = -1f;

        /// <summary>Kartı bu numara ve başlıkla gösterir. Numara ≤ 0 ise gösterilmez.</summary>
        public void Show(int roomNumber, string title)
        {
            string number = RoomCardMath.FormatNumber(roomNumber);
            if (number.Length == 0) return;

            _numberText.text = number;
            _titleText.text = title ?? string.Empty;
            _titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));

            _content.gameObject.SetActive(true);
            _numberExtrusion.Refresh();

            foreach (var g in _groups)
            {
                g.Slide.Value = 0f;
                g.Slide.Velocity = 0f;
                g.Slide.Target = 0f;
                g.HasPending = false;
                ApplySlide(g);
            }

            _numberVisible = false;
            _numberAt = -1f;
            _numberRect.localScale = Vector3.one * 0.0001f;
            _startAt = Time.unscaledTime + startDelay;
            _hideAt = _startAt + SettleSeconds + holdSeconds;
        }

        // ---- yaşam döngüsü ----

        private void Awake()
        {
            _theme = UiThemeSO.Load();
            BuildUi();
        }

        private void OnDestroy()
        {
            if (_canvasRoot != null) Destroy(_canvasRoot);
        }

        /// <summary>
        /// Oyuna giriş: LevelBootstrapper.Start oyuncuyu başlangıç odasına koyar; kayıtlı yarışa devamda MultiplayerSession iki kare
        /// sonra kayıtlı odaya taşır (<c>RoomPlacement</c>). Bu yüzden birkaç kare bekleyip oyuncunun içinde durduğu odayı gösteririz.
        /// </summary>
        private async UniTaskVoid Start()
        {
            await UniTask.DelayFrame(entryWaitFrames);
            if (this == null) return;

            var health = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            if (health == null) return;

            Vector2 position = health.transform.position;
            foreach (var room in FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (room.RoomBounds == null || !room.RoomBounds.OverlapPoint(position)) continue;
                ShowRoom(room);
                return;
            }
        }

        /// <summary>Verilen odanın kartını gösterir (oyuna giriş, ileride tek kişilik kayıt yükleme).</summary>
        public void ShowRoom(Room room)
        {
            if (room != null) Show(room.RoomId, room.RoomTitle);
        }

        // ---- hareket ----

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;

            if (_startAt >= 0f && now >= _startAt)
            {
                _startAt = -1f;
                _numberAt = now + numberDelay;
                Order(1f, 0f);
            }

            if (_hideAt >= 0f && now >= _hideAt)
            {
                _hideAt = -1f;
                Order(0f, 0f);
            }

            bool anyMoving = _startAt >= 0f || _hideAt >= 0f;
            foreach (var g in _groups)
            {
                if (g.HasPending)
                {
                    anyMoving = true;
                    g.Wait -= dt;
                    if (g.Wait <= 0f)
                    {
                        g.HasPending = false;
                        g.Slide.Target = g.PendingTarget;
                    }
                }

                g.Slide.Step(dt);
                ApplySlide(g);
                if (g.Slide.Target > 0f || g.Slide.Value > 0.03f) anyMoving = true;
            }

            StepNumber(dt, now);
            if (!anyMoving) _content.gameObject.SetActive(false);
        }

        /// <summary>Gruplara hedef verir; gruplar kendi gecikmeleriyle sırayla başlar (çıkışta gecikme yok, hep birlikte çekilir).</summary>
        private void Order(float target, float extraWait)
        {
            foreach (var g in _groups)
            {
                g.PendingTarget = target;
                g.HasPending = true;
                g.Wait = extraWait + (target > 0f ? g.Delay : 0f);
            }
        }

        private void StepNumber(float dt, float now)
        {
            // Numara, giriş emri verildiğinden itibaren numberDelay sonra büyükten çakılır; çıkışta levhayla birlikte gider.
            if (_numberAt >= 0f && now >= _numberAt)
            {
                _numberAt = -1f;
                _numberVisible = true;
                _numberSlam.Value = numberSlamScale;
                _numberSlam.Velocity = 0f;
                _numberSlam.Target = 1f;
            }

            if (!_numberVisible) return;
            _numberSlam.Step(dt);
            _numberRect.localScale = Vector3.one * Mathf.Max(0.0001f, _numberSlam.Value);
        }

        private static void ApplySlide(Group g)
        {
            float offset = -g.Extent * Unit * (1f - g.Slide.Value);
            g.Rect.anchoredPosition = new Vector2(offset, 0f);
        }

        // ---- arayüz ----

        private float A(float mockup) => Mathf.Round(mockup * cardScale);

        private Vector2 AP(float x, float y) => new Vector2(A(x), A(y));

        private void BuildUi()
        {
            _canvasRoot = new GameObject("RoomTitleCardCanvas", typeof(RectTransform));
            _canvasRoot.transform.SetParent(transform, false);
            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110; // HUD sayacının (100) üstünde, pause'un (200) altında
            _canvasRoot.AddComponent<CanvasScaler>();
            _canvasRoot.AddComponent<UiPixelScale>();

            float cardH = A(MockupHeight);
            float cardW = A(480f);

            // Tüm kart tek kapta: sol kenarda, dikeyde ortalı. Kapın pivotu sol-üst; sol-üst köşe (0,0) sanat koordinatı.
            _content = NewRect("Content", _canvasRoot.transform);
            _content.anchorMin = _content.anchorMax = new Vector2(0f, 0.5f);
            _content.pivot = new Vector2(0f, 1f);
            _content.sizeDelta = Vector2.zero;
            _content.anchoredPosition = new Vector2(0f, cardH * Unit * 0.5f);

            float border = _theme.BorderWidthFor(UiScreen.RoomCard) * Unit;
            float shadow = _theme.ShadowOffsetFor(UiScreen.RoomCard) * Unit;

            // 1) levha + mavi disk + "ROOM"
            _slab = NewGroup("Slab", A(372f) + 30f, 0f, cardW, cardH);
            var slabPts = new[] { AP(0f, 0f), AP(372f, 0f), AP(256f, 270f), AP(0f, 270f) };
            var slabFill = AddSolid(_slab.Rect, "Slab", slabPts, _theme.ConcreteSlab, Vector2.zero, border, shadow, cardW, cardH);
            var slabSurface = slabFill.gameObject.AddComponent<UiConcreteSurface>();
            slabSurface.Configure(UiScreen.RoomCard, ConcreteTone.Slab, asPaint: false);
            slabSurface.Apply();

            var discPts = RoomCardMath.ClippedDisc(new Vector2(A(26f), A(208f)), A(92f), 0f, A(270f), 48);
            // Disk, levhanın sol-alt köşesinden büyür: kırpma kenarları levha kenarında kalır.
            var discFill = AddSolid(_slab.Rect, "Disc", discPts, _theme.Blue, new Vector2(0f, A(270f)), border, 0f, cardW, cardH);
            var discSurface = discFill.gameObject.AddComponent<UiConcreteSurface>();
            discSurface.Configure(UiScreen.RoomCard, ConcreteTone.Wall, asPaint: true);
            discSurface.Apply();
            discFill.color = _theme.Blue;

            var roomLabel = AddText("RoomLabel", _slab.Rect, _theme.MonoBoldFont != null ? _theme.MonoBoldFont : _theme.MonoFont,
                Mathf.Max(6f, A(10f)), _theme.Ink, TextAlignmentOptions.MidlineLeft, AP(22f, 16f), new Vector2(A(120f), A(14f)), new Vector2(0f, 0.5f));
            roomLabel.text = "ROOM";

            // Numara: kendi kabında (ekstrüzyon kopyaları kapla birlikte ölçeklenir), sol-orta noktasından çakılır.
            var numberGo = NewRect("Number", _slab.Rect);
            numberGo.anchorMin = numberGo.anchorMax = new Vector2(0f, 1f);
            numberGo.pivot = new Vector2(0.5f, 0.5f);
            numberGo.sizeDelta = Vector2.zero;
            numberGo.anchoredPosition = new Vector2(A(20f), -A(84f)) * Unit;
            _numberRect = numberGo;
            _numberText = AddText("NumberText", numberGo, _theme.TitleFont, A(112f), _theme.Red,
                TextAlignmentOptions.MidlineLeft, Vector2.zero, new Vector2(A(220f), A(130f)), new Vector2(0f, 0.5f), anchorTopLeft: false);
            _numberText.text = "00";
            _numberExtrusion = _numberText.gameObject.AddComponent<UiExtrudedText>();
            _numberExtrusion.Screen = UiScreen.RoomCard;
            _numberRect.localScale = Vector3.one * 0.0001f;

            // 2) sarı kiriş + eğimi izleyen başlık
            _beam = NewGroup("Beam", BeamRightExtent(), beamDelay, cardW, cardH);
            var beamPts = BeamPolygon();
            var beamFill = AddSolid(_beam.Rect, "Beam", beamPts, _theme.Yellow, Vector2.zero, border, shadow, cardW, cardH);
            var beamSurface = beamFill.gameObject.AddComponent<UiConcreteSurface>();
            beamSurface.Configure(UiScreen.RoomCard, ConcreteTone.Wall, asPaint: true);
            beamSurface.Apply();
            beamFill.color = _theme.Yellow;

            const float textX = 196f;
            float textY = RoomCardMath.LineY(BeamFrom, BeamTo, textX);
            _titleText = AddText("BeamTitle", _beam.Rect, _theme.TitleFont, Mathf.Max(6f, A(17f)), _theme.Ink,
                TextAlignmentOptions.Midline, AP(textX, textY), new Vector2(A(300f), A(26f)), new Vector2(0.5f, 0.5f));
            _titleText.rectTransform.localEulerAngles = new Vector3(0f, 0f, RoomCardMath.BeamAngleDegrees(BeamFrom, BeamTo));

            // 3) tehlike şeridi
            _hazard = NewGroup("Hazard", A(236f) + 30f, hazardDelay, cardW, cardH);
            var stripPts = new[] { AP(16f, 248f), AP(236f, 248f), AP(230f, 260f), AP(16f, 260f) };
            var stripFill = AddSolid(_hazard.Rect, "Strip", stripPts, _theme.Yellow, Vector2.zero, border, 0f, cardW, cardH);
            var stripes = stripFill.gameObject.AddComponent<UiHazardStripes>();
            stripes.Screen = UiScreen.RoomCard;

            foreach (var g in _groups) ApplySlide(g);
            _content.gameObject.SetActive(false);
        }

        private float BeamRightExtent() => A(480f) + 30f;

        private Vector2[] BeamPolygon()
        {
            float rightX = BeamTo.x;
            float cLeft = RoomCardMath.LineY(BeamFrom, BeamTo, BeamLeftX);
            float cRight = RoomCardMath.LineY(BeamFrom, BeamTo, rightX);
            return new[]
            {
                AP(BeamLeftX, cLeft - BeamHalfThickness), AP(rightX, cRight - BeamHalfThickness),
                AP(rightX, cRight + BeamHalfThickness), AP(BeamLeftX, cLeft + BeamHalfThickness)
            };
        }

        private Group NewGroup(string name, float extentArt, float delay, float width, float height)
        {
            var rect = NewRect(name, _content);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height) * Unit;
            var g = new Group { Rect = rect, Extent = extentArt, Delay = delay };
            _groups.Add(g);
            return g;
        }

        /// <summary>
        /// Gölge + çerçeve + dolgu: üç <see cref="UiPolygon"/> (HudCounterPanel ile aynı kalıp). Noktalar sanat pikseli (y aşağı, sol-üstten);
        /// <paramref name="pivotArt"/> grubun içinde bu şeklin ölçek/dönüş merkezidir. Dolguyu döndürür.
        /// </summary>
        private UiPolygon AddSolid(RectTransform parent, string name, Vector2[] artPoints, Color fill, Vector2 pivotArt,
            float border, float shadow, float width, float height)
        {
            var local = new Vector2[artPoints.Length];
            for (int i = 0; i < local.Length; i++)
                local[i] = new Vector2(artPoints[i].x - pivotArt.x, -(artPoints[i].y - pivotArt.y)) * Unit;

            if (shadow > 0f)
            {
                var shifted = PolygonMath.OffsetConvex(local, border);
                for (int i = 0; i < shifted.Length; i++) shifted[i] += new Vector2(shadow, -shadow);
                NewPolygon(name + " Shadow", parent, shifted, _theme.Ink, pivotArt, width, height);
            }

            NewPolygon(name + " Outline", parent, PolygonMath.OffsetConvex(local, border), _theme.Ink, pivotArt, width, height);
            return NewPolygon(name + " Fill", parent, local, fill, pivotArt, width, height);
        }

        private static UiPolygon NewPolygon(string name, RectTransform parent, Vector2[] localPoints, Color color,
            Vector2 pivotArt, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var poly = go.AddComponent<UiPolygon>();
            var rt = poly.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(pivotArt.x / width, 1f - pivotArt.y / height);
            rt.sizeDelta = new Vector2(width, height) * Unit;
            rt.anchoredPosition = new Vector2(pivotArt.x, -pivotArt.y) * Unit;
            poly.raycastTarget = false;
            poly.SetPoints(localPoints);
            poly.color = color;
            return poly;
        }

        private TMP_Text AddText(string name, RectTransform parent, TMP_FontAsset font, float artSize, Color color,
            TextAlignmentOptions alignment, Vector2 positionArt, Vector2 sizeArt, Vector2 pivot, bool anchorTopLeft = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
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
            Vector2 anchor = anchorTopLeft ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = sizeArt * Unit;
            rt.anchoredPosition = new Vector2(positionArt.x, anchorTopLeft ? -positionArt.y : positionArt.y) * Unit;
            return text;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
