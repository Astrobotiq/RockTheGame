using System.Collections.Generic;
using New_Scripts.LevelChange;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;
using UnityEngine.Events;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit dışındaki ölümlerde (diken, hareketli tuzak) kalan kafaların ortak havuzu. Kafa fiziksel bir nesnedir:
    /// düşer, zeminde (<c>Ground</c>) ve diğer kafalarda (<c>DeadHead</c> katmanı) durur, seker, yuvarlanır (sprite dik kalır).
    /// Oyuncuyla ÇARPIŞMAZ, iç içe geçebilir: oyuncunun zemin sorguları <c>Ground</c> katmanına baktığı için kafa onun
    /// için yoktur, üstünde duramaz ve dikenlerin üstünü örtemez (kafa <c>Death</c> katmanını da görmez, dikenden geçip
    /// zemine düşer). Oyuncunun collider'ı kafanın collider'ına değince kafa biraz itilir; kafalar birbirini iter.
    /// Tüm oyunda en fazla <c>maxGroundHeads</c> kafa. Havuz taşarsa en eski kafa iris kapalıyken silinir
    /// (<see cref="IResettable"/>: <c>LevelResetManager.ResetAll</c> iris kapandıktan sonra çalışır), yani görünmez.
    /// Kafalar oyuncu odadan çıkınca (<see cref="Room.OnRoomExited"/>) da silinir.
    /// İlk <see cref="Spawn"/> çağrısında sahnede kendiliğinden oluşur; sahneyle birlikte yok olur.
    /// </summary>
    public class GroundHeadPool : MonoBehaviour, IResettable
    {
        private const int SpareItems = 3;
        private const float FallKillMargin = 40f;

        private class Item
        {
            public GameObject Root;
            public Transform View;
            public SpriteRenderer Renderer;
            public Rigidbody2D Body;
            public CircleCollider2D Collider;
            public bool Active;
            public float Born;
            public float ShowAt;
            public float ReleaseAt;
            public bool Released;
            public Vector2 Launch;
            public float NextPushAt;
            public Room Room;
            public float SquashAt = -10f;
            public float SquashAmount;
            public bool SquashVertical = true;
        }

        private static GroundHeadPool _instance;

        private readonly List<Item> _items = new List<Item>();
        private readonly Dictionary<Room, UnityAction> _roomListeners = new Dictionary<Room, UnityAction>();
        private DeathBurstSettingsSO _settings;
        private PlayerController _player;
        private Collider2D _playerCollider;
        private PhysicsMaterial2D _material;
        private Room[] _rooms;

        /// <summary>Kafa bırakır. position: kafanın başlangıcı, launch: fırlatma hızı (hitstop bitince uygulanır).</summary>
        public static void Spawn(DeathBurstSettingsSO settings, Vector2 position, Vector2 launch,
            PlayerController player, int sortingLayerId, int sortingOrder)
        {
            if (settings == null || settings.headSprite == null) return;
            if (_instance == null)
            {
                var go = new GameObject("GroundHeadPool");
                _instance = go.AddComponent<GroundHeadPool>();
            }
            _instance.DoSpawn(settings, position, launch, player, sortingLayerId, sortingOrder);
        }

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Unregister(this);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if (_material != null) Destroy(_material);
        }

        /// <summary>Ölüm sıfırlaması (iris kapalıyken): havuz taştıysa en eski kafalar silinir.</summary>
        public void ResetToDefault()
        {
            if (_settings == null) return;
            while (ActiveCount() > _settings.maxGroundHeads)
            {
                Item oldest = Oldest();
                if (oldest == null) break;
                Deactivate(oldest);
            }
        }

        private int ActiveCount()
        {
            int n = 0;
            foreach (var it in _items) if (it.Active) n++;
            return n;
        }

        private Item Oldest()
        {
            Item oldest = null;
            foreach (var it in _items)
                if (it.Active && (oldest == null || it.Born < oldest.Born)) oldest = it;
            return oldest;
        }

        private void DoSpawn(DeathBurstSettingsSO s, Vector2 position, Vector2 launch, PlayerController player,
            int sortingLayerId, int sortingOrder)
        {
            _settings = s;
            if (player != null && player != _player)
            {
                _player = player;
                _playerCollider = player.GetComponent<Collider2D>();
            }

            Item item = _items.Find(i => !i.Active);
            if (item == null)
            {
                if (_items.Count < s.maxGroundHeads + SpareItems) item = NewItem();
                else item = Oldest(); // ardışık hızlı ölümler: en eskisini devral
            }

            item.Active = true;
            item.Released = false;
            item.Born = Time.time;
            item.ShowAt = Time.time + s.flashFrames / 60f;
            item.ReleaseAt = Time.time + s.hitstopSeconds;
            item.Launch = launch;
            item.NextPushAt = 0f;
            item.SquashAmount = 0f;
            item.SquashAt = -10f;
            item.Room = FindRoom(position);
            WatchRoom(item.Room);

            item.Body.simulated = false;
            item.Body.linearVelocity = Vector2.zero;
            item.Root.transform.position = position;
            item.Collider.radius = Mathf.Max(0.05f, s.headSprite.bounds.size.x * 0.5f * 0.95f);
            item.Collider.sharedMaterial = Material(s);
            item.Collider.includeLayers = s.headCollisionMask;
            item.Collider.excludeLayers = ~s.headCollisionMask;
            item.Body.gravityScale = s.headGravityScale;
            item.Body.linearDamping = s.headDamping;
            item.Body.angularDamping = s.headAngularDamping;
            item.Body.angularVelocity = 0f;
            item.Root.transform.rotation = Quaternion.identity;
            item.Renderer.sprite = s.headSprite;
            item.Renderer.sortingLayerID = sortingLayerId;
            item.Renderer.sortingOrder = sortingOrder - 1; // oyuncunun arkasında
            item.Renderer.enabled = false;
            item.View.SetPositionAndRotation(DeathBurstMath.Snap(position, s.pixelsPerUnit), Quaternion.identity);
            item.Root.SetActive(true);
        }

        private PhysicsMaterial2D Material(DeathBurstSettingsSO s)
        {
            if (_material == null) _material = new PhysicsMaterial2D("DeadHead");
            _material.friction = s.headFriction;
            _material.bounciness = s.headBounciness;
            return _material;
        }

        private Item NewItem()
        {
            var root = new GameObject("Head" + _items.Count);
            int headLayer = LayerMask.NameToLayer("DeadHead");
            if (headLayer >= 0) root.layer = headLayer;
            root.transform.SetParent(transform, false);
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var col = root.AddComponent<CircleCollider2D>();
            var hit = root.AddComponent<GroundHeadBody>();

            var view = new GameObject("View").transform;
            view.SetParent(root.transform, false);
            var sr = view.gameObject.AddComponent<SpriteRenderer>();
            sr.enabled = false;

            root.SetActive(false);
            var item = new Item { Root = root, View = view, Renderer = sr, Body = body, Collider = col };
            hit.Hit = collision => OnHit(item, collision);
            _items.Add(item);
            return item;
        }

        private void Deactivate(Item item)
        {
            item.Active = false;
            item.Released = false;
            item.Room = null;
            item.Body.simulated = false;
            item.Renderer.enabled = false;
            item.Root.SetActive(false);
        }

        /// <summary>Çarpma: hıza göre yassılma başlatır (zemin/duvar ekseninde).</summary>
        private void OnHit(Item item, Collision2D collision)
        {
            if (_settings == null || !item.Released) return;
            float speed = collision.relativeVelocity.magnitude;
            if (speed < _settings.headSquashMinSpeed) return;

            Vector2 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector2.up;
            StartSquash(item, Mathf.Min(speed * _settings.headSquashPerSpeed, _settings.headSquashMax),
                Mathf.Abs(normal.y) >= Mathf.Abs(normal.x));
        }

        private static void StartSquash(Item item, float amount, bool vertical)
        {
            // Süren daha büyük bir yassılmayı küçüğüyle ezme
            float current = Mathf.Abs(DeathBurstMath.SquashAmountAt(item.SquashAmount, Time.time - item.SquashAt, 1f, 0f));
            if (amount < current * 0.5f) return;
            item.SquashAmount = amount;
            item.SquashVertical = vertical;
            item.SquashAt = Time.time;
        }

        /// <summary>Hız uzaması + çarpma yassılması; piksele oturtulur, alt kenar sabit kalır.</summary>
        private void ApplyScale(Item it, float now)
        {
            var s = _settings;
            Vector2 stretch = DeathBurstMath.StretchScale(it.Body.linearVelocity, s.headStretchPerSpeed, s.headStretchMax);
            float amount = DeathBurstMath.SquashAmountAt(it.SquashAmount, now - it.SquashAt, s.headSquashFreq, s.headSquashDecay);
            Vector2 squash = DeathBurstMath.SquashScale(amount, it.SquashVertical);
            Vector2 scale = DeathBurstMath.CombineScale(stretch, squash, 0.6f, 1.5f);
            scale = DeathBurstMath.QuantizeScale(scale, Mathf.RoundToInt(s.headSprite.rect.width));

            it.View.localScale = new Vector3(scale.x, scale.y, 1f);

            // Alt kenar zeminde kalsın (yassılınca havaya kalkmasın)
            float halfHeight = s.headSprite.bounds.size.y * 0.5f;
            Vector2 anchor = (Vector2)it.Root.transform.position + Vector2.up * (-halfHeight * (1f - scale.y));
            it.View.SetPositionAndRotation(DeathBurstMath.Snap(anchor, s.pixelsPerUnit), Quaternion.identity);
        }

        // --- Oda ---

        private Room FindRoom(Vector2 position)
        {
            if (_rooms == null) _rooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var room in _rooms)
            {
                if (room != null && room.RoomBounds != null && room.RoomBounds.bounds.Contains(position)) return room;
            }
            return null;
        }

        private void WatchRoom(Room room)
        {
            if (room == null || _roomListeners.ContainsKey(room)) return;
            UnityAction listener = () => ReleaseRoom(room);
            _roomListeners[room] = listener;
            room.OnRoomExited.AddListener(listener);
        }

        private void ReleaseRoom(Room room)
        {
            foreach (var it in _items)
                if (it.Active && it.Room == room) Deactivate(it);
        }

        // --- Her kare ---

        private void Update()
        {
            if (_settings == null) return;
            float now = Time.time;

            foreach (var it in _items)
            {
                if (!it.Active) continue;

                if (!it.Released && now >= it.ReleaseAt)
                {
                    it.Released = true;
                    it.Body.simulated = true;
                    it.Body.linearVelocity = it.Launch;
                }
                if (!it.Renderer.enabled && now >= it.ShowAt) it.Renderer.enabled = true;

                // Collider döner (yuvarlanır) ama sprite dik kalır; squash and stretch burada uygulanır
                ApplyScale(it, now);

                if (it.Released && IsFarBelow(it)) Deactivate(it);
            }
        }

        private void FixedUpdate()
        {
            if (_settings == null || _playerCollider == null || !_playerCollider.isActiveAndEnabled) return;
            float now = Time.time;
            bool dashing = _player != null && _player.CurrentState is DashState;

            foreach (var it in _items)
            {
                if (!it.Active || !it.Released || now < it.NextPushAt) continue;

                ColliderDistance2D d = it.Collider.Distance(_playerCollider);
                if (!d.isOverlapped) continue;

                Vector2 from = _playerCollider.bounds.center;
                Vector2 dir = (Vector2)it.Root.transform.position - from;
                Vector2 velocity = _player != null ? _player.Velocity : Vector2.zero;

                if (dashing)
                {
                    // Dash vuruşu: daha hızlı, dash yönünde ve hafif yukarı eğimli; kafanın hızını ezip değiştirir (yumruk)
                    Vector2 hit = DeathBurstMath.DashPush(velocity, dir, _settings.headDashPushSpeed,
                        _settings.headDashLiftDeg, _settings.headDashPushMaxSpeed);
                    it.Body.linearVelocity = hit;
                    it.NextPushAt = now + _settings.headDashPushInterval;
                    StartSquash(it, _settings.headDashSquash, Mathf.Abs(hit.y) > Mathf.Abs(hit.x));
                    continue;
                }

                Vector2 push = DeathBurstMath.HeadPush(dir, _settings.headPushSpeed, velocity,
                    _settings.headPushInherit, _settings.headPushMaxSpeed);
                it.Body.linearVelocity = DeathBurstMath.AddCapped(it.Body.linearVelocity, push, _settings.headPushMaxSpeed);
                it.NextPushAt = now + _settings.headPushInterval;
                StartSquash(it, _settings.headPushSquash, Mathf.Abs(push.y) > Mathf.Abs(push.x));
            }
        }

        private static bool IsFarBelow(Item it)
        {
            float y = it.Root.transform.position.y;
            if (it.Room != null && it.Room.RoomBounds != null) return y < it.Room.RoomBounds.bounds.min.y - FallKillMargin;
            return y < -10000f;
        }
    }
}
