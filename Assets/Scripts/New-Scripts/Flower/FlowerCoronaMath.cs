using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// İplik tacının (pasiflora corona) açılma ve uyarı hesabı, saf. Taç, yapraklar açıldıktan sonra yelpaze gibi açılır:
    /// kapalıyken şeritler kısa, yüksekte değil, dik ve içe kıvrık (yaprakların altında gizli); açıkken uzun, yaprakların
    /// üstünde yüzer, yataya yakın. Uyarıda boyu ve yarıçapı kısalır (büzülme) ve biraz kalkar (gerilme).
    /// Her şey tek açılma değerinden (<c>open</c>) ve uyarıdan (<c>warn01</c>) türer; oyunda bunları <see cref="FlowerBloom"/> verir.
    /// </summary>
    public static class FlowerCoronaMath
    {
        /// <summary>Uyarıda şerit boyunun ve yarıçapın en fazla kısalma oranı.</summary>
        public const float WarnShrink = 0.3f;

        /// <summary>Uyarıda şeritlerin ek kalkışı (radyan), gerilme hissi.</summary>
        public const float WarnLift = 0.2f;

        /// <summary>Taç, <c>open</c> bu eşiği geçince açılmaya başlar (yapraklar önce açılır).</summary>
        public const float DefaultOpenStart = 0.35f;

        /// <summary>Overshoot'ta (open &gt; 1) her +1 için tacın ek açılması.</summary>
        public const float OvershootGain = 0.5f;

        /// <summary>Bir şeridin o anki duruşu. Çarpanlar halkanın kendi değerleriyle çarpılır.</summary>
        public readonly struct Pose
        {
            /// <summary>Yelpaze açıklığı: 0 kapalı, 1 tam açık, &gt; 1 overshoot.</summary>
            public readonly float Fan;
            /// <summary>Şerit boy çarpanı.</summary>
            public readonly float Length;
            /// <summary>Tabanın eksenden uzaklığı çarpanı.</summary>
            public readonly float Radius;
            /// <summary>Yaprakların üstündeki yükseklik çarpanı (0 = yaprak düzeyi).</summary>
            public readonly float Height;
            /// <summary>Yataydan kalkma açısı (radyan).</summary>
            public readonly float Pitch;
            /// <summary>Boy boyunca eğrilik (radyan / boy birimi).</summary>
            public readonly float Curl;

            public Pose(float fan, float length, float radius, float height, float pitch, float curl)
            {
                Fan = fan;
                Length = length;
                Radius = radius;
                Height = height;
                Pitch = pitch;
                Curl = curl;
            }

            /// <summary>Şeritler görünür mü? Tamamen kapalıyken çizilmez.</summary>
            public bool Visible => Fan > 0.001f;
        }

        /// <summary>Şerit azimutu (radyan): şeritler eşit aralıklı, <c>phase</c> adımın oranı kadar döndürür (0.5 = yarım adım).</summary>
        public static float StripAzimuth(int index, int strips, float phase)
        {
            strips = Mathf.Max(strips, 3);
            return (index + phase) * (2f * Mathf.PI / strips);
        }

        /// <summary>
        /// Yelpaze açıklığı. open &lt;= start iken 0 (anticipation dahil), open = 1'de tam 1, sonra overshoot kadar daha fazla.
        /// Aradaki geçiş yumuşak (smoothstep) ve monotondur.
        /// </summary>
        public static float Fan(float open, float start = DefaultOpenStart)
        {
            start = Mathf.Clamp(start, 0f, 0.95f);
            float t = Mathf.Clamp01((open - start) / (1f - start));
            float eased = t * t * (3f - 2f * t);
            return eased + Mathf.Max(0f, open - 1f) * OvershootGain;
        }

        /// <summary>
        /// Duruş: kapalıdan (<paramref name="closedPitch"/>, <paramref name="closedCurl"/>) açığa (<paramref name="openPitch"/>,
        /// <paramref name="openCurl"/>) geçiş + uyarı büzülmesi. Overshoot'ta (fan &gt; 1) açık değerin ötesine geçilmez,
        /// yalnızca boy bir miktar uzar.
        /// </summary>
        public static Pose Evaluate(float open, float warn01, float openPitch, float closedPitch, float openCurl, float closedCurl,
            float start = DefaultOpenStart)
        {
            return Evaluate(open, warn01, openPitch, closedPitch, openCurl, closedCurl, start, 0f, GripShape.Default);
        }

        /// <summary>
        /// Tutunma yumruğu (<paramref name="grip01"/> 0..1): şeritler tutan ele sarılır; kalkar, kıvrılır, tabana büzülür, kısalır
        /// (shader değişmez, aynı duruş kanalları). <paramref name="grip01"/> = 0 iken çıktı grip'siz Evaluate ile birebir aynıdır.
        /// Yaprakların kendisi kıpırdamaz: "açık" işareti yapraklarda kalır, yumruk "kapanıyor" okunmasın diye hafif tutulur.
        /// </summary>
        public static Pose Evaluate(float open, float warn01, float openPitch, float closedPitch, float openCurl, float closedCurl,
            float start, float grip01, GripShape grip)
        {
            float fan = Fan(open, start);
            float f = Mathf.Clamp01(fan);
            float over = Mathf.Max(0f, fan - 1f);
            float warn = Mathf.Clamp01(warn01);
            float g = Mathf.Clamp01(grip01);

            float shrink = 1f - WarnShrink * warn;
            float length = Mathf.Lerp(0.1f, 1f, f) * (1f + over * 0.5f) * shrink * (1f - grip.Shorten * g);
            float radius = Mathf.Lerp(0.5f, 1f, f) * shrink * (1f - grip.Pinch * g);
            float height = f;
            float pitch = Mathf.Lerp(closedPitch, openPitch, f) + WarnLift * warn + grip.Lift * g;
            float curl = Mathf.Lerp(closedCurl, openCurl, f) + grip.Curl * g;
            return new Pose(fan, length, radius, height, pitch, curl);
        }

        /// <summary>Tutunma yumruğunun şeritlere etkisi (tam kavramada, grip01 = 1). Ayar SO'sundan gelir.</summary>
        [System.Serializable]
        public struct GripShape
        {
            [Tooltip("Şeritlerin ek kalkışı (radyan).")] public float Lift;
            [Tooltip("Ek eğrilik (radyan / boy birimi).")] public float Curl;
            [Tooltip("Tabanın eksene çekilmesi (oran).")] public float Pinch;
            [Tooltip("Şerit boyunun kısalması (oran).")] public float Shorten;

            public static GripShape Default => new GripShape { Lift = 0.35f, Curl = 0.3f, Pinch = 0.25f, Shorten = 0.1f };
        }
    }
}
