using New_Scripts.Player;
using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin karaktere ve kollarına tepkisini hesaplar (docs/flower-hareket-plani.md T11, F-etkilesim.md F1/F3/F4). Görsel yok:
    /// yerel <see cref="PlayerController"/>'ı (gövde, omuzlar) ve <see cref="FlowerCycle"/>'ın tutunma durumunu okur, hedefleri
    /// <see cref="FlowerReactMath"/> ile üretir, yaprak başına yaylarla sürer ve sonucu <see cref="Reaction"/> durumuna yazar;
    /// <see cref="FlowerBloom"/> bunu çizer (T12). Çiçeği yalnızca okur (<c>FlowerCycle</c> kuralları, kanca, itme dokunulmaz).
    ///
    /// Kapı: yalnızca tutulabilirken ve açıkken tepki verilir (kapalı tomurcuğun "bakması" tutulabilir sanılmasın), uyarıda genlik
    /// yarıya iner. Oyuncu uzakken ve yaylar durgunken hiçbir şey hesaplanmaz. <c>NodeDetector</c> çağrılmaz (her çağrıda dizi ayırır).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(FlowerBloom), typeof(FlowerCycle))]
    public class FlowerReactor : MonoBehaviour
    {
        private const float PlayerSearchInterval = 0.5f;
        private const float TipFraction = 0.85f;   // yaprak ucu: merkezden yaprak boyunun %85'i (shader bükmesi CPU'da bilinmez)

        [SerializeField] private FlowerReactionSettingsSO settings;

        private FlowerBloom _bloom;
        private FlowerCycle _cycle;
        private PlayerController _player;
        private float _nextPlayerSearch;
        private float _scaleCache = 1f;

        private FlowerReactionState _state;
        private UiSpring[] _yaw;
        private UiSpring[] _dip;
        private UiSpring _grip;
        private bool _gripReleasing;
        private readonly UiSpring[] _armDir = new UiSpring[2];   // 0 sol, 1 sağ: gecikmeli kol azimutu
        private bool _armDirInit0, _armDirInit1;

        /// <summary>Tepki durumu: yaprak yaw/dip, kavrama, kapı. <see cref="FlowerBloom"/> bunu çizer.</summary>
        public FlowerReactionState Reaction => _state;
        public FlowerReactionSettingsSO Settings => settings;

        private void OnEnable()
        {
            _bloom = GetComponent<FlowerBloom>();
            _cycle = GetComponent<FlowerCycle>();
            BuildSprings();
            _bloom.Reaction = _state;
            _cycle.Grabbed += OnGrabbed;
        }

        private void OnDisable()
        {
            if (_cycle != null) _cycle.Grabbed -= OnGrabbed;
            if (_bloom != null && _bloom.Reaction == _state) _bloom.Reaction = null;
        }

        private void BuildSprings()
        {
            int n = _bloom.PetalCount;
            _state = new FlowerReactionState(n);
            if (settings != null) _state.Shape = settings.GripShape;
            _yaw = new UiSpring[n];
            _dip = new UiSpring[n];
            var s = settings;
            float stiff = s != null ? s.petalStiffness : FlowerReactMath.PetalStiffness;
            float damp = s != null ? s.petalDamping : FlowerReactMath.PetalDamping;
            float variation = s != null ? s.petalSeedVariation : 0f;
            for (int i = 0; i < n; i++)
            {
                // Yaprak başına küçük sertlik sapması (ζ sabit kalsın diye sönüm √ ile ölçeklenir): yapraklar eşzamanlı oynamasın.
                float k = stiff * (1f + (FlowerBloomMath.PetalRank(i) - 0.5f) * variation);
                float c = damp * Mathf.Sqrt(k / stiff);
                _yaw[i] = new UiSpring(k, c);
                _dip[i] = new UiSpring(k, c);
            }
            _grip = MakeGripSpring(releasing: false, null);
            _gripReleasing = false;
            float ds = s != null ? s.directionStiffness : FlowerReactMath.DirectionStiffness;
            float dd = s != null ? s.directionDamping : FlowerReactMath.DirectionDamping;
            _armDir[0] = new UiSpring(ds, dd);
            _armDir[1] = new UiSpring(ds, dd);
            _armDirInit0 = _armDirInit1 = false;
        }

        private UiSpring MakeGripSpring(bool releasing, UiSpring from)
        {
            var s = settings;
            float k = s != null ? s.gripStiffness : FlowerReactMath.GripStiffness;
            float c = releasing
                ? (s != null ? s.gripReleaseDamping : FlowerReactMath.GripReleaseDamping)
                : (s != null ? s.gripDamping : FlowerReactMath.GripDamping);
            var spring = new UiSpring(k, c);
            if (from != null) { spring.Value = from.Value; spring.Velocity = from.Velocity; spring.Target = from.Target; }
            return spring;
        }

        private void OnGrabbed(ActiveArm arm, Vector2 point)
        {
            // Kol azimutunun yayı, tutunma anında kolun gerçek açısından başlar (çiçek boyunca dönmesin).
            if (arm == ActiveArm.Left) _armDirInit0 = false;
            else if (arm == ActiveArm.Right) _armDirInit1 = false;

            // Yakalama darbesi: kule kabarır, çiçek bir an çekinir (yalnızca çiçek tepki verecek durumdaysa).
            if (settings != null && settings.enabled && _state != null && _state.Gate > 0.2f)
                _bloom.Poke(settings.pokeCenterKick, settings.pokeOpenKick);
        }

        private void Update()
        {
            if (!Application.isPlaying || _state == null) return;
            if (settings == null || !settings.enabled)
            {
                if (_state.MaxPetalMotion > 0f || _state.Grip01 != 0f) ResetToRest();
                return;
            }

            float dt = Time.deltaTime;
            float gate = FlowerReactMath.Gate(_cycle.IsGrabbable, _bloom.Open, _cycle.Warn01);
            bool held = _cycle.IsHeld;

            Vector2 center = transform.position;
            float reach = _bloom.PetalReach;
            if (!TryGetPlayer(out Vector2 body, out Vector2 velocity))
            {
                if (!held) { RelaxToRest(dt); return; }
                body = center;
            }

            float bodyRadius = settings.bodyRadiusRel * reach;
            bool near = (body - center).sqrMagnitude < (reach + bodyRadius + settings.farMargin) * (reach + bodyRadius + settings.farMargin);
            if (!near && !held && Resting())
            {
                // Uzak ve durgun: hesap yok. Durum zaten sıfıra yakın.
                _state.Gate = gate;
                return;
            }

            ComputeTargets(gate, held, center, reach, bodyRadius, body, velocity);
            _state.Gate = gate;
            WriteState();
        }

        // --- Hesap ---

        private void ComputeTargets(float gate, bool held, Vector2 center, float reach, float bodyRadius, Vector2 body, Vector2 velocity)
        {
            var s = settings;
            int n = _bloom.PetalCount;
            Quaternion rot = _bloom.ModelRotation;
            float halfWidth = s.halfWidthDeg * Mathf.Deg2Rad;
            float maxYaw = s.maxYawDeg * Mathf.Deg2Rad, maxDip = s.maxDipDeg * Mathf.Deg2Rad;
            float bodyYaw = s.bodyMaxYawDeg * Mathf.Deg2Rad, bodyDip = s.bodyMaxDipDeg * Mathf.Deg2Rad;
            float vRef = s.bodySpeedReference > 0f ? s.bodySpeedReference : (_player != null && _player.Stats != null ? _player.Stats.MoveSpeed : 10f);
            float speedScale = FlowerReactMath.SpeedScale(velocity.magnitude, vRef, s.bodySpeedFloor);
            float dt = Time.deltaTime;

            // Tutunan kolların (gecikmeli) azimutu ve kavrama hedefi
            int holders = 0;
            bool left = _cycle.IsHeldBy(ActiveArm.Left), right = _cycle.IsHeldBy(ActiveArm.Right);
            if (left) { holders++; StepArmDirection(0, ShoulderOf(ActiveArm.Left, body), center, rot, dt); }
            if (right) { holders++; StepArmDirection(1, ShoulderOf(ActiveArm.Right, body), center, rot, dt); }

            // Kavrama hedefi: tek kol gripOneArm, iki kol 1; kapı kapalıysa (kapanış/uyarı sonrası) 0.
            float gripTarget = gate > 0f ? (holders >= 2 ? 1f : holders == 1 ? s.gripOneArm : 0f) : 0f;
            StepGrip(gripTarget, dt);
            float armScale = Mathf.Clamp(_grip.Value, 0f, 1.2f);

            for (int i = 0; i < n; i++)
            {
                float az = _bloom.PetalAzimuthOf(i);

                // Kol süpürmesi (iki kol: büyük olan kazanır)
                // Bırakılmış kolun son yönü kalır (init bayrağı), etkisi kavrama yayıyla söner.
                float armYaw = 0f, armDip = 0f;
                if (armScale > 0.001f)
                {
                    if (_armDirInit0)
                    {
                        armYaw = FlowerReactMath.Combine(armYaw, FlowerReactMath.PartingYaw(az, _armDir[0].Value, halfWidth, maxYaw));
                        armDip = Mathf.Max(armDip, FlowerReactMath.PartingDip(az, _armDir[0].Value, halfWidth, maxDip));
                    }
                    if (_armDirInit1)
                    {
                        armYaw = FlowerReactMath.Combine(armYaw, FlowerReactMath.PartingYaw(az, _armDir[1].Value, halfWidth, maxYaw));
                        armDip = Mathf.Max(armDip, FlowerReactMath.PartingDip(az, _armDir[1].Value, halfWidth, maxDip));
                    }
                    armYaw *= armScale;
                    armDip *= armScale;
                }

                // Gövde yakınlığı: yaprağın ucu gövdeden yol verir (hıza göre)
                Vector3 d3 = rot * new Vector3(Mathf.Cos(az), 0f, Mathf.Sin(az));
                Vector2 dir = new Vector2(d3.x, d3.y);
                float dl = dir.magnitude;
                float yawBody = 0f, dipBody = 0f;
                if (dl > 1e-4f)
                {
                    dir /= dl;
                    Vector2 tip = center + dir * (reach * TipFraction);
                    yawBody = FlowerReactMath.BodyYaw(tip, body, dir, bodyRadius, bodyYaw, speedScale);
                    dipBody = FlowerReactMath.BodyDip(tip, body, bodyRadius, bodyDip, speedScale);
                }

                _yaw[i].Target = FlowerReactMath.Combine(armYaw, yawBody) * gate;
                _dip[i].Target = Mathf.Max(armDip, dipBody) * gate;
                _yaw[i].Step(dt);
                _dip[i].Step(dt);
                Settle(_yaw[i]);
                Settle(_dip[i]);
            }
        }

        // Hedefi 0 olan yay dinlenmeye girince tam 0'a oturur: "durgun" erken çıkışı için (yoksa 1e-5 artıklar sonsuza dek kalır).
        private static void Settle(UiSpring s)
        {
            if (s.Target == 0f && s.IsRest) { s.Value = 0f; s.Velocity = 0f; }
        }

        private Vector2 ShoulderOf(ActiveArm arm, Vector2 fallback)
        {
            if (_player == null) return fallback;
            ArmController a = arm == ActiveArm.Left ? _player.LeftArm : _player.RightArm;
            return a != null ? (Vector2)a.transform.position : fallback;
        }

        // Kolun çiçek-yerel azimutu: omuz yönü dünyadan yerele FlowerReactMath ile çevrilir, yay sarılmadan izler.
        private void StepArmDirection(int index, Vector2 shoulder, Vector2 center, Quaternion rot, float dt)
        {
            Vector2 d = shoulder - center;
            if (d.sqrMagnitude < 1e-6f) return;   // omuz tam merkezde: yön belirsiz, son değer kalır
            float target = FlowerReactMath.WorldToLocalAzimuth(rot, d);
            bool init = index == 0 ? _armDirInit0 : _armDirInit1;
            var spring = _armDir[index];
            if (!init)
            {
                spring.Value = target;
                spring.Velocity = 0f;
                if (index == 0) _armDirInit0 = true; else _armDirInit1 = true;
            }
            spring.Target = FlowerReactMath.UnwrapAngle(spring.Value, target);
            spring.Step(dt);
            _state.ArmAzimuth = spring.Value;
        }

        private void StepGrip(float target, float dt)
        {
            bool releasing = target < _grip.Value - 1e-3f;
            if (releasing != _gripReleasing)
            {
                _gripReleasing = releasing;
                _grip = MakeGripSpring(releasing, _grip);
            }
            _grip.Target = target;
            _grip.Step(dt);
            Settle(_grip);
        }

        private void RelaxToRest(float dt)
        {
            // Oyuncu yok: her şey sıfıra yaylanır.
            for (int i = 0; i < _yaw.Length; i++)
            {
                _yaw[i].Target = 0f; _dip[i].Target = 0f;
                _yaw[i].Step(dt); _dip[i].Step(dt);
                Settle(_yaw[i]); Settle(_dip[i]);
            }
            StepGrip(0f, dt);
            _state.Gate = 0f;
            WriteState();
        }

        private void WriteState()
        {
            for (int i = 0; i < _yaw.Length; i++)
            {
                _state.PetalYaw[i] = _yaw[i].Value;
                _state.PetalDip[i] = _dip[i].Value;
            }
            _state.Grip01 = _grip.Value;
        }

        private bool Resting()
        {
            for (int i = 0; i < _yaw.Length; i++)
                if (!_yaw[i].IsRest || !_dip[i].IsRest) return false;
            return _grip.IsRest && _grip.Value == 0f && _grip.Target == 0f;
        }

        private void ResetToRest()
        {
            BuildSprings();
            _bloom.Reaction = _state;
        }

        private bool TryGetPlayer(out Vector2 body, out Vector2 velocity)
        {
            if (_player == null && Time.unscaledTime >= _nextPlayerSearch)
            {
                _nextPlayerSearch = Time.unscaledTime + PlayerSearchInterval;
                _player = FindFirstObjectByType<PlayerController>();
            }
            if (_player == null) { body = default; velocity = default; return false; }
            body = _player.PlayerRigidbody != null ? _player.PlayerRigidbody.position : (Vector2)_player.transform.position;
            velocity = _player.Velocity;
            return true;
        }
    }
}
