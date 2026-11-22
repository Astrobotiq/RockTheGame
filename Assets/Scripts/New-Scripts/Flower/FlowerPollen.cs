using New_Scripts.Audio;
using New_Scripts.Player;
using New_Scripts.Player.IFramePauseable;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğe tutunulunca organ uçlarından dökülen polen ("Delgeç Konfetisi", docs/flower-hareket-plani.md T16, G-polen.md).
    /// <see cref="FlowerCycle"/>'ın tutunma bildirimini dinler; yaprak sarsıntısından <see cref="FlowerPollenSettingsSO.delay"/> sonra
    /// (follow-through) oyuncuya dönük 2-3 organdan tek patlama verir. Çiçek uyarıya/kapanışa girince bekleyen patlama iptal olur
    /// ve yenisi çıkmaz (uyarı renksiz kanalda kalır, polen karışmaz). AcidReaction deseni: parçacık sistemi ilk patlamada
    /// çocuk olarak bir kez kurulur (hiç tutulmamış çiçekte maliyet sıfır), emisyon kapalıdır, parçacıklar <c>Emit(EmitParams)</c> ile
    /// boyut/hız/renk/konumla doğar. İki sistem: <c>Chips</c> (nokta ve elips) ve <c>Pieces</c> (iki tonlu kâğıt parçası).
    /// </summary>
    [RequireComponent(typeof(FlowerCycle), typeof(FlowerBloom))]
    public class FlowerPollen : MonoBehaviour
    {
        private const int MaxPending = 2;   // en çok iki kol

        [SerializeField] private FlowerPollenSettingsSO settings;
        [Tooltip("FX_FlowerPollen: Chips ve Pieces çocuk sistemleri. İlk patlamada bu çiçeğin çocuğu olarak bir kez kurulur.")]
        [SerializeField] private GameObject fxPrefab;
        [Tooltip("SFX-Play kanalı (polen cue'su için). Boşsa ses çalmaz.")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;

        private FlowerCycle _cycle;
        private FlowerBloom _bloom;
        private PlayerController _player;
        private float _nextPlayerSearch;
        private ParticleSystem _chips;
        private ParticleSystem _pieces;

        private struct Pending
        {
            public bool Active;
            public float FireAt;
            public Vector2 Point;
            public float Speed;
        }
        private readonly Pending[] _pending = new Pending[MaxPending];
        private readonly int[] _near = new int[FlowerPollenMath.DefaultOrganCount];
        private readonly float[] _scratchWeights = new float[8];

        /// <summary>Şu an canlı polen parçacığı sayısı (test ve ölçüm için).</summary>
        public int AliveParticles => (_chips != null ? _chips.particleCount : 0) + (_pieces != null ? _pieces.particleCount : 0);
        /// <summary>Patlama bekleyen (gecikmede) tutunma sayısı.</summary>
        public int PendingBursts
        {
            get { int n = 0; for (int i = 0; i < _pending.Length; i++) if (_pending[i].Active) n++; return n; }
        }

        private void OnEnable()
        {
            _cycle = GetComponent<FlowerCycle>();
            _bloom = GetComponent<FlowerBloom>();
            _cycle.Grabbed += OnGrabbed;
            _cycle.PhaseChanged += OnPhaseChanged;
            HitStopEvents.HitStopStarted += OnHitStopStarted;
            HitStopEvents.HitStopEnded += OnHitStopEnded;
            ClearAll();
        }

        private void OnDisable()
        {
            if (_cycle != null)
            {
                _cycle.Grabbed -= OnGrabbed;
                _cycle.PhaseChanged -= OnPhaseChanged;
            }
            HitStopEvents.HitStopStarted -= OnHitStopStarted;
            HitStopEvents.HitStopEnded -= OnHitStopEnded;
        }

        /// <summary>Bekleyen patlamaları iptal eder ve canlı parçacıkları siler (oda uyanınca/sıfırlamada donmuş polen kalmasın).</summary>
        public void ClearAll()
        {
            for (int i = 0; i < _pending.Length; i++) _pending[i].Active = false;
            if (_chips != null) _chips.Clear(true);
            if (_pieces != null) _pieces.Clear(true);
        }

        private void OnGrabbed(ActiveArm arm, Vector2 point)
        {
            if (settings == null || !settings.enabled || fxPrefab == null) return;
            if (_cycle.Phase != FlowerPhase.Open) return;   // uyarı/kapanışta polen çıkmaz

            float speed = TryGetPlayer(out _, out Vector2 velocity) ? velocity.magnitude : 0f;
            for (int i = 0; i < _pending.Length; i++)
            {
                if (_pending[i].Active) continue;
                _pending[i] = new Pending { Active = true, FireAt = Time.time + settings.delay, Point = point, Speed = speed };
                return;
            }
        }

        private void OnPhaseChanged(FlowerPhase phase)
        {
            if (phase == FlowerPhase.Open) return;
            // Uyarı, snap ve kapalıda bekleyen patlama iptal: polen yalnızca açık fazda tutunmanın karşılığıdır.
            for (int i = 0; i < _pending.Length; i++) _pending[i].Active = false;
        }

        private void Update()
        {
            for (int i = 0; i < _pending.Length; i++)
            {
                if (!_pending[i].Active || Time.time < _pending[i].FireAt) continue;
                _pending[i].Active = false;
                if (_cycle.Phase == FlowerPhase.Open) Burst(_pending[i].Point, _pending[i].Speed);
            }
        }

        /// <summary>
        /// Anında patlama (Play Mode ayar/ekran görüntüsü için): <paramref name="point"/> oyuncunun tutunma noktası, <paramref name="speed"/>
        /// tutunma hızı. Gecikme ve faz kapısı yok.
        /// </summary>
        public void DebugBurst(Vector2 point, float speed) => Burst(point, speed);

        private void Burst(Vector2 point, float speed)
        {
            if (settings == null || !EnsureFx()) return;
            var s = settings;

            Vector2 center = transform.position;
            TryGetPlayer(out Vector2 playerPos, out Vector2 playerVel);
            Vector2 toPlayer = TryGetPlayer(out _, out _) ? playerPos - center : point - center;

            float strength = FlowerPollenMath.Strength(speed, s.minSpeed, s.maxSpeed, s.strengthFloor);
            int wanted = FlowerPollenMath.Count(strength, s.strengthFloor, s.count.x, s.count.y);
            int room = Mathf.Max(0, s.maxParticles - AliveParticles);
            int n = Mathf.Min(wanted, room);
            if (n <= 0) return;

            float ortho = Camera.main != null && Camera.main.orthographic ? Camera.main.orthographicSize : 8f;
            float ring = _bloom.OrganRingRadius;
            int organCount = FlowerPollenMath.DefaultOrganCount;
            int nearTake = FlowerPollenMath.NearestOrgans(Vector2.zero, toPlayer, organCount, s.nearOrgans, _near);

            var main1 = _chips.main; main1.gravityModifier = s.gravity;
            var main2 = _pieces.main; main2.gravityModifier = s.gravity;
            var lim1 = _chips.limitVelocityOverLifetime; lim1.drag = s.drag;
            var lim2 = _pieces.limitVelocityOverLifetime; lim2.drag = s.drag;

            var emit = new ParticleSystem.EmitParams { applyShapeToPosition = false };
            for (int i = 0; i < n; i++)
            {
                PollenKind kind = FlowerPollenMath.Kind(i, n, s.dotShare, s.ellipseShare);
                int organ = FlowerPollenMath.PickOrgan(Random.value, Random.value, _near, nearTake, organCount, s.nearShare);
                Vector2 origin = FlowerPollenMath.OrganPoint(organ, organCount, center, ring);
                Vector2 dir = FlowerPollenMath.EmitDirection(origin - center, toPlayer, s.towardBias, s.coneDeg, Random.Range(-1f, 1f));

                Vector2 sp = s.SpeedRange(kind);
                float v = Random.Range(sp.x, sp.y) * Mathf.Lerp(0.8f, 1f, strength);
                Vector2 vel = FlowerPollenMath.Velocity(dir, v, playerVel, s.inherit, s.maxInherit);

                Vector2 sz = s.SizeRange(kind);
                float size = FlowerPollenMath.ClampSize(Random.Range(sz.x, sz.y), ortho);

                // Kâğıt parçaları uzun yaşar: ömrün üst yarısı.
                float life = kind == PollenKind.Piece
                    ? Random.Range(Mathf.Lerp(s.lifetime.x, s.lifetime.y, 0.5f), s.lifetime.y)
                    : Random.Range(s.lifetime.x, s.lifetime.y);

                float[] weights = s.ColorWeights(kind);
                int ci = FlowerPollenMath.PickWeighted(Random.value, weights);
                Color c = s.palette != null && ci < s.palette.Length ? s.palette[ci] : Color.white;

                emit.position = origin;
                emit.velocity = vel;
                emit.startLifetime = life;
                emit.startColor = c;
                emit.rotation = Random.Range(0f, 360f);
                if (kind == PollenKind.Ellipse) emit.startSize3D = new Vector3(size, size * s.ellipseAspect, 1f);
                else emit.startSize = size;

                (kind == PollenKind.Piece ? _pieces : _chips).Emit(emit, 1);
            }

            PlayCue(s.pollenCue, point);
        }

        private bool EnsureFx()
        {
            if (_chips != null && _pieces != null) return true;
            if (fxPrefab == null) return false;
            var go = Instantiate(fxPrefab, transform);
            go.name = fxPrefab.name;
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps.name == "Chips") _chips = ps;
                else if (ps.name == "Pieces") _pieces = ps;
            }
            if (_chips == null || _pieces == null)
            {
                Debug.LogWarning($"[{name}] FX_FlowerPollen 'Chips' ve 'Pieces' çocuk sistemlerini içermiyor.");
                return false;
            }
            return true;
        }

        private void PlayCue(AudioCueSO cue, Vector2 point)
        {
            if (sfxPlayChannel == null || cue == null || cue.AudioClips == null || cue.AudioClips.Length == 0) return;
            var p = AudioCuePlayParams.Default;
            p.Position = point;
            p.Is3D = true;
            p.PitchMultiplier = Random.Range(0.95f, 1.05f);
            sfxPlayChannel.RaisePlayEvent(cue, p);
        }

        private void OnHitStopStarted()
        {
            if (_chips != null) _chips.Pause(true);
            if (_pieces != null) _pieces.Pause(true);
        }

        private void OnHitStopEnded()
        {
            if (_chips != null) _chips.Play(true);
            if (_pieces != null) _pieces.Play(true);
        }

        private bool TryGetPlayer(out Vector2 position, out Vector2 velocity)
        {
            if (_player == null && Time.unscaledTime >= _nextPlayerSearch)
            {
                _nextPlayerSearch = Time.unscaledTime + 0.5f;
                _player = FindFirstObjectByType<PlayerController>();
            }
            if (_player == null) { position = default; velocity = default; return false; }
            position = _player.PlayerRigidbody != null ? _player.PlayerRigidbody.position : (Vector2)_player.transform.position;
            velocity = _player.Velocity;
            return true;
        }
    }
}
