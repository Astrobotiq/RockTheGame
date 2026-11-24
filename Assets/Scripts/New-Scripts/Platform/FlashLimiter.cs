using System.Collections.Generic;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Flaş sınırlayıcı (saf mantık), WCAG 2.3.1: herhangi bir 1 saniyelik pencerede en fazla <c>maxPerWindow</c> tam
    /// şiddetli flaş. Fazlası <c>reducedIntensity</c> ile (yumuşak, flaş sayılmayacak kadar düşük) oynatılır.
    /// Pad'ler arası ortaktır: art arda pad'lere vuran oyuncunun toplam flaşı sınırlanır. Saat geriye giderse
    /// (yeni Play oturumu) hafıza sıfırlanır.
    /// </summary>
    public sealed class FlashLimiter
    {
        private readonly int maxPerWindow;
        private readonly float window;
        private readonly float reducedIntensity;
        private readonly List<float> fullFlashTimes = new List<float>();

        public FlashLimiter(int maxPerWindow, float window, float reducedIntensity)
        {
            this.maxPerWindow = maxPerWindow < 1 ? 1 : maxPerWindow;
            this.window = window;
            this.reducedIntensity = reducedIntensity;
        }

        /// <summary>Yeni bir patlama için şiddet çarpanı: 1 (tam) ya da <c>reducedIntensity</c>.</summary>
        public float Acquire(float now)
        {
            if (fullFlashTimes.Count > 0 && now < fullFlashTimes[fullFlashTimes.Count - 1])
                fullFlashTimes.Clear();

            fullFlashTimes.RemoveAll(t => now - t >= window);
            if (fullFlashTimes.Count >= maxPerWindow) return reducedIntensity;

            fullFlashTimes.Add(now);
            return 1f;
        }
    }
}
