using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Havada duvara carpinca yatay hizi sifirlar, duvar kisa sure icinde biterse (kose) hizi geri verir
    /// (docs/systems/movement-core.md "Havada duvar carpmasi", Celeste'in <c>wallSpeedRetentionTimer</c>'i).
    /// Unity'ye dokunmaz; <c>AirborneState</c> her fizik adiminda <see cref="Step"/> cagirir.
    /// </summary>
    public struct WallSpeedRetention
    {
        private float _retainedX;
        private float _timer;

        public bool IsRetaining => _timer > 0f;

        public void Reset()
        {
            _retainedX = 0f;
            _timer = 0f;
        }

        /// <summary>
        /// <paramref name="velocityX"/> oyuncunun kendi yatay hizi (tasiyan yuzeyin hizi haric).
        /// Donen deger yeni yatay hizdir.
        /// </summary>
        public float Step(float velocityX, bool touchingLeft, bool touchingRight, float retentionTime, float fixedDeltaTime)
        {
            if (retentionTime <= 0f)
            {
                // Geri verme kapali: yalnizca duvara carpan hiz sifirlanir.
                _timer = 0f;
                bool hit = (velocityX > 0f && touchingRight) || (velocityX < 0f && touchingLeft);
                return hit ? 0f : velocityX;
            }

            if (_timer > 0f)
            {
                int retainedSign = _retainedX > 0f ? 1 : -1;
                int currentSign = velocityX > 0f ? 1 : (velocityX < 0f ? -1 : 0);

                if (currentSign == -retainedSign)
                {
                    // Oyuncu ters yone basti: geri verilecek bir sey kalmadi.
                    _timer = 0f;
                }
                else if (!(retainedSign > 0 ? touchingRight : touchingLeft))
                {
                    // Duvar retention penceresi icinde bitti (kose): hiz geri.
                    velocityX = _retainedX;
                    _timer = 0f;
                    return velocityX;
                }
                else
                {
                    _timer -= fixedDeltaTime;
                }
            }

            bool blocked = (velocityX > 0f && touchingRight) || (velocityX < 0f && touchingLeft);
            if (!blocked) return velocityX;

            // Pencere zaten calisiyorsa ilk carpma hizi korunur, yeni (kucuk) hiz ustune yazmaz.
            if (_timer <= 0f)
            {
                _retainedX = velocityX;
                _timer = retentionTime;
            }

            return 0f;
        }
    }
}
