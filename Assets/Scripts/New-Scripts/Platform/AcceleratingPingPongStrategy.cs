using UnityEngine;
using New_Scripts.Player;

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

        [Header("Boost Settings")]
        [Tooltip("Platformdan zıplarken oyuncunun alacağı ek hız çarpanı.")]
        [SerializeField] private float jumpBoostMultiplier = 1.5f;

        [Header("Editor Trajectory Gizmo")]
        [Tooltip("Karakterin fizik ayarlarını barındıran ScriptableObject.")]
        [SerializeField] private PlayerStatsSO playerStats;

        [Tooltip("Yörünge çizgisinin uzunluğu (adım sayısı).")]
        [SerializeField] private int trajectorySteps = 60;

        [Tooltip("Simülasyondaki her adımın zaman aralığı (saniye).")]
        [SerializeField] private float stepDeltaTime = 0.02f;

        public override float Period => forwardDuration + endDelay + returnDuration;
        public override float JumpBoostMultiplier => jumpBoostMultiplier;

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

            // --- Trajectory Preview ---
            if (playerStats == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PlayerStatsSO");
                if (guids != null && guids.Length > 0)
                {
                    string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    playerStats = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(assetPath);
                }
            }

            if (playerStats == null) return;

            // Calculate peak platform velocities at the end point (where velocity magnitude is highest for both trips)
            Vector2 forwardVelocity = forwardDuration > 0f ? (currentEnd - currentStart) * (2f / forwardDuration) : Vector2.zero;
            Vector2 returnVelocity = returnDuration > 0f ? (currentStart - currentEnd) * (2f / returnDuration) : Vector2.zero;

            // Forward jump trajectory (Cyan)
            DrawTrajectory(currentEnd, forwardVelocity, new Color(0f, 0.8f, 1f, 0.8f), "Forward Jump Boost");

            // Return jump trajectory (Magenta/Pink)
            DrawTrajectory(currentEnd, returnVelocity, new Color(1f, 0.2f, 0.6f, 0.8f), "Return Jump Boost");
        }

        private void DrawTrajectory(Vector2 startPos, Vector2 platformVelocity, Color color, string labelText)
        {
            if (playerStats == null) return;

            Gizmos.color = color;
            Vector2 currentPos = startPos;
            
            // Initial velocity = base player jump velocity (upwards) + platform velocity boosted
            Vector2 currentVel = new Vector2(0f, playerStats.JumpVelocity) + platformVelocity * jumpBoostMultiplier;

            float gravity = playerStats.Gravity;
            float fallGravityMult = playerStats.FallGravityMultiplier;
            float jumpEarlyGravityMult = playerStats.JumpEndEarlyGravityMultiplier;
            float terminalVel = playerStats.TerminalVelocity;
            float airDrag = playerStats.AirDrag;

            Vector2 lastPos = currentPos;
            float maxHeight = startPos.y;
            Vector2 maxHeightPos = startPos;

            for (int i = 0; i < trajectorySteps; i++)
            {
                // Determine gravity multiplier
                float gravityMultiplier = 1f;
                if (currentVel.y < 0f)
                {
                    gravityMultiplier = fallGravityMult;
                }
                else if (currentVel.y > 0f)
                {
                    // Since bypassJumpGravity is true, endEarlyGravityMultiplier is 0.5f
                    gravityMultiplier = jumpEarlyGravityMult * 0.5f;
                }

                // Apply gravity
                float gravityStep = gravity * gravityMultiplier * stepDeltaTime;
                currentVel.y += gravityStep;
                currentVel.y = Mathf.Max(currentVel.y, terminalVel);

                // Apply air drag (without input)
                currentVel.x = Mathf.MoveTowards(currentVel.x, 0f, airDrag * stepDeltaTime);

                // Update position
                currentPos += currentVel * stepDeltaTime;

                // Draw line segment
                Gizmos.DrawLine(lastPos, currentPos);

                if (currentPos.y > maxHeight)
                {
                    maxHeight = currentPos.y;
                    maxHeightPos = currentPos;
                }

                // Draw dots at intervals
                if (i % 4 == 0)
                {
                    Gizmos.DrawSphere(currentPos, 0.05f);
                }

                lastPos = currentPos;
            }

            // Draw a marker at the peak of the jump
            Gizmos.color = new Color(color.r, color.g, color.b, 0.5f);
            Gizmos.DrawWireSphere(maxHeightPos, 0.15f);
            
            // Draw a dashed or thin line at the max height
            Gizmos.DrawLine(new Vector2(maxHeightPos.x - 0.5f, maxHeight), new Vector2(maxHeightPos.x + 0.5f, maxHeight));

            // Use UnityEditor.Handles to draw text
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            
            float relativeHeight = maxHeight - startPos.y;
            string displayText = $"{labelText} (Max Height: {relativeHeight:F2}m)";
            UnityEditor.Handles.Label(maxHeightPos + Vector2.up * 0.25f, displayText, style);
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
