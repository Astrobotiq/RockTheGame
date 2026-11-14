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
            
            _dashDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : context.FacingDirection;
        }

        public void EnterState()
        {
            _context.UseDash();
            _dashTimer = 0f;
            
            _context.Velocity = _dashDirection * _stats.DashSpeed;
            _context.NotifyImpact(_dashDirection * _stats.DashImpactMultiplier);

            if (_context.VibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.Vibrate(_context.VibrationSettings.Dash);
            }

            if (_context.Audio != null)
            {
                _context.Audio.PlayDash();
            }
        }

        public void UpdateState() { }

        public void FixedUpdateState()
        {
            // Sure fizik adiminda sayilir: dash her zaman ayni adim sayisi kadar (DashDuration / fixedDeltaTime) surer,
            // mesafe frame hizindan bagimsiz. Epsilon, 0.02 * 10 gibi birikimli float hatasinin fazladan adim eklemesini onler.
            if (_dashTimer + 1e-4f >= _stats.DashDuration)
            {
                ExitDashSequence();
                return;
            }

            _context.Velocity = _dashDirection * _stats.DashSpeed;
            _dashTimer += Time.fixedDeltaTime;
        }

        public void ExitState() { }

        private void ExitDashSequence()
        {
            Vector2 exitVelocity = _context.Velocity * _stats.DashEndMomentumMultiplier;
            _context.TransitionToState(new AirborneState(_context, exitVelocity));
        }
    }
}