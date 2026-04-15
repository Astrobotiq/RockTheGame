using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin zemine temas ettigi durumu yonetir. Yatay hareketleri, ziplamayi ve yetenek yenilenmelerini ScriptableObject verileriyle isler.
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
            Vector2 currentVelocity = context.PlayerRigidbody.linearVelocity;
            currentVelocity.y = 0f;
            context.PlayerRigidbody.linearVelocity = currentVelocity;
            
            context.LeftAnchor = null;
            context.RightAnchor = null;
            context.ResetDash();
            context.ResetSlingshot();
            context.RefillWallStamina();
            context.ColorController.ResetAllColors();
            context.ResetWallSlideTime();
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
            float targetVelocityX = context.Input.LeftStick.x * stats.MoveSpeed;
            context.PlayerRigidbody.linearVelocity = new Vector2(targetVelocityX, context.PlayerRigidbody.linearVelocity.y);
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            Vector2 facingDirection = new Vector2(context.Input.LeftStick.x, 0f).normalized;
            context.LeftArm.UpdateArmRotation(facingDirection);
            context.RightArm.UpdateArmRotation(context.Input.RightStick);
        }

        private void CheckGrappleInput()
        {
            if (context.Input.IsRightTriggerHeld && !context.RightAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.RightStick, out Vector2 hitPoint))
                {
                    context.RightAnchor = hitPoint;
                    context.TransitionToState(new SwingingState(context, ActiveArm.Right));
                }
            }
        }

        private void CheckAirborneTransitions()
        {
            // Sadece o an basildiysa degil, hafizada (buffer) ziplama istegi varsa da zipla
            if (context.JumpBufferTimer > 0f)
            {
                context.ConsumeJumpBuffer(); // Hakki tuket
                Vector2 jumpVelocityVector = new Vector2(context.PlayerRigidbody.linearVelocity.x, stats.JumpVelocity);
                context.TransitionToState(new AirborneState(context, jumpVelocityVector,true));
            }
            else if (!context.IsGrounded)
            {
                // Ucurumdan dustu! Airborne durumuna Coyote Time gondererek gecis yap
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity,true, 0f, 0f, stats.CoyoteTimeDuration));
            }
        }

        private void CheckDashTransition()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick));
            }
        }

        private void CheckWallClimb()
        {
            if (!context.CanWallClimb) return;

            if (context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall)
            {
                context.TransitionToState(new WallClimbingState(context, -1));
                return;
            }

            if (context.Input.IsRightBumperHeld && context.IsTouchingRightWall)
            {
                context.TransitionToState(new WallClimbingState(context, 1));
            }
        }
    }
}