using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin iki farkli kanca noktasina ayni anda tutundugu durumu yonetir. 
    /// Verilerini ScriptableObject uzerinden okur ve Hooke yasasi (yay fizigi) kullanarak sarkaç/esneme mekanigini isletir.
    /// </summary>
    public class DualSwingingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;

        private float leftRopeLength;
        private float rightRopeLength;
        private Vector2 currentVelocity;

        public DualSwingingState(PlayerController context)
        {
            this.context = context;
            this.stats = context.Stats;
        }

        public void EnterState()
        {
            if (!context.LeftAnchor.HasValue || !context.RightAnchor.HasValue)
            {
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity));
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
                context.TransitionToState(new AirborneState(context, currentVelocity));
                return;
            }

            if (!leftHeld)
            {
                context.LeftAnchor = null;
                context.TransitionToState(new SwingingState(context, ActiveArm.Right));
                return;
            }

            if (!rightHeld)
            {
                context.RightAnchor = null;
                context.TransitionToState(new SwingingState(context, ActiveArm.Left));
                return;
            }
        }

        private void ApplyDualSpringPhysics()
        {
            currentVelocity.y -= stats.SwingGravity * Time.fixedDeltaTime;

            float playerInputX = (context.Input.LeftStick.x + context.Input.RightStick.x) * 0.5f;
            currentVelocity.x += playerInputX * stats.DualSwingForceMultiplier * Time.fixedDeltaTime;

            currentVelocity += CalculateSpringForce(context.LeftAnchor.Value, leftRopeLength);
            currentVelocity += CalculateSpringForce(context.RightAnchor.Value, rightRopeLength);

            Vector2 playerPos = context.PlayerRigidbody.position;
            Vector2 nextPosition = playerPos + currentVelocity * Time.fixedDeltaTime;
            Vector2 movementDelta = nextPosition - playerPos;

            RaycastHit2D hit = Physics2D.CircleCast(playerPos, stats.DualSwingCollisionRadius, movementDelta.normalized, movementDelta.magnitude, context.GroundLayerMask);

            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > stats.MaxWallAngle)
                {
                    context.LeftAnchor = null;
                    context.RightAnchor = null;
                    context.TransitionToState(new AirborneState(context, currentVelocity));
                    return;
                }
                else
                {
                    context.LeftAnchor = null;
                    context.RightAnchor = null;
                    context.TransitionToState(new GroundedState(context));
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

            // Ip yalnizca gerildiginde (distance > ropeLength) kuvvet uygular. Gevsek oldugunda sarkan bir ip gibi davranir.
            if (distance > ropeLength)
            {
                Vector2 normalizedDir = direction / distance;
                float displacement = distance - ropeLength;
                
                float springForce = displacement * stats.DualSpringStiffness;
                float relativeVelocity = Vector2.Dot(currentVelocity, normalizedDir);
                float dampingForce = relativeVelocity * stats.DualSpringDamping;

                return normalizedDir * ((springForce - dampingForce) * Time.fixedDeltaTime);
            }

            return Vector2.zero;
        }
    }
}