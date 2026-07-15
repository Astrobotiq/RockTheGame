using New_Scripts.Platform;
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
        private float ledgeClimbTimer;
        private bool isLedgeDetected;

        public WallClimbingState(PlayerController context, int wallDirection)
        {
            this.context = context;
            this.stats = context.Stats;
            this.wallDirection = wallDirection;
        }

        public void EnterState()
        {
            context.PhysicsHandler.ClingingWallDirection = wallDirection;
            warningTriggered = context.CurrentWallStamina <= stats.StaminaWarningThreshold;
            context.Velocity = Vector2.zero; 
            
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
                context.TransitionToState(new AirborneState(context, Vector2.zero,false, grappleLockout:0f, wallClimbLockout:0.5f));
                return;
            }

            // Check ledge climb
            var ledgeResult = context.CheckLedge(wallDirection);
            isLedgeDetected = ledgeResult.LedgeDetected;
            if (isLedgeDetected)
            {
                if (context.Input.LeftStick.y > 0.5f)
                {
                    ledgeClimbTimer += Time.deltaTime;
                    context.LedgeHoldTimerProgress = ledgeClimbTimer;
                    if (ledgeClimbTimer >= stats.LedgeClimbHoldTime)
                    {
                        context.TransitionToState(new LedgeClimbState(context, context.PlayerRigidbody.position, ledgeResult.ClimbTarget));
                        return;
                    }
                }
                else
                {
                    ledgeClimbTimer = 0f;
                    context.LedgeHoldTimerProgress = 0f;
                }
            }
            else
            {
                ledgeClimbTimer = 0f;
                context.LedgeHoldTimerProgress = 0f;
            }

            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            float inputY = context.Input.LeftStick.y;
            if (isLedgeDetected && inputY > 0f)
            {
                inputY = 0f; // Lock upward climbing when at a ledge
            }
            context.Velocity = new Vector2(0f, inputY * stats.ClimbSpeed);
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
                if (context.Audio != null) context.Audio.PlayJump();
                Vector2 jumpVelocity = new Vector2(0f, stats.ClimbVerticalJumpVelocity);
                
                IMovingSurface movingSurface = wallDirection == -1 
                    ? context.PhysicsHandler.CurrentLeftMovingSurface 
                    : context.PhysicsHandler.CurrentRightMovingSurface;
                
                bool bypassJumpGravity = movingSurface != null && movingSurface.JumpBoostMultiplier > 0f;
                if (bypassJumpGravity)
                {
                    jumpVelocity += movingSurface.SurfaceVelocity * movingSurface.JumpBoostMultiplier;
                }

                context.TransitionToState(new AirborneState(
                    context: context,
                    inheritedVelocity: jumpVelocity,
                    isJumping: true,
                    grappleLockout: 0f,
                    wallClimbLockout: 0.2f,
                    bypassJumpGravity: bypassJumpGravity,
                    endEarlyGravityMultiplier: bypassJumpGravity ? 0.5f : 1f
                ));
                return;
            }

            bool isHoldingCurrentWall = (wallDirection == -1 && context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall) ||
                                        (wallDirection == 1 && context.Input.IsRightBumperHeld && context.IsTouchingRightWall);

            if (!isHoldingCurrentWall)
            {
                context.TransitionToState(new AirborneState(context, Vector2.zero,false, grappleLockout:0f, wallClimbLockout:0.2f));
            }
        }

        public void ExitState()
        {
            context.PhysicsHandler.ClingingWallDirection = 0;
            context.LatestLedgeResult = default;
            context.LedgeHoldTimerProgress = 0f;
        }
    }
}