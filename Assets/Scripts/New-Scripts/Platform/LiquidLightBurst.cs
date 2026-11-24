using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Bounce pad'in kaleydoskop patlamasını sürer (<c>Sprites/LiquidLightBurst</c> shader'ı): fırlatmada pedin şeklini
    /// takip ederek her yöne açılan sıvı ışık dalgası. Simetri sayısı notanın enstrümanından (3/4/5/6), renk notanın
    /// rengine kayar. Pad'in Visual'ından AYRI, büyük bir quad'dedir (squash/stretch'ten etkilenmez); patlama
    /// bitince renderer kapanır. Flaş şiddeti paylaşılan <see cref="FlashLimiter"/>'dan gelir: WCAG 2.3.1,
    /// saniyede en fazla 2 tam şiddetli flaş, fazlası kısık oynar.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LiquidLightBurst : MonoBehaviour
    {
        [SerializeField] private LaunchPad pad;

        [Tooltip("Gövde boyutu. Boşsa pad'in BoxCollider2D boyutu alınır.")]
        [SerializeField] private Vector2 bodySize;

        [Header("Timing")]
        [Tooltip("Patlamanın ömrü (sn); shader'daki _BurstLife ile aynı olmalı.")]
        [SerializeField] private float life = 1.3f;
        [Tooltip("Mockup: 0.05 sn (patlama zamanı 0.75× yavaşlatılmış).")]
        [SerializeField] private float attack = 0.05f;
        [Tooltip("Mockup: 0.22.")]
        [SerializeField] private float decay = 0.22f;
        [SerializeField] private float flowSpeed = 1.5f;

        [Header("Front speed (dünya birimi / sn)")]
        [Tooltip("Mockup: 270 px/sn = 5.082 birim/sn.")]
        [SerializeField] private float baseSpeed = 5.082f;
        [Tooltip("Üst oktav notada ek hız. Mockup: +60 px/sn = 1.129 birim/sn.")]
        [SerializeField] private float octaveSpeedBonus = 1.129f;

        [Header("Flash safety (WCAG 2.3.1)")]
        [Tooltip("Bir saniyelik pencerede izin verilen tam şiddetli patlama sayısı (3'ten az).")]
        [SerializeField] private int maxFullFlashesPerSecond = 2;
        [Tooltip("Sınır aşılınca patlamanın şiddet çarpanı.")]
        [Range(0f, 1f)] [SerializeField] private float reducedIntensity = 0.3f;

        [Header("Hue")]
        [SerializeField] private bool rotateToNoteColor = true;

        private static readonly int SpeedId = Shader.PropertyToID("_BurstSpeed");
        private static readonly int AgeId = Shader.PropertyToID("_BurstAge");
        private static readonly int EnvId = Shader.PropertyToID("_BurstEnv");
        private static readonly int SymmetryId = Shader.PropertyToID("_BurstSymmetry");
        private static readonly int FlowId = Shader.PropertyToID("_Flow");
        private static readonly int HueShiftId = Shader.PropertyToID("_HueShift");
        private static readonly int QuadSizeId = Shader.PropertyToID("_QuadSize");
        private static readonly int BodySizeId = Shader.PropertyToID("_BodySize");
        private static readonly int BodyOffsetId = Shader.PropertyToID("_BodyOffset");

        // Pad'ler arası ortak: art arda farklı pad'lere vuran oyuncunun toplam flaşı sınırlanır.
        private static FlashLimiter _limiter;
        private static int _limiterMax;

        private SpriteRenderer _renderer;
        private LiquidLightPad _bodyVisual;
        private Vector2 _restBody;
        private MaterialPropertyBlock _block;
        private float _age = float.PositiveInfinity;
        private float _intensity = 1f;
        private float _flow;
        private int _symmetry = 3;
        private float _speed;
        private float _hueRadians;
        private bool _hasPendingNote;
        private NoteInfo _pendingNote;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
            if (pad == null) pad = GetComponentInParent<LaunchPad>();

            Vector3 scale = transform.lossyScale;
            Vector2 body = LiquidLightPad.ResolveBodySize(pad, bodySize);
            _restBody = body;
            if (pad != null) _bodyVisual = pad.GetComponentInChildren<LiquidLightPad>();
            _renderer.GetPropertyBlock(_block);
            _block.SetVector(QuadSizeId, new Vector4(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 0f, 0f));
            if (body != Vector2.zero) _block.SetVector(BodySizeId, new Vector4(body.x, body.y, 0f, 0f));
            _renderer.SetPropertyBlock(_block);

            _speed = baseSpeed;
            _renderer.enabled = false;
        }

        private void OnEnable()
        {
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
            if (_limiter == null || _limiterMax != maxFullFlashesPerSecond)
            {
                _limiterMax = maxFullFlashesPerSecond;
                _limiter = new FlashLimiter(maxFullFlashesPerSecond, 1f, reducedIntensity);
            }

            _intensity = _limiter.Acquire(Time.unscaledTime);
            _age = 0f;

            if (_hasPendingNote)
            {
                _symmetry = LiquidLightMath.BurstSymmetry(_pendingNote.Instrument);
                _speed = baseSpeed + (_pendingNote.Octave > 0 ? octaveSpeedBonus : 0f);
                _hueRadians = rotateToNoteColor
                    ? LiquidLightMath.HueShiftDegrees(LiquidLightPad.HueOf(_pendingNote.Color)) * Mathf.Deg2Rad
                    : 0f;
            }
            else
            {
                _hueRadians = 0f; // nota çalmadıysa simetri bir öncekinde kalır, renk varsayılan palet
            }
            _hasPendingNote = false;
            _renderer.enabled = true;
        }

        private void Update()
        {
            if (!_renderer.enabled) return;

            float dt = Time.deltaTime;
            _age += dt;
            if (_age > life)
            {
                _renderer.enabled = false;
                return;
            }

            _flow += dt * flowSpeed;
            float env = LiquidLightMath.BurstEnvelope(_age, attack, decay) * _intensity;

            _renderer.GetPropertyBlock(_block);
            // Mockup: patlama o anki (ezilmiş) ped şeklinden doğar; alt kenar sabitse merkez aşağı kayar.
            Vector2 ratio = _bodyVisual != null ? _bodyVisual.SquashRatio : Vector2.one;
            Vector2 body = new Vector2(_restBody.x * ratio.x, _restBody.y * ratio.y);
            float offsetY = _bodyVisual != null && _bodyVisual.AnchorBottom ? _restBody.y * 0.5f * (ratio.y - 1f) : 0f;
            _block.SetVector(BodySizeId, new Vector4(body.x, body.y, 0f, 0f));
            _block.SetVector(BodyOffsetId, new Vector4(0f, offsetY, 0f, 0f));
            _block.SetFloat(SpeedId, _speed);
            _block.SetFloat(AgeId, _age);
            _block.SetFloat(EnvId, env);
            _block.SetFloat(SymmetryId, _symmetry);
            _block.SetFloat(FlowId, _flow);
            _block.SetFloat(HueShiftId, _hueRadians);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
