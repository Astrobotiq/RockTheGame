using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin ses/titreşim geri bildirimi için saf hesap. Uyarıda kalan süre yükselen tıklarla verilir (<see cref="TicksDue"/>);
    /// her tık bir öncekinden tizdir (<see cref="TickPitch"/>), böylece ses kapanışa doğru "sayar".
    /// </summary>
    public static class FlowerFeedbackMath
    {
        /// <summary>
        /// Uyarı ilerlemesi <paramref name="warn01"/> iken şimdiye kadar çalınmış olması gereken tık sayısı. İlk tık uyarı başlar
        /// başlamaz, kalanlar eşit aralıkla gelir; uyarı bitmeden hepsi çalınır. Uyarı yoksa (0) tık yok.
        /// </summary>
        public static int TicksDue(float warn01, int count)
        {
            if (count <= 0 || warn01 <= 0f) return 0;
            return Mathf.Min(count, Mathf.FloorToInt(Mathf.Clamp01(warn01) * count) + 1);
        }

        /// <summary>
        /// <paramref name="index"/>. tıkın perde çarpanı: ilk tık <paramref name="min"/>, son tık <paramref name="max"/>, arası doğrusal.
        /// Tek tıkta <paramref name="min"/>.
        /// </summary>
        public static float TickPitch(int index, int count, float min, float max)
        {
            if (count <= 1) return min;
            return Mathf.Lerp(min, max, Mathf.Clamp01(index / (float)(count - 1)));
        }
    }
}
