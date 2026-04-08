using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin duvara tutunma, tirmanma, duvardan ziplama limitlerini ve merkezilestirilmis stamina tuketimini yoneten durum sinifi.
    /// </summary>
    public class WallClimbingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float moveSpeedCache;
        private readonly float gravityCache;
        private readonly int wallDirection; 
        
        private readonly float climbSpeed = 8f;
        private readonly Vector2 wallJumpForce = new Vector2(15f, 20f);
        
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
            warningTriggered = context.CurrentWallStamina <= 1.5f;
            context.PlayerRigidbody.linearVelocity = Vector2.zero; 
            
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
            context.ConsumeWallStamina(Time.deltaTime);

            if (!warningTriggered && context.CurrentWallStamina <= 1.5f)
            {
                warningTriggered = true;
            }

            if (context.CurrentWallStamina <= 0f)
            {
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, Vector2.zero, 0f, 0.5f));
            }

            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            float inputY = context.Input.LeftStick.y;
            context.PlayerRigidbody.linearVelocity = new Vector2(0f, inputY * climbSpeed);
        }

        private void CheckInputTransitions()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick, moveSpeedCache));
                return;
            }

            if (context.Input.IsJumpPressed)
            {
                Vector2 jumpVelocity = new Vector2(-wallDirection * wallJumpForce.x, wallJumpForce.y);
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, jumpVelocity, 0f, 0.2f));
                return;
            }

            bool isHoldingCurrentWall = (wallDirection == -1 && context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall()) ||
                                        (wallDirection == 1 && context.Input.IsRightBumperHeld && context.IsTouchingRightWall());

            if (!isHoldingCurrentWall)
            {
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravityCache, -30f, Vector2.zero, 0f, 0.2f));
            }
        }

        public void ExitState()
        {
            Debug.Log($"Exiting WallClimbingState {context.CurrentWallStamina}");
        }
    }
}