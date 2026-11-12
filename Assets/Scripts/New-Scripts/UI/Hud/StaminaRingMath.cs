using UnityEngine;

namespace New_Scripts.UI.Hud
{
    /// <summary>
    /// Kırılan stamina halkasının saf mantığı (EditMode'da test edilir): kaç parça sağlam, parçalar halkada nerede durur.
    /// Parçalar 12 yönünden saat yönünde dizilir; enerji azalınca <b>son</b> parça önce kopar (mockup'ta kopanlar halkanın sonunda).
    /// </summary>
    public static class StaminaRingMath
    {
        private const float Epsilon = 1e-4f;

        /// <summary>
        /// Enerji oranına (0–1) göre sağlam parça sayısı. Tam enerjide hepsi; her <c>1/count</c> kaybında bir parça kopar,
        /// enerji bitince (0) hiçbiri kalmaz. Oran sınırın hemen üstündeyse parça henüz kopmaz.
        /// </summary>
        public static int IntactPieces(float fraction, int count)
        {
            if (count <= 0) return 0;
            float f = Mathf.Clamp01(fraction);
            return Mathf.Clamp(Mathf.CeilToInt(f * count - Epsilon), 0, count);
        }

        /// <summary><paramref name="index"/>. parçanın başlangıç açısı (derece, 12 yönünden saat yönünde); iki yanında boşluğun yarısı kadar pay bırakır.</summary>
        public static float PieceStartDegrees(int index, int count, float gapDegrees)
        {
            return index * (360f / count) + gapDegrees * 0.5f;
        }

        /// <summary>Bir parçanın süpürme açısı (derece): dilim açısı eksi boşluk. Boşluk dilimden büyükse 0.</summary>
        public static float PieceSweepDegrees(int count, float gapDegrees)
        {
            return Mathf.Max(0f, 360f / count - gapDegrees);
        }

        /// <summary>Parçanın orta açısı (derece, 12 yönünden saat yönünde).</summary>
        public static float PieceMidDegrees(int index, int count)
        {
            return (index + 0.5f) * (360f / count);
        }

        /// <summary>Orta açıya karşılık gelen birim yön (12 yönü = yukarı, saat yönünde).</summary>
        public static Vector2 DirectionOf(float clockDegrees)
        {
            float rad = clockDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        }
    }
}
