using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Kapanış uyarı halkasının saf hesabı. Uyarı renge güvenmez (turuncu, turuncu zeminde kayboluyor ve renk körlüğünde asit
    /// yeşiline yaklaşıyor): çiçeğin etrafında kesikli bir halka yanıp söner. Yanıp sönme hızı saniyede 3 flaşı geçmez
    /// (erişilebilirlik sınırı); halka açılış halka dalgasıyla aynı renderer'ı kullandığı için dalga varken çizilmez.
    /// </summary>
    public static class FlowerWarnMath
    {
        /// <summary>En fazla yanıp sönme hızı (Hz); üstü bu değere kırpılır.</summary>
        public const float MaxBlinkHz = 3f;

        /// <summary>Uyarı halkası bu uyarı değerinin üstünde görünür.</summary>
        public const float VisibleAbove = 0.001f;

        /// <summary>
        /// Saat (saniye) ve hıza göre halka yanık mı? Saat 0'da yanık başlar (uyarı başlar başlamaz görünür), her periyodun ilk
        /// yarısı yanık, ikinci yarısı sönüktür. hz &lt;= 0: sürekli yanık.
        /// </summary>
        public static bool BlinkOn(float clock, float hz)
        {
            if (hz <= 0f) return true;
            hz = Mathf.Min(hz, MaxBlinkHz);
            return Mathf.Repeat(clock * hz, 1f) < 0.5f;
        }

        /// <summary>Uyarı halkası çizilsin mi? Uyarı varken ve açılış halka dalgası yokken.</summary>
        public static bool RingVisible(float warn01, bool openWaveOn) => warn01 > VisibleAbove && !openWaveOn;

        /// <summary>Halka yarıçapının quad yarıçapına oranı (shader'daki <c>_Ring</c>), 0..1.</summary>
        public static float RingRadius01(float ringRadius, float quadRadius) =>
            quadRadius <= 0f ? 0f : Mathf.Clamp01(ringRadius / quadRadius);
    }
}
