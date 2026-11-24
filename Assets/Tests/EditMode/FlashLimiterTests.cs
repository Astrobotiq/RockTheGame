using New_Scripts.Platform;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlashLimiterTests
    {
        private static FlashLimiter Limiter() => new FlashLimiter(2, 1f, 0.3f);

        [Test]
        public void FirstTwoBursts_AreFullIntensity()
        {
            var l = Limiter();
            Assert.That(l.Acquire(10.0f), Is.EqualTo(1f));
            Assert.That(l.Acquire(10.2f), Is.EqualTo(1f));
        }

        [Test]
        public void ThirdBurstWithinOneSecond_IsReduced()
        {
            var l = Limiter();
            l.Acquire(10.0f);
            l.Acquire(10.2f);
            Assert.That(l.Acquire(10.4f), Is.EqualTo(0.3f).Within(1e-6f));
        }

        [Test]
        public void ReducedBursts_DoNotCountAgainstTheBudget()
        {
            var l = Limiter();
            l.Acquire(10.0f);
            l.Acquire(10.2f);
            l.Acquire(10.4f); // kısık
            l.Acquire(10.6f); // kısık
            // 10.0'daki tam flaş 1 sn sonra düştü; bütçeye yalnızca tam flaşlar yazılır
            Assert.That(l.Acquire(11.05f), Is.EqualTo(1f));
        }

        [Test]
        public void NeverMoreThanTwoFullFlashesInAnyOneSecondWindow()
        {
            var l = Limiter();
            var fullTimes = new System.Collections.Generic.List<float>();
            // 0.2 sn cooldown'lu pad'lerin en hızlı vuruş akışı: saniyede 5 patlama
            for (float t = 0f; t < 20f; t += 0.2f)
                if (l.Acquire(t) >= 1f) fullTimes.Add(t);

            for (int i = 0; i < fullTimes.Count; i++)
            {
                int inWindow = 0;
                for (int j = 0; j < fullTimes.Count; j++)
                    if (fullTimes[j] >= fullTimes[i] && fullTimes[j] < fullTimes[i] + 1f) inWindow++;
                Assert.That(inWindow, Is.LessThanOrEqualTo(2), $"t={fullTimes[i]}");
            }
        }

        [Test]
        public void AfterTheWindow_BudgetRefills()
        {
            var l = Limiter();
            l.Acquire(10.0f);
            l.Acquire(10.2f);
            Assert.That(l.Acquire(11.3f), Is.EqualTo(1f));
        }

        [Test]
        public void ClockGoingBackwards_ResetsTheBudget()
        {
            var l = Limiter();
            l.Acquire(100f);
            l.Acquire(100.2f);
            Assert.That(l.Acquire(0.5f), Is.EqualTo(1f));
        }
    }
}
