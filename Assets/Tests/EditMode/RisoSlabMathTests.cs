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

        // ---- Oyuncu teması (aşama 2) ----

        [Test]
        public void StepInfluence_Landing_JumpsToOne()
        {
            float a = RisoSlabMath.StepInfluence(0f, true, true, 0f, 0.15f, 0.25f);
            Assert.That(a, Is.EqualTo(1f));
        }

        [Test]
        public void StepInfluence_AfterLanding_SettlesToStandingLevel()
        {
            float a = 1f;
            for (int i = 0; i < 200; i++) a = RisoSlabMath.StepInfluence(a, true, false, 0.016f, 0.15f, 0.25f);
            Assert.That(a, Is.EqualTo(0.15f).Within(1e-3f));
        }

        [Test]
        public void StepInfluence_LeavingPlatform_FadesToZero()
        {
            float a = 0.15f;
            for (int i = 0; i < 200; i++) a = RisoSlabMath.StepInfluence(a, false, false, 0.016f, 0.15f, 0.25f);
            Assert.That(a, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void StepInfluence_NoContact_StaysZero()
        {
            Assert.That(RisoSlabMath.StepInfluence(0f, false, false, 0.016f, 0.15f, 0.25f), Is.EqualTo(0f));
        }

        [Test]
        public void StepInfluence_LandingWhileFading_RestartsFromOne()
        {
            float a = RisoSlabMath.StepInfluence(0.05f, true, true, 0.016f, 0.15f, 0.25f);
            Assert.That(a, Is.GreaterThan(0.9f));
        }

        [Test]
        public void StepInfluence_ZeroDecayTime_SnapsToTarget()
        {
            Assert.That(RisoSlabMath.StepInfluence(1f, true, false, 0.016f, 0.15f, 0f), Is.EqualTo(0.15f));
        }

        // ---- Mürekkep değişimi dalgası ----

        [Test]
        public void AdvanceFront_Inactive_StaysZero()
        {
            Assert.That(RisoSlabMath.AdvanceFront(0f, 0.1f, 6f, 8f, 1.2f), Is.EqualTo(0f));
        }

        [Test]
        public void AdvanceFront_GrowsAtSpeed()
        {
            Assert.That(RisoSlabMath.AdvanceFront(1f, 0.5f, 6f, 8f, 1.2f), Is.EqualTo(4f).Within(1e-4f));
        }

        [Test]
        public void AdvanceFront_PastMaxPlusBand_TurnsOff()
        {
            Assert.That(RisoSlabMath.AdvanceFront(9f, 0.1f, 6f, 8f, 1.2f), Is.EqualTo(0f));
            Assert.That(RisoSlabMath.AdvanceFront(8f, 0.05f, 6f, 8f, 1.2f), Is.GreaterThan(0f));
        }

        [Test]
        public void IsInShiftBand_OnlyInsideBand()
        {
            Assert.That(RisoSlabMath.IsInShiftBand(3.5f, 4f, 1.2f), Is.True);
            Assert.That(RisoSlabMath.IsInShiftBand(4.5f, 4f, 1.2f), Is.False); // dalga henüz gelmedi
            Assert.That(RisoSlabMath.IsInShiftBand(2.0f, 4f, 1.2f), Is.False); // dalga geçti, eski renk
            Assert.That(RisoSlabMath.IsInShiftBand(1f, 0f, 1.2f), Is.False);   // kapalı
        }
    }
}
