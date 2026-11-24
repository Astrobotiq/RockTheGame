using System.Collections.Generic;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Bounce pad notaları için zaman kapısı (saf mantık): ardışık nota arasında en az <c>minInterval</c>,
    /// aynı anda en fazla <c>maxVoices</c> nota sesi, ve "zincir" tespiti (son notadan <c>chainWindow</c> içinde).
    /// Dolu olunca yeni nota <b>atlanır</b> (en eski ses kesilmez; AudioManager'da fade'li kesme yok, tık riski var).
    /// Zaman dışarıdan verilir; saat geriye giderse (yeni Play oturumu) hafıza sıfırlanır.
    /// </summary>
    public sealed class NoteGate
    {
        private readonly float chainWindow;
        private readonly float minInterval;
        private readonly int maxVoices;
        private readonly float voiceDuration;
        private readonly List<float> voiceStarts = new List<float>();
        private float lastAccepted = float.NegativeInfinity;

        public NoteGate(float chainWindow, float minInterval, int maxVoices, float voiceDuration)
        {
            this.chainWindow = chainWindow;
            this.minInterval = minInterval;
            this.maxVoices = maxVoices < 1 ? 1 : maxVoices;
            this.voiceDuration = voiceDuration;
        }

        /// <summary>
        /// Nota çalınabilir mi? Evetse kaydeder ve <paramref name="chained"/> ile zincirde olup olmadığını döndürür.
        /// </summary>
        public bool TryAcquire(float now, out bool chained)
        {
            chained = false;
            if (now < lastAccepted)
            {
                voiceStarts.Clear();
                lastAccepted = float.NegativeInfinity;
            }

            if (now - lastAccepted < minInterval) return false;

            voiceStarts.RemoveAll(start => now - start >= voiceDuration);
            if (voiceStarts.Count >= maxVoices) return false;

            chained = now - lastAccepted <= chainWindow;
            voiceStarts.Add(now);
            lastAccepted = now;
            return true;
        }
    }
}
