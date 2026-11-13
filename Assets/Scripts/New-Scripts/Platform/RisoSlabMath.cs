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
    }
}
