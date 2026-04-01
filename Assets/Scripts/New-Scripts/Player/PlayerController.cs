using UnityEngine;
using UnityEngine.InputSystem;

namespace New_Scripts.Player
{
    public enum ActiveArm { None, Left, Right, Both }
    /// <summary>
    /// Karakterin temel fizik bileşenlerini barındıran, girdi okuyan ve FSM geçişlerini koordine eden ana bağlam sınıfı.
    /// </summary>
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

        public Rigidbody2D PlayerRigidbody { get; private set; }
        public IInputReader Input { get; private set; }
        public ArmController LeftArm => leftArm;
        public ArmController RightArm => rightArm;
        public bool IsGrounded { get; private set; }

        public Vector2? LeftAnchor { get; set; }
        public Vector2? RightAnchor { get; set; }

        private BoxCollider2D playerCollider;
        private IPlayerState currentState;

        private void Awake()
        {
            PlayerRigidbody = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<BoxCollider2D>();
            Input = GetComponent<IInputReader>();
            PlayerRigidbody.bodyType = RigidbodyType2D.Kinematic;

            TransitionToState(new AirborneState(this, 10f, 0.5f, 25f, -30f, 0f));
        }

        private void Update()
        {
            CheckGrounded();
            currentState?.UpdateState();
        }

        private void FixedUpdate()
        {
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
            RaycastHit2D hit = Physics2D.BoxCast(playerCollider.bounds.center, boxCastSize, 0f, Vector2.down,
                boxCastDistance, groundLayerMask);
            IsGrounded = hit.collider != null;
        }

        public bool TryCastGrapple(Vector2 direction, out RaycastHit2D hit)
        {
            if (direction.sqrMagnitude < 0.1f)
            {
                hit = new RaycastHit2D();
                return false;
            }

            hit = Physics2D.Raycast(PlayerRigidbody.position, direction.normalized, maxGrappleDistance,
                grappleLayerMask);
            return hit.collider != null;
        }

        public bool CheckNodeCoincidence()
        {
            if (!LeftAnchor.HasValue || !RightAnchor.HasValue) return false;

            float nodeCoincidenceThreshold = 0.5f;
            return Vector2.Distance(LeftAnchor.Value, RightAnchor.Value) < nodeCoincidenceThreshold;
        }
    }
}