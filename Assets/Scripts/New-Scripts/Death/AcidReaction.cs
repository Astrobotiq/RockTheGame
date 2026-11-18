using New_Scripts.Audio;
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

        [SerializeField] private AcidReactionSettingsSO settings;
        [Tooltip("Damla parçacık sistemi (FX_AcidImpact). İlk temasta bu asidin child'ı olarak bir kez oluşturulur.")]
        [SerializeField] private ParticleSystem impactFxPrefab;
        [Tooltip("Yüzeyi olan asit (PixelArtAcid). Akan asitte (FlowingPixelAcid) kapalı: temas yüzeye kilitlenmez.")]
        [SerializeField] private bool hasSurface = true;
        [Tooltip("SFX-Play kanalı. Asit sesleri 2D çalar (kamera uzak, 3D neredeyse duyulmaz).")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;

        private DeathZone _deathZone;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private AcidHazard _hazard;
        private MaterialPropertyBlock _block;
        private ParticleSystem _fx;

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
            _deathZone.Killed += OnKilled;
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Register(this);
            ClearImpact();
            // Oda uyanınca donmuş damla kalmasın
            if (_fx != null) _fx.Clear();
        }

        private void OnDisable()
        {
            _deathZone.Killing -= OnKilling;
            _deathZone.Killed -= OnKilled;
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Unregister(this);
        }

        /// <summary>Ölüm sıfırlaması (iris kapalıyken): yuva ve damlalar temizlenir.</summary>
        public void ResetToDefault()
        {
            ClearImpact();
            if (_fx != null) _fx.Clear();
        }

        private void OnKilling(Collider2D other)
        {
            Vector2 position = other.attachedRigidbody != null ? other.attachedRigidbody.position : (Vector2)other.bounds.center;
            Vector2 velocity = other.TryGetComponent(out PlayerController pc) ? pc.Velocity : Vector2.zero;

            // Trigger fizik adımından sonra gelir; bir adım geri sarıp girişten önceki noktadan en yakın sınırı bul.
            Vector2 previous = AcidImpactMath.BacktrackPoint(position, velocity, Time.fixedDeltaTime);
            Vector2 world = _collider.OverlapPoint(previous) ? position : _collider.ClosestPoint(previous);

            float strength = settings != null
                ? AcidImpactMath.Strength(velocity.magnitude, settings.minSpeed, settings.maxSpeed, settings.strengthFloor)
                : 1f;
            TriggerImpact(transform.InverseTransformPoint(world), strength, 0f, velocity);
        }

        /// <summary>
        /// Kill()'den sonra: asit titreşimi genel Death titreşimini ezer (HapticManager çalanı keser),
        /// sesler 2D çalar. Ayar ya da cue boşsa sessiz geçer.
        /// </summary>
        private void OnKilled(Collider2D other)
        {
            if (settings == null) return;
            if (HapticManager.Instance != null) HapticManager.Instance.Vibrate(settings.acidDeathRumble);
            float strength = _impact.w > 0f ? _impact.w : settings.strengthFloor;
            PlayCue(settings.splashCue, strength);
            PlayCue(settings.sizzleCue, strength);
        }

        private void PlayCue(AudioCueSO cue, float strength)
        {
            if (sfxPlayChannel == null || cue == null || cue.AudioClips == null || cue.AudioClips.Length == 0) return;
            var p = AudioCuePlayParams.Default;
            p.VolumeMultiplier = Mathf.Lerp(0.75f, 1f, strength);
            p.PitchMultiplier = Mathf.Lerp(1.05f, 0.95f, strength);   // ağır temas daha pes
            sfxPlayChannel.RaisePlayEvent(cue, p);
        }

        private void TriggerImpact(Vector2 localPoint, float strength, float age, Vector2 playerVelocity)
        {
            Vector2 pixelScale = _hazard.PixelScale;
            Rect rect = SpriteLocalRect();
            Vector2 pixel = AcidImpactMath.LocalToPixel(localPoint, rect, pixelScale);
            pixel = AcidImpactMath.SurfaceContact(pixel, AcidLevel(), pixelScale.y, hasSurface);

            _duration = ImpactDuration();
            _impact = new Vector4(pixel.x, pixel.y, age, strength);
            WriteImpact();

            Vector2 contactLocal = AcidImpactMath.PixelToLocal(pixel, rect, pixelScale);
            EmitDroplets(transform.TransformPoint(contactLocal), contactLocal, strength, playerVelocity);
        }

        private void EmitDroplets(Vector3 worldPoint, Vector2 contactLocal, float strength, Vector2 playerVelocity)
        {
            if (settings == null || impactFxPrefab == null) return;
            if (_fx == null) _fx = Instantiate(impactFxPrefab, transform);

            // Yüzeyli asitte yüzey normali; akan asitte merkezden temas noktasına
            Vector2 normal = hasSurface
                ? (Vector2)transform.up
                : (Vector2)transform.TransformDirection(contactLocal.sqrMagnitude > 1e-6f ? contactLocal.normalized : Vector2.up);

            int small = Mathf.RoundToInt(Mathf.Lerp(settings.dropletCount.x, settings.dropletCount.y, strength));
            int big = Mathf.RoundToInt(Mathf.Lerp(settings.bigDropletCount.x, settings.bigDropletCount.y, strength));
            EmitBatch(worldPoint, normal, small, settings.dropletSpeed, settings.dropletSizePx, strength, playerVelocity);
            EmitBatch(worldPoint, normal, big, settings.bigDropletSpeed, settings.bigDropletSizePx, strength, playerVelocity);
        }

        private void EmitBatch(Vector3 worldPoint, Vector2 normal, int count, Vector2 speedRange, int sizePx,
            float strength, Vector2 playerVelocity)
        {
            var emit = new ParticleSystem.EmitParams
            {
                startSize = sizePx / settings.pixelsPerUnit,
                applyShapeToPosition = false
            };
            for (int i = 0; i < count; i++)
            {
                // Taç şekli: koninin ortasına yakın damlalar daha hızlı (uzun sütunlar), kenarlar kısa
                float r = Random.value;
                float centreBias = 1f - Mathf.Abs(r - 0.5f) * 0.8f;
                float speed = Random.Range(speedRange.x, speedRange.y) * centreBias;
                emit.position = worldPoint;
                emit.velocity = AcidImpactMath.SplashVelocity(normal, settings.coneDeg, r, speed, strength,
                    playerVelocity, settings.inheritX, settings.inheritY, settings.maxDropletSpeed);
                _fx.Emit(emit, 1);
            }
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
            DebugImpact(localPoint, age, Vector2.zero);
        }

        /// <summary>Oyuncu hızıyla (damla yönü için). Damlalar age kadar ileri simüle edilir.</summary>
        public void DebugImpact(Vector2 localPoint, float age, Vector2 playerVelocity)
        {
            if (!Application.isPlaying) return;
            float strength = settings != null
                ? AcidImpactMath.Strength(playerVelocity.magnitude, settings.minSpeed, settings.maxSpeed, settings.strengthFloor)
                : 1f;
            if (_fx != null) _fx.Clear();
            TriggerImpact(localPoint, strength, age, playerVelocity);
            if (_fx != null && age > 0f) _fx.Simulate(age, true, false);
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
