using UnityEngine;

namespace New_Scripts.Player.Nodes.Spiral
{
    /// <summary>
    /// Ters dönen spiralin sayacı: anchor etrafında yalnızca spin yönündeki dönüşü biriktirir (toplam 0'ın altına inmez).
    /// Eşiğe ulaşınca sıfırlanır ve true döner.
    /// </summary>
    public class SpiralReverseCounter
    {
        private const float AngleEpsilon = 0.01f;
        private readonly float _degrees;
        private float _accumulated;
        private Vector2 _previousDirection;
        private bool _isInitialized;

        public SpiralReverseCounter(float degrees)
        {
            _degrees = degrees;
        }

        /// <summary>0..1, görsel ilerleme halkası için.</summary>
        public float Progress => _degrees > 0f ? Mathf.Clamp01(_accumulated / _degrees) : 0f;

        /// <summary>Her motor adımında çağrılır. İlk çağrı yalnızca yönü kaydeder.</summary>
        public bool Tick(Vector2 playerPos, Vector2 anchorPos, int spinSign)
        {
            Vector2 direction = (playerPos - anchorPos).normalized;
            if (!_isInitialized)
            {
                _previousDirection = direction;
                _isInitialized = true;
                return false;
            }

            _accumulated = Mathf.Max(0f, _accumulated + Vector2.SignedAngle(_previousDirection, direction) * spinSign);
            _previousDirection = direction;

            // SignedAngle birikiminde float hatası: 72 × 5° = 359.9999°. Küçük tolerans.
            if (_accumulated < _degrees - AngleEpsilon) return false;
            _accumulated = 0f;
            return true;
        }

        public void Reset()
        {
            _accumulated = 0f;
            _isInitialized = false;
        }
    }
}
