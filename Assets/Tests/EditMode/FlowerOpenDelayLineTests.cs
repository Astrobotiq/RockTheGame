using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerOpenDelayLineTests
    {
        private static FlowerOpenDelayLine Ramp(int capacity = 128)
        {
            // 0..1 saniye boyunca değer = zaman (10 ms adım).
            var line = new FlowerOpenDelayLine(capacity);
            line.Reset(0f, 0f);
            for (int i = 1; i <= 100; i++) line.Push(i * 0.01f, i * 0.01f);
            return line;
        }

        [Test]
        public void ZeroDelayReturnsNewestValue()
        {
            Assert.That(Ramp().Sample(1f), Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void DelayedSampleReadsEarlierValue()
        {
            Assert.That(Ramp().Sample(1f - 0.12f), Is.EqualTo(0.88f).Within(1e-4f));
        }

        [Test]
        public void SampleBetweenPointsInterpolates()
        {
            Assert.That(Ramp().Sample(0.555f), Is.EqualTo(0.555f).Within(1e-4f));
        }

        [Test]
        public void BeforeOldestReturnsOldest_AfterNewestReturnsNewest()
        {
            var line = new FlowerOpenDelayLine(8);
            line.Reset(5f, 2f);
            line.Push(5.1f, 3f);
            Assert.That(line.Sample(0f), Is.EqualTo(2f));
            Assert.That(line.Sample(9f), Is.EqualTo(3f));
        }

        [Test]
        public void SmallBufferOverwritesOldest()
        {
            var line = new FlowerOpenDelayLine(4);
            line.Reset(0f, 0f);
            for (int i = 1; i <= 10; i++) line.Push(i, i);   // yalnız 7..10 kalır
            Assert.That(line.Sample(8.5f), Is.EqualTo(8.5f).Within(1e-5f));
            Assert.That(line.Sample(2f), Is.EqualTo(7f), "eskisi silindi, en eski örnek döner");
        }

        [Test]
        public void ResetForgetsHistory()
        {
            var line = Ramp();
            line.Reset(10f, 0.25f);
            Assert.That(line.Sample(9f), Is.EqualTo(0.25f));
            Assert.That(line.Sample(11f), Is.EqualTo(0.25f));
        }

        [Test]
        public void SameTimePushOverwritesInsteadOfGrowing()
        {
            var line = new FlowerOpenDelayLine(4);
            line.Reset(0f, 0f);
            line.Push(1f, 5f);
            line.Push(1f, 7f);
            Assert.That(line.Sample(1f), Is.EqualTo(7f));
            Assert.That(line.Sample(0.5f), Is.EqualTo(3.5f).Within(1e-5f));
        }
    }
}
