using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Rigidbody2D tabanlı kinematik fizik çözücü.
    /// Eylemlerden gelen hareket isteklerini Rigidbody2D'ye iletir ve delta hareket/hız hesaplar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicSolver : MonoBehaviour, IKinematicSolver
    {
        private Rigidbody2D _rb;
        private Vector2 _previousPosition;
        private Vector2 _currentPosition;
        private Vector2 _externalVelocity;

        public Vector2 DeltaPosition { get; private set; }
        public Vector2 SurfaceVelocity { get; private set; }

        public void Initialize(GameObject owner)
        {
            _rb = owner.GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _previousPosition = _rb.position;
            _currentPosition = _rb.position;
            ResetSolver();
        }

        public void UpdateSolver(Vector3 targetPosition, float deltaTime)
        {
            if (_rb == null) return;
            if (deltaTime <= 0) return;

            Vector2 nextPos = targetPosition;
            if (_externalVelocity != Vector2.zero)
            {
                nextPos += _externalVelocity * deltaTime;
                // Momentum sönümleme (damping)
                _externalVelocity = Vector2.MoveTowards(_externalVelocity, Vector2.zero, deltaTime * 10f);
            }

            _rb.MovePosition(nextPos);
            _currentPosition = _rb.position;
            DeltaPosition = _currentPosition - _previousPosition;
            SurfaceVelocity = DeltaPosition / deltaTime;
            _previousPosition = _currentPosition;
        }

        public void ApplyVelocity(Vector2 velocity)
        {
            _externalVelocity += velocity;
        }

        public void ResetSolver()
        {
            _externalVelocity = Vector2.zero;
            DeltaPosition = Vector2.zero;
            SurfaceVelocity = Vector2.zero;
            if (_rb != null)
            {
                _previousPosition = _rb.position;
                _currentPosition = _rb.position;
            }
        }
    }
}
