using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerWarnMathTests
    {
        [Test]
        public void Blink_StartsOn_AndTogglesEveryHalfPeriod()
        {
            Assert.That(FlowerWarnMath.BlinkOn(0f, 2f), Is.True, "uyarı başlar başlamaz halka görünür");
            Assert.That(FlowerWarnMath.BlinkOn(0.24f, 2f), Is.True);
            Assert.That(FlowerWarnMath.BlinkOn(0.26f, 2f), Is.False);
            Assert.That(FlowerWarnMath.BlinkOn(0.49f, 2f), Is.False);
            Assert.That(FlowerWarnMath.BlinkOn(0.51f, 2f), Is.True, "periyot 0.5 sn");
        }

        [Test]
        public void Blink_IsPeriodic()
        {
            for (float t = 0f; t < 0.5f; t += 0.01f)
                Assert.That(FlowerWarnMath.BlinkOn(t + 1f, 2f), Is.EqualTo(FlowerWarnMath.BlinkOn(t, 2f)), $"t={t}");
        }

        [Test]
        public void Blink_NeverFlashesMoreThanThreeTimesPerSecond()
        {
            // 10 Hz istense bile 3 Hz'e kırpılır: bir saniyede en fazla 3 yanma başlangıcı.
            int risingEdges = 0;
            bool prev = FlowerWarnMath.BlinkOn(0f, 10f);
            for (int i = 1; i <= 10000; i++)
            {
                bool on = FlowerWarnMath.BlinkOn(i * 0.0001f, 10f);
                if (on && !prev) risingEdges++;
                prev = on;
            }

            Assert.That(risingEdges, Is.LessThanOrEqualTo((int)FlowerWarnMath.MaxBlinkHz));
        }

        [Test]
        public void ZeroOrNegativeHz_MeansSteadyOn()
        {
            Assert.That(FlowerWarnMath.BlinkOn(0.3f, 0f), Is.True);
            Assert.That(FlowerWarnMath.BlinkOn(0.3f, -1f), Is.True);
        }

        [Test]
        public void Ring_IsVisibleOnlyDuringWarn_AndNotOverTheOpenWave()
        {
            Assert.That(FlowerWarnMath.RingVisible(0f, false), Is.False);
            Assert.That(FlowerWarnMath.RingVisible(0.0005f, false), Is.False);
            Assert.That(FlowerWarnMath.RingVisible(0.5f, false), Is.True);
            Assert.That(FlowerWarnMath.RingVisible(1f, false), Is.True);
            Assert.That(FlowerWarnMath.RingVisible(1f, true), Is.False, "açılış dalgası aynı renderer'ı kullanıyor");
        }

        [Test]
        public void RingRadius_IsAFractionOfTheQuad_Clamped()
        {
            Assert.That(FlowerWarnMath.RingRadius01(1.5f, 3f), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(FlowerWarnMath.RingRadius01(5f, 3f), Is.EqualTo(1f));
            Assert.That(FlowerWarnMath.RingRadius01(-1f, 3f), Is.EqualTo(0f));
            Assert.That(FlowerWarnMath.RingRadius01(1f, 0f), Is.EqualTo(0f));
        }
    }
}
