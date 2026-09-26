using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Platformun dairesel yörüngede hareket etmesini ve isteğe göre yönlenmesini sağlayan hareket stratejisi.
    /// </summary>
    public class CircularMovementStrategy : MovementStrategy
    {
        [Header("Center Settings")]
        [Tooltip("Dairesel hareketin merkez transformu. Seçilmezse platformun başlangıç pozisyonu merkez kabul edilir.")]
        [SerializeField] private Transform centerTransform;
        
        [Tooltip("Dairesel hareketin yarıçapı.")]
        [SerializeField] private float radius = 5f;

        [Header("Movement Settings")]
        [Tooltip("Saniye cinsinden tam bir tur süresi.")]
        [SerializeField] private float period = 4f;

        public override float Period => period;

        [Tooltip("Saat yönünde mi yoksa saat yönünün tersinde mi döneceğini belirler.")]
        [SerializeField] private bool clockwise = true;

        [Tooltip("Derece cinsinden başlangıç açısı offseti.")]
        [Range(0f, 360f)]
        [SerializeField] private float initialAngle = 0f;

        [Header("Rotation Settings")]
        [Tooltip("True ise platform dünyadaki yönelimini korur (başlangıçtaki açısını korur). False ise dairesel hareketin merkezine doğru döner.")]
        [SerializeField] private bool keepUpright = true;

        [Tooltip("Dairesel hareketin merkezine bakarken uygulanan ek rotasyon ofseti (derece cinsinden). Örneğin, platformun üst yüzeyinin merkeze bakması için -90, merkezden dışarı bakması (yerçekiminin merkeze doğru olduğu durumlar) için 90 derece kullanılabilir.")]
        [SerializeField] private float rotationOffset = 0f;

        private Vector2 _centerPosition;
        private float _initialRotation;
        private bool _isInitialized;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (_isInitialized) return;

            if (centerTransform != null)
            {
                _centerPosition = centerTransform.position;
            }
            else
            {
                _centerPosition = transform.position;
            }
            
            _initialRotation = transform.eulerAngles.z;
            _isInitialized = true;
        }

        public override Vector2 GetPositionAtTime(float time)
        {
            if (!_isInitialized)
            {
                InitializeIfNeeded();
            }

            // Eğer centerTransform atandıysa, hareketli bir merkez olabileceği için pozisyonunu güncel tutuyoruz.
            Vector2 currentCenter = centerTransform != null ? (Vector2)centerTransform.position : _centerPosition;

            // Zaman bazlı açıyı hesapla (radyan cinsinden)
            float absPeriod = Mathf.Abs(period);
            float angularSpeed = (2f * Mathf.PI) / (absPeriod > 0f ? absPeriod : 1f);
            
            // Yön seçimi: saat yönünde açı azalır, tersinde artar.
            float directionSign = clockwise ? -1f : 1f;

            // Başlangıç açısını radyana çevir
            float initialAngleRad = initialAngle * Mathf.Deg2Rad;

            // Mevcut açı = Başlangıç açısı + (Hız * Zaman * Yön)
            float currentAngle = initialAngleRad + (angularSpeed * time * directionSign);

            // Yeni pozisyon
            float x = currentCenter.x + Mathf.Cos(currentAngle) * radius;
            float y = currentCenter.y + Mathf.Sin(currentAngle) * radius;

            return new Vector2(x, y);
        }

        public override float? GetRotationAtTime(float time)
        {
            if (!_isInitialized)
            {
                InitializeIfNeeded();
            }

            if (keepUpright)
            {
                // Dünyadaki başlangıç açısını koru.
                return _initialRotation;
            }
            else
            {
                // Merkez yönüne dönmeli (tidally locked).
                Vector2 currentPosition = GetPositionAtTime(time);
                Vector2 currentCenter = centerTransform != null ? (Vector2)centerTransform.position : _centerPosition;
                Vector2 directionToCenter = currentCenter - currentPosition;

                // Mathf.Atan2 ile açıyı bulalım (radyan -> derece)
                float angle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
                
                return angle + rotationOffset;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Editörde seçili değilken dairesel yörüngeyi çok silik bir şekilde gösterir.
            Vector2 center = centerTransform != null ? (Vector2)centerTransform.position : (Vector2)transform.position;
            DrawGizmoCircle(center, radius, new Color(0f, 1f, 1f, 0.1f));
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 center = centerTransform != null ? (Vector2)centerTransform.position : (Vector2)transform.position;

            // Dairesel hareketin merkez noktasını çiz
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(center, 0.15f);

            // Eğer centerTransform atanmışsa, bu obje ile merkez arasında bir çizgi çiz
            if (centerTransform != null)
            {
                Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
                Gizmos.DrawLine(transform.position, centerTransform.position);
            }

            // Dairesel hareket yörüngesini çiz (parlak turkuaz)
            DrawGizmoCircle(center, radius, Color.cyan);

            // Başlangıç açısına göre başlangıç pozisyonunu çiz (yeşil küre)
            float startAngleRad = initialAngle * Mathf.Deg2Rad;
            Vector2 startPos = center + new Vector2(Mathf.Cos(startAngleRad), Mathf.Sin(startAngleRad)) * radius;
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPos, 0.2f);
            Gizmos.DrawLine(center, startPos);

            // Başlangıç noktasında dönme yönünü gösteren bir ok çiz (kırmızı)
            Vector2 tangent = new Vector2(-Mathf.Sin(startAngleRad), Mathf.Cos(startAngleRad));
            if (clockwise)
            {
                tangent = -tangent;
            }
            Vector2 arrowEnd = startPos + tangent * 0.8f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, arrowEnd);
            
            // Ok başı
            Vector2 arrowHeadLeft = arrowEnd - tangent * 0.2f + new Vector2(-tangent.y, tangent.x) * 0.15f;
            Vector2 arrowHeadRight = arrowEnd - tangent * 0.2f - new Vector2(-tangent.y, tangent.x) * 0.15f;
            Gizmos.DrawLine(arrowEnd, arrowHeadLeft);
            Gizmos.DrawLine(arrowEnd, arrowHeadRight);
        }

        private void DrawGizmoCircle(Vector2 center, float radius, Color color)
        {
            Gizmos.color = color;
            int segments = 64;
            Vector2 lastPoint = center + new Vector2(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * 2f * Mathf.PI;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }
        }
#endif
    }
}
