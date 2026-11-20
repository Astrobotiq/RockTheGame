using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Duvar dayanıklılığı uyarısının yanıp sönme mantığı (saf, MonoBehaviour'suz). Tırmanma aktifken ve kalan süre
    /// uyarı eşiğinin altındayken faz ilerler; hız eşikten sıfıra doğru artar. Faz yarıdan küçükse flash gösterilir,
    /// yani uyarıya girilen ilk kare flash'tır.
    /// </summary>
    public sealed class StaminaFlashState
    {
        private bool _active;
        private float _phase;

        /// <summary>Kalan süreye göre yanıp sönme hızı (Hz): eşikte <paramref name="hzAtThreshold"/>, sıfırda <paramref name="hzAtZero"/>.</summary>
        public static float FlashHz(float remaining, float threshold, float hzAtThreshold, float hzAtZero)
        {
            if (threshold <= 0f) return hzAtThreshold;
            return Mathf.Lerp(hzAtZero, hzAtThreshold, Mathf.Clamp01(remaining / threshold));
        }

        /// <summary>Tırmanma başladı/bitti. Bitince faz sıfırlanır.</summary>
        public void SetActive(bool active)
        {
            _active = active;
            if (!active) _phase = 0f;
        }

        /// <summary>Etkinliği kapatır ve fazı sıfırlar.</summary>
        public void Stop() => SetActive(false);

        /// <summary>Bu karede flash gösterilsin mi.</summary>
        public bool Step(float dt, float remaining, float threshold, float hzAtThreshold, float hzAtZero)
        {
            if (!_active || threshold <= 0f || remaining <= 0f || remaining > threshold)
            {
                _phase = 0f;
                return false;
            }

            _phase += FlashHz(remaining, threshold, hzAtThreshold, hzAtZero) * dt;
            _phase -= Mathf.Floor(_phase);
            return _phase < 0.5f;
        }
    }
}
