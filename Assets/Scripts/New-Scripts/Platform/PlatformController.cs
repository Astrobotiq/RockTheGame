using UnityEngine;

namespace New_Scripts.Platform
{
    [RequireComponent(typeof(Rigidbody2D)), DefaultExecutionOrder(-100)]
    public class PlatformController : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private MovementStrategy movementStrategy;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _previousPosition;

        public Vector2 DeltaPosition { get; private set; }
        public Vector2 SurfaceVelocity { get; private set; }
        
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

            Vector2 newPosition = movementStrategy.GetPositionAtTime(Time.time);
            
            DeltaPosition = newPosition - _previousPosition;
            SurfaceVelocity = DeltaPosition / Time.fixedDeltaTime;
            
            _rigidbody2D.MovePosition(newPosition);
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
            SurfaceVelocity = DeltaPosition / Time.fixedDeltaTime;
            
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
            SurfaceVelocity = Vector2.zero;
        }
    }
}