using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class KinematicPhysicsHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float skinWidth = 0.02f;
        [SerializeField] private float groundedDistance = 0.05f;
        [SerializeField] private float boxShrinkOffset = 0.1f;
        [SerializeField] private float movementThreshold = 0.001f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingLeftWall { get; private set; }
        public bool IsTouchingRightWall { get; private set; }
        public bool IsTouchingCeiling { get; private set; }

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

        private IMovingSurface _currentMovingSurface;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
        }

        public Vector2 Move(Vector2 deltaMovement)
        {
            Vector2 position = _body.position;

            ResolvePenetrations(ref position);

            deltaMovement = ResolveHorizontalCollisions(position, deltaMovement);
            position.x += deltaMovement.x;

            deltaMovement = ResolveVerticalCollisions(position, deltaMovement);
            position.y += deltaMovement.y;

            UpdateSensors(position);

            if (IsGrounded && _currentMovingSurface != null)
            {
                position += _currentMovingSurface.DeltaPosition;
            }

            _body.MovePosition(position);

            return deltaMovement / Time.fixedDeltaTime;
        }

        private void ResolvePenetrations(ref Vector2 position)
        {
            int overlapCount = Physics2D.OverlapBoxNonAlloc(position + _boxCollider.offset, _boxCollider.bounds.size, 0f, _overlapBuffer, groundLayerMask);
            for (int i = 0; i < overlapCount; i++)
            {
                Collider2D overlap = _overlapBuffer[i];
                if (overlap == _boxCollider || overlap.isTrigger) continue;

                ColliderDistance2D distance = Physics2D.Distance(_boxCollider, overlap);
                if (distance.isOverlapped)
                {
                    position += distance.normal * distance.distance;
                }
            }
        }

        private Vector2 ResolveHorizontalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.x) < movementThreshold) return movement;

            float directionX = Mathf.Sign(movement.x);
            float distance = Mathf.Abs(movement.x) + skinWidth;
            Vector2 boxSize = _boxCollider.bounds.size;
            boxSize.y -= boxShrinkOffset;

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f, new Vector2(directionX, 0f), _hitBuffer, distance, groundLayerMask);
            
            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider.isTrigger) continue;
                if (_hitBuffer[i].distance < minDistance)
                {
                    minDistance = _hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit) movement.x = (minDistance - skinWidth) * directionX;
            return movement;
        }

        private Vector2 ResolveVerticalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.y) < movementThreshold) return movement;

            float directionY = Mathf.Sign(movement.y);
            float distance = Mathf.Abs(movement.y) + skinWidth;
            Vector2 boxSize = _boxCollider.bounds.size;
            boxSize.x -= boxShrinkOffset;

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f, new Vector2(0f, directionY), _hitBuffer, distance, groundLayerMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider.isTrigger) continue;
                if (_hitBuffer[i].distance < minDistance)
                {
                    minDistance = _hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit) movement.y = (minDistance - skinWidth) * directionY;
            return movement;
        }

        private void UpdateSensors(Vector2 position)
        {
            Vector2 horizontalBoxSize = _boxCollider.bounds.size;
            horizontalBoxSize.y -= boxShrinkOffset;

            int leftHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f, Vector2.left, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingLeftWall = HasValidSensorHit(leftHitCount);

            int rightHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f, Vector2.right, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingRightWall = HasValidSensorHit(rightHitCount);

            Vector2 verticalBoxSize = _boxCollider.bounds.size;
            verticalBoxSize.x -= boxShrinkOffset;

            int groundHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f, Vector2.down, _hitBuffer, groundedDistance + skinWidth, groundLayerMask);
            
            IsGrounded = false;
            _currentMovingSurface = null;

            for (int i = 0; i < groundHitCount; i++)
            {
                if (!_hitBuffer[i].collider.isTrigger)
                {
                    IsGrounded = true;
                    _hitBuffer[i].collider.TryGetComponent(out _currentMovingSurface);
                    break; 
                }
            }

            int ceilingHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f, Vector2.up, _hitBuffer, groundedDistance + skinWidth, groundLayerMask);
            IsTouchingCeiling = HasValidSensorHit(ceilingHitCount);
        }

        private bool HasValidSensorHit(int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!_hitBuffer[i].collider.isTrigger) return true;
            }
            return false;
        }
    }
}