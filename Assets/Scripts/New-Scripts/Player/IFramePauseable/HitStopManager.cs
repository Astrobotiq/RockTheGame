using UnityEngine;

namespace New_Scripts.Player.IFramePauseable
{
    /// <summary>
    /// Frame Pause (Hit Stop) durumunu yoneten, yalnizca olaylari dinleyerek calisan bagimsiz yonetici sinif.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        private bool _isHitStopActive;
        private float _hitStopTimer;

        private void OnEnable()
        {
            HitStopEvents.RequestHitStop += OnTriggerRequested;
        }

        private void OnDisable()
        {
            HitStopEvents.RequestHitStop -= OnTriggerRequested;
        }

        private void OnTriggerRequested(float duration)
        {
            _hitStopTimer = duration;

            if (!_isHitStopActive)
            {
                _isHitStopActive = true;
                HitStopEvents.HitStopStarted?.Invoke();
            }
        }

        private void Update()
        {
            if (!_isHitStopActive) return;

            _hitStopTimer -= Time.unscaledDeltaTime;

            if (_hitStopTimer <= 0f)
            {
                _isHitStopActive = false;
                HitStopEvents.HitStopEnded?.Invoke();
            }
        }
    }
}