using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin duvardan asagi dogru surtunerek kaydigi durumu yonetir.
    /// Maksimum 2 saniye surebilir. Oyuncu analogu duvara itmezse veya sure biterse karakter duser.
    /// Kayarken ziplama (Wall Jump), tirmanma (Wall Climb), dash atma ve kanca firlatma (Swing) yeteneklerine izin verir.
    /// </summary>
    public class WallSlidingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly int wallDirection; // -1: Sol Duvar, 1: Sag Duvar

        public WallSlidingState(PlayerController context, int wallDirection)
        {
            this.context = context;
            this.stats = context.Stats;
            this.wallDirection = wallDirection;
        }

        public void EnterState()
        {
            context.ResetDash();
        }

        public void UpdateState()
        {
            context.ConsumeWallSlideTime(Time.deltaTime);

            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            Vector2 velocity = context.Velocity;

            if (context.CurrentWallSlideTime <= 0f)
            {
                context.TransitionToState(new AirborneState(context, velocity));
                return;
            }

            bool isPushingTowardsWall = (wallDirection == -1 && context.Input.LeftStick.x < -0.1f) || 
                                        (wallDirection == 1 && context.Input.LeftStick.x > 0.1f);
            
            if (!isPushingTowardsWall)
            {
                context.TransitionToState(new AirborneState(context, velocity, false, 0f, 0.2f, stats.CoyoteTimeDuration));
                return;
            }

            velocity.y = Mathf.MoveTowards(velocity.y, -stats.WallSlideMaxSpeed, stats.WallSlideFriction * Time.fixedDeltaTime);
            context.Velocity = velocity;

            if (context.IsGrounded)
            {
                context.TransitionToState(new GroundedState(context));
            }
            else if (!IsStillOnWall())
            {
                context.TransitionToState(new AirborneState(context, velocity));
            }
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            context.LeftArm.UpdateArmRotation(context.Input.LeftStick);
            context.RightArm.UpdateArmRotation(context.Input.RightStick);
        }

        private void CheckInputTransitions()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick));
                return;
            }

            if (CheckGrappleInputs()) return;

            if (context.Input.IsJumpPressed)
            {
                context.ResetWallSlideTime();
                
                Vector2 jumpDirection = new Vector2(-wallDirection * stats.WallSlideJumpForce.x, stats.WallSlideJumpForce.y);
                
                context.TransitionToState(new AirborneState(context, jumpDirection, true, 0f, 0f, 0f, stats.WallJumpInputLockoutTime));
                return;
            }

            bool climbInput = (wallDirection == -1 && context.Input.IsLeftBumperHeld) || 
                              (wallDirection == 1 && context.Input.IsRightBumperHeld);
                              
            if (climbInput && context.CanWallClimb)
            {
                context.TransitionToState(new WallClimbingState(context, wallDirection));
                return;
            }
        }

        private bool CheckGrappleInputs()
        {
            bool transitionTriggered = false;

            // Sol Kanca Atisi
            if (context.Input.IsLeftTriggerHeld && !context.LeftAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.LeftStick, out Vector2 hitPoint))
                {
                    context.LeftAnchor = hitPoint;
                    transitionTriggered = true;
                }
            }

            // Sag Kanca Atisi
            if (context.Input.IsRightTriggerHeld && !context.RightAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.RightStick, out Vector2 hitPoint))
                {
                    context.RightAnchor = hitPoint;
                    transitionTriggered = true;
                }
            }

            if (transitionTriggered)
            {
                EvaluateGrappleTransition();
                return true;
            }

            return false;
        }

        private void EvaluateGrappleTransition()
        {
            if (context.LeftAnchor.HasValue && context.RightAnchor.HasValue)
            {
                if (context.CheckNodeCoincidence() && context.CanSlingshot)
                {
                    context.TransitionToState(new SlingshotState(context));
                }
                else
                {
                    context.TransitionToState(new DualSwingingState(context));
                }
            }
            else if (context.LeftAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Left));
            }
            else if (context.RightAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Right));
            }
        }

        private bool IsStillOnWall()
        {
            return wallDirection == -1 ? context.IsTouchingLeftWall : context.IsTouchingRightWall;
        }
    }
}