using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.UI.Shapes
{
    /// <summary>
    /// Dışbükey çokgen ve iki dairenin kesişimi (mercek) için saf geometri. Unity bileşeni yok, EditMode'da test edilir
    /// (<c>PolygonMathTests</c>). <see cref="UiPolygon"/> ve pause arka planı kullanır.
    /// </summary>
    public static class PolygonMath
    {
        /// <summary>Pozitif = saat yönünün tersi (CCW).</summary>
        public static float SignedArea(IList<Vector2> p)
        {
            float sum = 0f;
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 a = p[i], b = p[(i + 1) % p.Count];
                sum += a.x * b.y - b.x * a.y;
            }

            return sum * 0.5f;
        }

        /// <summary>Köşelerin ortalaması (dışbükey çokgende yelpaze merkezi olarak yeter).</summary>
        public static Vector2 Centroid(IList<Vector2> p)
        {
            var sum = Vector2.zero;
            for (int i = 0; i < p.Count; i++) sum += p[i];
            return p.Count > 0 ? sum / p.Count : Vector2.zero;
        }

        /// <summary>
        /// Dışbükey çokgeni her kenarından <paramref name="distance"/> kadar dışa (negatifse içe) iter; köşeler gönye (miter).
        /// Çerçeve ve sert gölge için. Sivri köşelerde gönye uzar: çokgen sivri değilse kullan (üçgen, dikdörtgen).
        /// </summary>
        public static Vector2[] OffsetConvex(IList<Vector2> p, float distance)
        {
            int n = p.Count;
            var result = new Vector2[n];
            float orientation = SignedArea(p) >= 0f ? 1f : -1f;

            for (int i = 0; i < n; i++)
            {
                Vector2 prev = p[(i + n - 1) % n], cur = p[i], next = p[(i + 1) % n];
                Vector2 n1 = OutwardNormal(cur - prev, orientation);
                Vector2 n2 = OutwardNormal(next - cur, orientation);
                float denom = 1f + Vector2.Dot(n1, n2);
                result[i] = cur + (n1 + n2) / Mathf.Max(denom, 1e-4f) * distance;
            }

            return result;
        }

        private static Vector2 OutwardNormal(Vector2 edge, float orientation)
        {
            Vector2 e = edge.normalized;
            return new Vector2(e.y, -e.x) * orientation;
        }

        /// <summary>
        /// İki dairenin kesişim bölgesi (mercek) çokgeni; her yay <paramref name="segmentsPerArc"/> parça. Kesişim yoksa boş;
        /// biri ötekinin içindeyse küçük dairenin çokgeni.
        /// </summary>
        public static Vector2[] CircleLens(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB, int segmentsPerArc)
        {
            Vector2 delta = centerB - centerA;
            float d = delta.magnitude;
            segmentsPerArc = Mathf.Max(2, segmentsPerArc);

            if (d >= radiusA + radiusB) return new Vector2[0];
            if (d <= Mathf.Abs(radiusA - radiusB))
            {
                bool aSmaller = radiusA < radiusB;
                return Circle(aSmaller ? centerA : centerB, aSmaller ? radiusA : radiusB, segmentsPerArc * 2);
            }

            Vector2 u = delta / d;
            Vector2 n = new Vector2(-u.y, u.x);
            float x = (d * d + radiusA * radiusA - radiusB * radiusB) / (2f * d); // A merkezinden kesişim kirişine uzaklık
            float alphaA = Mathf.Acos(Mathf.Clamp(x / radiusA, -1f, 1f));
            float alphaB = Mathf.Acos(Mathf.Clamp((d - x) / radiusB, -1f, 1f));

            var points = new Vector2[segmentsPerArc * 2 + 2];
            int k = 0;
            // A'nın B içindeki yayı: üst köşeden B'ye bakan taraftan alt köşeye.
            for (int i = 0; i <= segmentsPerArc; i++)
            {
                float t = Mathf.Lerp(alphaA, -alphaA, i / (float)segmentsPerArc);
                points[k++] = centerA + radiusA * (Mathf.Cos(t) * u + Mathf.Sin(t) * n);
            }

            // B'nin A içindeki yayı: alt köşeden A'ya bakan taraftan üst köşeye.
            for (int i = 0; i <= segmentsPerArc; i++)
            {
                float t = Mathf.Lerp(-alphaB, alphaB, i / (float)segmentsPerArc);
                points[k++] = centerB + radiusB * (-Mathf.Cos(t) * u + Mathf.Sin(t) * n);
            }

            return points;
        }

        public static Vector2[] Circle(Vector2 center, float radius, int segments)
        {
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 2f;
                points[i] = center + radius * new Vector2(Mathf.Cos(t), Mathf.Sin(t));
            }

            return points;
        }
    }
}
