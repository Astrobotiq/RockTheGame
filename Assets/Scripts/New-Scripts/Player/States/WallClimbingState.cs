using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin duvara tutunma, tirmanma, duvardan ziplama limitlerini ve yuzey hizalama (snap) islemlerini ScriptableObject uzerinden yoneten durum sinifi.
    /// </summary>
    public class WallClimbingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly int wallDirection; 
        
        private bool warningTriggered;

        public WallClimbingState(PlayerController context, int wallDirection)
        {
            this.context = context;
            this.stats = context.Stats;
            this.wallDirection = wallDirection;
        }

        public void EnterState()
        {
            warningTriggered = context.CurrentWallStamina <= stats.StaminaWarningThreshold;
            context.PlayerRigidbody.linearVelocity = Vector2.zero; 
            
            Vector2 direction = wallDirection == -1 ? Vector2.left : Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(context.PlayerCollider.bounds.center, direction, stats.WallSnapRaycastDistance, context.GroundLayerMask);
            
            if (hit.collider != null)
            {
                float extentsX = context.PlayerCollider.bounds.extents.x;
                float snapX = hit.point.x - (direction.x * (extentsX + stats.WallSnapSafetyOffset));
                
                context.PlayerRigidbody.position = new Vector2(snapX, context.PlayerRigidbody.position.y);
            }
            
            context.UIController.ShowStaminaBar();
            context.ResetWallSlideTime();
        }

        public void UpdateState()
        {
            context.ConsumeWallStamina(Time.deltaTime);

            if (!warningTriggered && context.CurrentWallStamina <= stats.StaminaWarningThreshold)
            {
                warningTriggered = true;
                // Ileride UI barini kirmizi yapip yanip sondurmek istersen cagriyi buraya ekleyebilirsin.
            }

            if (context.CurrentWallStamina <= 0f)
            {
                context.TransitionToState(new AirborneState(context, Vector2.zero,false, 0f, 0.5f));
            }

            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            float inputY = context.Input.LeftStick.y;
            context.PlayerRigidbody.linearVelocity = new Vector2(0f, inputY * stats.ClimbSpeed);
        }

        private void CheckInputTransitions()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick));
                return;
            }

            if (context.Input.IsJumpPressed)
            {
                Vector2 jumpVelocity = new Vector2(-wallDirection * stats.WallJumpForce.x, stats.WallJumpForce.y);
                context.TransitionToState(new AirborneState(context, jumpVelocity,false, 0f, 0.2f));
                return;
            }

            bool isHoldingCurrentWall = (wallDirection == -1 && context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall) ||
                                        (wallDirection == 1 && context.Input.IsRightBumperHeld && context.IsTouchingRightWall);

            if (!isHoldingCurrentWall)
            {
                context.TransitionToState(new AirborneState(context, Vector2.zero,false, 0f, 0.2f));
            }
        }

        public void ExitState()
        {
        }
    }
}