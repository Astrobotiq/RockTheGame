using System;
using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.UI.Shapes
{
    /// <summary>
    /// Halka, yay, dilim ve daire için saf mesh geometrisi (Unity'ye bağımlı değil, EditMode'da test edilir).
    /// Açı saat gibi ölçülür: 0 = 12 yönü, pozitif süpürme saat yönünde.
    /// Overlay canvas MSAA'dan yararlanmaz; kenarlar yumuşasın diye dış/iç kenarda alfası 0'a inen ince bir "tüy" şeridi eklenir.
    /// </summary>
    public static class ArcMeshBuilder
    {
        public struct Settings
        {
            public float OuterRadius;
            /// <summary>0 = dolu dilim/daire; &gt;0 = halka. OuterRadius'a kırpılır.</summary>
            public float InnerRadius;
            public float StartDegrees;
            /// <summary>-360..360; işaret yönü verir (pozitif saat yönü), 0 = hiçbir şey çizilmez.</summary>
            public float SweepDegrees;
            /// <summary>Tam daire için kenar sayısı; yay için oranla azalır.</summary>
            public int SegmentsPerCircle;
            /// <summary>Kenar yumuşatma şeridi genişliği (mesh birimi). 0 = sert kenar.</summary>
            public float Feather;
        }

        /// <summary>Halkalar dıştan içe dizilir; her halkanın (n+1) köşesi vardır. Alfa dış/iç tüyde 0, çekirdekte 1.</summary>
        public static void Build(Settings s, List<Vector2> positions, List<float> alphas, List<int> triangles)
        {
            positions.Clear();
            alphas.Clear();
            triangles.Clear();

            float outer = Mathf.Max(0f, s.OuterRadius);
            float sweep = Mathf.Clamp(s.SweepDegrees, -360f, 360f);
            if (outer <= 0f || Mathf.Approximately(sweep, 0f)) return;

            float inner = Mathf.Clamp(s.InnerRadius, 0f, outer);
            float feather = Mathf.Max(0f, s.Feather);
            int perCircle = Mathf.Max(3, s.SegmentsPerCircle);
            int n = Mathf.Max(1, Mathf.CeilToInt(perCircle * Mathf.Abs(sweep) / 360f));

            var rings = new List<(float radius, float alpha)>(4);
            if (feather > 0f) rings.Add((outer + feather, 0f));
            rings.Add((outer, 1f));
            rings.Add((inner, 1f));
            // Delik varsa içe doğru da yumuşat; dolu dilimde merkez noktasında tüy olmaz.
            if (feather > 0f && inner > 0f) rings.Add((Mathf.Max(0f, inner - feather), 0f));

            for (int r = 0; r < rings.Count; r++)
            {
                for (int i = 0; i <= n; i++)
                {
                    float theta = (s.StartDegrees + sweep * i / n) * Mathf.Deg2Rad;
                    positions.Add(new Vector2(Mathf.Sin(theta), Mathf.Cos(theta)) * rings[r].radius);
                    alphas.Add(rings[r].alpha);
                }
            }

            int stride = n + 1;
            for (int r = 0; r < rings.Count - 1; r++)
            {
                // Dolu dilimde iç halka tek noktaya çöker: o şeritte sıfır alanlı üçgen kalır, zararsız.
                for (int i = 0; i < n; i++)
                {
                    int a = r * stride + i;
                    int b = a + 1;
                    int c = (r + 1) * stride + i;
                    int d = c + 1;
                    triangles.Add(a); triangles.Add(b); triangles.Add(d);
                    triangles.Add(a); triangles.Add(d); triangles.Add(c);
                }
            }
        }
    }
}
