using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Riso levha platformunun renk izi. Tek ParticleSystem (World uzayı, Emission kapalı); parçacıkları
    /// <c>ParticleSystem.Emit</c> ile bu script doğurur. Her mürekkep platformun GECİKMELİ kopyasından doğar
    /// (teal 0, sarı 0.06, pembe 0.12 sn) ve o mürekkebin kafes hücresiyle, hücrenin çapıyla belirir.
    /// Saf hesap <see cref="HalftoneTrailMath"/>, <see cref="TrailHistory"/> ve <see cref="TrailBudget"/> içindedir.
    /// Parçacık görünümü (shader, sıralama) H6'nın işi.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class HalftoneTrailEmitter : MonoBehaviour, IResettable
    {
        [SerializeField] private RisoSlabSettingsSO settings;
        [Tooltip("İzi bırakan platform. Boşsa üst obje (parent) kullanılır.")]
        [SerializeField] private Transform source;

        private ParticleSystem _particleSystem;
        private TrailHistory _history;
        private TrailBudget _budget;
        private System.Random _rng;
        private readonly float[] _accumulators = new float[3];
        private Vector2 _previousPosition;
        private bool _hasPrevious;

        private void Awake()
        {
            if (source == null) source = transform.parent != null ? transform.parent : transform;
            _particleSystem = GetComponent<ParticleSystem>();
            _rng = new System.Random(GetInstanceID());
            ConfigureParticleSystem();
            CreateState();
        }

        private void CreateState()
        {
            if (settings == null) return;
            float maxDelay = Mathf.Max(settings.inkDelays.x, Mathf.Max(settings.inkDelays.y, settings.inkDelays.z));
            _history = new TrailHistory(maxDelay + 0.2f);
            _budget = new TrailBudget(settings.maxParticlesPerInk, settings.maxParticlesTotal);
        }

        private void ConfigureParticleSystem()
        {
            var main = _particleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            main.playOnAwake = true;
            main.loop = true;
            main.maxParticles = settings != null ? settings.maxParticlesTotal : 54;

            main.scalingMode = ParticleSystemScalingMode.Local; // platformun ölçeği parçacık boyutunu bozmasın

            var emission = _particleSystem.emission;
            emission.enabled = false;

            // Boyut: ilk %40 sabit, sonra küçül; renk alfası: son %25'te sön (0'a inmeden önce alfa ile biter)
            var sol = _particleSystem.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(0.4f, 1f), new Keyframe(0.75f, 0.5f), new Keyframe(1f, 0.5f)));
            var col = _particleSystem.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            // RisoDot shader'ı için vertex stream'ler: Position, Color, UV, StableRandom.x
            var psr = _particleSystem.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.sortingLayerName = "Default"; // Background < Default < Platform: platformun ve node'ların arkasında
            psr.sortingOrder = -5;
            var streams = new System.Collections.Generic.List<ParticleSystemVertexStream>
            {
                ParticleSystemVertexStream.Position,
                ParticleSystemVertexStream.Color,
                ParticleSystemVertexStream.UV,
                ParticleSystemVertexStream.StableRandomX
            };
            psr.SetActiveVertexStreams(streams);

            if (settings != null)
            {
                // Sürtünme: kare başına hız azaltma oranı (limit 0 + dampen). Kare hızına bağımlı, yaklaşık.
                var limit = _particleSystem.limitVelocityOverLifetime;
                limit.enabled = true;
                limit.limit = 0f;
                limit.dampen = 1f - Mathf.Exp(-settings.trailDrag / 60f);
            }
        }

        private void OnEnable()
        {
            // Oda uyanınca (dynamicContent yeniden etkinleşir) eski iz ve geçmiş kalmasın.
            ResetState();
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Unregister(this);
            }
        }

        /// <summary>Ölüm sıfırlaması: parçacıklar, geçmiş tamponu ve birikmiş mesafe temizlenir.</summary>
        public void ResetToDefault()
        {
            ResetState();
        }

        private void ResetState()
        {
            if (_particleSystem != null) _particleSystem.Clear();
            _history?.Clear();
            _budget?.Clear();
            for (int i = 0; i < _accumulators.Length; i++) _accumulators[i] = 0f;
            _hasPrevious = false;
        }

        private void LateUpdate()
        {
            if (settings == null || _history == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            float now = Time.time;
            Vector2 pos = source.position;
            float rot = source.eulerAngles.z;

            if (!_hasPrevious)
            {
                _previousPosition = pos;
                _hasPrevious = true;
                _history.Record(now, pos, rot);
                return;
            }

            // Teleport (wrap, ölüm sıfırlaması): bu kare emisyon yok, geçmiş ve birikim sıfır. Eski parçacıklar kalır.
            if (HalftoneTrailMath.IsTeleport(_previousPosition, pos, settings.teleportThreshold))
            {
                _history.Clear();
                for (int i = 0; i < _accumulators.Length; i++) _accumulators[i] = 0f;
                _history.Record(now, pos, rot);
                _previousPosition = pos;
                return;
            }

            _history.Record(now, pos, rot);

            Vector2 delta = pos - _previousPosition;
            float distance = delta.magnitude;
            _previousPosition = pos;
            if (distance < 1e-5f) return;

            Vector2 velocity = delta / dt;
            Vector2 dirWorld = delta / distance;
            Vector3 lossy = source.lossyScale;
            Vector2 size = new Vector2(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y));
            float perUnit = HalftoneTrailMath.DensityPerUnit(settings.trailPerUnitPerInk, size.x, settings.densityReferenceWidth);

            for (int i = 0; i < 3; i++)
            {
                var ink = (TrailInk)i;
                int count = HalftoneTrailMath.Accumulate(ref _accumulators[i], distance, perUnit);
                float delay = i == 0 ? settings.inkDelays.x : (i == 1 ? settings.inkDelays.y : settings.inkDelays.z);
                for (int n = 0; n < count; n++)
                {
                    // Aynı karedeki doğumlar yol boyunca yayılsın
                    float sampleTime = now - delay - (float)_rng.NextDouble() * dt;
                    TryEmit(ink, sampleTime, size, dirWorld, velocity, now);
                }
            }
        }

        private void TryEmit(TrailInk ink, float sampleTime, Vector2 size, Vector2 dirWorld, Vector2 velocity, float now)
        {
            if (!_history.TrySample(sampleTime, out Vector2 pos, out float rotDeg)) return;

            // Dünya vektörlerini gecikmeli kopyanın yerel uzayına çevir
            Quaternion inverse = Quaternion.Euler(0f, 0f, -rotDeg);
            Vector2 upLocal = inverse * Vector3.up;
            Vector2 dirLocal = inverse * (Vector3)dirWorld;

            if (!HalftoneTrailMath.TryPickCell(_rng, ink, size, upLocal, dirLocal, settings.dotPitch,
                    out Vector2 centerLocal, out float v))
                return;

            float diameter = HalftoneTrailMath.DiameterFor(ink, v, settings.dotPitch);
            if (_rng.NextDouble() < settings.bigDropChance) diameter = 1.2f * settings.dotPitch;

            float lifetime = Mathf.Lerp(settings.trailLifetime.x, settings.trailLifetime.y, (float)_rng.NextDouble());
            if (!_budget.TryReserve(ink, now, lifetime)) return;

            Vector2 spawn = pos + (Vector2)(Quaternion.Euler(0f, 0f, rotDeg) * centerLocal);
            Vector2 perp = new Vector2(-dirWorld.y, dirWorld.x);
            Vector2 startVelocity = velocity * settings.trailInheritVelocity
                                    + perp * (settings.trailPerpJitter * ((float)_rng.NextDouble() * 2f - 1f));

            var emit = new ParticleSystem.EmitParams
            {
                position = new Vector3(spawn.x, spawn.y, 0f),
                velocity = new Vector3(startVelocity.x, startVelocity.y, 0f),
                startLifetime = lifetime,
                startSize = diameter * 1.25f, // shader daireyi quad'ın %80'ine çizer (hayalet payı)
                randomSeed = (uint)_rng.Next(),
                startColor = InkColor(ink)
            };
            _particleSystem.Emit(emit, 1);
        }

        private Color32 InkColor(TrailInk ink)
        {
            switch (ink)
            {
                case TrailInk.Teal: return settings.tealInk;
                case TrailInk.Yellow: return settings.yellowInk;
                default: return settings.pinkInk;
            }
        }
    }
}
