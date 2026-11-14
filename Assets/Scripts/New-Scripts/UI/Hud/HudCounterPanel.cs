using System.Collections.Generic;
using New_Scripts.Collectible;
using New_Scripts.Death;
using New_Scripts.LevelChange;
using New_Scripts.UI.Shapes;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace New_Scripts.UI.Hud
{
    /// <summary>
    /// HUD sayacı (mockup): iç içe iki şekil, ikisi bağımsız görünür. Sol-üst köşede mavi çeyrek daire: yalnızca ananas ikonu ve sayısı.
    /// Onu saran koyu mavi dış halka: bulunulan odanın anahtarları (silüet ikon: toplanan boyalı, eksik çıplak beton).
    /// Overlay canvas'ta çizilir (CRT dışında, keskin) ve <see cref="UiPixelScale"/> ile tam sayı sanat pikseline sabitlenir.
    /// <list type="bullet">
    /// <item><b>Ananas olay güdümlü:</b> çeyrek daire yalnızca ananas toplanınca köşeden büyüyerek görünür, <c>showSeconds</c> sonra çekilir
    /// (ekranın köşesini sürekli kapatmasın). <c>OnCoinsChanged</c>'den okunur. Ölünce toplananlar sessizce geri döndüğü için
    /// <c>OnDeath</c>'ten bir kare sonra yenilenir (görünmeden).</item>
    /// <item><b>Anahtar şekli bağımsız ve o da olay güdümlü:</b> anahtarlı odaya girince (<c>OnRoomEntered</c>) eksik anahtar varsa
    /// <c>keysIntroSeconds</c> boyunca açılıp "bu odada anahtar toplanacak" der, sonra çekilir; anahtar toplanınca <c>showSeconds</c> boyunca
    /// yeniden açılır (toplananı boyar, çakılır) ve çekilir (köşeyi ve altındaki ananası kapatmasın). Anahtar toplamak ananas sayacını açmaz.
    /// Yalnız anahtar varken köşede yalnızca halka görünür (ortası boş); anahtarlar köşegen etrafında ortalanır.</item>
    /// <item>Hareket: yaylar + unscaled zaman; değer değişince ikon ve yazı sıçrar, toplanan anahtar çakılır. Boşta hareket yok (okunabilirlik).</item>
    /// </list>
    /// Değerler başlangıç önerisi: gamepad ile ayarlanır (docs/systems/collectibles.md, ui-motion.md).
    /// </summary>
    [AddComponentMenu("RockTheGame/UI/Hud Counter Panel")]
    public sealed class HudCounterPanel : MonoBehaviour
    {
        private sealed class KeySlot
        {
            public Image Icon;
            public RectTransform Rect;
            public UiSpring Pop = new UiSpring(300f, 12f);
            public UiSpring Enter = new UiSpring(260f, 20f);
            public float EnterWait;
            public bool Collected;
            public bool Active;
        }

        public const int MaxKeys = 3;
        /// <summary>Ananas çeyrek dairesinin yarıçapı (sanat pikseli): yalnızca ikon ve iki haneli sayı sığar (48'de "12" kenara değdi). Başlangıç önerisi (78 idi).</summary>
        public const float CircleRadius = 52f;
        /// <summary>Anahtar halkasının dış yarıçapı (sanat pikseli): ananas çeyrek dairesini saran dış halka.</summary>
        public const float RingOuterRadius = 78f;
        private const float KeyRingRadius = (CircleRadius + RingOuterRadius) * 0.5f;
        private const float KeyCenterDegrees = 45f;
        private const float KeyStepDegrees = 25f;
        /// <summary>Karakter şeklin altındayken şeklin saydamlığı (başlangıç önerisi, gamepad ile ayarlanır).</summary>
        public const float FadedAlpha = 0.3f;
        /// <summary>Karakter sprite'ının yarısı kadar pay (sanat pikseli): kenara değerken de soluklaşır.</summary>
        public const float FadeMargin = 14f;
        private const float FadeSpeed = 6f; // saydamlık/sn: 1 → 0,3 yaklaşık 0,12 sn
        private const float IconArtPixels = 24f;
        private static readonly Vector2 PineappleCenter = new Vector2(14f, 14f);
        private const float CountLeft = 25f;
        private const float CountCenterY = 14f;
        private const float Unit = UiPixelScale.UnitsPerArtPixel;

        /// <summary>
        /// <paramref name="index"/>. anahtarın merkezi: sanat pikseli, sol-üst köşeden, y aşağı. <paramref name="count"/> anahtar
        /// dış halkanın ortasında, köşegen (45°) etrafında simetrik dizilir (tek anahtar tam köşegende).
        /// </summary>
        public static Vector2 KeyCenter(int index, int count)
        {
            float radians = (KeyCenterDegrees + (index - (count - 1) * 0.5f) * KeyStepDegrees) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * KeyRingRadius;
        }

        /// <summary>
        /// Karakterin ekran noktası köşeden <paramref name="radiusArtPixels"/> + <see cref="FadeMargin"/> içinde mi? Köşe sol-üsttür;
        /// <paramref name="screenPoint"/> Unity ekran koordinatı (sol-alt orijin), <paramref name="artScale"/> ekran pikseli / sanat pikseli.
        /// </summary>
        public static bool IsBehind(Vector2 screenPoint, float screenHeight, float artScale, float radiusArtPixels)
        {
            float dy = screenHeight - screenPoint.y;
            float reach = (radiusArtPixels + FadeMargin) * artScale;
            return screenPoint.x * screenPoint.x + dy * dy <= reach * reach;
        }

        [Header("Veri")]
        [SerializeField] private CollectibleInventorySO inventory;

        [Header("İkonlar (Assets/UI/Icons)")]
        [SerializeField] private Sprite pineappleIcon;
        [SerializeField] private Sprite keyIcon;
        [Tooltip("Henüz toplanmamış anahtar: çıplak beton hâli.")]
        [SerializeField] private Sprite keyMissingIcon;

        [Header("Görünürlük ve hareket (başlangıç önerisi)")]
        [Tooltip("Bir şey toplandıktan sonra sayacın ekranda kaldığı süre (sn, unscaled).")]
        [SerializeField, Range(0.5f, 6f)] private float showSeconds = 2.5f;
        [Tooltip("Anahtarlı odaya girince anahtar halkasının açık kaldığı süre (sn, unscaled): oyuncu bu odada anahtar olduğunu okusun.")]
        [SerializeField, Range(0.5f, 6f)] private float keysIntroSeconds = 3f;
        [SerializeField, Range(0f, 0.15f)] private float keyEnterStagger = 0.06f;

        private readonly KeySlot[] _slots = new KeySlot[MaxKeys];
        private readonly List<Collectible.Collectible> _keys = new List<Collectible.Collectible>();
        private readonly Dictionary<Room, UnityAction> _roomListeners = new Dictionary<Room, UnityAction>();
        // Görünürlük: 0 = köşede gizli, 1 = tam; tüm sayaç köşeden büyür/küçülür.
        private readonly UiSpring _circle = new UiSpring(260f, 18f);
        // Anahtar şekli ayrı: oda girişinde ve anahtar toplanınca açılır, süre sonunda çekilir (0 = köşede gizli, 1 = tam).
        private readonly UiSpring _keysPanel = new UiSpring(260f, 18f);
        private readonly UiSpring _countPop = new UiSpring(320f, 11f);
        private readonly UiSpring _iconPop = new UiSpring(320f, 11f);

        private UiThemeSO _theme;
        private GameObject _canvasRoot;
        private RectTransform _content;
        private RectTransform _keysContent;
        private CanvasGroup _contentGroup, _keysGroup;
        private float _contentAlpha = 1f, _keysAlpha = 1f;
        private RectTransform _circleRect;
        private RectTransform _pineappleRect, _countRect;
        private TMP_Text _count;
        private PlayerHealth _health;
        private int _lastCount = -1;
        private int _refreshAtFrame = -1;
        private float _hideAt;
        private float _keysHideAt;

        private void Awake()
        {
            _theme = UiThemeSO.Load();
            BuildUi();
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnCoinsChanged += OnCoinsChanged;
                SetCount(inventory.TotalCoins, animate: false);
            }

            foreach (var room in FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var r = room;
                UnityAction handler = () => SetRoom(r);
                room.OnRoomEntered.AddListener(handler);
                _roomListeners[room] = handler;
            }

            // Anahtar şekli oda girişinde (SetRoom) açılır: önce gizli başlat.
            _keysPanel.Value = 0f;
            _keysPanel.Velocity = 0f;
            _keysPanel.Target = 0f;

            // Start yalnızca bir kez çalışır: abonelik her açılışta yenilenir (panel kapatılıp açılabilir).
            SubscribeToHealth();
            EnterRoomContainingPlayer();

            _circle.Value = 0f;
            _circle.Velocity = 0f;
            _circle.Target = 0f;
        }

        // OnEnable'da oyuncu henüz yoksa bir kez daha dene.
        private void Start()
        {
            if (_health == null && SubscribeToHealth()) EnterRoomContainingPlayer();
        }

        private bool SubscribeToHealth()
        {
            if (_health == null) _health = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            if (_health == null) return false;
            _health.OnDeath -= OnPlayerDeath;
            _health.OnDeath += OnPlayerDeath;
            return true;
        }

        /// <summary>
        /// İlk odanın OnRoomEntered'ı HUD abone olmadan gelmiş olabilir: oyuncunun içinde durduğu oda ilk oda sayılır
        /// (LocalPlayerBroadcaster.EnterRoomContainingPlayer ile aynı mantık). Sonra gelen gerçek oda girişi bunu ezer.
        /// </summary>
        private void EnterRoomContainingPlayer()
        {
            if (_health == null) return;
            Vector2 position = _health.transform.position;
            foreach (var room in _roomListeners.Keys)
            {
                if (room.RoomBounds == null || !room.RoomBounds.OverlapPoint(position)) continue;
                SetRoom(room);
                return;
            }
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.OnCoinsChanged -= OnCoinsChanged;
            foreach (var pair in _roomListeners)
                if (pair.Key != null) pair.Key.OnRoomEntered.RemoveListener(pair.Value);
            _roomListeners.Clear();
            UnsubscribeKeys();
            if (_health != null) _health.OnDeath -= OnPlayerDeath;
        }

        // ---- veri ----

        private void OnCoinsChanged(int total) => SetCount(total, animate: true);

        private void SetCount(int total, bool animate)
        {
            if (total == _lastCount) return;
            bool increased = _lastCount >= 0 && total > _lastCount;
            _lastCount = total;
            _count.text = total.ToString("D2");
            if (animate && increased)
            {
                _countPop.Kick(3f);
                _iconPop.Kick(3.5f);
                Show();
            }
        }

        private void SetRoom(Room room)
        {
            UnsubscribeKeys();
            _keys.Clear();
            foreach (var manager in room.GetComponentsInChildren<RoomKeyManager>(true))
            {
                foreach (var key in manager.Keys)
                    if (key != null && !_keys.Contains(key)) _keys.Add(key);
            }

            if (_keys.Count > MaxKeys)
            {
                Debug.LogWarning($"[HudCounter] {room.name} odasında {_keys.Count} anahtar var, HUD en çok {MaxKeys} gösterir.", this);
            }

            foreach (var key in _keys) key.OnCollected += OnKeyCollected;
            RefreshKeys(animateCollected: false, replayEnter: true);

            // Odada toplanacak anahtar varsa halka bir süre açılıp bunu bildirir; hepsi toplanmışsa ya da anahtar yoksa gizli kalır.
            bool anyMissing = false;
            foreach (var key in _keys)
                if (key != null && !key.IsCollected) anyMissing = true;
            if (anyMissing) ShowKeys(keysIntroSeconds);
            else _keysPanel.Target = 0f;
        }

        private void UnsubscribeKeys()
        {
            foreach (var key in _keys)
                if (key != null) key.OnCollected -= OnKeyCollected;
        }

        private void OnKeyCollected(Collectible.Collectible key)
        {
            // Halkayı açar (zaten açıksa süreyi uzatır); ananas sayacını tetiklemez.
            RefreshKeys(animateCollected: true, replayEnter: false);
            ShowKeys(showSeconds);
        }

        /// <summary>Anahtar halkasını açar; kapanış zamanı hiçbir zaman öne çekilmez (giriş bildirimi toplamayla kesilmesin).</summary>
        private void ShowKeys(float seconds)
        {
            _keysHideAt = Mathf.Max(_keysHideAt, Time.unscaledTime + seconds);
            _keysPanel.Target = 1f;
        }

        /// <summary>Sayacı gösterir ve <c>showSeconds</c> sonra çekilecek şekilde zamanlar.</summary>
        private void Show()
        {
            _hideAt = Time.unscaledTime + showSeconds;
            _circle.Target = 1f;
        }

        /// <summary>Ölünce toplananlar sessizce geri gelir; sonuç bir kare sonra okunur.</summary>
        private void OnPlayerDeath() => _refreshAtFrame = Time.frameCount + 1;

        private void RefreshKeys(bool animateCollected, bool replayEnter)
        {
            int shown = Mathf.Min(_keys.Count, MaxKeys);

            for (int i = 0; i < MaxKeys; i++)
            {
                var slot = _slots[i];
                bool active = i < shown;
                bool collected = active && _keys[i] != null && _keys[i].IsCollected;

                if (active && (replayEnter || !slot.Active))
                {
                    slot.Enter.Value = 1f;
                    slot.Enter.Velocity = 0f;
                    slot.Enter.Target = 1f;
                    slot.EnterWait = 0.12f + i * keyEnterStagger;
                }

                if (active && animateCollected && collected && !slot.Collected) slot.Pop.Kick(5f); // boya vurur, çakılır

                if (active) slot.Rect.anchoredPosition = KeyCenterUnits(i, shown);

                slot.Active = active;
                slot.Collected = collected;
                slot.Icon.sprite = collected ? keyIcon : keyMissingIcon;
                slot.Icon.gameObject.SetActive(active);
            }
        }

        // ---- hareket ----

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (_refreshAtFrame >= 0 && Time.frameCount >= _refreshAtFrame)
            {
                _refreshAtFrame = -1;
                if (inventory != null) SetCount(inventory.TotalCoins, animate: false);
                RefreshKeys(animateCollected: false, replayEnter: false);
            }

            if (_circle.Target > 0f && Time.unscaledTime >= _hideAt) _circle.Target = 0f;
            _circle.Step(dt);
            float shown = Mathf.Max(0f, _circle.Value);
            _content.gameObject.SetActive(shown > 0.002f || _circle.Target > 0f);
            _content.localScale = Vector3.one * Mathf.Max(0.0001f, shown);

            if (_keysPanel.Target > 0f && Time.unscaledTime >= _keysHideAt) _keysPanel.Target = 0f;
            _keysPanel.Step(dt);
            float keysShown = Mathf.Max(0f, _keysPanel.Value);
            UpdateFade(dt);
            _keysContent.gameObject.SetActive(keysShown > 0.002f || _keysPanel.Target > 0f);
            _keysContent.localScale = Vector3.one * Mathf.Max(0.0001f, keysShown);

            _countPop.Step(dt);
            _iconPop.Step(dt);
            _countRect.localScale = Vector3.one * (1f + Mathf.Clamp(_countPop.Value, -0.3f, 0.5f));
            _pineappleRect.localScale = Vector3.one * (1f + Mathf.Clamp(_iconPop.Value, -0.3f, 0.5f));

            for (int i = 0; i < MaxKeys; i++)
            {
                var s = _slots[i];
                if (!s.Active) continue;

                if (s.EnterWait > 0f)
                {
                    s.EnterWait -= dt;
                    if (s.EnterWait <= 0f) s.Enter.Target = 0f;
                }

                s.Pop.Step(dt);
                s.Enter.Step(dt);

                float enter = s.Enter.Value; // 1 = girişin başı, 0 = oturdu
                float scale = (1f + Mathf.Clamp(s.Pop.Value, -0.3f, 0.5f)) * (1f - 0.6f * Mathf.Clamp01(enter));
                s.Rect.localScale = Vector3.one * Mathf.Max(0.0001f, scale);
                var c = s.Icon.color;
                c.a = 1f - Mathf.Clamp01(enter);
                s.Icon.color = c;
            }
        }

        /// <summary>Karakter bir şeklin altına girince o şekil soluklaşır (örn. oda 11'e düşerken halka); çıkınca geri döner.</summary>
        private void UpdateFade(float dt)
        {
            bool behindCircle = false, behindRing = false;
            var cam = Camera.main;
            if (_health != null && cam != null)
            {
                Vector3 s = cam.WorldToScreenPoint(_health.transform.position);
                if (s.z > 0f)
                {
                    float art = UiPixelScale.ArtScaleFor(Screen.width, Screen.height);
                    var point = new Vector2(s.x, s.y);
                    behindCircle = IsBehind(point, Screen.height, art, CircleRadius);
                    behindRing = IsBehind(point, Screen.height, art, RingOuterRadius);
                }
            }

            _contentAlpha = Mathf.MoveTowards(_contentAlpha, behindCircle ? FadedAlpha : 1f, FadeSpeed * dt);
            _keysAlpha = Mathf.MoveTowards(_keysAlpha, behindRing ? FadedAlpha : 1f, FadeSpeed * dt);
            _contentGroup.alpha = _contentAlpha;
            _keysGroup.alpha = _keysAlpha;
        }

        // ---- arayüz ----

        private void BuildUi()
        {
            _canvasRoot = new GameObject("HudCounterCanvas", typeof(RectTransform));
            _canvasRoot.transform.SetParent(transform, false);
            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // pause (200) üstünde değil
            _canvasRoot.AddComponent<CanvasScaler>();
            _canvasRoot.AddComponent<UiPixelScale>();

            float balance = _theme.BalanceFor(UiScreen.Hud);
            float border = _theme.BorderWidthFor(UiScreen.Hud) * Unit;
            float shadow = UiBalance.ShadowOffset(4f, balance) * Unit;
            float radius = CircleRadius * Unit;

            // Tüm sayaç tek kapta: kap köşeden büyür/küçülür (pivot köşe).
            _content = NewRect("Content", _canvasRoot.transform);
            _content.anchorMin = _content.anchorMax = _content.pivot = new Vector2(0f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = Vector2.zero;
            _contentGroup = AddFadeGroup(_content);

            // Çeyrek daire: köşede merkezli, sağa-aşağı bakar (saat gibi 90°'den 180°'ye).
            _circleRect = NewRect("Quarter", _content);
            Corner(_circleRect, Vector2.zero, radius * 2f);
            AddSlice("Shadow", _circleRect, radius + border, new Vector2(shadow, -shadow), _theme.Ink);
            AddSlice("Outline", _circleRect, radius + border, Vector2.zero, _theme.Ink);
            AddSlice("Fill", _circleRect, radius, Vector2.zero, _theme.Blue);

            _pineappleRect = AddIcon("Pineapple", _content, PineappleCenter, pineappleIcon).rectTransform;

            var countGo = new GameObject("Count", typeof(RectTransform));
            countGo.transform.SetParent(_content, false);
            var text = countGo.AddComponent<TextMeshProUGUI>();
            if (_theme.TitleFont != null) text.font = _theme.TitleFont;
            text.fontSize = 17f * Unit;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.color = _theme.Paper;
            text.text = "00";
            _count = text;
            _countRect = text.rectTransform;
            _countRect.anchorMin = _countRect.anchorMax = new Vector2(0f, 1f);
            _countRect.pivot = new Vector2(0f, 0.5f);
            _countRect.sizeDelta = new Vector2(34f * Unit, 24f * Unit);
            _countRect.anchoredPosition = new Vector2(CountLeft * Unit, -CountCenterY * Unit);

            // Anahtar halkası: ananas çeyrek dairesini saran dış halka. Ayrı kap (pivot köşe): ananastan bağımsız büyür/küçülür,
            // ananasın arkasında çizilir (SetAsFirstSibling).
            _keysContent = NewRect("Keys", _canvasRoot.transform);
            _keysContent.SetAsFirstSibling();
            _keysContent.anchorMin = _keysContent.anchorMax = _keysContent.pivot = new Vector2(0f, 1f);
            _keysContent.anchoredPosition = Vector2.zero;
            _keysContent.sizeDelta = Vector2.zero;
            _keysGroup = AddFadeGroup(_keysContent);

            var keyRing = NewRect("Ring", _keysContent);
            Corner(keyRing, Vector2.zero, RingOuterRadius * Unit * 2f);
            float inner = CircleRadius * Unit;
            float outer = RingOuterRadius * Unit;
            float edgeRatio = (inner - border) / (outer + border);
            AddSlice("Shadow", keyRing, outer + border, new Vector2(shadow, -shadow), _theme.Ink, edgeRatio);
            AddSlice("Outline", keyRing, outer + border, Vector2.zero, _theme.Ink, edgeRatio);
            AddSlice("Fill", keyRing, outer, Vector2.zero, _theme.BlueDark, inner / outer);

            for (int i = 0; i < MaxKeys; i++)
            {
                var icon = AddIcon("Key " + i, _keysContent, KeyCenter(i, MaxKeys), keyMissingIcon);
                _slots[i] = new KeySlot { Icon = icon, Rect = icon.rectTransform };
                icon.gameObject.SetActive(false);
            }
        }

        private static Vector2 KeyCenterUnits(int index, int count)
        {
            Vector2 c = KeyCenter(index, count);
            return new Vector2(c.x, -c.y) * Unit;
        }

        /// <summary>Sağ-alta bakan çeyrek daire (3 yönünden 6 yönüne). <paramref name="innerRatio"/> &gt; 0 ise halka.</summary>
        private static void AddSlice(string name, Transform parent, float radius, Vector2 offset, Color color, float innerRatio = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var arc = go.AddComponent<UiArc>();
            var rt = arc.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(radius * 2f, radius * 2f);
            arc.StartDegrees = 90f;
            arc.FillAmount = 0.25f;
            arc.InnerRadiusRatio = innerRatio;
            arc.raycastTarget = false;
            arc.color = color;
        }

        private Image AddIcon(string name, Transform parent, Vector2 centerArtPixels, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(IconArtPixels * Unit, IconArtPixels * Unit);
            rt.anchoredPosition = new Vector2(centerArtPixels.x, -centerArtPixels.y) * Unit;
            return image;
        }

        private static CanvasGroup AddFadeGroup(RectTransform rect)
        {
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            return group;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Corner(RectTransform rt, Vector2 position, float size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(size, size);
        }
    }
}
