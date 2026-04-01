using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// İki kancanın aynı node'a bağlandığı durumu yönetir. Karaktere merkez noktasına doğru interpolasyonlu (Lerp) ivmeli bir sapan hareketi uygular.
    /// </summary>
    public class SlingshotState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly float gravity;
        private readonly float moveSpeedCache;
        
        private Vector2 midpoint;
        private Vector2 initialLaunchVelocity;
        
        private float launchTimer;
        private readonly float launchDuration = 0.5f; 
        private readonly float maxSlingshotSpeed = 40f;

        public SlingshotState(PlayerController context, float gravity, float moveSpeedCache)
        {
            this.context = context;
            this.gravity = gravity;
            this.moveSpeedCache = moveSpeedCache;
        }

        public void EnterState()
        {
            if (!context.LeftAnchor.HasValue || !context.RightAnchor.HasValue)
            {
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, 0f));
                return;
            }

            midpoint = (context.LeftAnchor.Value + context.RightAnchor.Value) / 2f;
            launchTimer = 0f;
            initialLaunchVelocity = context.PlayerRigidbody.linearVelocity;
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            ApplySlingshotPhysics();
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
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, context.PlayerRigidbody.linearVelocity.y));
            }
        }

        private void ApplySlingshotPhysics()
        {
            launchTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(launchTimer / launchDuration);
            
            Vector2 launchDirection = (midpoint - context.PlayerRigidbody.position).normalized;
            Vector2 targetVelocity = launchDirection * maxSlingshotSpeed;
            
            context.PlayerRigidbody.linearVelocity = Vector2.Lerp(initialLaunchVelocity, targetVelocity, t * t); 
            
            if (t >= 1f || Vector2.Distance(context.PlayerRigidbody.position, midpoint) < 0.5f)
            {
                context.LeftAnchor = null;
                context.RightAnchor = null;
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, gravity, -30f, context.PlayerRigidbody.linearVelocity.y));
            }
        }
    }
}