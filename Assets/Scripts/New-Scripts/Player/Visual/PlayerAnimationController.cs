using UnityEngine;
using New_Scripts.Player.States;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Karakterin hareket durumlarını (hız, havada olma, dikey hız vb.) ve 
    /// özel state durumlarını (dash, swinging, wall climbing) Animator bileşenine aktaran, 
    /// aynı zamanda yönelimine göre sprite görselini çeviren animasyon kontrolcüsü.
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Animator animator;

        [Header("Sprite Flipping")]
        [SerializeField] private bool flipSpriteBasedOnDirection = true;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Animator parametre hash değerleri (Performans optimizasyonu için)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsSwingingHash = Animator.StringToHash("IsSwinging");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int IsWallClimbingHash = Animator.StringToHash("IsWallClimbing");
        private static readonly int IsWallSlidingHash = Animator.StringToHash("IsWallSliding");
        private static readonly int DashTriggerHash = Animator.StringToHash("DashTrigger");

        private bool _wasDashing;

        private void Awake()
        {
            // Eğer inspector üzerinden atanmamışlarsa otomatik olarak bulmaya çalış
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>() ?? GetComponent<PlayerController>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>() ?? GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (playerController == null || animator == null) return;

            UpdateAnimatorParameters();
            HandleSpriteFlipping();
        }

        /// <summary>
        /// PlayerController'dan güncel durum ve hız verilerini çekip Animator'a yansıtır.
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            // Temel Hareket Parametreleri
            float horizontalSpeed = Mathf.Abs(playerController.Velocity.x);
            animator.SetFloat(SpeedHash, horizontalSpeed);
            animator.SetBool(IsGroundedHash, playerController.IsGrounded);
            animator.SetFloat(VerticalVelocityHash, playerController.Velocity.y);

            // Gelişmiş State/Yetenek Parametreleri (Gelecekteki animasyon genişletmeleri için)
            var currentState = playerController.CurrentState;
            bool isDashing = currentState is DashState;

            // Dash yeni başladığında Trigger tetikle
            if (isDashing && !_wasDashing)
            {
                animator.SetTrigger(DashTriggerHash);
            }
            _wasDashing = isDashing;

            animator.SetBool(IsSwingingHash, currentState is SwingingState || currentState is DualSwingingState);
            animator.SetBool(IsDashingHash, isDashing);
            animator.SetBool(IsWallClimbingHash, currentState is WallClimbingState);
            animator.SetBool(IsWallSlidingHash, currentState is WallSlidingState);
        }

        /// <summary>
        /// Karakterin girdi veya hız yönüne göre sprite görselini sola/sağa çevirir.
        /// </summary>
        private void HandleSpriteFlipping()
        {
            if (!flipSpriteBasedOnDirection || spriteRenderer == null) return;

            float horizontalInput = 0f;
            if (playerController.Input != null)
            {
                horizontalInput = playerController.Input.LeftStick.x;
            }

            // Girdi varsa girdiye göre, yoksa anlık hıza göre yön belirle
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                spriteRenderer.flipX = horizontalInput < 0f;
            }
            else if (Mathf.Abs(playerController.Velocity.x) > 0.01f)
            {
                spriteRenderer.flipX = playerController.Velocity.x < 0f;
            }
        }
    }
}
