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
            if (!context.LeftAnchor.HasValue || !context.RightAnchor.HasValue)
            {
                context.TransitionToState(new NormalState(context, context.Velocity, AirEntry.Fall()));
                return;
            }

            context.UseSlingshot();

            midpoint = (context.LeftAnchor.Value + context.RightAnchor.Value) / 2f;
            cachedLaunchDirection = (midpoint - context.PlayerRigidbody.position).normalized;
            
            currentPhase = SlingshotPhase.Anticipation;
            stateTimer = 0f;

            if (context.Audio != null)
            {
                context.Audio.PlaySlingshotAnticipation();
            }
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            stateTimer += Time.fixedDeltaTime;
            if (context.LeftAnchor.HasValue && context.RightAnchor.HasValue)
                midpoint = (context.LeftAnchor.Value + context.RightAnchor.Value) / 2f;

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
                Exit();
            }
        }

        private void ApplyAnticipationPhysics()
        {
            context.Velocity = -cachedLaunchDirection * stats.SlingshotAnticipationSpeed + CarryVelocity();

            if (stateTimer >= stats.SlingshotAnticipationDuration)
            {
                currentPhase = SlingshotPhase.Launch;
                stateTimer = 0f;
                
                context.NotifySlingshotLaunch();
                if (context.Audio != null)
                {
                    context.Audio.PlaySlingshotLaunch();
                }
                
                initialLaunchVelocity = Vector2.zero;
                context.Velocity = initialLaunchVelocity + CarryVelocity();

                context.NotifyImpact(cachedLaunchDirection);
            }
        }

        private void ApplyLaunchPhysics()
        {
            float t = Mathf.Clamp01(stateTimer / stats.SlingshotLaunchDuration);
            
            float easeOutT = 1f - ((1f - t) * (1f - t));

            Vector2 targetVelocity = cachedLaunchDirection * stats.MaxSlingshotSpeed;
            context.Velocity = Vector2.Lerp(initialLaunchVelocity, targetVelocity, easeOutT) + CarryVelocity();

            Vector2 currentDirectionToMidpoint = midpoint - context.PlayerRigidbody.position;
            bool hasPassedMidpoint = Vector2.Dot(cachedLaunchDirection, currentDirectionToMidpoint) < 0f;

            if (t >= 1f || hasPassedMidpoint)
            {
                Exit();
            }
        }

        /// <summary>Hareketli node: iki anchor'ın ortalama kayması hız olarak (Move çarpışmayla uygular).</summary>
        private Vector2 CarryVelocity() =>
            (context.AnchorDelta(ActiveArm.Left) + context.AnchorDelta(ActiveArm.Right)) * 0.5f / Time.fixedDeltaTime;

        /// <summary>Fırlatma sonu ya da erken bırakma: node'a göre hız (taşıma çıkarılır, çift sayılmasın) + taşınan node hızı.</summary>
        private void Exit()
        {
            Vector2 relativeVelocity = context.Velocity - CarryVelocity();
            Vector2 nodeVelocity = (context.AnchorVelocity(ActiveArm.Left) + context.AnchorVelocity(ActiveArm.Right))
                                   * (0.5f * stats.MovingNodeLaunchMultiplier);
            context.LeftAnchor = null;
            context.RightAnchor = null;
            context.TransitionToState(new NormalState(context, relativeVelocity,
                AirEntry.SlingshotLaunch(stats.SlingshotGrappleLockout, nodeVelocity)));
        }
    }
}