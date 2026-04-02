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

        public Rigidbody2D PlayerRigidbody { get; private set; }
        public IInputReader Input { get; private set; }
        public ArmController LeftArm => leftArm;
        public ArmController RightArm => rightArm;
        public LayerMask GroundLayerMask => groundLayerMask;
        public bool IsGrounded { get; private set; }

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
            CheckGrounded();
            currentState?.FixedUpdateState();
        }

        public void TransitionToState(IPlayerState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        private void CheckGrounded()
        {
            float dynamicDistance = boxCastDistance;
            Vector2 currentVelocity = PlayerRigidbody.linearVelocity;

            if (currentVelocity.y < 0f)
            {
                dynamicDistance += Mathf.Abs(currentVelocity.y * Time.fixedDeltaTime);
            }

            RaycastHit2D hit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.down,
                dynamicDistance, groundLayerMask);

            if (hit.collider != null && currentVelocity.y <= 0f)
            {
                if (!IsGrounded)
                {
                    float travelDistance = hit.distance - 0.02f;
                    if (travelDistance > 0f)
                    {
                        PlayerRigidbody.position += Vector2.down * travelDistance;
                    }
                }

                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
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
    }
}