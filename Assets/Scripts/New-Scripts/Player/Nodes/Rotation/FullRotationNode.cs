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
        
        [Header("Referances")]
        [Tooltip("360 derece tamamlandığında ne olacağını belirleyen efekt bileşeni.")] 
        [SerializeField] private MonoBehaviour rotationEffectComponent;
        [SerializeField] private Image progressRingImage;

        private IFullRotationEffect _rotationEffect;
        
        private bool _hasTriggeredEver = false;
        private bool _hasTriggeredThisConnection = false;

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
        }
        
        public void InitializeNodeConnection(Vector2 playerStartPosition, bool isClockwise)
        {
            _hasTriggeredThisConnection = false;

            if (progressRingImage != null)
            {
                progressRingImage.gameObject.SetActive(CanTrigger);
            }

            if (!CanTrigger || progressRingImage == null) return;

            Vector2 direction = (playerStartPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            progressRingImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            progressRingImage.fillAmount = 0f;
            progressRingImage.fillClockwise = isClockwise; 
        }

        public void UpdateProgressVisual(float progress)
        {
            if (progressRingImage != null && CanTrigger)
            {
                progressRingImage.fillAmount = progress;
            }
        }

        public void TriggerRotationEffect()
        {
            if (!CanTrigger) return;

            _hasTriggeredEver = true;
            _hasTriggeredThisConnection = true;

            _rotationEffect?.OnFullRotationCompleted();
            
            if (progressRingImage != null)
            {
                progressRingImage.fillAmount = 0f;
                if (restrictionMode == RotationRestrictionMode.OncePerNode)
                {
                    progressRingImage.gameObject.SetActive(false);
                }
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