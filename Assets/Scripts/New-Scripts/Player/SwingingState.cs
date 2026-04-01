using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin tek bir kanca ile salındığı arcade durumunu yönetir. Hızı teğete izdüşürür ve yayın üzerinde tutar. Duvar çarpışmalarını denetler.
    /// </summary>
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
        private readonly float wallCheckRayDistance = 1f;

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
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, 0f));
                return;
            }

            ropeLength = Vector2.Distance(context.PlayerRigidbody.position, anchorPoint);
            currentVelocity = context.PlayerRigidbody.linearVelocity;
            currentVelocity.y += initialBoost;
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
            CheckWallCollisions();
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
            bool triggerHeld = swingingArm == ActiveArm.Left ? context.Input.IsLeftTriggerHeld : context.Input.IsRightTriggerHeld;
            
            if (!triggerHeld)
            {
                if (swingingArm == ActiveArm.Left) context.LeftAnchor = null;
                if (swingingArm == ActiveArm.Right) context.RightAnchor = null;
                
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, currentVelocity.y));
                return;
            }
            
            if (swingingArm == ActiveArm.Left) context.RightAnchor = null;
            if (swingingArm == ActiveArm.Right) context.LeftAnchor = null;
        }

        private void CheckWallCollisions()
        {
            if (currentVelocity.sqrMagnitude < 0.1f) return;
            
            RaycastHit2D hit = Physics2D.Raycast(context.PlayerRigidbody.position, currentVelocity.normalized, wallCheckRayDistance);
            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > maxWallAngle)
                {
                    BreakGrapple();
                }
            }
        }

        private void BreakGrapple()
        {
            context.LeftAnchor = null;
            context.RightAnchor = null;
            context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, 0f));
        }

        private void ApplyArcadePendulum()
        {
            currentVelocity.y -= gravity * Time.fixedDeltaTime;
            
            Vector2 playerPos = context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (anchorPoint - playerPos).normalized;
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);
            
            float speedAlongTangent = Vector2.Dot(currentVelocity, tangent);
            currentVelocity = tangent * speedAlongTangent;
            
            context.PlayerRigidbody.position = anchorPoint - directionToAnchor * ropeLength;
            context.PlayerRigidbody.linearVelocity = currentVelocity;
        }
    }
}