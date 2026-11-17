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
        [SerializeField] private PlayerVibrationSettingsSO vibrationSettings;
        public PlayerVibrationSettingsSO VibrationSettings => vibrationSettings;

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
        /// <summary>Normal hareket durumunda ve yerde (kanca, duvar, dash değil). Yer/hava durumlarının nasıl
        /// bölündüğünden bağımsız soru (yer-hava-birlestirme-plani.md Aşama 2).</summary>
        public bool IsInNormalGround => CurrentState is NormalState { IsOnGround: true };
        /// <summary>Normal hareket durumunda ve havada (kanca, duvar, dash değil).</summary>
        public bool IsInNormalAir => CurrentState is NormalState { IsOnGround: false };
        /// <summary>Ham nokta atamak sabit anchor demektir: o kolun izlenen node'u silinir.
        /// Hareketli node'a tutunmak için <see cref="AttachAnchor"/> (docs/hareketli-node-tasarimi.md).</summary>
        public Vector2? LeftAnchor
        {
            get => _leftAnchor;
            set { _leftAnchor = value; LeftAnchorNode = null; _leftAnchorDelta = Vector2.zero; _leftSimulatedVelocity = Vector2.zero; }
        }
        public Vector2? RightAnchor
        {
            get => _rightAnchor;
            set { _rightAnchor = value; RightAnchorNode = null; _rightAnchorDelta = Vector2.zero; _rightSimulatedVelocity = Vector2.zero; }
        }
        /// <summary>Anchor'ın izlediği node; sabit anchor'da null.</summary>
        public Transform LeftAnchorNode { get; private set; }
        public Transform RightAnchorNode { get; private set; }

        private Vector2? _leftAnchor;
        private Vector2? _rightAnchor;
        private Vector2 _leftAnchorDelta;
        private Vector2 _rightAnchorDelta;
        // Node'un fizik gövdesi ve gövdeye göre konumu: interpolation açık node'un Transform'u adımlar arasında
        // düzgün değişmez, gürültülü kayma ataletle büyüyüp titreme yapar; konum gövdenin fizik pozundan okunur.
        private Rigidbody2D _leftNodeBody;
        private Rigidbody2D _rightNodeBody;
        private Vector2 _leftNodeLocal;
        private Vector2 _rightNodeLocal;
        // Test/tasarım aracı: yerinde duran node'un bırakışta verdiği sahte hız (Nodes.SimulatedNodeVelocity)
        private Vector2 _leftSimulatedVelocity;
        private Vector2 _rightSimulatedVelocity;

        public bool HasDashCharge { get; private set; } = true;
        public bool CanSlingshot { get; private set; } = true;
        public bool CanWallClimb { get; private set; } = true;
        public float CurrentWallStamina { get; private set; }
        
        public float ActiveSpeedMultiplier { get; set; } = 1f;

        /// <summary>Spiral motoru çalışırken sprite'ın baktığı yön: +1 sağ (CCW), −1 sol (CW), 0 = kilit yok.</summary>
        public int SpiralFacingSign { get; set; }

        // --- Hit Stop & Frame Pause ---
        private bool _isPaused;
        /// <summary>Respawn ya da hitstop sırasında true; oyuncu girdi ve fizik işlemez.</summary>
        public bool IsPaused => _isPaused;
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
            ResetWallSlideTime();

            TransitionToState(new NormalState(this, Vector2.zero, AirEntry.Fall()));
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
            if (_isPaused || New_Scripts.Pause.PauseManager.BlocksGameplay) return;

            if (Input.IsJumpPressed)
                JumpBufferTimer = Stats.JumpBufferDuration;
            else if (JumpBufferTimer > 0f)
                JumpBufferTimer -= Time.deltaTime;

            CurrentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            if (_isPaused) return;

            RefreshMovingAnchors();
            CurrentState?.FixedUpdateState();

            // 2. FSM'in hesapladığı teorik hızı bir delta mesafeye çevirip handler'a veriyoruz.
            // Handler, çarpışmaları çözüp objeyi hareket ettirdikten sonra "Gerçekten ulaştığım hız bu" diyerek gerçeği FSM'e geri döndürür.
            // Kose duzeltmesi yalnizca serbest hareketde (hava, dash); ip ve duvar durumlarinda kaydirma yok.
            bool isDashing = CurrentState is DashState;
            float ceilingCorrection = isDashing || IsInNormalAir ? Stats.CeilingCornerCorrection : 0f;
            float dashCorrection = isDashing ? Stats.DashCornerCorrection : 0f;
            Velocity = physicsHandler.Move(Velocity * Time.fixedDeltaTime, ceilingCorrection, dashCorrection);
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
            return TryCastGrapple(direction, out hitPoint, out _);
        }

        public bool TryCastGrapple(Vector2 direction, out Vector2 hitPoint, out Transform node)
        {
            return nodeDetector.TryFindBestNode(direction, PlayerRigidbody.position, out hitPoint, out node);
        }

        /// <summary>Kolu bir node'a bağlar: anchor her fizik adımında node'un konumunu izler.</summary>
        public void AttachAnchor(ActiveArm arm, Vector2 point, Transform node)
        {
            // Node'un o anki hızı: tutunma anında da AnchorVelocity geçerli olsun (dünya → node'a göre hız dönüşümü).
            var surface = node != null ? node.GetComponentInParent<New_Scripts.Platform.IMovingSurface>() : null;
            Vector2 delta = surface != null ? surface.DeltaPosition : Vector2.zero;
            var simulatedNode = node != null ? node.GetComponentInParent<Nodes.SimulatedNodeVelocity>() : null;
            Vector2 simulated = simulatedNode != null ? simulatedNode.Velocity : Vector2.zero;
            Rigidbody2D body = BindNodeBody(node, out Vector2 local);
            if (body != null) point = body.GetRelativePoint(local);
            if (arm == ActiveArm.Left)
            {
                LeftAnchor = point;
                LeftAnchorNode = node;
                _leftAnchorDelta = delta;
                _leftNodeBody = body;
                _leftNodeLocal = local;
                _leftSimulatedVelocity = simulated;
            }
            else
            {
                RightAnchor = point;
                RightAnchorNode = node;
                _rightAnchorDelta = delta;
                _rightNodeBody = body;
                _rightNodeLocal = local;
                _rightSimulatedVelocity = simulated;
            }
        }

        /// <summary>Kolun node'unun bu fizik adımındaki kayması; sabit anchor'da sıfır.</summary>
        public Vector2 AnchorDelta(ActiveArm arm) => arm == ActiveArm.Left ? _leftAnchorDelta : _rightAnchorDelta;

        /// <summary>Kolun node'unun bu fizik adımındaki hızı (<c>AnchorDelta / fixedDeltaTime</c>).</summary>
        public Vector2 AnchorVelocity(ActiveArm arm) => AnchorDelta(arm) / Time.fixedDeltaTime;

        /// <summary>Bırakışta aktarılacak node hızı: gerçek hız + (varsa) <see cref="Nodes.SimulatedNodeVelocity"/>.</summary>
        public Vector2 AnchorReleaseVelocity(ActiveArm arm) =>
            AnchorVelocity(arm) + (arm == ActiveArm.Left ? _leftSimulatedVelocity : _rightSimulatedVelocity);

        /// <summary>Durumdan önce: izlenen node'lar kaydıysa anchor'ı yeniler. Pasif ya da yok olmuş node'da anchor son konumda kalır.</summary>
        private void RefreshMovingAnchors()
        {
            _leftAnchorDelta = FollowNode(LeftAnchorNode, _leftNodeBody, _leftNodeLocal, ref _leftAnchor);
            _rightAnchorDelta = FollowNode(RightAnchorNode, _rightNodeBody, _rightNodeLocal, ref _rightAnchor);
        }

        /// <summary>Node'u taşıyan Rigidbody2D ve node'un gövdeye göre (ölçeksiz) konumu; gövde yoksa null.</summary>
        private static Rigidbody2D BindNodeBody(Transform node, out Vector2 local)
        {
            local = Vector2.zero;
            Rigidbody2D body = node != null ? node.GetComponentInParent<Rigidbody2D>() : null;
            if (body == null) return null;
            Transform bodyTransform = body.transform;
            local = Quaternion.Inverse(bodyTransform.rotation) * (node.position - bodyTransform.position);
            return body;
        }

        private static Vector2 FollowNode(Transform node, Rigidbody2D body, Vector2 local, ref Vector2? anchor)
        {
            if (node == null || !anchor.HasValue || !node.gameObject.activeInHierarchy) return Vector2.zero;
            Vector2 current = body != null ? body.GetRelativePoint(local) : (Vector2)node.position;
            Vector2 delta = current - anchor.Value;
            anchor = current;
            return delta;
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
            if (vibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.Vibrate(vibrationSettings.HighImpact);
            }
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

            UIController.UpdateStamina(CurrentWallStamina, Stats.MaxWallStamina, Stats.StaminaWarningThreshold);
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
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.StopContinuousVibration();
            }
            // Spiraldeyken ölünce SwingingState ancak OnEndRespawn'da çıkar; geçiş boyunca yön kilidi ve döngü sesi sürmesin.
            SpiralFacingSign = 0;
            if (Audio != null) Audio.StopSpiralLoop();
        }
        
        public void OnEndRespawn(){
            TransitionToState(new NormalState(this, Vector2.zero, AirEntry.Fall()));
            _isPaused = false;
        }

        public void ConsumeJumpBuffer()
        {
            JumpBufferTimer = 0f;
        }

        public float CurrentWallSlideTime { get; private set; }

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
                    TransitionToState(new NormalState(this,
                        new Vector2(_preTransitionVelocity.x, Stats.JumpVelocity), AirEntry.Fall()));
                    break;

                case TransitionDirection.Right:
                case TransitionDirection.Left:
                case TransitionDirection.Down:
                    TransitionToState(new NormalState(this, _preTransitionVelocity, AirEntry.Fall()));
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

        /// <summary>
        /// wallDirection (-1/1) yonunde, collider kenari ile duvar arasindaki bosluk
        /// <see cref="PlayerStatsSO.WallClimbContactTolerance"/> icindeyse true doner (tirmanma/kayma yapismasi icin).
        /// </summary>
        public bool CanGrabWall(int wallDirection)
        {
            bool touching = wallDirection == -1 ? IsTouchingLeftWall : IsTouchingRightWall;
            return touching || TryMeasureWallGap(wallDirection, out _);
        }

        public bool TryMeasureWallGap(int wallDirection, out float gap)
        {
            gap = 0f;
            if (wallDirection == 0) return false;

            Bounds bounds = PlayerCollider.bounds;
            float extentsX = bounds.extents.x;
            ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayerMask);

            int count = Physics2D.Raycast(bounds.center, new Vector2(wallDirection, 0f), filter, _wallGapHits,
                extentsX + Stats.WallClimbContactTolerance);
            for (int i = 0; i < count; i++)
            {
                if (_wallGapHits[i].collider == PlayerCollider) continue;
                gap = _wallGapHits[i].distance - extentsX;
                return true;
            }

            return false;
        }

        private readonly RaycastHit2D[] _wallGapHits = new RaycastHit2D[4];

        /// <summary>
        /// Celeste WallJumpCheck: oyuncu kutusu wallDirection yonunde distance icinde bir duvara degiyor mu.
        /// Merkez isini degil tum kutuyu tarar (yalnizca ust ya da alt kismi duvara bakan oyuncu da sayilir);
        /// zemin ve tavana degmemek icin kutu dikeyde 0.1 kucultulur.
        /// </summary>
        public bool IsWallWithin(int wallDirection, float distance)
        {
            if (wallDirection == 0 || distance <= 0f) return false;

            Bounds bounds = PlayerCollider.bounds;
            ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayerMask);
            Vector2 size = new Vector2(bounds.size.x, bounds.size.y - 0.1f);

            int count = Physics2D.BoxCast(bounds.center, size, 0f, new Vector2(wallDirection, 0f), filter,
                _wallGapHits, distance);
            for (int i = 0; i < count; i++)
            {
                if (_wallGapHits[i].collider != PlayerCollider) return true;
            }

            return false;
        }

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