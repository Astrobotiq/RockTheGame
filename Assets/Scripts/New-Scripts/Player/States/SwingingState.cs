using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin tek bir kanca ile salindigi durumu yonetir. Verileri ScriptableObject uzerinden okur.
    /// Tegetsel kuvvet ile sarkaç fizigini uygular ve fiziksel supurme (Sweep Test) ile clipping hatalarini engeller.
    /// </summary>
    public class SwingingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly ActiveArm swingingArm;
        
        private Vector2 anchorPoint;
        private float ropeLength;
        private Vector2 currentVelocity;

        public SwingingState(PlayerController context, ActiveArm swingingArm)
        {
            this.context = context;
            this.stats = context.Stats;
            this.swingingArm = swingingArm;
        }

        public void EnterState()
        {
            if (swingingArm == ActiveArm.Left && context.LeftAnchor.HasValue)
            {
                anchorPoint = context.LeftAnchor.Value;
            }
            else if (swingingArm == ActiveArm.Right && context.RightAnchor.HasValue)
            {
                anchorPoint = context.RightAnchor.Value;
            }
            else
            {
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity));
                return;
            }

            ropeLength = Vector2.Distance(context.PlayerRigidbody.position, anchorPoint);
            currentVelocity = context.PlayerRigidbody.linearVelocity;
            
            if (currentVelocity.y <= 0f && stats.SwingInitialBoost > 0f)
            {
                currentVelocity.y += stats.SwingInitialBoost;
            }
            
            context.ResetDash();
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            ApplyArcadePendulum();
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            context.LeftArm.UpdateArmRotation(context.Input.LeftStick);
            context.RightArm.UpdateArmRotation(context.Input.RightStick);
            
            if (swingingArm == ActiveArm.Left) context.LeftArm.UpdateArmRotation((anchorPoint - context.PlayerRigidbody.position).normalized);
            if (swingingArm == ActiveArm.Right) context.RightArm.UpdateArmRotation((anchorPoint - context.PlayerRigidbody.position).normalized);
        }

        private void CheckInputTransitions()
        {
            bool activeTriggerHeld = swingingArm == ActiveArm.Left ? context.Input.IsLeftTriggerHeld : context.Input.IsRightTriggerHeld;
            
            if (!activeTriggerHeld)
            {
                if (swingingArm == ActiveArm.Left) context.LeftAnchor = null;
                if (swingingArm == ActiveArm.Right) context.RightAnchor = null;
                
                context.TransitionToState(new AirborneState(context, currentVelocity));
                return;
            }
            
            bool oppositeTriggerHeld = swingingArm == ActiveArm.Left ? context.Input.IsRightTriggerHeld : context.Input.IsLeftTriggerHeld;
            if (oppositeTriggerHeld)
            {
                Vector2 aimStick = swingingArm == ActiveArm.Left ? context.Input.RightStick : context.Input.LeftStick;
                if (context.TryCastGrapple(aimStick, out Vector2 hitPoint))
                {
                    if (swingingArm == ActiveArm.Left) context.RightAnchor = hitPoint;
                    else context.LeftAnchor = hitPoint;

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
            }
        }

        private void BreakGrapple()
        {
            context.LeftAnchor = null;
            context.RightAnchor = null;
            context.TransitionToState(new AirborneState(context, currentVelocity));
        }

        private void ApplyArcadePendulum()
        {
            float playerInputX = swingingArm == ActiveArm.Left ? context.Input.LeftStick.x : context.Input.RightStick.x;
            
            currentVelocity.y -= stats.SwingGravity * Time.fixedDeltaTime;
            
            Vector2 playerPos = context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (anchorPoint - playerPos).normalized;
            
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);
            
            float speedAlongTangent = Vector2.Dot(currentVelocity, tangent);
            float inputForce = playerInputX * stats.SwingForceMultiplier * Time.fixedDeltaTime;
            
            if (Mathf.Abs(speedAlongTangent + inputForce) < stats.MaxSwingSpeed)
            {
                speedAlongTangent += inputForce;
            }
            
            currentVelocity = tangent * speedAlongTangent;
            
            Vector2 nextPosition = anchorPoint - directionToAnchor * ropeLength;
            Vector2 movementDelta = nextPosition - playerPos;

            RaycastHit2D hit = Physics2D.CircleCast(playerPos, stats.SwingCollisionRadius, movementDelta.normalized, movementDelta.magnitude, context.GroundLayerMask);
            
            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > stats.MaxWallAngle)
                {
                    BreakGrapple();
                    return;
                }
                
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new GroundedState(context));
                return;
            }

            context.PlayerRigidbody.position = nextPosition;
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }
    }
}