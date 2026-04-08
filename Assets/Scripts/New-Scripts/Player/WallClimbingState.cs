using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin duvara tutunma, tırmanma, duvardan zıplama ve stamina limitlerini yöneten durum sınıfı.
    /// Kanca atılmasına izin vermez, ancak Dash atılabilir.
    /// </summary>
    public class WallClimbingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float moveSpeedCache;
        private readonly float gravityCache;
        
        // 1 = Sağ Duvar, -1 = Sol Duvar
        private readonly int wallDirection; 
        
        private readonly float climbSpeed = 8f;
        private readonly Vector2 wallJumpForce = new Vector2(15f, 20f); // Sabit dışarı ve yukarı fırlatma
        
        private float staminaTimer;
        private bool warningTriggered;

        public WallClimbingState(PlayerController context, int wallDirection, float moveSpeedCache, float gravityCache)
        {
            this.context = context;
            this.wallDirection = wallDirection;
            this.moveSpeedCache = moveSpeedCache;
            this.gravityCache = gravityCache;
        }

        public void EnterState()
        {
            staminaTimer = 0f;
            warningTriggered = false;
            
            // 1. Hızı Sıfırla
            context.PlayerRigidbody.linearVelocity = Vector2.zero; 

            // 2. Duvara Yapışma (Snapping)
            Vector2 direction = wallDirection == -1 ? Vector2.left : Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(context.PlayerCollider.bounds.center, direction, 2f, context.GroundLayerMask);
            
            if (hit.collider != null)
            {
                // Karakterin genişliğinin yarısı (merkezden kenara olan mesafe)
                float extentsX = context.PlayerCollider.bounds.extents.x;
                
                // Duvarın yüzeyinden (hit.point.x), karakterin yarıçapı kadar geriye gel 
                // (0.02f güvenlik payı bırakıyoruz ki karakter duvarın içine geçip sıkışmasın)
                float snapX = hit.point.x - (direction.x * (extentsX + 0.02f));
                
                // Karakteri duvara hizala
                context.PlayerRigidbody.position = new Vector2(snapX, context.PlayerRigidbody.position.y);
            }
        }

        public void UpdateState()
        {
            Debug.Log("WallClimbingState: UpdateState called. Stamina Timer: " + staminaTimer);
            HandleStamina();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            // Yukarı/Aşağı tırmanma (Analog çubuğun Y ekseni okunur)
            float inputY = context.Input.LeftStick.y;
            context.PlayerRigidbody.linearVelocity = new Vector2(0f, inputY * climbSpeed);
        }

        public void ExitState()
        {
        }

        private void HandleStamina()
        {
            staminaTimer += Time.deltaTime;

            if (staminaTimer >= 3f && !warningTriggered)
            {
                warningTriggered = true;
                context.TriggerStaminaWarning(); // Görsel UI veya Efekt dinleyicilerine haber ver
            }

            if (staminaTimer >= 6f)
            {
                // Stamina bitti, tutunmayı bırak ve düş.
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, Vector2.zero));
            }
        }

        private void CheckInputTransitions()
        {
            Debug.Log("WallClimbingState: Checking input transitions. Dash: " + context.Input.IsDashPressed + ", Jump: " + context.Input.IsJumpPressed);
            // Dash atma
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick, moveSpeedCache));
                return;
            }

            // Duvardan Zıplama (Sabit fırlatma)
            if (context.Input.IsJumpPressed)
            {
                Debug.Log("WallClimbingState: Jump pressed. Transitioning to AirborneState with wall jump velocity.");
                Vector2 jumpVelocity = new Vector2(-wallDirection * wallJumpForce.x, wallJumpForce.y);
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, jumpVelocity));
                return;
            }

            // İlgili tuşu (Bumper) bırakırsa veya duvar biterse düş
            bool isHoldingCurrentWall = (wallDirection == -1 && context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall()) ||
                                        (wallDirection == 1 && context.Input.IsRightBumperHeld && context.IsTouchingRightWall());

            if (!isHoldingCurrentWall)
            {
                Debug.Log("WallClimbingState: Wall lost or bumper released. Transitioning to AirborneState.");
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, Vector2.zero));
            }
        }
    }
}