using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// RisoSlabVisual'ın saf hesabı (Unity nesnesi gerektirmez, EditMode'da test edilir).
    /// </summary>
    public static class RisoSlabMath
    {
        /// <summary>
        /// Hızı hedefe doğru kare hızından bağımsız üstel yumuşatır. smoothTime ≤ 0 ise doğrudan hedef.
        /// </summary>
        public static Vector2 SmoothVelocity(Vector2 current, Vector2 target, float deltaTime, float smoothTime)
        {
            if (smoothTime <= 0f || deltaTime <= 0f) return smoothTime <= 0f ? target : current;
            float t = 1f - Mathf.Exp(-deltaTime / smoothTime);
            return Vector2.Lerp(current, target, t);
        }

        /// <summary>
        /// Oyuncu temasının etki miktarı (aşama 2). Temas varken küçük kalıcı değer (standing) hedeflenir;
        /// iniş anında (temas yeni başladı) değer 1'e fırlar ve decayTime ile hedefe yaylanır.
        /// Temas bitince hedef 0, aynı zaman sabitiyle sönülür. Dönüş: yeni değer.
        /// </summary>
        public static float StepInfluence(float amount, bool touching, bool justLanded, float deltaTime,
            float standing, float decayTime)
        {
            if (justLanded) amount = 1f;
            float target = touching ? Mathf.Clamp01(standing) : 0f;
            if (deltaTime <= 0f) return amount;
            if (decayTime <= 0f) return target;
            float t = 1f - Mathf.Exp(-deltaTime / decayTime);
            return Mathf.Lerp(amount, target, t);
        }

        /// <summary>
        /// Mürekkep değişimi halkasını ilerletir. front ≤ 0 kapalıdır (0 döner). Halka ve bandı maxRadius'u geçince kapanır.
        /// </summary>
        public static float AdvanceFront(float front, float deltaTime, float speed, float maxRadius, float band)
        {
            if (front <= 0f) return 0f;
            front += Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime);
            return front > maxRadius + band ? 0f : front;
        }

        /// <summary>Mesafe halka bandının içinde mi: front - band ≤ distance ≤ front. front ≤ 0 ise hiçbir yer.</summary>
        public static bool IsInShiftBand(float distance, float front, float band)
        {
            return front > 0f && distance <= front && distance >= front - band;
        }
    }
}
