using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class CameraOverrideZone : MonoBehaviour, ICameraOverrideProvider
    {
        [SerializeField] private int priority = 10;
        [SerializeField] private CameraOverrideSettings settings;

        private bool isActiveZone = false;
        private Collider2D triggerCollider;

        public int Priority => priority;
        public bool IsActive => isActiveZone;
        public CameraOverrideSettings Settings => settings;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            // Sınırlandırıcının trigger olduğundan emin olalım
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                isActiveZone = true;
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.RegisterOverride(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                isActiveZone = false;
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.UnregisterOverride(this);
                }
            }
        }

        private void OnDisable()
        {
            // Obje veya sahne deaktif edildiğinde kaydı temizleyelim
            if (isActiveZone)
            {
                isActiveZone = false;
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.UnregisterOverride(this);
                }
            }
        }

#if UNITY_EDITOR
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
                if (settings.focusType == CameraFocusType.StaticPosition)
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
