using New_Scripts.UI.Shapes;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class PolygonMathTests
    {
        private static float Area(Vector2[] p) => Mathf.Abs(PolygonMath.SignedArea(p));

        [Test]
        public void CircleLens_UnitCirclesOneApart_HasCorrectTipsAndArea()
        {
            var lens = PolygonMath.CircleLens(Vector2.zero, 1f, new Vector2(1f, 0f), 1f, 48);

            Assert.IsNotEmpty(lens);
            // Köşeler (0.5, ±√3/2).
            bool top = false, bottom = false;
            foreach (var p in lens)
            {
                if (Vector2.Distance(p, new Vector2(0.5f, Mathf.Sqrt(3f) / 2f)) < 1e-4f) top = true;
                if (Vector2.Distance(p, new Vector2(0.5f, -Mathf.Sqrt(3f) / 2f)) < 1e-4f) bottom = true;
            }

            Assert.IsTrue(top && bottom, "iki köşe noktası dairelerin kesişiminde");
            // Gerçek alan: 2·acos(0,5) − 0,5·√3 = 1,2284; çokgen yaklaşığı %1 içinde.
            Assert.AreEqual(1.2284f, Area(lens), 1.2284f * 0.01f);
        }

        [Test]
        public void CircleLens_AllPointsInsideBothCircles()
        {
            var a = new Vector2(-0.3f, 0.2f);
            var b = new Vector2(1.1f, -0.4f);
            var lens = PolygonMath.CircleLens(a, 1.3f, b, 0.9f, 32);

            Assert.IsNotEmpty(lens);
            foreach (var p in lens)
            {
                Assert.LessOrEqual(Vector2.Distance(p, a), 1.3f + 1e-4f);
                Assert.LessOrEqual(Vector2.Distance(p, b), 0.9f + 1e-4f);
            }
        }

        [Test]
        public void CircleLens_Disjoint_IsEmpty()
        {
            Assert.IsEmpty(PolygonMath.CircleLens(Vector2.zero, 1f, new Vector2(3f, 0f), 1f, 16));
        }

        [Test]
        public void CircleLens_OneInsideOther_IsTheSmallerCircle()
        {
            var lens = PolygonMath.CircleLens(Vector2.zero, 3f, new Vector2(0.5f, 0f), 1f, 32);

            Assert.IsNotEmpty(lens);
            Assert.AreEqual(Mathf.PI, Area(lens), Mathf.PI * 0.02f);
        }

        [Test]
        public void OffsetConvex_Square_GrowsEverySideByDistance()
        {
            var square = new[] { new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 2), new Vector2(0, 2) };

            var grown = PolygonMath.OffsetConvex(square, 1f);

            Assert.AreEqual(16f, Area(grown), 1e-3f, "kenar 2 → 4");
        }

        [Test]
        public void OffsetConvex_WorksForClockwiseInput()
        {
            var square = new[] { new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 2), new Vector2(2, 0) };

            Assert.AreEqual(16f, Area(PolygonMath.OffsetConvex(square, 1f)), 1e-3f);
        }

        [Test]
        public void OffsetConvex_NegativeDistance_Shrinks()
        {
            var square = new[] { new Vector2(0, 0), new Vector2(4, 0), new Vector2(4, 4), new Vector2(0, 4) };

            Assert.AreEqual(4f, Area(PolygonMath.OffsetConvex(square, -1f)), 1e-3f, "kenar 4 → 2");
        }

        [Test]
        public void OffsetConvex_EquilateralTriangle_GrowsInradiusByDistance()
        {
            // İç yarıçapı 1 olan eşkenar üçgen (köşe uzaklığı 2).
            var tri = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                float a = (90f + 120f * i) * Mathf.Deg2Rad;
                tri[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2f;
            }

            var grown = PolygonMath.OffsetConvex(tri, 1f);

            // İç yarıçap 2 olmalı: köşe uzaklığı 4.
            foreach (var p in grown) Assert.AreEqual(4f, p.magnitude, 1e-3f);
        }

        [Test]
        public void Centroid_OfSquare_IsCenter()
        {
            var square = new[] { new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 2), new Vector2(0, 2) };

            Assert.AreEqual(Vector2.one, PolygonMath.Centroid(square));
        }
    }
}
