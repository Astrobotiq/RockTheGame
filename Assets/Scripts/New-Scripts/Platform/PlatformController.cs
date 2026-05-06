using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Fiziksel platform hareketini ve IMovingSurface hız aktarımını yöneten ana kontrolcü sınıf.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D)), DefaultExecutionOrder(-100)]
    public class PlatformController : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private MovementStrategy movementStrategy;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _previousPosition;

        public Vector2 SurfaceVelocity { get; private set; }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _previousPosition = _rigidbody2D.position;
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = movementStrategy.GetPositionAtTime(Time.time);
            SurfaceVelocity = (newPosition - _previousPosition) / Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);
            _previousPosition = newPosition;
        }
    }
}