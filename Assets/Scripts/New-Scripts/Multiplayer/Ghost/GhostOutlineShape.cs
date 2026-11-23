using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Bir konturun tek bir yolu: yüzeyin yerel uzayında noktalar.</summary>
    public sealed class OutlinePath
    {
        public readonly Vector2[] Points;
        public readonly bool Loop;
        /// <summary>LineRenderer'a yazarken yeniden ayırmamak için tampon.</summary>
        public readonly Vector3[] Scratch;

        public OutlinePath(Vector2[] points, bool loop)
        {
            Points = points;
            Loop = loop;
            Scratch = new Vector3[points.Length];
        }
    }

    /// <summary>
    /// Hareketli bir yüzeyin collider şeklinden kontur yolları çıkarır. Noktalar yüzeyin (kayıtlı transform) yerel uzayında
    /// tutulur: böylece kontur, rakibin dünyasındaki poza (konum + dönüş + ölçek) tek matrisle oturur.
    /// Kutu, daire, kapsül, çokgen, bileşik ve kenar collider'ları desteklenir. Tetik olmayan collider varsa yalnızca onlar
    /// (platformun görünen gövdesi), yoksa tetikler (tehlike alanı). Başka bir sürücüye ait alt nesneler ve bileşiğe
    /// katılan collider'lar atlanır.
    /// </summary>
    public static class GhostOutlineShape
    {
        private const int CircleSegments = 24;
        private const int CapsuleArcSegments = 8;

        public static List<OutlinePath> Extract(Transform surface, MovingSurfaceRegistry registry)
        {
            var solid = new List<Collider2D>();
            var triggers = new List<Collider2D>();
            foreach (var collider in surface.GetComponentsInChildren<Collider2D>(true))
            {
                if (collider == null || BelongsToOtherDriver(collider.transform, surface, registry)) continue;
                if (collider.compositeOperation != Collider2D.CompositeOperation.None && !(collider is CompositeCollider2D)) continue;
                (collider.isTrigger ? triggers : solid).Add(collider);
            }

            var paths = new List<OutlinePath>();
            foreach (var collider in solid.Count > 0 ? solid : triggers) AddPaths(collider, surface, paths);
            return paths;
        }

        private static bool BelongsToOtherDriver(Transform t, Transform surface, MovingSurfaceRegistry registry)
        {
            for (; t != null && t != surface; t = t.parent)
                if (registry != null && registry.TryGetId(t, out _)) return true;
            return false;
        }

        private static void AddPaths(Collider2D collider, Transform surface, List<OutlinePath> into)
        {
            switch (collider)
            {
                case BoxCollider2D box:
                {
                    Vector2 h = box.size * 0.5f;
                    Add(into, surface, collider, new[] { new Vector2(-h.x, -h.y), new Vector2(h.x, -h.y), new Vector2(h.x, h.y), new Vector2(-h.x, h.y) }, box.offset, true);
                    break;
                }
                case CircleCollider2D circle:
                {
                    var points = new Vector2[CircleSegments];
                    for (int i = 0; i < CircleSegments; i++)
                    {
                        float a = i * Mathf.PI * 2f / CircleSegments;
                        points[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * circle.radius;
                    }
                    Add(into, surface, collider, points, circle.offset, true);
                    break;
                }
                case CapsuleCollider2D capsule:
                    Add(into, surface, collider, CapsulePoints(capsule), capsule.offset, true);
                    break;
                case PolygonCollider2D polygon:
                    for (int p = 0; p < polygon.pathCount; p++)
                        Add(into, surface, collider, polygon.GetPath(p), polygon.offset, true);
                    break;
                case CompositeCollider2D composite:
                    for (int p = 0; p < composite.pathCount; p++)
                    {
                        var points = new Vector2[composite.GetPathPointCount(p)];
                        composite.GetPath(p, points);
                        Add(into, surface, collider, points, Vector2.zero, true);
                    }
                    break;
                case EdgeCollider2D edge:
                    Add(into, surface, collider, edge.points, edge.offset, false);
                    break;
            }
        }

        private static Vector2[] CapsulePoints(CapsuleCollider2D capsule)
        {
            bool vertical = capsule.direction == CapsuleDirection2D.Vertical;
            float radius = (vertical ? capsule.size.x : capsule.size.y) * 0.5f;
            float half = Mathf.Max(0f, ((vertical ? capsule.size.y : capsule.size.x) * 0.5f) - radius);
            var points = new List<Vector2>();
            for (int cap = 0; cap < 2; cap++)
            {
                for (int i = 0; i <= CapsuleArcSegments; i++)
                {
                    float a = (cap * Mathf.PI) + (i * Mathf.PI / CapsuleArcSegments);
                    Vector2 arc = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    Vector2 centre = new Vector2(0f, cap == 0 ? half : -half);
                    points.Add(vertical ? arc + centre : new Vector2(arc.y + centre.y, arc.x));
                }
            }
            return points.ToArray();
        }

        /// <summary>Collider yerel noktalarını (+ ofset) dünyadan geçirip yüzeyin yerel uzayına çevirir.</summary>
        private static void Add(List<OutlinePath> into, Transform surface, Collider2D collider, Vector2[] colliderLocal, Vector2 offset, bool loop)
        {
            if (colliderLocal == null || colliderLocal.Length < 2) return;
            var points = new Vector2[colliderLocal.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 world = collider.transform.TransformPoint(colliderLocal[i] + offset);
                points[i] = surface.InverseTransformPoint(world);
            }
            into.Add(new OutlinePath(points, loop));
        }
    }
}
