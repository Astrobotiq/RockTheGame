using NUnit.Framework;
using New_Scripts.Multiplayer.Penalty;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Balon cezasının saf matematiği: yol, süre, salınım, kalkış ve şişme (docs/multiplayer-ceza-balonu-plani.md → BB1).
    /// </summary>
    public class PenaltyBubbleMathTests
    {
        private static readonly Vector2 Start = new Vector2(0f, 0f);
        // Ceza hep geriye (önceki odaya) gider: kapı ve hedef oyuncunun solunda.
        private static readonly Vector2 Door = new Vector2(-40f, 10f);
        private static readonly Vector2 End = new Vector2(-120f, 25f);

        // ---------- Yol ----------

        [Test]
        public void Path_WithoutDoor_IsStraightLine()
        {
            var path = PenaltyBubblePath.Build(Start, null, new Vector2(60f, 0f));

            Assert.IsFalse(path.HasDoor);
            Assert.AreEqual(60f, path.Length, 0.01f);
            Assert.AreEqual(30f, path.PointAt(0.5f).x, 0.01f);
            Assert.AreEqual(0f, path.PointAt(0.5f).y, 0.01f);
        }

        [Test]
        public void Path_WithDoor_StartsAtStartAndEndsAtEnd()
        {
            var path = PenaltyBubblePath.Build(Start, Door, End);

            Assert.AreEqual(0f, Vector2.Distance(Start, path.PointAt(0f)), 0.001f);
            Assert.AreEqual(0f, Vector2.Distance(End, path.PointAt(1f)), 0.001f);
        }

        [Test]
        public void Path_WithDoor_PassesThroughTheDoor()
        {
            var path = PenaltyBubblePath.Build(Start, Door, End);

            Assert.IsTrue(path.HasDoor);
            Assert.Less(Vector2.Distance(Door, path.PointAt(path.DoorFraction)), 0.5f,
                "Balon kapı noktasından geçmeli; yoksa uyuyan bir odanın üstünden süzülür.");
        }

        [Test]
        public void Path_WithDoor_DoorFractionIsStrictlyInside()
        {
            var path = PenaltyBubblePath.Build(Start, Door, End);

            Assert.Greater(path.DoorFraction, 0f);
            Assert.Less(path.DoorFraction, 1f);
        }

        [Test]
        public void Path_WithDoor_DoorFractionFollowsSegmentLengths()
        {
            // Kapı yolun yaklaşık dörtte birinde: 30 + 90 birim.
            var path = PenaltyBubblePath.Build(new Vector2(0f, 0f), new Vector2(30f, 0f), new Vector2(120f, 0f));

            Assert.AreEqual(0.25f, path.DoorFraction, 0.02f);
        }

        [Test]
        public void Path_WithDoor_IsLongerThanTheStraightLine_ButNotWild()
        {
            var path = PenaltyBubblePath.Build(Start, Door, End);
            float polyline = Vector2.Distance(Start, Door) + Vector2.Distance(Door, End);

            Assert.GreaterOrEqual(path.Length, Vector2.Distance(Start, End));
            Assert.LessOrEqual(path.Length, polyline * 1.1f, "Yumuşatma köşeleri keser; köşeli yoldan uzun olamaz.");
        }

        [Test]
        public void Path_UnequalSegments_DoesNotLoopOrOvershoot()
        {
            // Kısa bir kapı bacağı + çok uzun ikinci bacak: tek tip Catmull-Rom burada döngü yapar.
            var start = new Vector2(0f, 0f);
            var door = new Vector2(10f, 0f);
            var end = new Vector2(300f, 40f);
            var path = PenaltyBubblePath.Build(start, door, end);

            for (int i = 0; i <= 100; i++)
            {
                Vector2 p = path.PointAt(i / 100f);
                Assert.GreaterOrEqual(p.x, -3f, $"i={i}");
                Assert.LessOrEqual(p.x, 303f, $"i={i}");
                Assert.GreaterOrEqual(p.y, -3f, $"i={i}");
                Assert.LessOrEqual(p.y, 43f, $"i={i}");
            }
        }

        [Test]
        public void Path_PointAt_ClampsOutOfRangeFractions()
        {
            var path = PenaltyBubblePath.Build(Start, Door, End);

            Assert.AreEqual(0f, Vector2.Distance(Start, path.PointAt(-1f)), 0.001f);
            Assert.AreEqual(0f, Vector2.Distance(End, path.PointAt(2f)), 0.001f);
        }

        [Test]
        public void Path_PointAt_ProgressesEvenlyByDistance()
        {
            // Eşit aralıklı kesirler eşit yol alır: hız sabit, yumuşaklık Ease'ten gelir.
            var path = PenaltyBubblePath.Build(Start, Door, End);
            float step = path.Length / 20f;

            for (int i = 0; i < 20; i++)
            {
                float d = Vector2.Distance(path.PointAt(i / 20f), path.PointAt((i + 1) / 20f));
                Assert.AreEqual(step, d, step * 0.35f, $"i={i}");
            }
        }

        [Test]
        public void Path_Direction_IsUnitAndPointsForward()
        {
            var path = PenaltyBubblePath.Build(new Vector2(0f, 0f), null, new Vector2(10f, 0f));

            Vector2 dir = path.DirectionAt(0.5f);

            Assert.AreEqual(1f, dir.magnitude, 0.001f);
            Assert.AreEqual(1f, dir.x, 0.001f);
        }

        [Test]
        public void Path_StartEqualsEnd_DoesNotProduceNaN()
        {
            var path = PenaltyBubblePath.Build(Start, null, Start);

            Assert.AreEqual(0f, path.Length, 0.0001f);
            Vector2 p = path.PointAt(0.5f);
            Assert.IsFalse(float.IsNaN(p.x) || float.IsNaN(p.y));
            Vector2 d = path.DirectionAt(0.5f);
            Assert.IsFalse(float.IsNaN(d.x) || float.IsNaN(d.y));
        }

        // ---------- Süre ----------

        [Test]
        public void Duration_ShortPath_IsClampedToMinimum()
        {
            Assert.AreEqual(2.5f, PenaltyBubbleMath.Duration(10f, 20f, 2.5f, 5f), 0.0001f);
        }

        [Test]
        public void Duration_LongPath_IsClampedToMaximum()
        {
            Assert.AreEqual(5f, PenaltyBubbleMath.Duration(340f, 20f, 2.5f, 5f), 0.0001f);
        }

        [Test]
        public void Duration_MiddlePath_IsLengthOverSpeed()
        {
            Assert.AreEqual(3.5f, PenaltyBubbleMath.Duration(70f, 20f, 2.5f, 5f), 0.0001f);
        }

        [Test]
        public void Duration_ZeroSpeed_FallsBackToMaximum()
        {
            Assert.AreEqual(5f, PenaltyBubbleMath.Duration(70f, 0f, 2.5f, 5f), 0.0001f);
        }

        // ---------- Yumuşak hızlanma ----------

        [Test]
        public void Ease_HitsEndpoints_AndIsMonotonic()
        {
            Assert.AreEqual(0f, PenaltyBubbleMath.Ease(0f), 0.0001f);
            Assert.AreEqual(1f, PenaltyBubbleMath.Ease(1f), 0.0001f);
            Assert.AreEqual(0.5f, PenaltyBubbleMath.Ease(0.5f), 0.0001f);

            float prev = 0f;
            for (int i = 1; i <= 50; i++)
            {
                float v = PenaltyBubbleMath.Ease(i / 50f);
                Assert.GreaterOrEqual(v, prev);
                prev = v;
            }
        }

        [Test]
        public void Ease_IsSlowAtBothEnds()
        {
            Assert.Less(PenaltyBubbleMath.Ease(0.1f), 0.1f);
            Assert.Greater(PenaltyBubbleMath.Ease(0.9f), 0.9f);
        }

        [Test]
        public void Ease_ClampsOutOfRange()
        {
            Assert.AreEqual(0f, PenaltyBubbleMath.Ease(-1f), 0.0001f);
            Assert.AreEqual(1f, PenaltyBubbleMath.Ease(3f), 0.0001f);
        }

        [Test]
        public void InverseEase_RoundTrips()
        {
            for (int i = 0; i <= 10; i++)
            {
                float p = i / 10f;
                Assert.AreEqual(p, PenaltyBubbleMath.Ease(PenaltyBubbleMath.InverseEase(p)), 0.001f);
            }
        }

        // ---------- Kapıyı geçme ----------

        [Test]
        public void HasPassedDoor_FlipsAtTheEasedDoorTime()
        {
            var path = PenaltyBubblePath.Build(new Vector2(0f, 0f), new Vector2(30f, 0f), new Vector2(120f, 0f));
            float doorTime = PenaltyBubbleMath.DoorTime(path);

            Assert.IsFalse(PenaltyBubbleMath.HasPassedDoor(path, doorTime - 0.02f));
            Assert.IsTrue(PenaltyBubbleMath.HasPassedDoor(path, doorTime + 0.02f));
        }

        [Test]
        public void HasPassedDoor_NoDoor_IsNeverTrue()
        {
            var path = PenaltyBubblePath.Build(Start, null, End);

            Assert.IsFalse(PenaltyBubbleMath.HasPassedDoor(path, 1f));
        }

        [Test]
        public void DoorTime_IsLaterThanDoorFraction_ForEarlyDoors()
        {
            // Başta yavaş başlandığı için yolun dörtte birine zamanın dörtte birinden geç varılır.
            var path = PenaltyBubblePath.Build(new Vector2(0f, 0f), new Vector2(30f, 0f), new Vector2(120f, 0f));

            Assert.Greater(PenaltyBubbleMath.DoorTime(path), path.DoorFraction);
        }

        // ---------- Salınım ----------

        [Test]
        public void Wobble_IsZeroAtStartAndEnd()
        {
            Vector2 dir = Vector2.right;

            Assert.AreEqual(0f, PenaltyBubbleMath.Wobble(dir, 0f, 4f, 0.25f, 1.2f).magnitude, 0.0001f);
            Assert.AreEqual(0f, PenaltyBubbleMath.Wobble(dir, 1f, 4f, 0.25f, 1.2f).magnitude, 0.0001f);
        }

        [Test]
        public void Wobble_StaysWithinAmplitude()
        {
            for (int i = 0; i <= 200; i++)
            {
                float m = PenaltyBubbleMath.Wobble(Vector2.right, i / 200f, 4f, 0.25f, 1.2f).magnitude;
                Assert.LessOrEqual(m, 0.25f + 0.0001f, $"i={i}");
            }
        }

        [Test]
        public void Wobble_IsPerpendicularToTravelDirection()
        {
            Vector2 w = PenaltyBubbleMath.Wobble(Vector2.right, 0.3f, 4f, 0.25f, 1.2f);

            Assert.AreEqual(0f, w.x, 0.0001f);
        }

        [Test]
        public void Wobble_ReachesAmplitudeSomewhere()
        {
            float max = 0f;
            for (int i = 0; i <= 400; i++)
                max = Mathf.Max(max, PenaltyBubbleMath.Wobble(Vector2.right, i / 400f, 4f, 0.25f, 1.2f).magnitude);

            Assert.Greater(max, 0.2f);
        }

        [Test]
        public void Wobble_ZeroDirection_IsZeroNotNaN()
        {
            Vector2 w = PenaltyBubbleMath.Wobble(Vector2.zero, 0.5f, 4f, 0.25f, 1.2f);

            Assert.IsFalse(float.IsNaN(w.x) || float.IsNaN(w.y));
        }

        // ---------- Kalkış ve şişme ----------

        [Test]
        public void LiftOffset_RisesToHeight_WithEaseOut()
        {
            Assert.AreEqual(0f, PenaltyBubbleMath.LiftOffset(0f, 1.5f), 0.0001f);
            Assert.AreEqual(1.5f, PenaltyBubbleMath.LiftOffset(1f, 1.5f), 0.0001f);
            Assert.Greater(PenaltyBubbleMath.LiftOffset(0.5f, 1.5f), 0.75f, "Ease-out: yarı sürede yarıdan fazlası.");
        }

        [Test]
        public void InflateScale_StartsAtZero_EndsAtOne_OvershootsInBetween()
        {
            Assert.AreEqual(0f, PenaltyBubbleMath.InflateScale(0f, 1.15f), 0.0001f);
            Assert.AreEqual(1f, PenaltyBubbleMath.InflateScale(1f, 1.15f), 0.0001f);

            float max = 0f;
            for (int i = 0; i <= 100; i++)
                max = Mathf.Max(max, PenaltyBubbleMath.InflateScale(i / 100f, 1.15f));
            Assert.AreEqual(1.15f, max, 0.01f);
        }

        [Test]
        public void InflateScale_NoOvershoot_NeverExceedsOne()
        {
            for (int i = 0; i <= 100; i++)
                Assert.LessOrEqual(PenaltyBubbleMath.InflateScale(i / 100f, 1f), 1.0001f, $"i={i}");
        }
    }
}
