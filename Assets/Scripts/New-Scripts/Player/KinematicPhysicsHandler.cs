using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin kinematik fizik islemlerini, carpisma cozumlemelerini ve zemin/duvar sensorlerini yonetir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class KinematicPhysicsHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private LayerMask oneWayPlatformLayerMask;
        [SerializeField] private float skinWidth = 0.02f;
        [SerializeField] private float groundedDistance = 0.05f;
        [SerializeField] private float boxShrinkOffset = 0.1f;
        [SerializeField] private float movementThreshold = 0.001f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingLeftWall { get; private set; }
        public bool IsTouchingRightWall { get; private set; }
        public bool IsTouchingCeiling { get; private set; }
        public int ClingingWallDirection { get; set; }

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

        private IMovingSurface _currentMovingSurface;
        private IMovingSurface _currentLeftMovingSurface;
        private IMovingSurface _currentRightMovingSurface;
        private IMovingSurface _lastMovingSurface;
        private int _groundAndOneWayMask;

        public IMovingSurface CurrentMovingSurface => _currentMovingSurface;
        public IMovingSurface CurrentLeftMovingSurface => _currentLeftMovingSurface;
        public IMovingSurface CurrentRightMovingSurface => _currentRightMovingSurface;
        public IMovingSurface LastMovingSurface => _lastMovingSurface;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _groundAndOneWayMask = groundLayerMask | oneWayPlatformLayerMask;
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
            else if (ClingingWallDirection == -1 && _currentLeftMovingSurface != null)
            {
                position += _currentLeftMovingSurface.DeltaPosition;
            }
            else if (ClingingWallDirection == 1 && _currentRightMovingSurface != null)
            {
                position += _currentRightMovingSurface.DeltaPosition;
            }

            _body.MovePosition(position);

            return deltaMovement / Time.fixedDeltaTime;
        }

        private void ResolvePenetrations(ref Vector2 position)
        {
            int overlapCount = Physics2D.OverlapBoxNonAlloc(position + _boxCollider.offset, _boxCollider.bounds.size,
                0f, _overlapBuffer, groundLayerMask);

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

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f,
                new Vector2(directionX, 0f), _hitBuffer, distance, groundLayerMask);

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

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f,
                new Vector2(0f, directionY), _hitBuffer, distance, _groundAndOneWayMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitBuffer[i].collider;
                if (hitCollider.isTrigger) continue;

                bool isOneWay = ((1 << hitCollider.gameObject.layer) & oneWayPlatformLayerMask) != 0;

                if (isOneWay)
                {
                    if (directionY > 0) continue;
                    if (_hitBuffer[i].distance == 0) continue;
                }

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

            int leftHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f,
                Vector2.left, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingLeftWall = HasValidSensorHit(leftHitCount, groundLayerMask);
            TryGetMovingSurface(leftHitCount, out _currentLeftMovingSurface);

            int rightHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f,
                Vector2.right, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingRightWall = HasValidSensorHit(rightHitCount, groundLayerMask);
            TryGetMovingSurface(rightHitCount, out _currentRightMovingSurface);

            Vector2 verticalBoxSize = _boxCollider.bounds.size;
            verticalBoxSize.x -= boxShrinkOffset;

            int groundHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f,
                Vector2.down, _hitBuffer, groundedDistance + skinWidth, _groundAndOneWayMask);

            IsGrounded = false;
            _currentMovingSurface = null;

            for (int i = 0; i < groundHitCount; i++)
            {
                Collider2D hitCollider = _hitBuffer[i].collider;
                if (hitCollider.isTrigger) continue;

                bool isOneWay = ((1 << hitCollider.gameObject.layer) & oneWayPlatformLayerMask) != 0;
                if (isOneWay && _hitBuffer[i].distance == 0) continue;

                IsGrounded = true;
                hitCollider.TryGetComponent(out _currentMovingSurface);
                break;
            }

            if (IsGrounded)
            {
                _lastMovingSurface = _currentMovingSurface;
            }
            else if (ClingingWallDirection == -1 && _currentLeftMovingSurface != null)
            {
                _lastMovingSurface = _currentLeftMovingSurface;
            }
            else if (ClingingWallDirection == 1 && _currentRightMovingSurface != null)
            {
                _lastMovingSurface = _currentRightMovingSurface;
            }

            int ceilingHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f,
                Vector2.up, _hitBuffer, groundedDistance + skinWidth, groundLayerMask);
            IsTouchingCeiling = HasValidSensorHit(ceilingHitCount, groundLayerMask);
        }

        private bool HasValidSensorHit(int hitCount, LayerMask checkMask)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!_hitBuffer[i].collider.isTrigger) return true;
            }

            return false;
        }

        private bool TryGetMovingSurface(int hitCount, out IMovingSurface movingSurface)
        {
            movingSurface = null;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hitBuffer[i].collider;
                if (hitCollider.isTrigger) continue;
                if (hitCollider.TryGetComponent(out movingSurface))
                {
                    return true;
                }
            }
            return false;
        }
    }
}