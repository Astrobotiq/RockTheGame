using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Iris geçişi için saf hesap. Shader mesafeyi en-boy oranıyla çarpılmış uv uzayında ölçer
    /// (x: 0..aspect, y: 0..1); bu yüzden "tam açık" yarıçap merkeze en uzak ekran köşesine bağlıdır.
    /// </summary>
    public static class IrisMath
    {
        /// <summary>
        /// Ekranı tamamen açmak için gereken en küçük yarıçap: merkeze en uzak köşenin uzaklığı + kenar yumuşatma payı.
        /// Merkez ekran dışında da olabilir (kamera hâlâ hareket ederken).
        /// </summary>
        public static float RequiredRadius(Vector2 viewportCenter, float aspect, float edgeSmoothing)
        {
            if (aspect <= 0f || float.IsNaN(aspect)) aspect = 16f / 9f;

            float cx = viewportCenter.x * aspect;
            float cy = viewportCenter.y;

            float dx = Mathf.Max(cx, aspect - cx);
            float dy = Mathf.Max(cy, 1f - cy);

            return Mathf.Sqrt(dx * dx + dy * dy) + Mathf.Max(0f, edgeSmoothing);
        }
    }
}
