using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Yaprak plane'i: boyuna ve enine sık bölünmüş düz ızgara (x: 0..1 boy, y: -0.5..0.5 en, uv: 0..1).
    /// Gerçek şekli vertex shader (Flower/Petal) verir; ızgara sadece bükülecek yüzeyi sağlar.
    /// </summary>
    public static class FlowerPetalMesh
    {
        public static Mesh Build(int segmentsAlong = 24, int segmentsAcross = 8)
        {
            segmentsAlong = Mathf.Max(segmentsAlong, 1);
            segmentsAcross = Mathf.Max(segmentsAcross, 1);

            int cols = segmentsAlong + 1;
            int rows = segmentsAcross + 1;
            var verts = new Vector3[cols * rows];
            var uvs = new Vector2[cols * rows];
            var tris = new int[segmentsAlong * segmentsAcross * 6];

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    float u = i / (float)segmentsAlong;
                    float v = j / (float)segmentsAcross;
                    int idx = j * cols + i;
                    verts[idx] = new Vector3(u, v - 0.5f, 0f);
                    uvs[idx] = new Vector2(u, v);
                }
            }

            int t = 0;
            for (int j = 0; j < segmentsAcross; j++)
            {
                for (int i = 0; i < segmentsAlong; i++)
                {
                    int a = j * cols + i;
                    int b = a + 1;
                    int c = a + cols;
                    int d = c + 1;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }
            }

            var mesh = new Mesh { name = "FlowerPetalGrid" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            // Vertex shader yüzeyi mesh sınırlarının çok dışına büker; kameradan elenmesin.
            mesh.bounds = new Bounds(new Vector3(0.5f, 0.5f, 0f), Vector3.one * 8f);
            return mesh;
        }
    }
}
