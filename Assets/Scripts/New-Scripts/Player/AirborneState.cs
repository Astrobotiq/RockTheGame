/// <summary>
/// Karakterin havada olduğu durumu yönetir. Momentum korunumunu işletir, kanca ve kusursuz zemin geçişlerini yönetir.
/// </summary>

using UnityEngine;

namespace New_Scripts.Player
{
    public class AirborneState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float moveSpeed;
        private readonly float airControlMultiplier;
        private readonly float gravity;
        private readonly float terminalVelocity;

        private Vector2 currentVelocity;

        public AirborneState(PlayerController context, float moveSpeed, float airControlMultiplier, float gravity,
            float terminalVelocity, Vector2 inheritedVelocity)
        {
            this.context = context;
            this.moveSpeed = moveSpeed;
            this.airControlMultiplier = airControlMultiplier;
            this.gravity = gravity;
            this.terminalVelocity = terminalVelocity;

            currentVelocity = inheritedVelocity;
        }

        public void EnterState()
        {
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckGrappleInputs();
        }

        public void FixedUpdateState()
        {
            if (context.IsGrounded && currentVelocity.y <= 0f)
            {
                context.TransitionToState(new GroundedState(context, moveSpeed, 15f));
                return;
            }

            ApplyGravity();
            ApplyAirControl();
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            context.LeftArm.UpdateArmRotation(context.Input.LeftStick);
            context.RightArm.UpdateArmRotation(context.Input.RightStick);
        }

        private void CheckGrappleInputs()
        {
            if (context.Input.IsLeftTriggerHeld && !context.LeftAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.LeftStick, out Vector2 hitPoint))
                {
                    context.LeftAnchor = hitPoint;
                    EvaluateTransition();
                    return;
                }
            }

            if (context.Input.IsRightTriggerHeld && !context.RightAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.RightStick, out Vector2 hitPoint))
                {
                    context.RightAnchor = hitPoint;
                    EvaluateTransition();
                    return;
                }
            }
        }

        private void EvaluateTransition()
        {
            if (context.LeftAnchor.HasValue && context.RightAnchor.HasValue)
            {
                if (context.CheckNodeCoincidence())
                {
                    context.TransitionToState(new SlingshotState(context, gravity, moveSpeed));
                }
                else
                {
                    context.LeftAnchor = null;
                }
            }
            else if (context.LeftAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Left, gravity, 5f, moveSpeed));
            }
            else if (context.RightAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Right, gravity, 5f, moveSpeed));
            }
        }

        private void ApplyGravity()
        {
            currentVelocity.y -= gravity * Time.fixedDeltaTime;
            currentVelocity.y = Mathf.Max(currentVelocity.y, terminalVelocity);
        }

        private void ApplyAirControl()
        {
            Vector2 averageInput = (context.Input.LeftStick + context.Input.RightStick) / 2f;
            float targetVelocityX = averageInput.x * moveSpeed * airControlMultiplier;

            float airAcceleration = 20f;
            float airDrag = 5f;

            if (Mathf.Abs(averageInput.x) < 0.05f)
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0f, airDrag * Time.fixedDeltaTime);
            }
            else
            {
                if (Mathf.Abs(currentVelocity.x) > Mathf.Abs(targetVelocityX) &&
                    Mathf.Approximately(Mathf.Sign(currentVelocity.x), Mathf.Sign(targetVelocityX)))
                {
                    currentVelocity.x =
                        Mathf.MoveTowards(currentVelocity.x, targetVelocityX, airDrag * Time.fixedDeltaTime);
                }
                else
                {
                    currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocityX,
                        airAcceleration * Time.fixedDeltaTime);
                }
            }
        }
    }
}