/// <summary>
/// Karakterin tek bir kanca ile salındığı durumu yönetir. Teğetsel kuvvet ile sarkaç fiziğini uygular.
/// Fiziksel süpürme (Sweep Test) ile yerin içine girme (Clipping) hatalarını engeller.
/// </summary>
using UnityEngine;

namespace New_Scripts.Player
{
    public class SwingingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float gravity;
        private readonly float initialBoost;
        private readonly float moveSpeedCache;
        private readonly ActiveArm swingingArm;
        
        private Vector2 anchorPoint;
        private float ropeLength;
        private Vector2 currentVelocity;

        private readonly float maxWallAngle = 45f;
        private readonly float swingForceMultiplier = 10f; 
        private readonly float maxSwingSpeed = 17f;
        private readonly float collisionRadius = 0.5f; 

        public SwingingState(PlayerController context, ActiveArm swingingArm, float gravity, float initialBoost, float moveSpeedCache)
        {
            this.context = context;
            this.gravity = gravity;
            this.initialBoost = initialBoost;
            this.moveSpeedCache = moveSpeedCache;
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
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, context.PlayerRigidbody.linearVelocity));
                return;
            }

            ropeLength = Vector2.Distance(context.PlayerRigidbody.position, anchorPoint);
            currentVelocity = context.PlayerRigidbody.linearVelocity;
            
            if (currentVelocity.y <= 0f && initialBoost > 0f)
            {
                currentVelocity.y += initialBoost;
            }
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
                
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, currentVelocity));
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
                        context.TransitionToState(new SlingshotState(context, gravity, moveSpeedCache));
                    }
                    else
                    {
                        if (swingingArm == ActiveArm.Left) context.LeftAnchor = null;
                        else context.RightAnchor = null;
                        
                        ActiveArm newArm = swingingArm == ActiveArm.Left ? ActiveArm.Right : ActiveArm.Left;
                        context.TransitionToState(new SwingingState(context, newArm, gravity, 0f, moveSpeedCache));
                    }
                }
            }
        }

        private void BreakGrapple()
        {
            context.LeftAnchor = null;
            context.RightAnchor = null;
            context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, currentVelocity));
        }

        private void ApplyArcadePendulum()
        {
            float playerInputX = swingingArm == ActiveArm.Left ? context.Input.LeftStick.x : context.Input.RightStick.x;
            
            currentVelocity.y -= gravity * Time.fixedDeltaTime;
            
            Vector2 playerPos = context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (anchorPoint - playerPos).normalized;
            
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);
            
            float speedAlongTangent = Vector2.Dot(currentVelocity, tangent);
            float inputForce = playerInputX * swingForceMultiplier * Time.fixedDeltaTime;
            
            if (Mathf.Abs(speedAlongTangent + inputForce) < maxSwingSpeed)
            {
                speedAlongTangent += inputForce;
            }
            
            currentVelocity = tangent * speedAlongTangent;
            
            Vector2 nextPosition = anchorPoint - directionToAnchor * ropeLength;
            Vector2 movementDelta = nextPosition - playerPos;

            RaycastHit2D hit = Physics2D.CircleCast(playerPos, collisionRadius, movementDelta.normalized, movementDelta.magnitude, context.GroundLayerMask);
            
            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > maxWallAngle)
                {
                    BreakGrapple();
                    return;
                }
                
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new GroundedState(context, moveSpeedCache, 15f));
                return;
            }

            context.PlayerRigidbody.position = nextPosition;
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }
    }
}