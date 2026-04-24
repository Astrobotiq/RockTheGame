using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin hareket, yetenek ve fizik parametrelerini barindiran, Inspector uzerinden anlik degistirilebilir veri kapsayici sinif.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Player Stats")]
    public class PlayerStatsSO : ScriptableObject
    {
        [Header("Grounded Movement")]
        public float MoveSpeed = 10f; 
        
        [Header("Jump Design Parameters")]
        public float JumpHeight = 4f;
        public float TimeToApex = 0.4f;

        [Header("Swinging")]
        public float SwingGravity = 25f;
        public float SwingInitialBoost = 5f;
        public float SwingForceMultiplier = 10f;
        public float MaxSwingSpeed = 17f;
        public float SwingCollisionRadius = 0.5f;
        public float MaxWallAngle = 45f;
        
        [Header("Airborne & Gravity")]
        public float TerminalVelocity = -30f;
        public float AirControlMultiplier = 0.5f;
        public float AirAcceleration = 20f;
        public float AirDrag = 5f;
        public float MomentumDecay = 2f;

        [Header("Dash")]
        [Tooltip("Dash yeteneğinin kat edeceği toplam yatay/dikey mesafe (Birim).")]
        public float DashDistance = 5f;
        [Tooltip("Dash eyleminin başlangıcından bitişine kadar geçen süre (Saniye).")]
        public float DashDuration = 0.15f;
        public float DashImpactMultiplier = 2f;
        public float DashEndMomentumMultiplier = 0.15f;
        public float DashSpeed => DashDistance / DashDuration;

        [Header("Slingshot")]
        public float MaxSlingshotSpeed = 40f;
        public float SlingshotLaunchDuration = 0.2f;
        public float SlingshotGrappleLockout = 0.25f;
        [Tooltip("Sapan firlatilmadan once geriye dogru cekilme suresi.")]
        public float SlingshotAnticipationDuration = 0.1f;
        [Tooltip("Geriye dogru cekilme (gerilme) hizi.")]
        public float SlingshotAnticipationSpeed = 10f;

        [Header("Wall Climb")]
        public float ClimbSpeed = 8f;
        public Vector2 WallJumpForce = new Vector2(15f, 20f);
        public float MaxWallStamina = 6f;
        public float StaminaWarningThreshold = 1.5f;
        public float WallSnapRaycastDistance = 2f;
        public float WallSnapSafetyOffset = 0.02f;
        
        [Header("Wall Sliding")]
        public float WallSlideMaxSpeed = 3f;
        public float WallSlideFriction = 10f;
        public Vector2 WallSlideJumpForce = new Vector2(15f, 18f);
        public float MaxWallSlideTime = 2f;
        public float WallJumpInputLockoutTime = 0.15f;
        
        [Header("Dual Swinging")]
        public float DualSpringStiffness = 25f;
        public float DualSpringDamping = 5f;
        public float DualSwingForceMultiplier = 15f;
        public float DualSwingCollisionRadius = 0.4f;
        
        [Header("Core Systems")]
        public float HitStopDuration = 0.15f;
        public float NodeCoincidenceThreshold = 0.5f;
        
        [Header("Advanced Jump Physics")]
        public float FallGravityMultiplier = 1.5f;
        public float JumpEndEarlyGravityMultiplier = 3f;
        public float ApexThreshold = 2f;
        public float ApexHangGravityMultiplier = 0.5f;
        
        [Header("Input Tolerances (Game Feel)")]
        public float CoyoteTimeDuration = 0.1f;
        public float JumpBufferDuration = 0.1f;
        
        public float Gravity => -(2f * JumpHeight) / (TimeToApex * TimeToApex);
        public float JumpVelocity => Mathf.Abs(Gravity) * TimeToApex;
    }
}