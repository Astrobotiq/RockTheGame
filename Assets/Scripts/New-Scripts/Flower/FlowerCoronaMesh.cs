using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// İplik tacı halkası: tek mesh'te <c>strips</c> adet şerit ızgarası (<see cref="FlowerPetalMesh"/> ızgarasıyla aynı uv düzeni:
    /// x boy, y en). Her köşenin TEXCOORD1'i şeridin azimutunu (radyan) ve boy çarpanını taşır; konumlandırmayı ve bükmeyi
    /// <c>Flower/Corona</c> vertex shader'ı yapar. Halka başına tek mesh, tek renderer: şerit başına GameObject yok.
    /// </summary>
    public static class FlowerCoronaMesh
    {
        /// <summary>Komşu şeritlerin boy seed'ini dağıtan altın oran adımı (<see cref="FlowerBloomMath.PetalRank"/> ile aynı fikir).</summary>
        public const float GoldenRatioConjugate = 0.6180339887f;

        /// <param name="strips">Halkadaki şerit sayısı (en az 3).</param>
        /// <param name="phase">Halkanın dönüklüğü, şerit adımının oranı (0.5 = yarım adım; iç halka dış halkayla dişlensin).</param>
        public static Mesh Build(int strips = 10, float phase = 0f, int segmentsAlong = 12, int segmentsAcross = 4)
        {
            strips = Mathf.Max(strips, 3);
            segmentsAlong = Mathf.Max(segmentsAlong, 1);
            segmentsAcross = Mathf.Max(segmentsAcross, 1);

            int cols = segmentsAlong + 1;
            int rows = segmentsAcross + 1;
            int perStrip = cols * rows;

            var verts = new Vector3[strips * perStrip];
            var uvs = new Vector2[strips * perStrip];
            var uv2 = new Vector2[strips * perStrip];
            var tris = new int[strips * segmentsAlong * segmentsAcross * 6];

            float step = 2f * Mathf.PI / strips;
            int t = 0;
            for (int s = 0; s < strips; s++)
            {
                float azimuth = (s + phase) * step;
                float lengthMul = StripLengthMul(s, phase);
                int baseIdx = s * perStrip;

                for (int j = 0; j < rows; j++)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        float u = i / (float)segmentsAlong;
                        float v = j / (float)segmentsAcross;
                        int idx = baseIdx + j * cols + i;
                        verts[idx] = new Vector3(u, v - 0.5f, 0f);
                        uvs[idx] = new Vector2(u, v);
                        uv2[idx] = new Vector2(azimuth, lengthMul);
                    }
                }

                for (int j = 0; j < segmentsAcross; j++)
                {
                    for (int i = 0; i < segmentsAlong; i++)
                    {
                        int a = baseIdx + j * cols + i;
                        int b = a + 1;
                        int c = a + cols;
                        int d = c + 1;
                        tris[t++] = a; tris[t++] = c; tris[t++] = b;
                        tris[t++] = b; tris[t++] = c; tris[t++] = d;
                    }
                }
            }

            var mesh = new Mesh { name = "FlowerCoronaRing" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.uv2 = uv2;
            mesh.triangles = tris;
            // Vertex shader şeritleri mesh sınırlarının dışına yerleştirir ve büker; kameradan elenmesin.
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 8f);
            return mesh;
        }

        /// <summary>Şeridin boy çarpanı: altın oran dağılımlı seed'den <see cref="FlowerCutMath"/>; komşu şeritler farklı boyda.</summary>
        public static float StripLengthMul(int index, float phase)
        {
            float seed = Mathf.Repeat((index + 1) * GoldenRatioConjugate + phase * 0.37f, 1f);
            return FlowerCutMath.ForSeed(seed).Length;
        }
    }
}
