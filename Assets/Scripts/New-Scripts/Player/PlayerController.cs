/// <summary>
/// Karakterin temel fizik bileşenlerini barındıran, girdi okuyan ve FSM geçişlerini koordine eden ana bağlam sınıfı.
/// </summary>

using UnityEngine;

namespace New_Scripts.Player
{
    public enum ActiveArm
    {
        None,
        Left,
        Right,
        Both
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Physics Config")] [SerializeField]
        private LayerMask groundLayerMask;

        [SerializeField] private LayerMask grappleLayerMask;

        [Header("Sensors")] [SerializeField] private Vector2 boxCastSize;
        [SerializeField] private float boxCastDistance;
        [SerializeField] private float maxGrappleDistance = 15f;

        [Header("Visuals")] [SerializeField] private ArmController leftArm;
        [SerializeField] private ArmController rightArm;

        [SerializeField] private NodeDetector nodeDetector;
        [SerializeField] private KinematicPhysicsHandler physicsHandler;

        public Rigidbody2D PlayerRigidbody { get; private set; }
        public IInputReader Input { get; private set; }
        public ArmController LeftArm => leftArm;
        public ArmController RightArm => rightArm;
        public LayerMask GroundLayerMask => groundLayerMask;
        public bool IsGrounded => physicsHandler.IsGrounded;
        public BoxCollider2D PlayerCollider => playerCollider;

        public Vector2? LeftAnchor { get; set; }
        public Vector2? RightAnchor { get; set; }

        public bool HasDashCharge { get; private set; }

        public static event System.Action<Vector3> OnHighImpact;

        public IPlayerState CurrentState => currentState;

        private BoxCollider2D playerCollider;
        private IPlayerState currentState;

        private void Awake()
        {
            PlayerRigidbody = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<BoxCollider2D>();
            Input = GetComponent<IInputReader>();
            PlayerRigidbody.bodyType = RigidbodyType2D.Kinematic;

            TransitionToState(new AirborneState(this, 10f, 0.5f, 25f, -30f, Vector2.zero));
        }

        private void Update()
        {
            currentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            currentState?.FixedUpdateState();
            Vector2 desiredVelocity = PlayerRigidbody.linearVelocity;
            PlayerRigidbody.linearVelocity = physicsHandler.FilterVelocity(desiredVelocity);
        }

        public void TransitionToState(IPlayerState newState)
        {
            Debug.Log("Transitioning to state: " + newState.GetType().Name);
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public bool TryCastGrapple(Vector2 direction, out Vector2 hitPoint)
        {
            return nodeDetector.TryFindBestNode(direction, PlayerRigidbody.position, out hitPoint);
        }

        public bool CheckNodeCoincidence()
        {
            if (!LeftAnchor.HasValue || !RightAnchor.HasValue) return false;

            float nodeCoincidenceThreshold = 0.5f;
            return Vector2.Distance(LeftAnchor.Value, RightAnchor.Value) < nodeCoincidenceThreshold;
        }

        public void NotifyImpact(Vector3 velocity)
        {
            OnHighImpact?.Invoke(velocity);
        }

        public void ResetDash()
        {
            HasDashCharge = true;
        }

        public void UseDash()
        {
            HasDashCharge = false;
        }

        [Header("Wall Sensors")] [SerializeField]
        private float wallCheckDistance = 0.6f;

        [Header("Physics Config")] [SerializeField]
        private float gravity = 25f;

        public float Gravity => gravity;

        public static event System.Action OnStaminaWarning;

        public bool IsTouchingLeftWall() => physicsHandler.IsTouchingLeftWall;

        public bool IsTouchingRightWall() => physicsHandler.IsTouchingRightWall;
        public bool IsTouchingCeiling() => physicsHandler.IsTouchingCeiling;

        public void TriggerStaminaWarning()
        {
            OnStaminaWarning?.Invoke();
        }
    }
}