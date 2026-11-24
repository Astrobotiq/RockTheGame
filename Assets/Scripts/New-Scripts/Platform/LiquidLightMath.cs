using System;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Sıvı ışık ped görselinin saf matematiği (Unity'ye bağımlı değil, EditMode testlenebilir):
    /// vuruş zarfı, akış zamanı ve palet kaydırma açısı. Shader tarafı <c>LiquidLightPad.shader</c>.
    /// </summary>
    public static class LiquidLightMath
    {
        /// <summary>Paletin baskın tonu (derece): magenta tarafı. Nota rengine kaydırma bu tona göre hesaplanır.</summary>
        public const float BasePaletteHue = 330f;

        /// <summary>
        /// Vuruş zarfı (0..1): <paramref name="age"/> saniyede doğrusal yükselir (<paramref name="attack"/>),
        /// sonra üstel söner (<paramref name="decay"/> zaman sabiti). Negatif yaş (hiç vurulmamış) 0 verir.
        /// </summary>
        public static float Envelope(float age, float attack, float decay)
        {
            if (age < 0f) return 0f;
            if (age < attack) return attack <= 0f ? 1f : age / attack;
            return decay <= 0f ? 0f : (float)Math.Exp(-(age - attack) / decay);
        }

        /// <summary>Kaleydoskop simetri sayısı enstrümandan: 0..3 → 3/4/5/6 (4'ten fazla enstrümanda başa sarar).</summary>
        public static int BurstSymmetry(int instrument)
        {
            int i = instrument % 4;
            if (i < 0) i += 4;
            return 3 + i;
        }

        /// <summary>Patlamanın görsel zarfı: vuruş zarfı, patlama yaşıyla (0.75× yavaşlatılmış zaman).</summary>
        public static float BurstEnvelope(float age, float attack, float decay)
            => Envelope(age * 0.75f, attack, decay);

        /// <summary>Akış zamanı artışı: boşta <paramref name="idleSpeed"/>, vuruşta zarfla orantılı ek hız.</summary>
        public static float AdvanceFlow(float flow, float deltaTime, float idleSpeed, float hitBoost, float envelope)
            => flow + deltaTime * (idleSpeed + hitBoost * envelope);

        /// <summary>Nota renginin tonunu (0..360) paletin baskın tonuna göre kaydırma açısı (-180..180 derece).</summary>
        public static float HueShiftDegrees(float noteHue01)
            => WrapDegrees(noteHue01 * 360f - BasePaletteHue);

        /// <summary>Açıyı -180..180 aralığına sarar.</summary>
        public static float WrapDegrees(float degrees)
        {
            float d = (degrees + 180f) % 360f;
            if (d < 0f) d += 360f;
            return d - 180f;
        }

        /// <summary>
        /// Mevcut kaydırmayı hedefe en kısa yoldan, üstel yumuşatmayla (<paramref name="speed"/> 1/sn) yaklaştırır.
        /// </summary>
        public static float SmoothHue(float current, float target, float deltaTime, float speed)
        {
            float delta = WrapDegrees(target - current);
            float k = Math.Min(1f, Math.Max(0f, deltaTime * speed));
            return WrapDegrees(current + delta * k);
        }
    }
}
