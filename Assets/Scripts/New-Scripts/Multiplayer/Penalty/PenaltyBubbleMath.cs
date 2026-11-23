using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Balon cezasının saf matematiği: süre, yumuşak hızlanma, kapıyı geçme anı, salınım, kalkış ve şişme.
    /// Zaman parametreleri 0–1 oranıdır (<c>u</c>); sn'ye çeviren <see cref="PenaltyHandler"/>'dır.
    /// </summary>
    public static class PenaltyBubbleMath
    {
        // Şişme: bu orana kadar aşıma çıkar, sonra 1'e oturur.
        private const float InflatePeak = 0.6f;

        /// <summary>Yolculuk süresi: yol / hız, [min, max]'a kırpılır. Hız sıfırsa en uzun süre.</summary>
        public static float Duration(float pathLength, float cruiseSpeed, float minSeconds, float maxSeconds)
        {
            if (cruiseSpeed <= 0f) return maxSeconds;
            return Mathf.Clamp(pathLength / cruiseSpeed, minSeconds, maxSeconds);
        }

        /// <summary>Yumuşak hızlanma/yavaşlama (smoothstep). Yolun kesri = Ease(zaman kesri).</summary>
        public static float Ease(float u)
        {
            u = Mathf.Clamp01(u);
            return u * u * (3f - 2f * u);
        }

        /// <summary>Ease'in tersi: yolun <paramref name="progress"/> kesrine hangi zaman kesrinde varılır.</summary>
        public static float InverseEase(float progress)
        {
            progress = Mathf.Clamp01(progress);
            float lo = 0f, hi = 1f;
            for (int i = 0; i < 32; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (Ease(mid) < progress) lo = mid; else hi = mid;
            }
            return (lo + hi) * 0.5f;
        }

        /// <summary>Kapıya hangi zaman kesrinde varılır. Kapı yoksa 1.</summary>
        public static float DoorTime(PenaltyBubblePath path) => path.HasDoor ? InverseEase(path.DoorFraction) : 1f;

        /// <summary>Balon kapıyı geçti mi (oda geçişini tetikleme anı). Kapı yoksa hiç.</summary>
        public static bool HasPassedDoor(PenaltyBubblePath path, float u) =>
            path.HasDoor && Ease(u) >= path.DoorFraction;

        /// <summary>
        /// Gidiş yönüne dik salınım ofseti. Başta ve sonda sıfırdır (zarf sin(πu)), yani yola oturur ve varışta zıplamaz.
        /// </summary>
        public static Vector2 Wobble(Vector2 direction, float u, float durationSeconds, float amplitude, float hz)
        {
            if (direction.sqrMagnitude < 1e-12f) return Vector2.zero;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
            u = Mathf.Clamp01(u);
            float envelope = Mathf.Sin(Mathf.PI * u);
            float wave = Mathf.Sin(2f * Mathf.PI * hz * u * durationSeconds);
            return perpendicular * (amplitude * envelope * wave);
        }

        // Gidiş yönünün yatay bileşeni bundan küçükse (neredeyse düşey) pozun yönü değişmez.
        private const float PoseDeadZone = 0.2f;

        /// <summary>
        /// Uçma pozunun hedef açısı (derece, z): gidiş yönüne doğru yatar; sağa gidince saat yönünde (negatif).
        /// Düşeye yakın giderken sağ-sol belirsiz olduğu için <paramref name="previous"/> korunur.
        /// </summary>
        public static float TargetPoseAngle(float directionX, float degrees, float previous)
        {
            if (Mathf.Abs(directionX) < PoseDeadZone) return previous;
            return -Mathf.Sign(directionX) * degrees;
        }

        /// <summary>Balonun hız yönünde esneme miktarı (0–maxStretch): hızla doğrusal, referans hızda tavan.</summary>
        public static float StretchAmount(float speed, float referenceSpeed, float maxStretch)
        {
            if (referenceSpeed <= 0f) return 0f;
            return Mathf.Clamp01(speed / referenceSpeed) * maxStretch;
        }

        /// <summary>Kalkış: yukarı ofset, ease-out (hızlı çıkar, yavaşça durur).</summary>
        public static float LiftOffset(float u, float height)
        {
            float inv = 1f - Mathf.Clamp01(u);
            return height * (1f - inv * inv * inv);
        }

        /// <summary>Şişme ölçeği: 0 → overshoot → 1.</summary>
        public static float InflateScale(float u, float overshoot)
        {
            u = Mathf.Clamp01(u);
            if (u <= InflatePeak)
            {
                float x = u / InflatePeak;
                return overshoot * (1f - (1f - x) * (1f - x));
            }
            float v = (u - InflatePeak) / (1f - InflatePeak);
            return Mathf.Lerp(overshoot, 1f, v * v * (3f - 2f * v));
        }
    }
}
