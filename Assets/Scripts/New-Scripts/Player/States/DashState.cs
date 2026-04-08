using New_Scripts.Player.States;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin belirli bir yöne doğru yüksek hızla fırlatıldığı, yerçekimsiz kısa süreli durum.
    /// </summary>
    public class DashState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly Vector2 dashDirection;
        private readonly float dashSpeed = 35f;
        private readonly float dashDuration = 0.15f;
        private readonly float moveSpeedCache;
        
        private float dashTimer;

        public DashState(PlayerController context, Vector2 direction, float moveSpeedCache)
        {
            this.context = context;
            this.moveSpeedCache = moveSpeedCache;
            
            this.dashDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;
        }

        public void EnterState()
        {
            context.UseDash();
            dashTimer = 0f;
            context.PlayerRigidbody.linearVelocity = dashDirection * dashSpeed;
            context.NotifyImpact(dashDirection * 2f);
        }

        public void UpdateState()
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                context.TransitionToState(new AirborneState(context, moveSpeedCache, 0.5f, 25f, -30f, context.PlayerRigidbody.linearVelocity));
            }
        }

        public void FixedUpdateState()
        {
            context.PlayerRigidbody.linearVelocity = dashDirection * dashSpeed;
        }

        public void ExitState()
        {
        }
    }
}