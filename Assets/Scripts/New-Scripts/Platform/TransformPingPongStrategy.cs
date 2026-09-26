using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Baslangic ve bitis Transform nesnelerinin pozisyonlarini baslangicta onbellege alarak, iki nokta arasinda zamana bagli dogrusal hareket saglayan strateji.
    /// </summary>
    public class TransformPingPongStrategy : MovementStrategy
    {
        [SerializeField] private Transform startTransform;
        [SerializeField] private Transform endTransform;
        [SerializeField] private float period = 1f;
        [SerializeField] private float phaseOffset;

        public override float Period => period;

        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private bool _isInitialized;

        private void Awake()
        {
            if (startTransform != null && endTransform != null)
            {
                _startPosition = startTransform.position;
                _endPosition = endTransform.position;
                _isInitialized = true;
            }
            else
            {
                Debug.LogError("TransformPingPongStrategy: Baslangic veya bitis Transform referanslari atanmadi!", this);
            }
        }

        public override Vector2 GetPositionAtTime(float time)
        {
            if (!_isInitialized)
                return Vector2.zero;

            float evaluatedTime = (time + phaseOffset) % period;
            float t = Mathf.PingPong(evaluatedTime, period / 2f) / (period / 2f);

            return Vector2.Lerp(_startPosition, _endPosition, t);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector2 currentStart, currentEnd;
            if (Application.isPlaying)
            {
                if (!_isInitialized) return;
                currentStart = _startPosition;
                currentEnd = _endPosition;
            }
            else
            {
                if (startTransform == null || endTransform == null) return;
                currentStart = startTransform.position;
                currentEnd = endTransform.position;
            }

            // Draw a thin, semi-transparent path line
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawLine(currentStart, currentEnd);

            // Draw platform previews at start and end points
            DrawPlatformPreview(currentStart, new Color(0f, 1f, 0f, 0.25f));
            DrawPlatformPreview(currentEnd, new Color(1f, 0f, 0f, 0.25f));
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 currentStart, currentEnd;
            if (Application.isPlaying)
            {
                if (!_isInitialized) return;
                currentStart = _startPosition;
                currentEnd = _endPosition;
            }
            else
            {
                if (startTransform == null || endTransform == null) return;
                currentStart = startTransform.position;
                currentEnd = endTransform.position;
            }

            // Draw spheres at start and end points
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentStart, 0.15f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentEnd, 0.15f);

            // Draw line connecting them
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(currentStart, currentEnd);

            // Draw direction arrow at the center
            Vector2 path = currentEnd - currentStart;
            if (path.magnitude > 0.1f)
            {
                Vector2 middle = currentStart + path * 0.5f;
                Vector2 direction = path.normalized;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(middle, direction * 0.5f);
                
                // Draw arrowhead
                Vector2 right = new Vector2(-direction.y, direction.x);
                Gizmos.DrawLine(middle + direction * 0.5f, middle + direction * 0.3f + right * 0.15f);
                Gizmos.DrawLine(middle + direction * 0.5f, middle + direction * 0.3f - right * 0.15f);
            }

            // Draw text labels
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 10;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            UnityEditor.Handles.Label(currentStart + Vector2.up * 0.4f, "Start Point", labelStyle);
            UnityEditor.Handles.Label(currentEnd + Vector2.up * 0.4f, "End Point", labelStyle);

            float distance = Vector2.Distance(currentStart, currentEnd);
            Vector2 textPos = currentStart + path * 0.5f + Vector2.up * 0.25f;
            string distanceText = $"Distance: {distance:F2}m\nPeriod: {period}s";
            
            GUIStyle infoStyle = new GUIStyle(labelStyle);
            infoStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            infoStyle.fontSize = 9;
            UnityEditor.Handles.Label(textPos, distanceText, infoStyle);
        }

        private void DrawPlatformPreview(Vector2 position, Color color)
        {
            Color oldColor = Gizmos.color;
            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.color = color;

            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null) boxCollider = GetComponentInChildren<BoxCollider2D>();
            if (boxCollider == null) boxCollider = GetComponentInParent<BoxCollider2D>();

            if (boxCollider != null)
            {
                Matrix4x4 relMatrix = transform.worldToLocalMatrix * boxCollider.transform.localToWorldMatrix;
                Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, transform.lossyScale) * relMatrix;
                Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
                
                Gizmos.color = oldColor;
                Gizmos.matrix = oldMatrix;
                return;
            }

            Collider2D generalCollider = GetComponent<Collider2D>();
            if (generalCollider == null) generalCollider = GetComponentInChildren<Collider2D>();
            if (generalCollider == null) generalCollider = GetComponentInParent<Collider2D>();

            if (generalCollider != null)
            {
                Vector3 offset = generalCollider.bounds.center - transform.position;
                Gizmos.DrawWireCube((Vector3)position + offset, generalCollider.bounds.size);
                
                Gizmos.color = oldColor;
                Gizmos.matrix = oldMatrix;
                return;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Vector3 offset = spriteRenderer.bounds.center - transform.position;
                Gizmos.DrawWireCube((Vector3)position + offset, spriteRenderer.bounds.size);
                
                Gizmos.color = oldColor;
                Gizmos.matrix = oldMatrix;
                return;
            }

            // Fallback: draw generic platform bounds
            Gizmos.DrawWireCube((Vector3)position, new Vector3(2f, 0.5f, 0f));

            Gizmos.color = oldColor;
            Gizmos.matrix = oldMatrix;
        }
#endif
    }
}