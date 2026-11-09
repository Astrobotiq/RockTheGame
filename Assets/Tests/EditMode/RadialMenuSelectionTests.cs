using New_Scripts.Pause;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class RadialMenuSelectionTests
    {
        private static readonly bool[] All = { true, true, true, true };
        private static readonly bool[] NoOptions = { true, false, true, true };

        [Test]
        public void SliceAt_Cardinals_MapClockwiseFromUp()
        {
            Assert.AreEqual(0, RadialMenuSelection.SliceAt(Vector2.up, 4));
            Assert.AreEqual(1, RadialMenuSelection.SliceAt(Vector2.right, 4));
            Assert.AreEqual(2, RadialMenuSelection.SliceAt(Vector2.down, 4));
            Assert.AreEqual(3, RadialMenuSelection.SliceAt(Vector2.left, 4));
        }

        [Test]
        public void SliceAt_BoundaryBetweenSlices_IsAt45Degrees()
        {
            Assert.AreEqual(0, RadialMenuSelection.SliceAt(Dir(44f), 4));
            Assert.AreEqual(1, RadialMenuSelection.SliceAt(Dir(46f), 4));
            Assert.AreEqual(0, RadialMenuSelection.SliceAt(Dir(-44f), 4), "saat yönünün tersine 44°");
            Assert.AreEqual(3, RadialMenuSelection.SliceAt(Dir(-46f), 4));
        }

        [Test]
        public void SliceAt_InsideDeadzone_ReturnsNone()
        {
            Assert.AreEqual(-1, RadialMenuSelection.SliceAt(new Vector2(0.1f, 0.2f), 4, 0.5f));
            Assert.AreEqual(-1, RadialMenuSelection.SliceAt(Vector2.zero, 4, 0.5f));
        }

        [Test]
        public void SliceAt_MagnitudeDoesNotChangeSlice()
        {
            Assert.AreEqual(1, RadialMenuSelection.SliceAt(new Vector2(0.6f, 0f), 4, 0.5f));
            Assert.AreEqual(1, RadialMenuSelection.SliceAt(new Vector2(5f, 0f), 4, 0.5f));
        }

        [Test]
        public void SliceAt_WorksForOtherCounts()
        {
            Assert.AreEqual(0, RadialMenuSelection.SliceAt(Vector2.up, 3));
            Assert.AreEqual(1, RadialMenuSelection.SliceAt(Dir(120f), 3));
            Assert.AreEqual(2, RadialMenuSelection.SliceAt(Dir(240f), 3));
        }

        [Test]
        public void Pick_StickAtEnabledSlice_SelectsIt()
        {
            Assert.AreEqual(2, RadialMenuSelection.Pick(Vector2.down, 0, NoOptions));
        }

        [Test]
        public void Pick_StickAtDisabledSlice_KeepsCurrent()
        {
            Assert.AreEqual(0, RadialMenuSelection.Pick(Vector2.right, 0, NoOptions));
            Assert.AreEqual(3, RadialMenuSelection.Pick(Vector2.right, 3, NoOptions));
        }

        [Test]
        public void Pick_StickInDeadzone_KeepsCurrent()
        {
            Assert.AreEqual(2, RadialMenuSelection.Pick(Vector2.zero, 2, All));
        }

        [Test]
        public void Rotate_WrapsAndSkipsDisabled()
        {
            Assert.AreEqual(2, RadialMenuSelection.Rotate(0, +1, NoOptions), "options atlanır");
            Assert.AreEqual(3, RadialMenuSelection.Rotate(0, -1, NoOptions), "başa sarar");
            Assert.AreEqual(0, RadialMenuSelection.Rotate(3, +1, NoOptions), "sona sarar");
            Assert.AreEqual(0, RadialMenuSelection.Rotate(2, -1, NoOptions), "geriye giderken de atlar");
        }

        [Test]
        public void Rotate_NothingEnabled_KeepsCurrent()
        {
            Assert.AreEqual(1, RadialMenuSelection.Rotate(1, +1, new[] { false, false, false, false }));
        }

        [Test]
        public void Rotate_OnlyCurrentEnabled_KeepsCurrent()
        {
            Assert.AreEqual(2, RadialMenuSelection.Rotate(2, +1, new[] { false, false, true, false }));
        }

        [Test]
        public void SliceDirection_RoundTripsThroughSliceAt()
        {
            for (int count = 2; count <= 8; count++)
            for (int i = 0; i < count; i++)
                Assert.AreEqual(i, RadialMenuSelection.SliceAt(RadialMenuSelection.SliceDirection(i, count), count), $"count {count} slice {i}");
        }

        /// <summary>Saat yönünde, yukarıdan derece.</summary>
        private static Vector2 Dir(float degreesClockwiseFromUp)
        {
            float r = degreesClockwiseFromUp * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(r), Mathf.Cos(r));
        }
    }
}
