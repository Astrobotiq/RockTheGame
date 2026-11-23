using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Balonun izlediği yol: başlangıç → (kapı) → hedef. Kapı varsa iki parçalı yumuşak eğri, yoksa düz çizgi.
    /// Eğri Catmull-Rom benzeri bir Hermite'tir; teğetler parçanın kendi uzunluğuyla ölçeklenir. Tek tip Catmull-Rom,
    /// kapı bacakları çok farklı uzunluktaysa (kısa kapı + uzun oda) döngü yapar.
    /// Yol uzunluğa göre örneklenir, yani eşit kesir eşit mesafe demek; hız sabit, yumuşaklık <see cref="PenaltyBubbleMath.Ease"/>'ten gelir.
    /// Saf mantık, Unity nesnesi tutmaz.
    /// </summary>
    public sealed class PenaltyBubblePath
    {
        private const int SamplesPerSegment = 32;

        private readonly Vector2[] _points;
        private readonly float[] _cumulative;

        public float Length { get; }
        public bool HasDoor { get; }
        /// <summary>Kapının yolun kaçta kaçında olduğu (0–1). Kapı yoksa 0.</summary>
        public float DoorFraction { get; }

        private PenaltyBubblePath(Vector2[] points, float[] cumulative, bool hasDoor, int doorIndex)
        {
            _points = points;
            _cumulative = cumulative;
            Length = cumulative[cumulative.Length - 1];
            HasDoor = hasDoor;
            DoorFraction = hasDoor && Length > 0f ? cumulative[doorIndex] / Length : 0f;
        }

        public static PenaltyBubblePath Build(Vector2 start, Vector2? door, Vector2 end)
        {
            var points = new List<Vector2>();
            int doorIndex = 0;

            if (door.HasValue)
            {
                Vector2 d = door.Value;
                Vector2 toDoor = d - start;
                Vector2 fromDoor = end - d;
                float lenA = toDoor.magnitude;
                float lenB = fromDoor.magnitude;
                // Kapıdaki gidiş yönü: iki bacağın ortalaması. Tam geri dönüşte sıfır (balon kapıda yavaşlar).
                Vector2 dir = Unit(toDoor) + Unit(fromDoor);
                dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector2.zero;

                AddHermite(points, start, toDoor, d, dir * lenA, includeFirst: true);
                doorIndex = points.Count - 1;
                AddHermite(points, d, dir * lenB, end, fromDoor, includeFirst: false);
            }
            else
            {
                points.Add(start);
                points.Add(end);
            }

            var cumulative = new float[points.Count];
            for (int i = 1; i < points.Count; i++)
                cumulative[i] = cumulative[i - 1] + Vector2.Distance(points[i - 1], points[i]);

            return new PenaltyBubblePath(points.ToArray(), cumulative, door.HasValue, doorIndex);
        }

        /// <summary>Yolun <paramref name="fraction"/> kesrindeki (0–1, mesafeye göre) konumu.</summary>
        public Vector2 PointAt(float fraction)
        {
            if (Length <= 0f) return _points[0];
            int i = SegmentIndex(Mathf.Clamp01(fraction) * Length, out float t);
            return Vector2.Lerp(_points[i], _points[i + 1], t);
        }

        /// <summary>Yolun o kesrindeki birim gidiş yönü. Yol tek noktaysa sağ.</summary>
        public Vector2 DirectionAt(float fraction)
        {
            if (Length <= 0f) return Vector2.right;
            int i = SegmentIndex(Mathf.Clamp01(fraction) * Length, out _);
            Vector2 d = _points[i + 1] - _points[i];
            return d.sqrMagnitude > 1e-12f ? d.normalized : Vector2.right;
        }

        // distance'ı içeren örnek aralığını bulur; t aralık içindeki oran.
        private int SegmentIndex(float distance, out float t)
        {
            int lo = 0, hi = _cumulative.Length - 1;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) / 2;
                if (_cumulative[mid] <= distance) lo = mid; else hi = mid;
            }
            float span = _cumulative[lo + 1] - _cumulative[lo];
            t = span > 1e-9f ? Mathf.Clamp01((distance - _cumulative[lo]) / span) : 0f;
            return lo;
        }

        private static Vector2 Unit(Vector2 v) => v.sqrMagnitude > 1e-12f ? v.normalized : Vector2.zero;

        private static void AddHermite(List<Vector2> into, Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1, bool includeFirst)
        {
            for (int i = includeFirst ? 0 : 1; i <= SamplesPerSegment; i++)
            {
                float t = i / (float)SamplesPerSegment;
                float t2 = t * t, t3 = t2 * t;
                into.Add((2f * t3 - 3f * t2 + 1f) * p0
                         + (t3 - 2f * t2 + t) * m0
                         + (-2f * t3 + 3f * t2) * p1
                         + (t3 - t2) * m1);
            }
        }
    }
}
