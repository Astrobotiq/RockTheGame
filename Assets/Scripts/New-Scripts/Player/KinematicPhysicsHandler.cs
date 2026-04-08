using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin kinematik çarpışma testlerini, hız filtrelemesini ve sensör durumlarını tahsissiz bellek yönetimiyle sağlayan fizik modülü.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class KinematicPhysicsHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float skinWidth = 0.02f;
        [SerializeField] private float groundedDistance = 0.05f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingLeftWall { get; private set; }
        public bool IsTouchingRightWall { get; private set; }
        public bool IsTouchingCeiling { get; private set; }

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        public Vector2 FilterVelocity(Vector2 desiredVelocity)
        {
            Vector2 position = body.position;
            Vector2 movement = desiredVelocity * Time.fixedDeltaTime;

            movement = ResolveHorizontalCollisions(position, movement);
            position.x += movement.x;

            movement = ResolveVerticalCollisions(position, movement);
            position.y += movement.y; 

            UpdateSensors(position);

            return movement / Time.fixedDeltaTime;
        }

        private Vector2 ResolveHorizontalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.x) < 0.001f) return movement;

            float directionX = Mathf.Sign(movement.x);
            float distance = Mathf.Abs(movement.x) + skinWidth;
            
            Vector2 boxSize = boxCollider.bounds.size;
            boxSize.y -= 0.1f;

            int hitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, boxSize, 0f, new Vector2(directionX, 0f), hitBuffer, distance, groundLayerMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (hitBuffer[i].collider.isTrigger) continue;
                if (hitBuffer[i].distance < minDistance)
                {
                    minDistance = hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit)
            {
                movement.x = (minDistance - skinWidth) * directionX;
            }

            return movement;
        }

        private Vector2 ResolveVerticalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.y) < 0.001f) return movement;

            float directionY = Mathf.Sign(movement.y);
            float distance = Mathf.Abs(movement.y) + skinWidth;
            
            Vector2 boxSize = boxCollider.bounds.size;
            boxSize.x -= 0.1f;

            int hitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, boxSize, 0f, new Vector2(0f, directionY), hitBuffer, distance, groundLayerMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (hitBuffer[i].collider.isTrigger) continue;
                if (hitBuffer[i].distance < minDistance)
                {
                    minDistance = hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit)
            {
                movement.y = (minDistance - skinWidth) * directionY;
            }

            return movement;
        }

        private void UpdateSensors(Vector2 position)
        {
            Vector2 horizontalBoxSize = boxCollider.bounds.size;
            horizontalBoxSize.y -= 0.1f;
            
            int leftHitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, horizontalBoxSize, 0f, Vector2.left, hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingLeftWall = HasValidSensorHit(leftHitCount);

            int rightHitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, horizontalBoxSize, 0f, Vector2.right, hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingRightWall = HasValidSensorHit(rightHitCount);

            Vector2 verticalBoxSize = boxCollider.bounds.size;
            verticalBoxSize.x -= 0.1f;

            int groundHitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, verticalBoxSize, 0f, Vector2.down, hitBuffer, groundedDistance, groundLayerMask);
            IsGrounded = CheckGroundedValidHit(groundHitCount);
            
            int ceilingHitCount = Physics2D.BoxCastNonAlloc(position + boxCollider.offset, verticalBoxSize, 0f, Vector2.up, hitBuffer, groundedDistance, groundLayerMask);
            IsTouchingCeiling = HasValidSensorHit(ceilingHitCount);
        }

        private bool HasValidSensorHit(int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!hitBuffer[i].collider.isTrigger) return true;
            }
            return false;
        }

        private bool CheckGroundedValidHit(int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!hitBuffer[i].collider.isTrigger && hitBuffer[i].normal.y > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}