using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin belirli bir yöne doğru yüksek hızla fırlatıldığı, yerçekimsiz kısa süreli durum. Verilerini ScriptableObject üzerinden okur.
    /// </summary>
    public class DashState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly Vector2 dashDirection;
        
        private float dashTimer;

        public DashState(PlayerController context, Vector2 direction)
        {
            this.context = context;
            this.stats = context.Stats;
            
            this.dashDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;
        }

        public void EnterState()
        {
            context.UseDash();
            dashTimer = 0f;
            context.PlayerRigidbody.linearVelocity = dashDirection * stats.DashSpeed;
            context.NotifyImpact(dashDirection * stats.DashImpactMultiplier);
        }

        public void UpdateState()
        {
            dashTimer += Time.deltaTime;
            
            if (dashTimer >= stats.DashDuration)
            {
                Vector2 exitVelocity = context.PlayerRigidbody.linearVelocity * stats.DashEndMomentumMultiplier;
                context.TransitionToState(new AirborneState(context, exitVelocity));
            }
        }

        public void FixedUpdateState()
        {
            context.PlayerRigidbody.linearVelocity = dashDirection * stats.DashSpeed;
        }

        public void ExitState()
        {
        }
    }
}