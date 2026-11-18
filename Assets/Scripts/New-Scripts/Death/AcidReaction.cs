using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Oyuncu asite değdiğinde asidin tepkisi (docs/asit-tepki-plani.md).
    /// DeathZone.Killing'i dinler (Kill()'den önce, hız henüz sıfırlanmamışken), temas noktasını asidin
    /// piksel ızgarasına çevirir ve tek darbe yuvasını (_AcidImpact) MaterialPropertyBlock ile shader'a yazar.
    /// Darbenin yaşını shader'ın _Time'ı değil bu bileşen sayar (URP'de _Time sahne yüklemede sıfırlanmaz).
    /// </summary>
    [RequireComponent(typeof(DeathZone))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AcidHazard))]
    public class AcidReaction : MonoBehaviour, IResettable
    {
        private static readonly int AcidImpactId = Shader.PropertyToID("_AcidImpact");
        private static readonly int ImpactDurationId = Shader.PropertyToID("_ImpactDuration");
        private static readonly int AcidLevelId = Shader.PropertyToID("_AcidLevel");

        private const float DefaultImpactDuration = 0.6f;
        private const float MinSpeed = 4f;
        private const float MaxSpeed = 40f;
        private const float StrengthFloor = 0.35f;

        [Tooltip("Yüzeyi olan asit (PixelArtAcid). Akan asitte (FlowingPixelAcid) kapalı: temas yüzeye kilitlenmez.")]
        [SerializeField] private bool hasSurface = true;

        private DeathZone _deathZone;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private AcidHazard _hazard;
        private MaterialPropertyBlock _block;

        private Vector4 _impact;   // (x px, y px, yaş, güç)
        private float _duration = DefaultImpactDuration;

        private void Awake()
        {
            _deathZone = GetComponent<DeathZone>();
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _hazard = GetComponent<AcidHazard>();
            _block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _deathZone.Killing += OnKilling;
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Register(this);
            ClearImpact();
        }

        private void OnDisable()
        {
            _deathZone.Killing -= OnKilling;
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Unregister(this);
        }

        /// <summary>Ölüm sıfırlaması (iris kapalıyken): yuva temizlenir.</summary>
        public void ResetToDefault()
        {
            ClearImpact();
        }

        private void OnKilling(Collider2D other)
        {
            Vector2 position = other.attachedRigidbody != null ? other.attachedRigidbody.position : (Vector2)other.bounds.center;
            Vector2 velocity = other.TryGetComponent(out PlayerController pc) ? pc.Velocity : Vector2.zero;

            // Trigger fizik adımından sonra gelir; bir adım geri sarıp girişten önceki noktadan en yakın sınırı bul.
            Vector2 previous = AcidImpactMath.BacktrackPoint(position, velocity, Time.fixedDeltaTime);
            Vector2 world = _collider.OverlapPoint(previous) ? position : _collider.ClosestPoint(previous);

            float strength = AcidImpactMath.Strength(velocity.magnitude, MinSpeed, MaxSpeed, StrengthFloor);
            StartImpact(transform.InverseTransformPoint(world), strength, 0f);
        }

        private void StartImpact(Vector2 localPoint, float strength, float age)
        {
            Vector2 pixelScale = _hazard.PixelScale;
            Vector2 pixel = AcidImpactMath.LocalToPixel(localPoint, SpriteLocalRect(), pixelScale);
            pixel = AcidImpactMath.SurfaceContact(pixel, AcidLevel(), pixelScale.y, hasSurface);

            _duration = ImpactDuration();
            _impact = new Vector4(pixel.x, pixel.y, age, strength);
            WriteImpact();
        }

        private void Update()
        {
            // Boştayken iş yok. enabled kapatılmaz: OnDisable aboneliği ve reset kaydını keserdi.
            if (_impact.w <= 0f) return;

            _impact.z += Time.deltaTime;
            if (!AcidImpactMath.ImpactLive(_impact.z, _duration, _impact.w))
            {
                ClearImpact();
                return;
            }
            WriteImpact();
        }

        private void ClearImpact()
        {
            _impact = Vector4.zero;
            if (_renderer != null) WriteImpact();
        }

        private void WriteImpact()
        {
            // AcidHazard aynı renderer'a _PixelScale yazıyor: önce oku, sonra yaz.
            _renderer.GetPropertyBlock(_block);
            _block.SetVector(AcidImpactId, _impact);
            _renderer.SetPropertyBlock(_block);
        }

        private Rect SpriteLocalRect()
        {
            if (_renderer.sprite == null) return new Rect(-0.5f, -0.5f, 1f, 1f);
            Bounds b = _renderer.sprite.bounds;
            return new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
        }

        private float AcidLevel()
        {
            Material m = _renderer.sharedMaterial;
            return m != null && m.HasProperty(AcidLevelId) ? m.GetFloat(AcidLevelId) : 1f;
        }

        private float ImpactDuration()
        {
            Material m = _renderer.sharedMaterial;
            return m != null && m.HasProperty(ImpactDurationId) ? m.GetFloat(ImpactDurationId) : DefaultImpactDuration;
        }

        /// <summary>
        /// Doğrulama için (yalnızca Play Mode): yerel noktaya, verilen yaşta tam güçte bir darbe yazar.
        /// Time.timeScale = 0 iken yaş ilerlemez, kare deterministik olur.
        /// </summary>
        public void DebugImpact(Vector2 localPoint, float age)
        {
            if (!Application.isPlaying) return;
            StartImpact(localPoint, 1f, age);
        }

        [ContextMenu("Test Impact (center top)")]
        private void TestImpact()
        {
            DebugImpact(new Vector2(0f, 0.4f), 0f);
        }

        private void OnValidate()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
            if (sr.drawMode != SpriteDrawMode.Simple)
                Debug.LogWarning($"{name}: AcidReaction yalnızca Simple draw mode'u destekler (asit shader'ları Tiled'da bozuk).", this);
            if (sr.flipX || sr.flipY)
                Debug.LogWarning($"{name}: asit shader'ları _Flip uygulamıyor; flipX/flipY kapalı olmalı.", this);
        }
    }
}
