using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin havadaki durumunu, momentumunu, yercekimi limitlerini ve yetenek gecislerini ScriptableObject verileriyle yoneten durum sinifi.
    /// </summary>
    public class AirborneState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;

        private Vector2 currentVelocity;
        private float grappleLockoutTimer;
        private float wallClimbLockoutTimer;
        private float coyoteTimer;
        private bool isJumping;

        public AirborneState(PlayerController context, Vector2 inheritedVelocity, bool isJumping = false, float grappleLockoutDuration = 0f, float wallClimbLockoutDuration = 0f, float coyoteDuration = 0f)
        {
            this.context = context;
            this.stats = context.Stats;
            
            currentVelocity = inheritedVelocity;
            this.isJumping = isJumping;
            
            grappleLockoutTimer = grappleLockoutDuration;
            wallClimbLockoutTimer = wallClimbLockoutDuration;
            coyoteTimer = coyoteDuration;
        }

        public void EnterState()
        {
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }

        public void UpdateState()
        {
            HandleArmRouting();

            if (coyoteTimer > 0f)
            {
                coyoteTimer -= Time.deltaTime;

                if (context.JumpBufferTimer > 0f)
                {
                    context.ConsumeJumpBuffer();
                    coyoteTimer = 0f;

                    currentVelocity.y = stats.JumpVelocity;
                    context.PlayerRigidbody.linearVelocity = currentVelocity;
                }
            }

            if (grappleLockoutTimer > 0f)
                grappleLockoutTimer -= Time.deltaTime;
            else
                CheckGrappleInputs();

            if (wallClimbLockoutTimer > 0f)
                wallClimbLockoutTimer -= Time.deltaTime;
            else
                CheckWallClimb();

            CheckDashTransition();
        }

        public void FixedUpdateState()
        {
            if (context.IsGrounded && currentVelocity.y <= 0f)
            {
                context.TransitionToState(new GroundedState(context));
                return;
            }

            ApplyGravity();
            ApplyAirControl();
            HandleCeilingCollision();

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
                    if (context.CanSlingshot)
                    {
                        context.TransitionToState(new SlingshotState(context));
                    }
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

        private void CheckDashTransition()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick));
            }
        }

        private void ApplyGravity()
        {
            float currentGravity = stats.Gravity;

            if (currentVelocity.y < 0f)
            {
                currentGravity *= stats.FallGravityMultiplier;
            }
            else if (isJumping && currentVelocity.y > 0f && !context.Input.IsJumpHeld)
            {
                currentGravity *= stats.JumpEndEarlyGravityMultiplier;
            }
            else if (Mathf.Abs(currentVelocity.y) < stats.ApexThreshold)
            {
                currentGravity *= stats.ApexHangGravityMultiplier;
            }

            currentVelocity.y -= currentGravity * Time.fixedDeltaTime;
            currentVelocity.y = Mathf.Max(currentVelocity.y, stats.TerminalVelocity);
        }

        private void ApplyAirControl()
        {
            Vector2 averageInput = (context.Input.LeftStick + context.Input.RightStick) / 2f;
            float targetVelocityX = averageInput.x * stats.MoveSpeed * stats.AirControlMultiplier;

            if (Mathf.Abs(currentVelocity.x) > Mathf.Abs(targetVelocityX) && Mathf.Approximately(Mathf.Sign(currentVelocity.x), Mathf.Sign(targetVelocityX)))
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocityX, stats.MomentumDecay * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(averageInput.x) < 0.05f)
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0f, stats.AirDrag * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocityX, stats.AirAcceleration * Time.fixedDeltaTime);
            }
        }

        private void HandleCeilingCollision()
        {
            if (currentVelocity.y > 0f && context.IsTouchingCeiling)
            {
                currentVelocity.y = 0f;
            }
        }

        private void CheckWallClimb()
        {
            if (context.CanWallClimb && context.Input.IsLeftBumperHeld && context.IsTouchingLeftWall)
            {
                context.TransitionToState(new WallClimbingState(context, -1));
                return;
            }

            if (context.CanWallClimb && context.Input.IsRightBumperHeld && context.IsTouchingRightWall)
            {
                context.TransitionToState(new WallClimbingState(context, 1));
            }
        }
    }
}