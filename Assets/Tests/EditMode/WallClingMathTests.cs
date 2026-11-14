using New_Scripts.Player;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Duvara yapisma hesabi (docs/systems/wall-movement.md "Duvara yapisma").
    /// </summary>
    public class WallClingMathTests
    {
        private const float Dt = 0.02f;
        private const float Offset = 0.02f;
        private const float Tolerance = 0.15f;

        [Test]
        public void ClosingSpeed_GapWithinTolerance_ClosesGapInOneStep()
        {
            float speed = WallClingMath.ClosingSpeed(0.08f, Offset, Tolerance, Dt);
            Assert.AreEqual(0.06f, speed * Dt, 1e-5f);
        }

        [Test]
        public void ClosingSpeed_GapBeyondTolerance_IsZero()
        {
            Assert.AreEqual(0f, WallClingMath.ClosingSpeed(0.3f, Offset, Tolerance, Dt));
        }

        [Test]
        public void ClosingSpeed_AlreadyAtSafetyOffset_IsZero()
        {
            Assert.AreEqual(0f, WallClingMath.ClosingSpeed(Offset, Offset, Tolerance, Dt));
        }

        [Test]
        public void ClosingSpeed_Penetrating_IsZero()
        {
            Assert.AreEqual(0f, WallClingMath.ClosingSpeed(-0.01f, Offset, Tolerance, Dt));
        }
    }
}
