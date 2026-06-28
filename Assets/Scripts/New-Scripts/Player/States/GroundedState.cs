using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin zemine temas ettiği durumu yönetir. Yatay hareketleri, zıplamayı ve yetenek yenilenmelerini izler.
    /// </summary>
    public class GroundedState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;

        public GroundedState(PlayerController context)
        {
            this.context = context;
            this.stats = context.Stats;
        }

        public void EnterState()
        {
            Vector2 currentVelocity = this.context.Velocity;
            currentVelocity.y = 0f;
            this.context.Velocity = currentVelocity;
            
            this.context.LeftAnchor = null;
            this.context.RightAnchor = null;
            this.context.ResetDash();
            this.context.ResetSlingshot();
            this.context.RefillWallStamina();
            this.context.ColorController.ResetAllColors();
            this.context.ResetWallSlideTime();

            if (Time.timeSinceLevelLoad > 0.1f && this.context.Audio != null)
            {
                this.context.Audio.PlayLand();
            }
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckGrappleInput();
            CheckAirborneTransitions();
            CheckDashTransition();
            CheckWallClimb();
        }

        public void FixedUpdateState()
        {
            float targetVelocityX = this.context.Input.LeftStick.x * this.stats.MoveSpeed;
            this.context.Velocity = new Vector2(targetVelocityX, this.context.Velocity.y);
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            Vector2 facingDirection = new Vector2(this.context.Input.LeftStick.x, 0f).normalized;
            this.context.LeftArm.UpdateArmRotation(facingDirection);
            this.context.RightArm.UpdateArmRotation(this.context.Input.RightStick);
        }

        private void CheckGrappleInput()
        {
            if (this.context.Input.IsRightTriggerHeld && !this.context.RightAnchor.HasValue)
            {
                if (this.context.TryCastGrapple(this.context.Input.RightStick, out Vector2 hitPoint))
                {
                    this.context.RightAnchor = hitPoint;
                    this.context.TransitionToState(new SwingingState(this.context, ActiveArm.Right, wasGrounded: true));
                }
            }
        }

        private void CheckAirborneTransitions()
        {
            if (this.context.JumpBufferTimer > 0f)
            {
                this.context.ConsumeJumpBuffer();
                if (this.context.Audio != null) this.context.Audio.PlayJump();
                
                Vector2 jumpVelocityVector = new Vector2(this.context.Velocity.x, this.stats.JumpVelocity);
                
                IMovingSurface movingSurface = this.context.PhysicsHandler.CurrentMovingSurface;
                var bypassJumpGravity = false;
                if (movingSurface != null && movingSurface.JumpBoostMultiplier > 0f)
                {
                    jumpVelocityVector += movingSurface.SurfaceVelocity * movingSurface.JumpBoostMultiplier;
                    bypassJumpGravity = true;
                }
                
                this.context.TransitionToState(new AirborneState(this.context, jumpVelocityVector, isJumping:true, bypassJumpGravity: bypassJumpGravity, endEarlyGravityMultiplier: 0.5f));
                

                
            }
            else if (!this.context.IsGrounded)
            {
                this.context.TransitionToState(new AirborneState(this.context, this.context.Velocity, isJumping:true,grappleLockout:0f, wallClimbLockout:0f, coyote:this.stats.CoyoteTimeDuration));
            }
        }

        private void CheckDashTransition()
        {
            if (this.context.Input.IsDashPressed && this.context.HasDashCharge)
            {
                this.context.TransitionToState(new DashState(this.context, this.context.Input.LeftStick));
            }
        }

        private void CheckWallClimb()
        {
            if (!this.context.CanWallClimb) return;

            if (this.context.Input.IsLeftBumperHeld && this.context.IsTouchingLeftWall)
            {
                this.context.TransitionToState(new WallClimbingState(this.context, -1));
                return;
            }

            if (this.context.Input.IsRightBumperHeld && this.context.IsTouchingRightWall)
            {
                this.context.TransitionToState(new WallClimbingState(this.context, 1));
            }
        }
    }
}