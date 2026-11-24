using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>Launch pad fırlatma yönü kaynağı.</summary>
    public enum LaunchDirectionMode
    {
        /// <summary>Pad'in kendi <c>transform.up</c> yönü (dikdörtgen ve üçgen: tepe noktasının baktığı yön).</summary>
        TransformUp,
        /// <summary>Pad merkezinden temas noktasına doğru, dışa (daire).</summary>
        OutwardFromCenter,
    }

    /// <summary>Launch pad fırlatma yönü hesabı (saf mantık, EditMode testlenebilir).</summary>
    public static class LaunchDirectionMath
    {
        /// <summary>
        /// Yön birim vektör olarak döner. <see cref="LaunchDirectionMode.TransformUp"/> her zaman <paramref name="up"/>;
        /// <see cref="LaunchDirectionMode.OutwardFromCenter"/> merkezden <paramref name="contact"/>'a. Temas tam merkezdeyse
        /// (yön belirsiz) <paramref name="up"/>'a döner.
        /// </summary>
        public static Vector2 Resolve(LaunchDirectionMode mode, Vector2 center, Vector2 contact, Vector2 up)
        {
            Vector2 fallback = up.sqrMagnitude > 1e-8f ? up.normalized : Vector2.up;
            if (mode == LaunchDirectionMode.TransformUp) return fallback;

            Vector2 outward = contact - center;
            return outward.sqrMagnitude > 1e-6f ? outward.normalized : fallback;
        }

        /// <summary>
        /// Gizmo için daire pad'in olası fırlatma yönleri: <paramref name="up"/> etrafında <paramref name="halfAngle"/>
        /// derece açıyla eşit aralıklı <paramref name="count"/> yön (count tekse yalnızca up).
        /// </summary>
        public static Vector2[] FanDirections(Vector2 up, float halfAngle, int count)
        {
            if (count < 2) return new[] { up.normalized };
            var result = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(count - 1));
                result[i] = (Quaternion.Euler(0f, 0f, angle) * up).normalized;
            }
            return result;
        }
    }
}
