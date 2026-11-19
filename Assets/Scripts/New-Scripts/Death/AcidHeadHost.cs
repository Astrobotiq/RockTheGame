using New_Scripts.LevelChange;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit tehlikesine eklenir: oyuncu bu asitte ölünce kafası asitte kalır (<see cref="AcidHeadPool"/>).
    /// Pool: sabit havuz (Acid), kafa yüzeyden dibe doğru batıp orta derinlikte durur.
    /// Blob: hareket eden dolu asit (MovingDeath vb.), kafa asidin içine yapışıp onunla gider.
    /// Kafalar oyuncu odadan çıkınca (<see cref="Room.OnRoomExited"/>) ve asit devre dışı kalınca silinir.
    /// </summary>
    [RequireComponent(typeof(DeathZone))]
    [RequireComponent(typeof(Collider2D))]
    public class AcidHeadHost : MonoBehaviour
    {
        public enum HostMode { Pool, Blob }

        private static readonly int AcidLevelId = Shader.PropertyToID("_AcidLevel");

        [SerializeField] private HostMode mode = HostMode.Pool;
        [SerializeField] private DeathBurstSettingsSO settings;

        private DeathZone _zone;
        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private Room _room;

        public HostMode Mode => mode;
        public SpriteRenderer Renderer => _renderer;
        public Bounds Bounds => _collider.bounds;
        public Vector2 Center => _collider.bounds.center;

        private void Awake()
        {
            _zone = GetComponent<DeathZone>();
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _zone.Killing += OnKilling;
            _room = GetComponentInParent<Room>(true);
            if (_room != null) _room.OnRoomExited.AddListener(ClearHeads);
        }

        private void OnDisable()
        {
            _zone.Killing -= OnKilling;
            if (_room != null) _room.OnRoomExited.RemoveListener(ClearHeads);
            ClearHeads();
        }

        /// <summary>Bu asidin tüm kafalarını hemen siler.</summary>
        public void ClearHeads()
        {
            AcidHeadPool.Release(this);
        }

        /// <summary>Pool modunda yüzey yüksekliği (dünya y): asit seviyesi kadar yukarıda.</summary>
        public float SurfaceY()
        {
            Bounds b = _collider.bounds;
            float level = 1f;
            if (_renderer != null && _renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(AcidLevelId))
                level = Mathf.Clamp01(_renderer.sharedMaterial.GetFloat(AcidLevelId));
            return b.min.y + level * b.size.y;
        }

        private void OnKilling(Collider2D other)
        {
            if (settings == null || settings.headSprite == null) return;

            Vector2 position = other.attachedRigidbody != null ? other.attachedRigidbody.position : (Vector2)other.bounds.center;
            DeathBurst.Claim(mode == HostMode.Pool ? _renderer : null);
            AcidHeadPool.Spawn(settings, this, position + Vector2.up * settings.headStartOffsetY);
        }
    }
}
