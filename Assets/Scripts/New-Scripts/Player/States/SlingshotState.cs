using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Iki kancanin ayni node'a baglandigi durumu yonetir. Once geriye dogru cekilme (Anticipation), ardindan Ease-Out interpolasyonu ile patlayici bir sapan firlatmasi uygular.
    /// </summary>
    public class SlingshotState : IPlayerState
    {
        private enum SlingshotPhase { Anticipation, Launch }

        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;

        private Vector2 midpoint;
        private Vector2 initialLaunchVelocity;
        private Vector2 cachedLaunchDirection;

        private SlingshotPhase currentPhase;
        private float stateTimer;

        public SlingshotState(PlayerController context)
        {
            this.context = context;
            this.stats = context.Stats;
        }

        public void EnterState()
        {
            context.UseSlingshot();
            
            if (!context.LeftAnchor.HasValue || !context.RightAnchor.HasValue)
            {
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity));
                return;
            }

            midpoint = (context.LeftAnchor.Value + context.RightAnchor.Value) / 2f;
            cachedLaunchDirection = (midpoint - context.PlayerRigidbody.position).normalized;
            
            currentPhase = SlingshotPhase.Anticipation;
            stateTimer = 0f;
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            stateTimer += Time.fixedDeltaTime;

            if (currentPhase == SlingshotPhase.Anticipation)
            {
                ApplyAnticipationPhysics();
            }
            else
            {
                ApplyLaunchPhysics();
            }
        }

        public void ExitState()
        {
        }

        private void HandleArmRouting()
        {
            Vector2 facingDirection = (midpoint - context.PlayerRigidbody.position).normalized;
            context.LeftArm.UpdateArmRotation(facingDirection);
            context.RightArm.UpdateArmRotation(facingDirection);
        }

        private void CheckInputTransitions()
        {
            if (!context.Input.IsLeftTriggerHeld && !context.Input.IsRightTriggerHeld)
            {
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity, false ,stats.SlingshotGrappleLockout));
            }
        }

        private void ApplyAnticipationPhysics()
        {
            context.PlayerRigidbody.linearVelocity = -cachedLaunchDirection * stats.SlingshotAnticipationSpeed;

            if (stateTimer >= stats.SlingshotAnticipationDuration)
            {
                currentPhase = SlingshotPhase.Launch;
                stateTimer = 0f;
                
                initialLaunchVelocity = Vector2.zero;
                context.PlayerRigidbody.linearVelocity = initialLaunchVelocity;

                context.NotifyImpact(cachedLaunchDirection);
            }
        }

        private void ApplyLaunchPhysics()
        {
            float t = Mathf.Clamp01(stateTimer / stats.SlingshotLaunchDuration);
            
            float easeOutT = 1f - ((1f - t) * (1f - t));

            Vector2 targetVelocity = cachedLaunchDirection * stats.MaxSlingshotSpeed;
            context.PlayerRigidbody.linearVelocity = Vector2.Lerp(initialLaunchVelocity, targetVelocity, easeOutT);

            Vector2 currentDirectionToMidpoint = midpoint - context.PlayerRigidbody.position;
            bool hasPassedMidpoint = Vector2.Dot(cachedLaunchDirection, currentDirectionToMidpoint) < 0f;

            if (t >= 1f || hasPassedMidpoint)
            {
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new AirborneState(context, context.PlayerRigidbody.linearVelocity, false ,stats.SlingshotGrappleLockout));
            }
        }
    }
}