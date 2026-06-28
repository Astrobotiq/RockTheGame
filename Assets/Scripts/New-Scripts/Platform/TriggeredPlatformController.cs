using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Player üzerinde durduğu zaman harekete başlayan ve tam bir turu tamamlayana kadar durmayan platform kontrolcüsü.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D)), DefaultExecutionOrder(-100)]
    public class TriggeredPlatformController : MonoBehaviour, IMovingSurface, IResettable
    {
        [SerializeField] private MovementStrategy movementStrategy;
        [SerializeField] private LayerMask playerLayer;

        private Rigidbody2D _rigidbody2D;
        private Collider2D _collider;
        private Vector2 _previousPosition;
        private Vector2 _initialPosition;
        
        private bool _isMoving;
        private float _elapsedTime;
        private readonly Vector2[] _recentVelocities = new Vector2[3];

        public Vector2 DeltaPosition { get; private set; }
        public Vector2 SurfaceVelocity
        {
            get
            {
                Vector2 maxVel = Vector2.zero;
                float maxSqMag = -1f;
                for (int i = 0; i < 3; i++)
                {
                    float sqMag = _recentVelocities[i].sqrMagnitude;
                    if (sqMag > maxSqMag)
                    {
                        maxSqMag = sqMag;
                        maxVel = _recentVelocities[i];
                    }
                }
                return maxVel;
            }
        }
        public float JumpBoostMultiplier => movementStrategy != null ? movementStrategy.JumpBoostMultiplier : 0f;
        
        public Vector2 Position
        {
            get
            {
                EnsureRigidbody();
                return _rigidbody2D.position;
            }
        }

        private void Awake()
        {
            EnsureRigidbody();
            _collider = GetComponent<Collider2D>();
            _initialPosition = transform.position;
        }

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Unregister(this);
            }
        }

        private void EnsureRigidbody()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                _previousPosition = _rigidbody2D.position;
            }
        }

        private void FixedUpdate()
        {
            if (movementStrategy == null) return;

            EnsureRigidbody();

            bool playerOnTop = IsPlayerOnTop();

            if (!_isMoving)
            {
                if (playerOnTop)
                {
                    _isMoving = true;
                    _elapsedTime = 0f;
                }
            }

            float evaluatedTime = 0f;

            if (_isMoving)
            {
                _elapsedTime += Time.fixedDeltaTime;
                evaluatedTime = _elapsedTime;

                float period = movementStrategy.Period;
                if (period > 0f && _elapsedTime >= period)
                {
                    // A full tour has completed
                    if (playerOnTop)
                    {
                        // Player is still standing on the platform, start a new tour smoothly
                        _elapsedTime %= period;
                        evaluatedTime = _elapsedTime;
                    }
                    else
                    {
                        // Player has left, stop the platform at the starting position
                        _isMoving = false;
                        _elapsedTime = 0f;
                        evaluatedTime = 0f;
                    }
                }
            }

            Vector2 newPosition = movementStrategy.GetPositionAtTime(evaluatedTime);
            
            DeltaPosition = newPosition - _previousPosition;
            Vector2 currentVelocity = DeltaPosition / Time.fixedDeltaTime;

            _recentVelocities[2] = _recentVelocities[1];
            _recentVelocities[1] = _recentVelocities[0];
            _recentVelocities[0] = currentVelocity;
            
            _rigidbody2D.MovePosition(newPosition);

            float? newRotation = movementStrategy.GetRotationAtTime(evaluatedTime);
            if (newRotation.HasValue)
            {
                _rigidbody2D.MoveRotation(newRotation.Value);
            }

            _previousPosition = newPosition;
        }

        private bool IsPlayerOnTop()
        {
            if (_collider == null) return false;
            
            if (_collider is BoxCollider2D boxCollider)
            {
                Vector2 boxCenter = (Vector2)transform.position + boxCollider.offset + (Vector2.up * 0.05f);
                Vector2 boxSize = boxCollider.size;
                boxSize.x *= transform.lossyScale.x;
                boxSize.y *= transform.lossyScale.y;
                boxSize.y += 0.05f;
                
                Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, transform.eulerAngles.z, playerLayer);
                return hit != null;
            }
            else
            {
                Bounds bounds = _collider.bounds;
                Vector2 boxCenter = (Vector2)bounds.center + (Vector2.up * 0.05f);
                Vector2 boxSize = bounds.size;
                boxSize.y += 0.05f;
                
                Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, playerLayer);
                return hit != null;
            }
        }

        public void MoveTo(Vector2 newPosition)
        {
            EnsureRigidbody();
            DeltaPosition = newPosition - _previousPosition;
            Vector2 currentVelocity = DeltaPosition / Time.fixedDeltaTime;

            _recentVelocities[2] = _recentVelocities[1];
            _recentVelocities[1] = _recentVelocities[0];
            _recentVelocities[0] = currentVelocity;
            
            _rigidbody2D.MovePosition(newPosition);
            _previousPosition = newPosition;
        }

        public void TeleportTo(Vector2 newPosition)
        {
            EnsureRigidbody();
            _rigidbody2D.position = newPosition;
            transform.position = newPosition;
            _previousPosition = newPosition;
            DeltaPosition = Vector2.zero;
            
            _recentVelocities[0] = Vector2.zero;
            _recentVelocities[1] = Vector2.zero;
            _recentVelocities[2] = Vector2.zero;
        }

        public void ResetToDefault()
        {
            _isMoving = false;
            _elapsedTime = 0f;

            Vector2 startPos = movementStrategy != null 
                ? movementStrategy.GetPositionAtTime(0f) 
                : _initialPosition;

            TeleportTo(startPos);

            if (movementStrategy != null)
            {
                float? startRot = movementStrategy.GetRotationAtTime(0f);
                if (startRot.HasValue)
                {
                    EnsureRigidbody();
                    _rigidbody2D.rotation = startRot.Value;
                    transform.rotation = Quaternion.Euler(0, 0, startRot.Value);
                }
            }
        }
    }
}
