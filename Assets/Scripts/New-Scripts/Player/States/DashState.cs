using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin belirli bir mesafeyi, hedeflenen sürede kat etmesini sağlayan, yerçekimsiz ve kinematik atılma durumu.
    /// Hız hesaplaması tasarımcının belirlediği mesafe ve süre üzerinden dinamik olarak okur.
    /// </summary>
    public class DashState : IPlayerState
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;
        private readonly Vector2 _dashDirection;
        
        private float _dashTimer;

        public DashState(PlayerController context, Vector2 direction)
        {
            _context = context;
            _stats = context.Stats;
            
            _dashDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;
        }

        public void EnterState()
        {
            _context.UseDash();
            _dashTimer = 0f;
            
            _context.Velocity = _dashDirection * _stats.DashSpeed;
            _context.NotifyImpact(_dashDirection * _stats.DashImpactMultiplier);
        }

        public void UpdateState()
        {
            _dashTimer += Time.deltaTime;
            
            if (_dashTimer >= _stats.DashDuration)
            {
                ExitDashSequence();
            }
        }

        public void FixedUpdateState()
        {
            _context.Velocity = _dashDirection * _stats.DashSpeed;
        }

        public void ExitState() { }

        private void ExitDashSequence()
        {
            Vector2 exitVelocity = _context.Velocity * _stats.DashEndMomentumMultiplier;
            _context.TransitionToState(new AirborneState(_context, exitVelocity));
        }
    }
}