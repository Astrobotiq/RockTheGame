using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class RisoSlabMathTests
    {
        [Test]
        public void SmoothVelocity_OneTimeConstant_ReachesAbout63Percent()
        {
            Vector2 v = RisoSlabMath.SmoothVelocity(Vector2.zero, new Vector2(10f, 0f), 0.1f, 0.1f);
            Assert.That(v.x, Is.EqualTo(10f * (1f - Mathf.Exp(-1f))).Within(1e-4f));
        }

        [Test]
        public void SmoothVelocity_FrameRateIndependent()
        {
            Vector2 target = new Vector2(8f, -4f);
            Vector2 coarse = RisoSlabMath.SmoothVelocity(Vector2.zero, target, 0.1f, 0.1f);
            Vector2 fine = Vector2.zero;
            for (int i = 0; i < 10; i++) fine = RisoSlabMath.SmoothVelocity(fine, target, 0.01f, 0.1f);
            Assert.That(fine.x, Is.EqualTo(coarse.x).Within(1e-3f));
            Assert.That(fine.y, Is.EqualTo(coarse.y).Within(1e-3f));
        }

        [Test]
        public void SmoothVelocity_PlatformStops_ReturnsToZeroWithin150ms()
        {
            // Uçta ani duruş: hedef 0. 0.15 sn sonra başlangıç hızının ~%22'si kalır, 0.3 sn sonra ~%5.
            Vector2 v = new Vector2(8f, 0f);
            for (int i = 0; i < 15; i++) v = RisoSlabMath.SmoothVelocity(v, Vector2.zero, 0.01f, 0.1f);
            Assert.That(v.x, Is.LessThan(8f * 0.25f));
        }

        [Test]
        public void SmoothVelocity_ZeroSmoothTime_SnapsToTarget()
        {
            Vector2 v = RisoSlabMath.SmoothVelocity(new Vector2(3f, 3f), new Vector2(1f, 2f), 0.016f, 0f);
            Assert.That(v, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void SmoothVelocity_ZeroDelta_KeepsCurrent()
        {
            Vector2 v = RisoSlabMath.SmoothVelocity(new Vector2(3f, 3f), Vector2.zero, 0f, 0.1f);
            Assert.That(v, Is.EqualTo(new Vector2(3f, 3f)));
        }
    }
}
