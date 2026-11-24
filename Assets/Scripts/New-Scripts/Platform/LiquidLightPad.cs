using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Bounce pad'in sıvı ışık görselini sürer (<c>Sprites/LiquidLightPad</c> shader'ı): akış zamanı, vuruş zarfı ve
    /// nota rengine palet kaydırma. Pad'in Visual alt objesindeki SpriteRenderer'a konur; <see cref="LaunchPad"/>
    /// olaylarını dinler. Veriyi <c>MaterialPropertyBlock</c> ile gönderir (materyal ortak kalır).
    /// Sprite 1x1 birimlik, nesne uzayında ±0.5 olmalı; Visual'ın ölçeği (squash/stretch dahil) çizimi birlikte ezer.
    /// </summary>
    [ExecuteAlways] // Edit Mode'da da doğru şekil/boyutla görünsün (seviyeye yerleştirirken)
    [RequireComponent(typeof(SpriteRenderer))]
    public class LiquidLightPad : MonoBehaviour
    {
        [SerializeField] private LaunchPad pad;

        [Header("Shape (dünya birimi, dinlenme boyutu)")]
        [Tooltip("Gövde boyutu. Boşsa pad'in BoxCollider2D boyutu alınır.")]
        [SerializeField] private Vector2 bodySize;

        [Header("Flow")]
        [Tooltip("Boşta akış hızı.")]
        [SerializeField] private float idleFlowSpeed = 0.6f;
        [Tooltip("Vuruşta zarfla orantılı ek akış hızı.")]
        [SerializeField] private float hitFlowBoost = 5f;

        [Header("Hit envelope")]
        [Tooltip("Mockup: 0.05 sn.")]
        [SerializeField] private float attack = 0.05f;
        [Tooltip("Zarfın üstel sönme zaman sabiti (sn). Mockup: 0.22.")]
        [SerializeField] private float decay = 0.22f;

        [Header("Squash")]
        [Tooltip("LaunchPad squash/stretch'i Visual'ı merkezinden ölçekler; açıksa pedin ALT kenarı sabit kalır (mockup: ped zemine oturur).")]
        [SerializeField] private bool anchorBottom = true;

        [Header("Hue")]
        [Tooltip("Palet kaymasının hedefe yaklaşma hızı (1/sn).")]
        [SerializeField] private float hueSpeed = 10f;
        [Tooltip("Nota rengine döndür. Kapalıysa palet hep aynı kalır.")]
        [SerializeField] private bool rotateToNoteColor = true;

        private static readonly int FlowId = Shader.PropertyToID("_Flow");
        private static readonly int HitId = Shader.PropertyToID("_Hit");
        private static readonly int HueShiftId = Shader.PropertyToID("_HueShift");
        private static readonly int QuadSizeId = Shader.PropertyToID("_QuadSize");
        private static readonly int BodySizeId = Shader.PropertyToID("_BodySize");

        /// <summary>Squash/stretch oranı: şu anki ölçek / dinlenme ölçeği (x, y). Patlama bunu izleyerek ezilmiş pedin şeklinden doğar.</summary>
        public Vector2 SquashRatio => _restLocalScale.x > 0f && _restLocalScale.y > 0f
            ? new Vector2(transform.localScale.x / _restLocalScale.x, transform.localScale.y / _restLocalScale.y)
            : Vector2.one;

        /// <summary>Squash sırasında alt kenar sabit mi (patlamanın ofsetini belirler).</summary>
        public bool AnchorBottom => anchorBottom;

        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _block;
        private Vector3 _restLocalPosition;
        private Vector3 _restLocalScale;
        private Vector2 _bodySize;
        private float _flow;
        private float _hitAge = float.PositiveInfinity;
        private float _hueDegrees;
        private float _hueTarget;
        private bool _hasPendingNote;
        private NoteInfo _pendingNote;

        private void Awake()
        {
            EnsureInit();
            _restLocalPosition = transform.localPosition;
            _restLocalScale = transform.localScale;
            ApplyStaticShape();
            Apply();
        }

        private void EnsureInit()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            _block ??= new MaterialPropertyBlock();
            if (pad == null) pad = GetComponentInParent<LaunchPad>();
        }

        /// <summary>Edit Mode: akış sabit (0), boyut/şekil güncel; ölçek/konum/collider değişince yeniden gönderilir.</summary>
        private void RefreshEditMode()
        {
            EnsureInit();
            if (_renderer == null) return;
            ApplyStaticShape();
            Apply();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && isActiveAndEnabled) RefreshEditMode();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) RefreshEditMode();
            if (pad == null) return;
            pad.NotePlayed += OnNotePlayed;
            pad.Launched += OnLaunched;
        }

        private void OnDisable()
        {
            if (pad == null) return;
            pad.NotePlayed -= OnNotePlayed;
            pad.Launched -= OnLaunched;
        }

        private void OnNotePlayed(NoteInfo note)
        {
            _pendingNote = note;
            _hasPendingNote = true;
        }

        private void OnLaunched()
        {
            _hitAge = 0f;
            // NotePlayed, Launched'tan hemen önce gelir; nota çalmadıysa palet nota rengine kaymaz.
            _hueTarget = rotateToNoteColor && _hasPendingNote
                ? LiquidLightMath.HueShiftDegrees(HueOf(_pendingNote.Color))
                : 0f;
            _hasPendingNote = false;
        }

        private void Update()
        {
            if (!Application.isPlaying) { RefreshEditMode(); return; }
            float dt = Time.deltaTime;
            if (!float.IsPositiveInfinity(_hitAge)) _hitAge += dt;

            float env = LiquidLightMath.Envelope(_hitAge, attack, decay);
            _flow = LiquidLightMath.AdvanceFlow(_flow, dt, idleFlowSpeed, hitFlowBoost, env);

            // Zarf söndüğünde palet de eski haline döner; vuruş sürerken hedefe yaklaşır.
            float target = env > 0.01f ? _hueTarget : 0f;
            _hueDegrees = LiquidLightMath.SmoothHue(_hueDegrees, target, dt, hueSpeed);

            Apply(env);
        }

        private void LateUpdate()
        {
            // LaunchPad tween'i Update'te ölçeği değiştirir; konumu burada telafi ederiz (alt kenar sabit).
            if (!Application.isPlaying || !anchorBottom || _restLocalScale.y <= 0f) return;
            float k = transform.localScale.y / _restLocalScale.y;
            transform.localPosition = _restLocalPosition + new Vector3(0f, _bodySize.y * 0.5f * (k - 1f), 0f);
        }

        private void Apply(float env = 0f)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(FlowId, _flow);
            _block.SetFloat(HitId, env);
            _block.SetFloat(HueShiftId, _hueDegrees * Mathf.Deg2Rad);
            _renderer.SetPropertyBlock(_block);
        }

        /// <summary>
        /// Gövde boyutu: elle verilmediyse pad'in collider'ından (dünya birimi): kutu = boyut, daire = çap,
        /// diğerleri (çokgen) = sınır kutusu.
        /// </summary>
        internal static Vector2 ResolveBodySize(LaunchPad pad, Vector2 manual)
        {
            if (manual != Vector2.zero || pad == null) return manual;

            Vector3 s = pad.transform.lossyScale;
            if (pad.TryGetComponent(out BoxCollider2D box))
                return new Vector2(box.size.x * Mathf.Abs(s.x), box.size.y * Mathf.Abs(s.y));
            if (pad.TryGetComponent(out CircleCollider2D circle))
            {
                float d = 2f * circle.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
                return new Vector2(d, d);
            }
            if (pad.TryGetComponent(out Collider2D any))
                return any.bounds.size;
            return manual;
        }

        /// <summary>Quad ve gövde boyutunu bir kez gönderir (dinlenme ölçeği).</summary>
        private void ApplyStaticShape()
        {
            Vector3 scale = transform.lossyScale;
            Vector2 body = ResolveBodySize(pad, bodySize);
            _bodySize = body;

            _renderer.GetPropertyBlock(_block);
            _block.SetVector(QuadSizeId, new Vector4(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 0f, 0f));
            if (body != Vector2.zero) _block.SetVector(BodySizeId, new Vector4(body.x, body.y, 0f, 0f));
            _renderer.SetPropertyBlock(_block);
        }

        internal static float HueOf(Color c)
        {
            Color.RGBToHSV(c, out float h, out _, out _);
            return h;
        }
    }
}
