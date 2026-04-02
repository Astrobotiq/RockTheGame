using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin iki farklı kanca noktasına aynı anda tutunduğu durumu yönetir. Hooke yasası (yay fiziği) kullanarak iki noktalı salınım ve asılı kalma mekaniğini işletir.
    /// </summary>
    public class DualSwingingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float gravity;
        private readonly float moveSpeedCache;

        private float leftRopeLength;
        private float rightRopeLength;
        private Vector2 currentVelocity;

        private readonly float springStiffness = 25f;
        private readonly float springDamping = 5f;
        private readonly float maxWallAngle = 45f;
        private readonly float collisionRadius = 0.4f;
        private readonly float swingForceMultiplier = 15f;

        public DualSwingingState(PlayerController context, float gravity, float moveSpeedCache)
        {
            this.context = context;
            this.gravity = gravity;
            this.moveSpeedCache = moveSpeedCache;
        }

        public void EnterState()
        {
            if (!context.LeftAnchor.HasValue || !context.RightAnchor.HasValue)
            {
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, context.PlayerRigidbody.linearVelocity));
                return;
            }

            leftRopeLength = Vector2.Distance(context.PlayerRigidbody.position, context.LeftAnchor.Value);
            rightRopeLength = Vector2.Distance(context.PlayerRigidbody.position, context.RightAnchor.Value);
            currentVelocity = context.PlayerRigidbody.linearVelocity;
            context.ResetDash();
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            ApplyDualSpringPhysics();
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            context.LeftArm.UpdateArmRotation((context.LeftAnchor.Value - context.PlayerRigidbody.position).normalized);
            context.RightArm.UpdateArmRotation((context.RightAnchor.Value - context.PlayerRigidbody.position).normalized);
        }

        private void CheckInputTransitions()
        {
            bool leftHeld = context.Input.IsLeftTriggerHeld;
            bool rightHeld = context.Input.IsRightTriggerHeld;

            if (!leftHeld && !rightHeld)
            {
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, currentVelocity));
                return;
            }

            if (!leftHeld)
            {
                context.LeftAnchor = null;
                context.TransitionToState(new SwingingState(context, ActiveArm.Right, gravity, 0f, moveSpeedCache));
                return;
            }

            if (!rightHeld)
            {
                context.RightAnchor = null;
                context.TransitionToState(new SwingingState(context, ActiveArm.Left, gravity, 0f, moveSpeedCache));
                return;
            }
        }

        private void ApplyDualSpringPhysics()
        {
            currentVelocity.y -= gravity * Time.fixedDeltaTime;

            float playerInputX = (context.Input.LeftStick.x + context.Input.RightStick.x) * 0.5f;
            currentVelocity.x += playerInputX * swingForceMultiplier * Time.fixedDeltaTime;

            currentVelocity += CalculateSpringForce(context.LeftAnchor.Value, leftRopeLength);
            currentVelocity += CalculateSpringForce(context.RightAnchor.Value, rightRopeLength);

            Vector2 playerPos = context.PlayerRigidbody.position;
            Vector2 nextPosition = playerPos + currentVelocity * Time.fixedDeltaTime;
            Vector2 movementDelta = nextPosition - playerPos;

            RaycastHit2D hit = Physics2D.CircleCast(playerPos, collisionRadius, movementDelta.normalized, movementDelta.magnitude, context.GroundLayerMask);

            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > maxWallAngle)
                {
                    context.LeftAnchor = null;
                    context.RightAnchor = null;
                    context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, currentVelocity));
                    return;
                }
                else
                {
                    context.LeftAnchor = null;
                    context.RightAnchor = null;
                    context.TransitionToState(new GroundedState(context, moveSpeedCache, 15f));
                    return;
                }
            }

            context.PlayerRigidbody.position = nextPosition;
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }

        private Vector2 CalculateSpringForce(Vector2 anchor, float ropeLength)
        {
            Vector2 direction = anchor - context.PlayerRigidbody.position;
            float distance = direction.magnitude;

            if (distance > ropeLength)
            {
                Vector2 normalizedDir = direction / distance;
                float displacement = distance - ropeLength;
                
                float springForce = displacement * springStiffness;
                float relativeVelocity = Vector2.Dot(currentVelocity, normalizedDir);
                float dampingForce = relativeVelocity * springDamping;

                return normalizedDir * ((springForce - dampingForce) * Time.fixedDeltaTime);
            }

            return Vector2.zero;
        }
    }
}