using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Riso levha shader'ına (RisoSlab) platformun yumuşatılmış hızını ve ayarlarını verir.
    /// Hız aynı objedeki <see cref="IMovingSurface"/>'ten okunur; MaterialPropertyBlock kullanılır, materyal çoğalmaz.
    /// "Hızlanan platform" = zıplama desteği veren (JumpBoostMultiplier > 0) platform, yani AcceleratingPingPongStrategy.
    /// </summary>
    [DisallowMultipleComponent]
    public class RisoSlabVisual : MonoBehaviour
    {
        private static readonly int VelocityId = Shader.PropertyToID("_Velocity");
        private static readonly int AcceleratingId = Shader.PropertyToID("_Accelerating");
        private static readonly int MisregKId = Shader.PropertyToID("_MisregK");
        private static readonly int MisregMaxId = Shader.PropertyToID("_MisregMax");
        private static readonly int InfluencePosId = Shader.PropertyToID("_InfluencePos");
        private static readonly int InfluenceRadiusId = Shader.PropertyToID("_InfluenceRadius");
        private static readonly int InfluenceAmountId = Shader.PropertyToID("_InfluenceAmount");
        private static readonly int ShiftFrontId = Shader.PropertyToID("_ShiftFront");
        private static readonly int ShiftOriginId = Shader.PropertyToID("_ShiftOrigin");
        private static readonly int ShiftBandId = Shader.PropertyToID("_ShiftBand");

        [SerializeField] private RisoSlabSettingsSO settings;
        [SerializeField] private SpriteRenderer targetRenderer;

        private IMovingSurface _surface;
        private MaterialPropertyBlock _block;
        private Vector2 _smoothedVelocity;

        // Aşama 2: oyuncu teması
        private PlayerController _player;
        private Collider2D _collider;
        private float _nextPlayerSearch;
        private bool _wasTouching;
        private float _influenceAmount;
        private Vector2 _influencePos;

        // Mürekkep değişimi dalgası: köken platformun yerel uzayında saklanır (platform hareket etse de dalga ona yapışık)
        private float _shiftFront;
        private Vector2 _shiftOriginLocal;

        /// <summary>Halka yarıçapı (0 = kapalı). HalftoneTrailEmitter gecikmeli renk için okur.</summary>
        public float ShiftFront => _shiftFront;
        public float ShiftBandWidth => settings != null ? settings.shiftBandWidth : 0f;
        public Vector2 ShiftOriginWorld => transform.TransformPoint(_shiftOriginLocal);

        private void Awake()
        {
            _surface = GetComponent<IMovingSurface>();
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            // Oda uyanınca ve ölüm sıfırlamasında eski kayma kalmasın.
            _smoothedVelocity = Vector2.zero;
            _influenceAmount = 0f;
            _wasTouching = false;
            _shiftFront = 0f;
        }

        private void LateUpdate()
        {
            if (targetRenderer == null || settings == null) return;

            Vector2 target = _surface != null ? _surface.SurfaceVelocity : Vector2.zero;
            _smoothedVelocity = RisoSlabMath.SmoothVelocity(_smoothedVelocity, target, Time.deltaTime, settings.velocitySmoothTime);

            UpdateInfluence();

            targetRenderer.GetPropertyBlock(_block);
            _block.SetVector(VelocityId, _smoothedVelocity);
            _block.SetFloat(AcceleratingId, _surface != null && _surface.JumpBoostMultiplier > 0f ? 1f : 0f);
            _block.SetFloat(MisregKId, settings.misregVelocityK);
            _block.SetFloat(MisregMaxId, settings.misregMax);
            _block.SetVector(InfluencePosId, _influencePos);
            _block.SetFloat(InfluenceRadiusId, settings.influenceRadius);
            _block.SetFloat(InfluenceAmountId, _influenceAmount);
            _block.SetFloat(ShiftFrontId, _shiftFront);
            _block.SetVector(ShiftOriginId, ShiftOriginWorld);
            _block.SetFloat(ShiftBandId, settings.shiftBandWidth);
            targetRenderer.SetPropertyBlock(_block);
        }

        /// <summary>
        /// Oyuncu bu yüzeye değiyor mu (zemin ya da duvar teması)? Oyuncu koduna dokunulmaz,
        /// KinematicPhysicsHandler'ın bildiği yüzeyler okunur. Temas noktası: platform collider'ının oyuncuya en yakın noktası.
        /// </summary>
        private void UpdateInfluence()
        {
            bool touching = false;
            if (_surface != null)
            {
                if (_player == null && Time.unscaledTime >= _nextPlayerSearch)
                {
                    _player = FindFirstObjectByType<PlayerController>();
                    _nextPlayerSearch = Time.unscaledTime + 0.5f;
                }

                KinematicPhysicsHandler handler = _player != null ? _player.PhysicsHandler : null;
                if (handler != null)
                {
                    touching = ReferenceEquals(handler.CurrentMovingSurface, _surface)
                               || ReferenceEquals(handler.CurrentLeftMovingSurface, _surface)
                               || ReferenceEquals(handler.CurrentRightMovingSurface, _surface);
                    if (touching)
                    {
                        Vector2 playerPos = handler.transform.position;
                        _influencePos = _collider != null ? _collider.ClosestPoint(playerPos) : playerPos;
                    }
                }
            }

            bool justLanded = touching && !_wasTouching;
            _wasTouching = touching;

            if (justLanded)
            {
                _shiftOriginLocal = transform.InverseTransformPoint(_influencePos);
                _shiftFront = 0.001f;
            }
            _shiftFront = RisoSlabMath.AdvanceFront(_shiftFront, Time.deltaTime, settings.shiftSpeed,
                settings.shiftMaxRadius, settings.shiftBandWidth);
            _influenceAmount = RisoSlabMath.StepInfluence(_influenceAmount, touching, justLanded, Time.deltaTime,
                settings.contactStandingAmount, settings.contactDecayTime);
        }
    }
}
