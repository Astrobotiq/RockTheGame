using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Platform
{
    [RequireComponent(typeof(Rigidbody2D)), DefaultExecutionOrder(-100)]
    public class PlatformController : MonoBehaviour, IMovingSurface, IResettable
    {
        [SerializeField] private MovementStrategy movementStrategy;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _previousPosition;
        private Vector2 _initialPosition;
        private float _timeOffset;
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

            Vector2 newPosition = movementStrategy.GetPositionAtTime(Time.time - _timeOffset);
            
            DeltaPosition = newPosition - _previousPosition;
            Vector2 currentVelocity = DeltaPosition / Time.fixedDeltaTime;
            
            _recentVelocities[2] = _recentVelocities[1];
            _recentVelocities[1] = _recentVelocities[0];
            _recentVelocities[0] = currentVelocity;
            
            _rigidbody2D.MovePosition(newPosition);

            float? newRotation = movementStrategy.GetRotationAtTime(Time.time - _timeOffset);
            if (newRotation.HasValue)
            {
                _rigidbody2D.MoveRotation(newRotation.Value);
            }

            _previousPosition = newPosition;
        }

        /// <summary>
        /// Moves the platform to a new position, updating delta position and velocity.
        /// Used by external managers to drive the platform.
        /// </summary>
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

        /// <summary>
        /// Instantly teleports the platform to a new position, resetting physics velocities
        /// so that players standing on it do not experience the sudden teleportation delta.
        /// </summary>
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

        /// <summary>
        /// Platformu başlangıç pozisyonuna ışınlar ve zaman bazlı hareketi sıfırlar.
        /// </summary>
        public void ResetToDefault()
        {
            _timeOffset = Time.time;
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