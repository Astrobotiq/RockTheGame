using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// "Elle kesilmiş" yaprak farkı için saf hesap. Her yaprağın <c>Seed</c> değeri (0..1) o yaprağın boy ve en çarpanını
    /// ve ucunun yana yatıklığını belirler: aynı seed her zaman aynı kesimi verir, komşu yapraklar birbirinden farklıdır,
    /// ortalama çarpan 1'dir (çiçeğin genel boyutu değişmez).
    /// </summary>
    public static class FlowerCutMath
    {
        /// <summary>Boyda en fazla sapma oranı (±).</summary>
        public const float LengthJitter = 0.08f;

        /// <summary>Enda en fazla sapma oranı (±).</summary>
        public const float WidthJitter = 0.08f;

        /// <summary>Uç yatıklığı üst sınırı (yaprak enine oranla, ±); shader'da <c>_TipSkew</c>.</summary>
        public const float TipSkewMax = 0.12f;

        public readonly struct Cut
        {
            public readonly float Length;
            public readonly float Width;
            public readonly float TipSkew;

            public Cut(float length, float width, float tipSkew)
            {
                Length = length;
                Width = width;
                TipSkew = tipSkew;
            }

            /// <summary>Kesimsiz yaprak (çarpanlar 1, yatıklık 0).</summary>
            public static Cut None => new Cut(1f, 1f, 0f);
        }

        /// <summary>Seed'e göre boy çarpanı, en çarpanı ve uç yatıklığı. Çıktılar sınırların içindedir.</summary>
        public static Cut ForSeed(float seed01)
        {
            uint h = (uint)Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(seed01) * 16777215f), 0, 16777215);

            float length = 1f + LengthJitter * Signed(h, 0);
            float width = 1f + WidthJitter * Signed(h, 1);
            float skew = TipSkewMax * Signed(h, 2);
            return new Cut(length, width, skew);
        }

        // Kanal başına bağımsız görünen bir değer: seed bitlerinden tamsayı karıştırıcıyla -1..1.
        private static float Signed(uint seedBits, uint channel)
        {
            uint x = seedBits + channel * 0x9E3779B9u + 0x7F4A7C15u;
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return (x / 4294967295f) * 2f - 1f;
        }
    }
}
