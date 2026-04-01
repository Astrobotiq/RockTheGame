using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin havada olduğu durumu yönetir. Özel yerçekimi, terminal hız ve havada kontrol mantığını işletir.
    /// </summary>
    public class AirborneState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float moveSpeed;
        private readonly float airControlMultiplier;
        private readonly float gravity;
        private readonly float terminalVelocity;
    
        private Vector2 currentVelocity;

        public AirborneState(PlayerController context, float moveSpeed, float airControlMultiplier, float gravity, float terminalVelocity, float initialVelocityY)
        {
            this.context = context;
            this.moveSpeed = moveSpeed;
            this.airControlMultiplier = airControlMultiplier;
            this.gravity = gravity;
            this.terminalVelocity = terminalVelocity;
        
            currentVelocity = context.PlayerRigidbody.linearVelocity;
            currentVelocity.y = initialVelocityY != 0f ? initialVelocityY : currentVelocity.y;
        }

        public void EnterState()
        {
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckGrappleInputs();
            CheckGroundedTransition();
        }

        public void FixedUpdateState()
        {
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
                if (context.TryCastGrapple(context.Input.LeftStick, out RaycastHit2D hit))
                {
                    context.LeftAnchor = hit.point;
                    EvaluateTransition();
                    return;
                }
            }
            
            if (context.Input.IsRightTriggerHeld && !context.RightAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.RightStick, out RaycastHit2D hit))
                {
                    context.RightAnchor = hit.point;
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

        private void CheckGroundedTransition()
        {
            if (context.IsGrounded && currentVelocity.y <= 0f)
            {
                Debug.Log("Transitioning to GroundedState from AirborneState");
                context.TransitionToState(new GroundedState(context, moveSpeed, 15f));
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
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocityX, Time.fixedDeltaTime * 10f);
        }
    }
}