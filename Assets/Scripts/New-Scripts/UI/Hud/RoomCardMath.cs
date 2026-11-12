using UnityEngine;

namespace New_Scripts.UI.Hud
{
    /// <summary>
    /// Oda başlık kartının saf geometrisi (EditMode'da test edilir). Koordinatlar mockup gibi: x sağa, y aşağı, birim sanat pikseli
    /// (<c>docs/ui-research/mashup-ui.html</c> <c>uiCard</c>).
    /// </summary>
    public static class RoomCardMath
    {
        /// <summary>Karttaki oda numarası yazısı: iki haneli ("07", "12"). Numarasız oda (0 ve altı) için boş: kart gösterilmez.</summary>
        public static string FormatNumber(int roomId) => roomId > 0 ? roomId.ToString("D2") : string.Empty;

        /// <summary>İki nokta arasındaki doğrunun Unity Z dönüşü (derece, saat yönünün tersi pozitif). Y aşağı uzayda yukarı çıkan kiriş pozitif döner.</summary>
        public static float BeamAngleDegrees(Vector2 from, Vector2 to)
        {
            Vector2 d = to - from;
            return Mathf.Atan2(-d.y, d.x) * Mathf.Rad2Deg;
        }

        /// <summary>Doğru üzerinde <paramref name="x"/>'teki y (y aşağı uzay).</summary>
        public static float LineY(Vector2 from, Vector2 to, float x)
        {
            if (Mathf.Approximately(from.x, to.x)) return from.y;
            return Mathf.Lerp(from.y, to.y, (x - from.x) / (to.x - from.x));
        }

        /// <summary>
        /// Daireyi <c>x ≥ minX</c> ve <c>y ≤ maxY</c> ile kırpılmış dışbükey çokgen olarak verir (levhadan taşan disk köşede kesilir).
        /// Kırpma, çember noktalarını sınıra çekerek yapılır; sonuç dışbükey kalır.
        /// </summary>
        public static Vector2[] ClippedDisc(Vector2 center, float radius, float minX, float maxY, int segments)
        {
            segments = Mathf.Max(8, segments);
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 2f;
                Vector2 p = center + radius * new Vector2(Mathf.Cos(t), Mathf.Sin(t));
                points[i] = new Vector2(Mathf.Max(p.x, minX), Mathf.Min(p.y, maxY));
            }

            return points;
        }
    }
}
