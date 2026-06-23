using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Player.Nodes.Rotation
{
    public enum RotationRestrictionMode
    {
        OncePerConnection, // Bağlantı başına bir kez (kopup tekrar bağlanınca sıfırlanır)
        OncePerNode        // Tüm oyun/bölüm boyunca sadece tek bir kez çalışır
    }
    
    /// <summary>
    /// Sahnede bir "döndürülebilir node" görevi gören MonoBehaviour.
    /// IFullRotationEffect implementasyonuna bağımlıdır; efektin ne olduğunu bilmez.
    /// 
    /// Yeni efekt eklemek için:
    ///   1. IFullRotationEffect'i implemente eden yeni bir sınıf yaz.
    ///   2. Bu GameObject'e veya başka bir GameObject'e component olarak ekle.
    ///   3. Inspector'dan `rotationEffect` alanına bağla.
    ///   Mevcut hiçbir koda dokunman gerekmez.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FullRotationNode : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Bu efekte ulaşmak için kaç derece dönülmesi gerekiyor?")]
        [SerializeField] private float targetDegrees = 360f;
        
        [Tooltip("Efektin tetiklenme kısıtlaması.")]
        [SerializeField] private RotationRestrictionMode restrictionMode = RotationRestrictionMode.OncePerConnection;
        
        [Tooltip("Bağlantı koptuğunda progress'in sıfıra geri dönüş hızı (saniye cinsinden, 2 = 0.5 saniyede sıfırlanır).")]
        [SerializeField] private float lerpBackSpeed = 2f;

        [Header("Outer Radius Growth")]
        [Tooltip("Başlangıç dış yarıçapı (0-1 aralığında).")]
        [SerializeField] private float startOuterRadius = 0.2f;

        [Tooltip("Bitiş dış yarıçapı (0-1 aralığında).")]
        [SerializeField] private float endOuterRadius = 0.5f;
        
        [Header("Referances")]
        [Tooltip("360 derece tamamlandığında ne olacağını belirleyen efekt bileşeni.")] 
        [SerializeField] private MonoBehaviour rotationEffectComponent;
        [SerializeField] private SpriteRenderer progressRingRenderer;

        private IFullRotationEffect _rotationEffect;
        
        private bool _hasTriggeredEver = false;
        private bool _hasTriggeredThisConnection = false;
        private Material _materialInstance;
        private bool _isConnected = false;
        private float _currentProgress = 0f;

        public float TargetDegrees => targetDegrees;
        
        public bool CanTrigger => !(_hasTriggeredEver && restrictionMode == RotationRestrictionMode.OncePerNode) && !_hasTriggeredThisConnection;

        private void Awake()
        {
            if (rotationEffectComponent == null)
            {
                Debug.LogError($"[FullRotationNode] '{name}': rotationEffectComponent atanmamış!", this);
                return;
            }

            _rotationEffect = rotationEffectComponent as IFullRotationEffect;

            if (_rotationEffect == null)
            {
                Debug.LogError(
                    $"[FullRotationNode] '{name}': '{rotationEffectComponent.GetType().Name}' " +
                    $"sınıfı IFullRotationEffect'i implemente etmiyor!",
                    this);
            }

            if (progressRingRenderer != null && progressRingRenderer.sharedMaterial != null)
            {
                _materialInstance = new Material(progressRingRenderer.sharedMaterial);
                progressRingRenderer.material = _materialInstance;
                SetMaterialProgress(0f);
            }
        }
        
        public void InitializeNodeConnection(Vector2 playerStartPosition, bool isClockwise)
        {
            _hasTriggeredThisConnection = false;
            _isConnected = true;

            if (progressRingRenderer != null)
            {
                // Eğer node OncePerNode modundaysa ve zaten tamamlandıysa, görseli gizlemek yerine aktif tutuyoruz
                if (restrictionMode == RotationRestrictionMode.OncePerNode && _hasTriggeredEver)
                {
                    progressRingRenderer.gameObject.SetActive(true);
                }
                else
                {
                    progressRingRenderer.gameObject.SetActive(CanTrigger);
                }
            }

            if (!CanTrigger || progressRingRenderer == null) return;

            Vector2 direction = (playerStartPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            progressRingRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            _currentProgress = 0f;
            SetMaterialProgress(0f);
            SetMaterialClockwise(isClockwise);
        }

        public void UpdateProgressVisual(float progress)
        {
            if (progressRingRenderer != null && CanTrigger)
            {
                _currentProgress = progress;
                SetMaterialProgress(progress);
            }
        }

        public void OnConnectionLost()
        {
            _isConnected = false;
        }

        private void Update()
        {
            if (!_isConnected && _currentProgress > 0f)
            {
                // Eğer node bir kez tetiklenebiliyorsa ve tetiklendiyse, 1.0 (dolu) kalmaya devam etsin
                if (restrictionMode == RotationRestrictionMode.OncePerNode && _hasTriggeredEver)
                {
                    return;
                }

                _currentProgress = Mathf.MoveTowards(_currentProgress, 0f, Time.deltaTime * lerpBackSpeed);
                SetMaterialProgress(_currentProgress);
            }
        }

        public void TriggerRotationEffect()
        {
            if (!CanTrigger) return;

            _hasTriggeredEver = true;
            _hasTriggeredThisConnection = true;

            _rotationEffect?.OnFullRotationCompleted();
            
            if (progressRingRenderer != null)
            {
                _currentProgress = 1f;
                SetMaterialProgress(1f);
            }
        }

        private void SetMaterialProgress(float progress)
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetFloat("_Progress", progress);
                _materialInstance.SetFloat("_OuterRadius", Mathf.Lerp(startOuterRadius, endOuterRadius, progress));
            }
        }

        private void SetMaterialClockwise(bool isClockwise)
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetFloat("_Clockwise", isClockwise ? 1f : 0f);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f, "360° Node");
        }
#endif
    }
}