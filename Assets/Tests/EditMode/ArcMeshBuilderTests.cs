using System.Collections.Generic;
using New_Scripts.UI.Shapes;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class ArcMeshBuilderTests
    {
        private readonly List<Vector2> _pos = new List<Vector2>();
        private readonly List<float> _alpha = new List<float>();
        private readonly List<int> _tris = new List<int>();

        private static ArcMeshBuilder.Settings Ring(float sweep = 360f, float inner = 5f, float feather = 0f, int seg = 32) =>
            new ArcMeshBuilder.Settings
            {
                OuterRadius = 10f, InnerRadius = inner, StartDegrees = 0f, SweepDegrees = sweep,
                SegmentsPerCircle = seg, Feather = feather
            };

        private void Build(ArcMeshBuilder.Settings s) => ArcMeshBuilder.Build(s, _pos, _alpha, _tris);

        [Test]
        public void FullRing_HasTwoRings_AndTwoTrianglesPerSegment()
        {
            Build(Ring());
            Assert.AreEqual(2 * 33, _pos.Count, "iki halka x (n+1) köşe");
            Assert.AreEqual(32 * 2 * 3, _tris.Count);
        }

        [Test]
        public void AllTriangleIndices_AreValid()
        {
            foreach (var s in new[] { Ring(), Ring(90f), Ring(-120f, 0f), Ring(270f, 3f, 1f) })
            {
                Build(s);
                Assert.IsTrue(_tris.TrueForAll(i => i >= 0 && i < _pos.Count));
                Assert.AreEqual(_pos.Count, _alpha.Count);
                Assert.AreEqual(0, _tris.Count % 3);
            }
        }

        [Test]
        public void ZeroSweepOrZeroRadius_DrawsNothing()
        {
            Build(Ring(0f));
            Assert.AreEqual(0, _pos.Count);
            var s = Ring();
            s.OuterRadius = 0f;
            Build(s);
            Assert.AreEqual(0, _tris.Count);
        }

        [Test]
        public void Sweep_IsClampedTo360_AndInnerToOuter()
        {
            Build(Ring(720f));
            int clamped = _pos.Count;
            Build(Ring(360f));
            Assert.AreEqual(clamped, _pos.Count);

            Build(Ring(360f, inner: 99f));
            foreach (var p in _pos) Assert.LessOrEqual(p.magnitude, 10f + 1e-3f);
        }

        [Test]
        public void PositiveSweep_GoesClockwiseFromTwelveOClock()
        {
            Build(Ring(90f, 0f, 0f, 4));
            // Dıştaki halka ilk: başlangıç = tam yukarı, son = tam sağ.
            Assert.AreEqual(0f, _pos[0].x, 1e-4f);
            Assert.AreEqual(10f, _pos[0].y, 1e-4f);
            int last = (_pos.Count / 2) - 1; // 0 yarıçaplı ikinci halkadan önceki
            Assert.AreEqual(10f, _pos[last].x, 1e-3f);
            Assert.AreEqual(0f, _pos[last].y, 1e-3f);

            Build(Ring(-90f, 0f, 0f, 4));
            Assert.AreEqual(-10f, _pos[(_pos.Count / 2) - 1].x, 1e-3f, "negatif süpürme saat yönünün tersi");
        }

        [Test]
        public void Feather_AddsTransparentOuterRing_AndInnerRingOnlyWithHole()
        {
            Build(Ring(360f, 5f, feather: 1f, seg: 8));
            Assert.AreEqual(4 * 9, _pos.Count, "dış tüy, dış, iç, iç tüy");
            Assert.AreEqual(0f, _alpha[0]);
            Assert.AreEqual(11f, _pos[0].magnitude, 1e-3f);
            Assert.AreEqual(0f, _alpha[_alpha.Count - 1]);

            Build(Ring(360f, 0f, feather: 1f, seg: 8));
            Assert.AreEqual(3 * 9, _pos.Count, "dolu dilimde iç tüy yok");
        }

        [Test]
        public void ArcUsesFewerSegments_InProportionToSweep()
        {
            Build(Ring(360f, 5f, 0f, 64));
            int full = _tris.Count;
            Build(Ring(90f, 5f, 0f, 64));
            Assert.AreEqual(full / 4, _tris.Count);
        }
    }
}
