using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Kinematik platformun hareketini, momentumunu ve hedef noktaları arasındaki geçişini yönetir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D)), DefaultExecutionOrder(-100)]
    public class MovingPlatform : MonoBehaviour, IMovingSurface
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float arrivalThreshold = 0.01f;

        private Rigidbody2D _platformRigidbody;
        private IWaypointPath _waypointPath;
        private float _squaredArrivalThreshold;
        private int _currentWaypointIndex;
        private bool _isMovingForward = true;

        public Vector2 DeltaPosition { get; }
        public Vector2 SurfaceVelocity { get; private set;}

        private void Awake()
        {
            _platformRigidbody = GetComponent<Rigidbody2D>();
            _waypointPath = GetComponent<IWaypointPath>();
            _platformRigidbody.bodyType = RigidbodyType2D.Kinematic;
            _squaredArrivalThreshold = arrivalThreshold * arrivalThreshold;
        }

        private void FixedUpdate()
        {
            if (_waypointPath == null || !_waypointPath.IsValid())
            {
                return;
            }

            Vector2 currentPosition = _platformRigidbody.position;
            Vector2 targetPosition = _waypointPath.GetWaypoint(_currentWaypointIndex);
            float sqrDistance = (targetPosition - currentPosition).sqrMagnitude;

            if (sqrDistance <= _squaredArrivalThreshold)
            {
                _currentWaypointIndex = _waypointPath.GetNextIndex(_currentWaypointIndex, ref _isMovingForward);
                targetPosition = _waypointPath.GetWaypoint(_currentWaypointIndex);
            }

            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);
            SurfaceVelocity = (newPosition - currentPosition) / Time.fixedDeltaTime;

            _platformRigidbody.MovePosition(newPosition);
        }
    }
}