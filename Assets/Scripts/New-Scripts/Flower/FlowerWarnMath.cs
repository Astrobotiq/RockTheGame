using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Kapanış uyarısı için saf hesap. Uyarı renksizdir (renk kanalı başka şeylere ayrılmış): iki şekil işareti verir.
    /// (1) Yapraklar sırayla katlanır, böylece kalan süre yaprak sayısı olarak okunur (<see cref="Fold"/>);
    /// (2) uyarı başlarken kısa bir ürperme darbesi vardır (<see cref="StartBurst"/>).
    /// </summary>
    public static class FlowerWarnMath
    {
        /// <summary>
        /// Yaprağın katlanma miktarı 0..1. Yapraklar indeks sırasıyla (çiçeğin çevresinde ardışık) katlanır: 0. yaprak uyarı
        /// başlar başlamaz, son yaprak uyarı biterken tamamlanır. <paramref name="overlap"/> komşu yaprakların katlanma
        /// pencerelerinin ne kadar üst üste bineceğidir (0 = ardışık ve ayrık, 1 = pencere iki yaprak aralığı).
        /// </summary>
        public static float Fold(float warn01, int index, int count, float overlap)
        {
            warn01 = Mathf.Clamp01(warn01);
            if (count <= 1) return Smooth(warn01);

            overlap = Mathf.Clamp01(overlap);
            float window = Mathf.Min(1f, (1f / count) * (1f + overlap));
            float start = Mathf.Clamp(index, 0, count - 1) * (1f - window) / (count - 1);
            return Smooth((warn01 - start) / window);
        }

        /// <summary>Uyarı başlangıç darbesinin gücü: <paramref name="age"/> = 0'da 1, <paramref name="duration"/> sonunda 0 (kare söner).</summary>
        public static float StartBurst(float age, float duration)
        {
            if (duration <= 0f || age < 0f || age >= duration) return 0f;
            float k = 1f - age / duration;
            return k * k;
        }

        /// <summary>
        /// "Nefes çek" (ters yönlü anticipation) ilerlemesi 0..1: uyarının son <paramref name="windUpTime"/> saniyesinde
        /// yumuşakça 0'dan 1'e çıkar, öncesinde 0. Uyarının bitimine kalan süre bilinmiyorsa (sonsuz) 0'dır.
        /// </summary>
        public static float WindUp(float secondsLeft, float windUpTime)
        {
            if (windUpTime <= 0f || secondsLeft >= windUpTime) return 0f;
            return Smooth(1f - Mathf.Max(0f, secondsLeft) / windUpTime);
        }

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
