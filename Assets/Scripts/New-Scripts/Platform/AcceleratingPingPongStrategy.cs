using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// A movement strategy where the platform starts at 0 speed and accelerates until the end point,
    /// waits for a delay at the end point, and then returns while decelerating to 0 speed at the start point.
    /// </summary>
    public class AcceleratingPingPongStrategy : MovementStrategy
    {
        [Header("Path Settings")]
        [Tooltip("The starting position transform.")]
        [SerializeField] private Transform startTransform;

        [Tooltip("The destination position transform.")]
        [SerializeField] private Transform endTransform;

        [Header("Timing Settings")]
        [Tooltip("Time in seconds to travel from start to end (accelerating).")]
        [SerializeField] private float forwardDuration = 2f;

        [Tooltip("Time in seconds to travel from end to start (decelerating).")]
        [SerializeField] private float returnDuration = 2f;

        [Tooltip("Delay in seconds at the end point before returning.")]
        [SerializeField] private float endDelay = 1f;

        [Tooltip("Initial time offset to shift the movement phase.")]
        [SerializeField] private float phaseOffset;

        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private bool _isInitialized;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void InitializeIfNeeded()
        {
            if (_isInitialized) return;

            if (startTransform != null && endTransform != null)
            {
                _startPosition = startTransform.position;
                _endPosition = endTransform.position;
                _isInitialized = true;
            }
            else
            {
                _startPosition = transform.position;
                _endPosition = (Vector2)transform.position + Vector2.right * 5f;
                _isInitialized = true;
                Debug.LogWarning("AcceleratingPingPongStrategy: Missing start or end transform! Using default fallback path.", this);
            }
        }

        public override Vector2 GetPositionAtTime(float time)
        {
            InitializeIfNeeded();

            // Get current positions, fallback to cached values if transforms are missing
            Vector2 currentStart = startTransform != null ? (Vector2)startTransform.position : _startPosition;
            Vector2 currentEnd = endTransform != null ? (Vector2)endTransform.position : _endPosition;

            float loopDuration = forwardDuration + endDelay + returnDuration;
            if (loopDuration <= 0f) return currentStart;

            float evaluatedTime = (time + phaseOffset) % loopDuration;
            if (evaluatedTime < 0f) evaluatedTime += loopDuration;

            // 1. Forward trip: Accelerating from start to end [0, forwardDuration]
            if (evaluatedTime <= forwardDuration)
            {
                if (forwardDuration <= 0f) return currentEnd;
                float t = evaluatedTime / forwardDuration;
                float tSquared = t * t; // Constant acceleration profile
                return Vector2.Lerp(currentStart, currentEnd, tSquared);
            }

            // 2. End delay: Staying at end point [forwardDuration, forwardDuration + endDelay]
            float endTripTime = forwardDuration + endDelay;
            if (evaluatedTime <= endTripTime)
            {
                return currentEnd;
            }

            // 3. Return trip: Decelerating from end to start [endTripTime, loopDuration]
            float returnTime = evaluatedTime - endTripTime;
            if (returnDuration <= 0f) return currentStart;
            float tReturn = returnTime / returnDuration;
            
            // Deceleration profile (starts fast at endPosition, slows to 0 speed at startPosition)
            float tDecel = 1f - tReturn;
            float tDecelSquared = tDecel * tDecel;
            return Vector2.Lerp(currentStart, currentEnd, tDecelSquared);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;

            Vector2 currentStart = startTransform != null ? (Vector2)startTransform.position : (Vector2)transform.position;
            Vector2 currentEnd = endTransform != null ? (Vector2)endTransform.position : (Vector2)transform.position + Vector2.right * 5f;

            // Draw thin line for path
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
            Gizmos.DrawLine(currentStart, currentEnd);

            // Draw platform shapes at start and end points
            DrawPlatformPreview(currentStart, new Color(0f, 1f, 0f, 0.5f));
            DrawPlatformPreview(currentEnd, new Color(1f, 0f, 0f, 0.5f));
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 currentStart = startTransform != null ? (Vector2)startTransform.position : (Vector2)transform.position;
            Vector2 currentEnd = endTransform != null ? (Vector2)endTransform.position : (Vector2)transform.position + Vector2.right * 5f;

            // Draw spheres at start and end points
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentStart, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentEnd, 0.1f);

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
