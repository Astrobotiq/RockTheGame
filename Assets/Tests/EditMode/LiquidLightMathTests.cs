using New_Scripts.Platform;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class LiquidLightMathTests
    {
        [Test]
        public void Envelope_NeverHit_IsZero()
        {
            Assert.That(LiquidLightMath.Envelope(-100f, 0.03f, 0.35f), Is.EqualTo(0f));
        }

        [Test]
        public void Envelope_RisesLinearlyThenPeaksAtOne()
        {
            Assert.That(LiquidLightMath.Envelope(0f, 0.04f, 0.35f), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(LiquidLightMath.Envelope(0.02f, 0.04f, 0.35f), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(LiquidLightMath.Envelope(0.04f, 0.04f, 0.35f), Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void Envelope_DecaysMonotonically_ToNearZero()
        {
            float previous = 1f;
            for (float age = 0.05f; age < 3f; age += 0.05f)
            {
                float e = LiquidLightMath.Envelope(age, 0.04f, 0.35f);
                Assert.That(e, Is.LessThanOrEqualTo(previous));
                previous = e;
            }
            Assert.That(previous, Is.LessThan(0.01f));
        }

        [Test]
        public void Envelope_ZeroAttack_StartsAtOne()
        {
            Assert.That(LiquidLightMath.Envelope(0f, 0f, 0.35f), Is.EqualTo(1f));
        }

        [Test]
        public void AdvanceFlow_IdleVsHit()
        {
            Assert.That(LiquidLightMath.AdvanceFlow(1f, 0.5f, 0.6f, 5f, 0f), Is.EqualTo(1.3f).Within(1e-6f));
            Assert.That(LiquidLightMath.AdvanceFlow(1f, 0.5f, 0.6f, 5f, 1f), Is.EqualTo(3.8f).Within(1e-6f));
        }

        [Test]
        public void BurstSymmetry_MapsFourInstrumentsToThreeFourFiveSix()
        {
            Assert.That(LiquidLightMath.BurstSymmetry(0), Is.EqualTo(3));
            Assert.That(LiquidLightMath.BurstSymmetry(1), Is.EqualTo(4));
            Assert.That(LiquidLightMath.BurstSymmetry(2), Is.EqualTo(5));
            Assert.That(LiquidLightMath.BurstSymmetry(3), Is.EqualTo(6));
        }

        [Test]
        public void BurstSymmetry_WrapsAndHandlesNegatives()
        {
            Assert.That(LiquidLightMath.BurstSymmetry(4), Is.EqualTo(3));
            Assert.That(LiquidLightMath.BurstSymmetry(-1), Is.EqualTo(6));
        }

        [Test]
        public void BurstEnvelope_RunsAtThreeQuartersOfRealTime()
        {
            // yaş 0.04/0.75 sn'de zirve (attack 0.04), sonra söner
            Assert.That(LiquidLightMath.BurstEnvelope(0.04f / 0.75f, 0.04f, 0.35f), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(LiquidLightMath.BurstEnvelope(1.3f, 0.04f, 0.35f), Is.LessThan(0.2f));
        }

        [Test]
        public void HueShift_BasePaletteHue_IsZero()
        {
            Assert.That(LiquidLightMath.HueShiftDegrees(LiquidLightMath.BasePaletteHue / 360f), Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void HueShift_StaysInHalfTurn()
        {
            for (float h = 0f; h <= 1f; h += 0.01f)
                Assert.That(LiquidLightMath.HueShiftDegrees(h), Is.InRange(-180f, 180f));
        }

        [Test]
        public void WrapDegrees_WrapsBothDirections()
        {
            Assert.That(LiquidLightMath.WrapDegrees(190f), Is.EqualTo(-170f).Within(1e-3f));
            Assert.That(LiquidLightMath.WrapDegrees(-190f), Is.EqualTo(170f).Within(1e-3f));
            Assert.That(LiquidLightMath.WrapDegrees(720f), Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void SmoothHue_TakesTheShortWayAroundTheCircle()
        {
            // 170'ten -170'e: kısa yol +20 derece (artış), -340 değil.
            float next = LiquidLightMath.SmoothHue(170f, -170f, 0.05f, 10f);
            Assert.That(next, Is.GreaterThan(170f).Or.LessThan(-170f + 1f));
            Assert.That(LiquidLightMath.WrapDegrees(next - 170f), Is.EqualTo(10f).Within(1e-3f));
        }

        [Test]
        public void SmoothHue_LargeStep_ClampsToTarget()
        {
            Assert.That(LiquidLightMath.SmoothHue(0f, 90f, 1f, 10f), Is.EqualTo(90f).Within(1e-3f));
        }
    }
}
