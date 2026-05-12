using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.LevelChange;
using UnityEngine;
using New_Scripts.Player.IFramePauseable;
using New_Scripts.Player.States;
using New_Scripts.Player.UI;
using New_Scripts.Player.Visual;

namespace New_Scripts.Player
{
    public enum ActiveArm
    {
        None,
        Left,
        Right,
        Both
    }

    /// <summary>
    /// Karakterin temel fizik bilesenlerini barindiran, girdi okuyan ve FSM gecislerini koordine eden ana baglam sinifi.
    /// Tum bagimli degerleri merkezi ScriptableObject (PlayerStatsSO) uzerinden okur.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour, IFramePausable, IPlayerTransitionable
    {
        [Header("Data")] public PlayerStatsSO Stats;

        [Header("System References")] [SerializeField]
        private NodeDetector nodeDetector;

        [SerializeField] private KinematicPhysicsHandler physicsHandler;
        [SerializeField] private PlayerUIController uiController;
        [SerializeField] private PlayerColorController colorController;

        [Header("Visual References")] [SerializeField]
        private ArmController leftArm;

        [SerializeField] private ArmController rightArm;

        [Header("Environment Settings")] [SerializeField]
        private LayerMask groundLayerMask;

        [SerializeField] private LayerMask grappleLayerMask;

        // --- Core Components ---
        public Rigidbody2D PlayerRigidbody { get; private set; }
        public BoxCollider2D PlayerCollider { get; private set; }
        public IInputReader Input { get; private set; }
        public float JumpBufferTimer { get; private set; }
        
        public Vector2 Velocity { get; set; }

        // --- Accessors ---
        public ArmController LeftArm => leftArm;
        public ArmController RightArm => rightArm;
        public PlayerColorController ColorController => colorController;
        public PlayerUIController UIController => uiController;
        public LayerMask GroundLayerMask => groundLayerMask;

        // --- Sensor Data ---
        public bool IsGrounded => physicsHandler.IsGrounded;
        public bool IsTouchingLeftWall => physicsHandler.IsTouchingLeftWall;
        public bool IsTouchingRightWall => physicsHandler.IsTouchingRightWall;
        public bool IsTouchingCeiling => physicsHandler.IsTouchingCeiling;

        // --- State Management ---
        public IPlayerState CurrentState { get; private set; }
        public Vector2? LeftAnchor { get; set; }
        public Vector2? RightAnchor { get; set; }

        public bool HasDashCharge { get; private set; }
        public bool CanSlingshot { get; private set; } = true;
        public bool CanWallClimb { get; private set; } = true;
        public float CurrentWallStamina { get; private set; }

        // --- Hit Stop & Frame Pause ---
        private bool _isPaused;
        private Vector2 _velocityCache;

        // --- Events ---
        public static event System.Action<Vector3> OnHighImpact;
        public static event System.Action OnStaminaWarning;

        private void Awake()
        {
            PlayerRigidbody = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<BoxCollider2D>();
            Input = GetComponent<IInputReader>();

            PlayerRigidbody.bodyType = RigidbodyType2D.Kinematic;
            CurrentWallStamina = Stats.MaxWallStamina;

            TransitionToState(new AirborneState(this, Vector2.zero));
        }

        private void OnEnable()
        {
            HitStopEvents.HitStopStarted += OnPauseStarted;
            HitStopEvents.HitStopEnded += OnPauseEnded;
        }

        private void OnDisable()
        {
            HitStopEvents.HitStopStarted -= OnPauseStarted;
            HitStopEvents.HitStopEnded -= OnPauseEnded;
        }

        private void Update()
        {
            if (_isPaused) return;

            if (Input.IsJumpPressed)
                JumpBufferTimer = Stats.JumpBufferDuration;
            else if (JumpBufferTimer > 0f)
                JumpBufferTimer -= Time.deltaTime;

            CurrentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            if (_isPaused) return;
    
            CurrentState?.FixedUpdateState();

            // 2. FSM'in hesapladığı teorik hızı bir delta mesafeye çevirip handler'a veriyoruz.
            // Handler, çarpışmaları çözüp objeyi hareket ettirdikten sonra "Gerçekten ulaştığım hız bu" diyerek gerçeği FSM'e geri döndürür.
            Velocity = physicsHandler.Move(Velocity * Time.fixedDeltaTime);
        }

        public void TransitionToState(IPlayerState newState)
        {
            CurrentState?.ExitState();
            CurrentState = newState;
            CurrentState?.EnterState();
        }

        // --- Grapple & Core Actions ---

        public bool TryCastGrapple(Vector2 direction, out Vector2 hitPoint)
        {
            return nodeDetector.TryFindBestNode(direction, PlayerRigidbody.position, out hitPoint);
        }

        public bool CheckNodeCoincidence()
        {
            if (!LeftAnchor.HasValue || !RightAnchor.HasValue) return false;
            return Vector2.Distance(LeftAnchor.Value, RightAnchor.Value) < Stats.NodeCoincidenceThreshold;
        }

        public void NotifyImpact(Vector3 velocity)
        {
            OnHighImpact?.Invoke(velocity);
            HitStopEvents.RequestHitStop?.Invoke(Stats.HitStopDuration);
        }

        // --- Ability & Resource Management ---

        public void ResetDash()
        {
            HasDashCharge = true;
        }

        public void UseDash()
        {
            ColorController.SetDashExhausted();
            HasDashCharge = false;
        }

        public void ResetSlingshot() => CanSlingshot = true;

        public void UseSlingshot()
        {
            ColorController.SetSlingshotExhausted();
            CanSlingshot = false;
        }

        public void ConsumeWallStamina(float amount)
        {
            CurrentWallStamina -= amount;

            if (CurrentWallStamina <= 0f)
            {
                CurrentWallStamina = 0f;
                CanWallClimb = false;
            }

            UIController.UpdateStamina(CurrentWallStamina, Stats.MaxWallStamina);
        }

        public void RefillWallStamina()
        {
            CurrentWallStamina = Stats.MaxWallStamina;
            CanWallClimb = true;
            UIController.RefillAndHideStaminaBar();
        }

        public void TriggerStaminaWarning()
        {
            OnStaminaWarning?.Invoke();
        }

        // --- IFramePausable Implementation ---

        public void OnPauseStarted()
        {
            _isPaused = true;
            _velocityCache = Velocity; // Artık kendi hızımızı saklıyoruz
            Velocity = Vector2.zero;
        }

        public void OnPauseEnded()
        {
            _isPaused = false;
            Velocity = _velocityCache; // Kendi hızımızı geri yüklüyoruz
        }

        public void ConsumeJumpBuffer()
        {
            JumpBufferTimer = 0f;
        }

        public float CurrentWallSlideTime { get; private set; } = 2f;

        public void ResetWallSlideTime()
        {
            CurrentWallSlideTime = Stats.MaxWallSlideTime;
        }

        public void ConsumeWallSlideTime(float amount)
        {
            CurrentWallSlideTime -= amount;
        }
        
        private Vector2 _preTransitionVelocity;

        public void FreezeForTransition()
        {
            _preTransitionVelocity = Velocity; // linearVelocity yerine Velocity
            TransitionToState(new PlayerTransitionState(this));
        }

        public void UnfreezeFromTransition(TransitionDirection direction)
        {
            switch (direction)
            {
                case TransitionDirection.Up:
                    TransitionToState(new AirborneState(
                        this,
                        inheritedVelocity: new Vector2(_preTransitionVelocity.x, Stats.JumpVelocity),
                        isJumping: true
                    ));
                    break;

                case TransitionDirection.Right:
                case TransitionDirection.Left:
                case TransitionDirection.Down:
                    TransitionToState(new AirborneState(
                        this,
                        inheritedVelocity: _preTransitionVelocity,
                        isJumping: false
                    ));
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || PlayerRigidbody == null) return;

            Vector2 position = transform.position;
            Vector2 velocity = Velocity;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + (velocity * 0.1f));

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(position + (velocity * 0.1f), 0.05f);
        
            string stateName = CurrentState != null ? CurrentState.GetType().Name : "No State";
            string debugText = $"State: {stateName}\nVel: {velocity.x:F1}, {velocity.y:F1}";

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.LowerCenter;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;

            UnityEditor.Handles.Label(position + Vector2.up * 1.5f, debugText, style);
        }
#endif
    }
}