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
    /// HUD sayacı (mockup): sol-üst köşede mavi çeyrek daire; içinde ananas ikonu ve sayısı, altında bulunulan odanın anahtarları
    /// (silüet ikon: toplanan boyalı, eksik çıplak beton). Overlay canvas'ta çizilir (CRT dışında, keskin) ve
    /// <see cref="UiPixelScale"/> ile tam sayı sanat pikseline sabitlenir.
    /// <list type="bullet">
    /// <item><b>Olay güdümlü:</b> sayaç yalnızca bir şey toplanınca köşeden büyüyerek görünür, <c>showSeconds</c> sonra çekilir
    /// (ekranın köşesini sürekli kapatmasın). Ananas <c>OnCoinsChanged</c>'den, anahtar <c>OnCollected</c> ve oda girişinden okunur.
    /// Ölünce toplananlar sessizce geri döndüğü için <c>OnDeath</c>'ten bir kare sonra yenilenir (görünmeden).</item>
    /// <item>Daire sabit boyda: anahtarsız odada anahtar yuvaları boş kalır.</item>
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
        /// <summary>Çeyrek dairenin tam yarıçapı (sanat pikseli): ananas, yazı ve üç anahtar sığar.</summary>
        public const float CircleRadius = 78f;
        private const float KeyRingRadius = 58f;
        private const float KeyFirstDegrees = 40f;
        private const float KeyStepDegrees = 18f;
        private const float IconArtPixels = 24f;
        private static readonly Vector2 PineappleCenter = new Vector2(18f, 18f);
        private const float CountLeft = 34f;
        private const float CountCenterY = 17f;
        private const float Unit = UiPixelScale.UnitsPerArtPixel;

        /// <summary>
        /// <paramref name="index"/>. anahtarın merkezi: sanat pikseli, sol-üst köşeden, y aşağı. Çeyrek dairenin yayı boyunca
        /// 40°, 58°, 76° (köşeden, sağdan aşağı doğru); sayaç satırının altında kalır.
        /// </summary>
        public static Vector2 KeyCenter(int index)
        {
            float radians = (KeyFirstDegrees + KeyStepDegrees * index) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * KeyRingRadius;
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
        [SerializeField, Range(0f, 0.15f)] private float keyEnterStagger = 0.06f;

        private readonly KeySlot[] _slots = new KeySlot[MaxKeys];
        private readonly List<Collectible.Collectible> _keys = new List<Collectible.Collectible>();
        private readonly Dictionary<Room, UnityAction> _roomListeners = new Dictionary<Room, UnityAction>();
        // Görünürlük: 0 = köşede gizli, 1 = tam; tüm sayaç köşeden büyür/küçülür.
        private readonly UiSpring _circle = new UiSpring(260f, 18f);
        private readonly UiSpring _countPop = new UiSpring(320f, 11f);
        private readonly UiSpring _iconPop = new UiSpring(320f, 11f);

        private UiThemeSO _theme;
        private GameObject _canvasRoot;
        private RectTransform _content;
        private RectTransform _circleRect;
        private RectTransform _pineappleRect, _countRect;
        private TMP_Text _count;
        private PlayerHealth _health;
        private int _lastCount = -1;
        private int _refreshAtFrame = -1;
        private float _hideAt;

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
        }

        private void UnsubscribeKeys()
        {
            foreach (var key in _keys)
                if (key != null) key.OnCollected -= OnKeyCollected;
        }

        private void OnKeyCollected(Collectible.Collectible key)
        {
            RefreshKeys(animateCollected: true, replayEnter: false);
            Show();
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

            // Çeyrek daire: köşede merkezli, sağa-aşağı bakar (saat gibi 90°'den 180°'ye).
            _circleRect = NewRect("Quarter", _content);
            Corner(_circleRect, Vector2.zero, radius * 2f);
            AddQuarter("Shadow", _circleRect, radius + border, new Vector2(shadow, -shadow), _theme.Ink);
            AddQuarter("Outline", _circleRect, radius + border, Vector2.zero, _theme.Ink);
            AddQuarter("Fill", _circleRect, radius, Vector2.zero, _theme.Blue);

            _pineappleRect = AddIcon("Pineapple", PineappleCenter, pineappleIcon).rectTransform;

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

            for (int i = 0; i < MaxKeys; i++)
            {
                var icon = AddIcon("Key " + i, KeyCenter(i), keyMissingIcon);
                _slots[i] = new KeySlot { Icon = icon, Rect = icon.rectTransform };
                icon.gameObject.SetActive(false);
            }
        }

        private static void AddQuarter(string name, Transform parent, float radius, Vector2 offset, Color color)
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
            arc.raycastTarget = false;
            arc.color = color;
        }

        private Image AddIcon(string name, Vector2 centerArtPixels, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(_content, false);
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
