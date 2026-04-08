/// <summary>
/// Karakterin zemine temas ettiği durumu yönetir. Sol kolu yatay harekete kilitler ve sadece sağ kol ile kanca geçişine izin verir.
/// </summary>
using UnityEngine;

namespace New_Scripts.Player
{
    public class GroundedState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float moveSpeed;
        private readonly float jumpVelocity;

        public GroundedState(PlayerController context, float moveSpeed, float jumpVelocity)
        {
            this.context = context;
            this.moveSpeed = moveSpeed;
            this.jumpVelocity = jumpVelocity;
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
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckGrappleInput();
            CheckAirborneTransitions();
            CheckDashTransition();
            CheckWallClimb();
        }

        void CheckWallClimb()
        {
            if (context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall())
            {
                context.TransitionToState(new WallClimbingState(context, -1, moveSpeed, context.Gravity));
                return;
            }

            if (context.Input.IsRightBumperHeld && context.IsTouchingRightWall())
            {
                context.TransitionToState(new WallClimbingState(context, 1, moveSpeed, context.Gravity));
                return;
            }
        }


        public void FixedUpdateState()
        {
            Debug.Log($"GroundedState FixedUpdate: Applying horizontal movement {context.Input.LeftStick.x}");
            float targetVelocityX = context.Input.LeftStick.x * moveSpeed;
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
                    context.TransitionToState(new SwingingState(context, ActiveArm.Right, 25f, 5f, moveSpeed));
                }
            }
        }

        private void CheckAirborneTransitions()
        {
            if (context.Input.IsJumpPressed)
            {
                Vector2 jumpVelocityVector = new Vector2(context.PlayerRigidbody.linearVelocity.x, jumpVelocity);
                context.TransitionToState(new AirborneState(context, moveSpeed, 0.5f, 25f, -30f, jumpVelocityVector));
            }
            else if (!context.IsGrounded)
            {
                context.TransitionToState(new AirborneState(context, moveSpeed, 0.5f, 25f, -30f, context.PlayerRigidbody.linearVelocity));
            }
        }
        void CheckDashTransition()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
                context.TransitionToState(new DashState(context, context.Input.LeftStick, moveSpeed));
        }
    }
}