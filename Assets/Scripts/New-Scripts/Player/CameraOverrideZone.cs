using New_Scripts.LevelChange;
using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class CameraOverrideZone : MonoBehaviour, IPositionCameraOverride
    {
        [SerializeField] private int priority = 10;
        [SerializeField] private CameraOverrideSettings settings;

        [Tooltip("İçerideyken oyuncu collider'dan bu kadar uzaklaşana kadar bölge aktif kalır (sınırda titremeyi önler).")]
        [SerializeField, Min(0f)] private float exitMargin = 1f;

        private bool isActiveZone = false;
        private Collider2D triggerCollider;

        public int Priority => priority;
        public bool IsActive => isActiveZone;
        public CameraOverrideSettings Settings => settings;
        public Transform FrameSpace => transform;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            // Yalnızca alan tanımı; fiziksel çarpışma olmasın. Giriş/çıkış olayı değil, konum kullanılır.
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            Register();
        }

        private void Start()
        {
            // Sahne yüklenirken bölge, CameraController.Awake'ten önce açılmış olabilir.
            Register();
        }

        private void OnDisable()
        {
            // Uyuyan odanın bölgesi kayıttan çıkar (aktif bayrağıyla birlikte).
            isActiveZone = false;
            if (CameraController.Instance != null)
            {
                CameraController.Instance.UnregisterOverride(this);
            }
        }

        private void Register()
        {
            if (CameraController.Instance != null)
            {
                CameraController.Instance.RegisterOverride(this);
            }
        }

        public void UpdateActive(Vector2 playerPosition)
        {
            if (triggerCollider == null || !triggerCollider.enabled)
            {
                isActiveZone = false;
                return;
            }

            bool inside = triggerCollider.OverlapPoint(playerPosition);
            float distanceOutside = 0f;
            if (!inside && isActiveZone)
            {
                distanceOutside = Vector2.Distance(playerPosition, triggerCollider.ClosestPoint(playerPosition));
            }

            isActiveZone = CameraZoneMath.IsActive(isActiveZone, inside, distanceOutside, exitMargin);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Çerçeve (sarı), kameranın gerçekte göstereceği alan (16:9, padding dahil; cyan) ve oda sınırı (gri).
        /// Kamera görüşü oda sınırını aşıyorsa turuncu: hata değil, bilgi — taşan yerde uyuyan oda/boşluk görünmesin.
        /// </summary>
        private void DrawFrameGizmos()
        {
            Rect worldFrame = CameraFraming.ToWorldRect(transform, settings.frame);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(worldFrame.center, worldFrame.size);

            (Vector2 center, float size) fit = CameraFraming.Fit(worldFrame, 16f / 9f, settings.framePadding);
            if (settings.overrideZoom) fit.size = settings.cameraSize;
            Vector2 viewSize = new Vector2(fit.size * 2f * (16f / 9f), fit.size * 2f);

            bool exceedsRoom = false;
            Room room = GetComponentInParent<Room>();
            if (room != null && room.RoomBounds != null)
            {
                Bounds rb = room.RoomBounds.bounds;
                Gizmos.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                Gizmos.DrawWireCube(rb.center, rb.size);

                Rect view = new Rect(fit.center - viewSize * 0.5f, viewSize);
                exceedsRoom = view.xMin < rb.min.x || view.xMax > rb.max.x || view.yMin < rb.min.y || view.yMax > rb.max.y;
            }

            Gizmos.color = exceedsRoom ? new Color(1f, 0.55f, 0f) : Color.cyan;
            Gizmos.DrawWireCube(fit.center, viewSize);
        }

        private void OnDrawGizmos()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) return;

            // Bölge sınırlarını çizelim
            Gizmos.color = isActiveZone ? Color.green : new Color(0f, 0.6f, 1f, 0.5f);

            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube((Vector3)box.offset, (Vector3)box.size);
            }
            else
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }

            Gizmos.matrix = Matrix4x4.identity;

            // Odak noktalarını editörde görselleştirelim
            if (settings != null)
            {
                Vector3 center = col.bounds.center;
                if (settings.focusType == CameraFocusType.FrameArea)
                {
                    DrawFrameGizmos();
                }
                else if (settings.focusType == CameraFocusType.StaticPosition)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere((Vector3)settings.staticFocusPosition, 0.6f);
                    Gizmos.DrawLine(center, (Vector3)settings.staticFocusPosition);
                }
                else if (settings.focusType == CameraFocusType.TargetTransform && settings.targetFocusTransform != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(settings.targetFocusTransform.position, 0.6f);
                    Gizmos.DrawLine(center, settings.targetFocusTransform.position);
                }
                else if (settings.focusType == CameraFocusType.PlayerAndTransform && settings.targetFocusTransform != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(settings.targetFocusTransform.position, 0.6f);
                    Gizmos.DrawLine(center, settings.targetFocusTransform.position);
                }
            }
        }
#endif
    }
}
