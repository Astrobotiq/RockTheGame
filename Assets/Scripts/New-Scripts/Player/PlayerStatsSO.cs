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
        [Tooltip("Celeste gibi degisken ziplama (deneme): tus basiliyken VarJumpTime boyunca yukari hiz sabit tutulur, birakinca yukselis normal (1x) yercekimiyle surer ve tepede asili kalma olmaz. Kapaliyken tusu birakmak JumpEndEarlyGravityMultiplier verir. Tam ziplama yuksekligi iki modda ayni: baslangic hizi HoldJumpSpeed ile dusurulur. Yerden, coyote, duvar ve tirmanma ziplamalarinda; boost'lu yuzeyde degil.")]
        public bool CelesteVariableJump = false;
        [Tooltip("Celeste VarJumpTime: tus basiliyken yukari hizin sabit tutuldugu sure (sn).")]
        public float VarJumpTime = 0.2f;

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
        [Tooltip("Havada yatay hiz MoveSpeed'in ustundeyken ve stick ayni yondeyken sonum (birim/sn2). Celeste orani: 0.4 x AirAcceleration.")]
        public float AirOverspeedReduce = 16f;
        public float MomentumDecay = 2f;
        [Tooltip("Havada duvara carpinca yatay hiz sifirlanir; duvar bu sure (sn) icinde biterse hiz geri verilir (kose cikisi). 0 = geri verme.")]
        public float WallSpeedRetentionTime = 0.06f;

        [Header("Dash")]
        [Tooltip("Dash yeteneğinin kat edeceği toplam yatay/dikey mesafe (Birim).")]
        public float DashDistance = 5f;
        [Tooltip("Dash eyleminin başlangıcından bitişine kadar geçen süre (Saniye).")]
        public float DashDuration = 0.15f;
        public float DashImpactMultiplier = 2f;
        public float DashEndMomentumMultiplier = 0.15f;
        public float DashSpeed => DashDistance / DashDuration;

        [Header("Moving Node")]
        [Tooltip("Hareketli node'dan bırakınca node hızının oyuncuya aktarılan oranı (docs/hareketli-node-tasarimi.md).")]
        public float MovingNodeLaunchMultiplier = 1f;
        [Tooltip("Hareketli node'dan bırakınca taşınan yatay hızın sıfıra indiği süre (sn). 0 = sönmez (duvar kuralı gibi).")]
        public float MovingNodeCarryDecayTime = 0.6f;
        [Tooltip("Node'un ivmesinin salınıma geçen oranı: 0 = node yalnızca taşır, 1 = tam fizik (pivot ivmesinin ters kuvveti).")]
        public float MovingNodeInertia = 0.6f;
        [Tooltip("Ataletin hesaplandığı node hızının bir saniyede en fazla ne kadar değişebileceği (b/sn²). Ani dönüşlerin darbesini birkaç adıma yayar.")]
        public float MovingNodeMaxPivotAccel = 60f;

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
        public float ClimbVerticalJumpVelocity = 15f;
        public float MaxWallStamina = 6f;
        public float StaminaWarningThreshold = 1.5f;
        public float WallSnapRaycastDistance = 2f;
        public float WallSnapSafetyOffset = 0.02f;
        [Tooltip("Tirmanirken/kayarken duvar bu mesafeye kadar uzaklasirsa karakter duvara cekilir ve temas kopmus sayilmaz.")]
        public float WallClimbContactTolerance = 0.15f;
        public float LedgeClimbHoldTime = 0.3f;
        public float LedgeClimbSafetyOffset = 0.05f;
        public float LedgeClimbDuration = 0.25f;
        
        [Header("Wall Sliding")]
        public float WallSlideMaxSpeed = 3f;
        public float WallSlideFriction = 10f;
        public Vector2 WallSlideJumpForce = new Vector2(15f, 18f);
        public float MaxWallSlideTime = 2f;
        public float WallJumpInputLockoutTime = 0.15f;
        [Tooltip("Havadayken duvar bu mesafe (birim) icindeyse ziplama duvar ziplamasi olur; temas ve stick sart degil, yukselirken de. Celeste 3 px = 0.375. 0 = kapali.")]
        public float WallJumpCheckDistance = 0.375f;
        
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
        [Tooltip("Havada/dash'te yukari giderken tavan kosesine bu kadar (birim) yakinsa yana kaydirip gecirir. Celeste 4 px = 0.5; 0.5 gamepad'de fazla kaygan geldi. 0 = kapali.")]
        public float CeilingCornerCorrection = 0.25f;
        [Tooltip("Dash'te yatay hareket bir koseye bu kadar (birim) yakinsa dikey kaydirip gecirir. Celeste 4 px = 0.5; 0.5 gamepad'de fazla kaygan geldi. 0 = kapali.")]
        public float DashCornerCorrection = 0.25f;
        
        [Header("Swing Hop Settings")]
        [Tooltip("Yerden swing'e geçerken node'a doğru yatay zıplama hızı")]
        public float SwingHopForwardSpeed = 8f;
        [Tooltip("Yerden swing'e geçerken dikey zıplama hızı")]
        public float SwingHopUpwardSpeed = 12f;
        [Tooltip("Sıçramanın ne kadar süreceği (Bu süre bitince sarkaç başlar)")]
        public float SwingHopDuration = 0.15f;
        
        [Header("Spiral Node")]
        [Tooltip("Spiral yönünde, hedef hızın altındayken teğet ivme. |Gravity|'den büyük olmalı (tam tur garantisi).")]
        public float SpiralDriveAccel = 60f;
        [Tooltip("Spiralin tersine giderken hızın spiral yönüne çevrildiği ivme. |Gravity|'den büyük olmalı.")]
        public float SpiralOpposeAccel = 45f;
        [Tooltip("Stick hedef hızı en fazla bu oranda artırır/azaltır (0.4 = ±%40). 1'in altında kalmalı: dönüşü asla durdurmaz.")]
        public float SpiralInputInfluence = 0.4f;
        [Tooltip("Hedef hızın üstündeyken yavaşlama ivmesi. 0 = fazla hız korunur.")]
        public float SpiralOverspeedDecay = 0f;
        [Tooltip("Gerçekleşen teğet hız komutun bu oranının altındaysa adım takılmış sayılır.")]
        public float SpiralBlockedRatio = 0.25f;
        [Tooltip("Art arda bu kadar takılmış adımda kanca kopar.")]
        public int SpiralBlockedSteps = 3;

        public float Gravity => -(2f * JumpHeight) / (TimeToApex * TimeToApex);
        public float JumpVelocity => Mathf.Abs(Gravity) * TimeToApex;

        /// <summary>
        /// Hizi holdTime boyunca sabit tutulan bir ziplamanin, tutmasiz originalSpeed ziplamasiyla ayni tepe yuksekligine
        /// cikmasi icin baslangic hizi: v*t + v^2/(2g) = v0^2/(2g)  =>  v = -g*t + sqrt(g^2*t^2 + v0^2).
        /// </summary>
        public static float HoldJumpSpeed(float originalSpeed, float gravityMagnitude, float holdTime)
        {
            if (originalSpeed <= 0f || holdTime <= 0f) return originalSpeed;
            float gt = gravityMagnitude * holdTime;
            return -gt + Mathf.Sqrt(gt * gt + originalSpeed * originalSpeed);
        }
    }
}