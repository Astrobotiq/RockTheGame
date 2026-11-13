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

        [SerializeField] private RisoSlabSettingsSO settings;
        [SerializeField] private SpriteRenderer targetRenderer;

        private IMovingSurface _surface;
        private MaterialPropertyBlock _block;
        private Vector2 _smoothedVelocity;

        private void Awake()
        {
            _surface = GetComponent<IMovingSurface>();
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            // Oda uyanınca ve ölüm sıfırlamasında eski kayma kalmasın.
            _smoothedVelocity = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (targetRenderer == null || settings == null) return;

            Vector2 target = _surface != null ? _surface.SurfaceVelocity : Vector2.zero;
            _smoothedVelocity = RisoSlabMath.SmoothVelocity(_smoothedVelocity, target, Time.deltaTime, settings.velocitySmoothTime);

            targetRenderer.GetPropertyBlock(_block);
            _block.SetVector(VelocityId, _smoothedVelocity);
            _block.SetFloat(AcceleratingId, _surface != null && _surface.JumpBoostMultiplier > 0f ? 1f : 0f);
            _block.SetFloat(MisregKId, settings.misregVelocityK);
            _block.SetFloat(MisregMaxId, settings.misregMax);
            targetRenderer.SetPropertyBlock(_block);
        }
    }
}
