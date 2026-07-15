using System;
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
        [SerializeField] private PlayerAudioController audioController;
        [SerializeField] private PlayerVFXManager vfxManager;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Visual References")] [SerializeField]
        private ArmController leftArm;

        [SerializeField] private ArmController rightArm;

        [Header("Environment Settings")] [SerializeField]
        private LayerMask groundLayerMask;

        [SerializeField] private LayerMask grappleLayerMask;

        [SerializeField] private LayerMask deathLayerMask;

        // --- Core Components ---
        public Rigidbody2D PlayerRigidbody { get; private set; }
        public BoxCollider2D PlayerCollider { get; private set; }
        public IInputReader Input { get; private set; }
        public float JumpBufferTimer { get; private set; }
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        
        public Vector2 Velocity { get; set; }

        // --- Accessors ---
        public KinematicPhysicsHandler PhysicsHandler => physicsHandler;
        public ArmController LeftArm => leftArm;
        public ArmController RightArm => rightArm;
        public PlayerColorController ColorController => colorController;
        public PlayerUIController UIController => uiController;
        public PlayerAudioController Audio => audioController;
        public PlayerVFXManager VFX => vfxManager;
        public LayerMask GroundLayerMask => groundLayerMask;
        public LayerMask DeathLayerMask => deathLayerMask;

        public Vector2 FacingDirection
        {
            get
            {
                if (SpriteRenderer != null)
                {
                    return SpriteRenderer.flipX ? Vector2.left : Vector2.right;
                }
                return Vector2.right;
            }
        }

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
        
        public float ActiveSpeedMultiplier { get; set; } = 1f;

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
            
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

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

        public void ResetSlingshot()
        {
            CanSlingshot = true;
            if (colorController != null)
            {
                colorController.ResetArmColors();
            }
        }

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
            _velocityCache = Velocity;
            Velocity = Vector2.zero;
        }

        public void OnPauseEnded()
        {
            _isPaused = false;
            Velocity = _velocityCache; // Kendi hızımızı geri yüklüyoruz
        }
        
        public void OnStartRespawn(){
            _isPaused = true;
            Velocity = Vector2.zero;
            LeftAnchor = null;
            RightAnchor = null;
            ResetDash();
            ResetSlingshot();
            RefillWallStamina();
        }
        
        public void OnEndRespawn(){
            TransitionToState(new AirborneState(this, Vector2.zero));
            _isPaused = false;
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
                        isJumping: false
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
        
        public static event Action OnSlingshotLaunch;

        public void NotifySlingshotLaunch()
        {
            OnSlingshotLaunch?.Invoke();
        }

        public struct LedgeDetectionResult
        {
            public bool LedgeDetected;
            public Vector2 ClimbTarget;
            public Vector2 LowerHitPoint;
            public Vector2 UpperHitPoint;
            public Vector2 VerticalRayStart;
            public Vector2 GroundHitPoint;
            public bool SpaceClear;
            public Vector2 UpperStart;
            public Vector2 LowerStart;
        }

        public LedgeDetectionResult LatestLedgeResult { get; set; }
        public float LedgeHoldTimerProgress { get; set; }

        public LedgeDetectionResult CheckLedge(int wallDirection)
        {
            LedgeDetectionResult result = new LedgeDetectionResult();
            if (wallDirection == 0) return result;

            Bounds bounds = PlayerCollider.bounds;
            Vector2 direction = wallDirection == -1 ? Vector2.left : Vector2.right;

            // Look slightly ahead of the collider (extents.x + check offset)
            float horizCheckDist = bounds.extents.x + 0.3f;
            
            // Lower check point at waist/mid body level
            result.LowerStart = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y * 0.2f);
            // Upper check point near head level
            result.UpperStart = new Vector2(bounds.center.x, bounds.center.y + bounds.extents.y * 0.85f);

            RaycastHit2D lowerHit = Physics2D.Raycast(result.LowerStart, direction, horizCheckDist, groundLayerMask);
            RaycastHit2D upperHit = Physics2D.Raycast(result.UpperStart, direction, horizCheckDist, groundLayerMask);

            result.LowerHitPoint = lowerHit.collider != null ? lowerHit.point : result.LowerStart + direction * horizCheckDist;
            result.UpperHitPoint = upperHit.collider != null ? upperHit.point : result.UpperStart + direction * horizCheckDist;

            bool lowerTouched = lowerHit.collider != null && !lowerHit.collider.isTrigger;
            bool upperEmpty = upperHit.collider == null || upperHit.collider.isTrigger;

            if (lowerTouched && upperEmpty)
            {
                float wallX = lowerHit.point.x;
                float targetX = wallX + wallDirection * (bounds.extents.x + Stats.LedgeClimbSafetyOffset);
                float startY = bounds.center.y + bounds.extents.y + 0.5f;
                result.VerticalRayStart = new Vector2(targetX, startY);

                float castDistance = bounds.extents.y + 0.8f;
                RaycastHit2D groundHit = Physics2D.Raycast(result.VerticalRayStart, Vector2.down, castDistance, groundLayerMask);

                if (groundHit.collider != null && !groundHit.collider.isTrigger)
                {
                    result.GroundHitPoint = groundHit.point;
                    result.ClimbTarget = new Vector2(targetX, groundHit.point.y + bounds.extents.y + 0.05f);

                    // Check if ground itself is death layer
                    bool isGroundHazard = ((1 << groundHit.collider.gameObject.layer) & deathLayerMask) != 0;

                    if (!isGroundHazard)
                    {
                        Vector2 boxSize = bounds.size * 0.95f;
                        Collider2D overlap = Physics2D.OverlapBox(result.ClimbTarget, boxSize, 0f, groundLayerMask);
                        
                        // Check if there is an overlapping death layer hazard in the climb target space
                        Collider2D deathOverlap = Physics2D.OverlapBox(result.ClimbTarget, boxSize, 0f, deathLayerMask);

                        if ((overlap == null || overlap.isTrigger) && deathOverlap == null)
                        {
                            result.SpaceClear = true;
                            result.LedgeDetected = true;
                        }
                    }
                }
                else
                {
                    result.GroundHitPoint = result.VerticalRayStart + Vector2.down * castDistance;
                }
            }

            LatestLedgeResult = result;
            return result;
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

            if (LatestLedgeResult.LowerStart != Vector2.zero)
            {
                var ledge = LatestLedgeResult;

                // 1. Lower check
                Gizmos.color = ledge.LedgeDetected ? Color.green : Color.yellow;
                Gizmos.DrawLine(ledge.LowerStart, ledge.LowerHitPoint);
                Gizmos.DrawWireSphere(ledge.LowerHitPoint, 0.04f);

                // 2. Upper check
                Gizmos.color = ledge.LedgeDetected ? Color.green : Color.red;
                Gizmos.DrawLine(ledge.UpperStart, ledge.UpperHitPoint);
                Gizmos.DrawWireSphere(ledge.UpperHitPoint, 0.04f);

                if (ledge.LedgeDetected)
                {
                    // 3. Downward ray
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(ledge.VerticalRayStart, ledge.GroundHitPoint);
                    Gizmos.DrawWireSphere(ledge.GroundHitPoint, 0.04f);

                    // 4. Target Box
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(ledge.ClimbTarget, PlayerCollider.bounds.size);

                    debugText += $"\nLedge Hold: {LedgeHoldTimerProgress:F2}s / {Stats.LedgeClimbHoldTime:F2}s";
                }
            }

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