using UnityEngine;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// "Denge" değerinin (0 = Bauhaus, 1 = brutalist) yüzeye etkisi. Formüller <c>docs/ui-research/mashup-ui.html</c>'deki
    /// mockup'tan alındı (<c>bwOf</c>, <c>shOf</c>, <c>mass</c>). Birim sanat pikselidir (480x270 ızgara); ekrana
    /// çevirme (4x) çağıranda. Saf mantık: EditMode'da test edilir.
    /// </summary>
    public static class UiBalance
    {
        /// <summary>Bunun altında çerçeve çizilmez (saf Bauhaus).</summary>
        public const float NoBorderBelow = 0.06f;
        /// <summary>Bunun üstünde beton gürültü dokusu başlar.</summary>
        public const float TextureAbove = 0.12f;
        /// <summary>Bunun altında tehlike çizgileri çizilmez.</summary>
        public const float HazardStripesFrom = 0.3f;
        /// <summary>Bunun üstünde kalıp (şalyo) çizgileri başlar.</summary>
        public const float FormworkAbove = 0.4f;
        /// <summary>Bunun üstünde kalıp bağlantı delikleri (tie-hole) başlar.</summary>
        public const float TieHolesAbove = 0.6f;

        public static float Clamp(float v) => Mathf.Clamp01(v);

        /// <summary>Çerçeve kalınlığı: 0'a yakın dengede yok, yükseldikçe taban kalınlığın %35'inden %100'üne.</summary>
        public static float BorderWidth(float baseWidth, float v)
        {
            v = Clamp(v);
            return v < NoBorderBelow ? 0f : baseWidth * (0.35f + 0.65f * v);
        }

        /// <summary>Sert gölge ofseti (tam sayı piksel): denge ile doğrusal, 1,6 çarpanıyla.</summary>
        public static int ShadowOffset(float baseOffset, float v) => Mathf.RoundToInt(baseOffset * Clamp(v) * 1.6f);

        /// <summary>Ekstrüzyon derinliği (yazı/küp): denge ile doğrusal, 1,5 çarpanıyla.</summary>
        public static int ExtrusionDepth(float baseDepth, float v) => Mathf.RoundToInt(baseDepth * Clamp(v) * 1.5f);

        /// <summary>
        /// Beton yüzeyin kâğıttan betona geçişi: 0 = düz kâğıt tonu, 1 = tam beton. 0,625 ve üstünde tam beton.
        /// </summary>
        public static float ConcreteBlend(float v) => Mathf.Min(1f, Clamp(v) * 1.6f);

        public static bool HasTexture(float v) => Clamp(v) > TextureAbove;
        public static bool HasHazardStripes(float v) => Clamp(v) >= HazardStripesFrom;
        public static bool HasFormworkLines(float v) => Clamp(v) > FormworkAbove;
        public static bool HasTieHoles(float v) => Clamp(v) > TieHolesAbove;
    }
}
