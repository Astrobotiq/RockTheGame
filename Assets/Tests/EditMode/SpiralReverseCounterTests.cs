using New_Scripts.Player.Nodes.Spiral;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Ters dönen spiralin sayacı: yalnızca spin yönündeki dönüşü sayar, eşikte true verir.
    /// </summary>
    public class SpiralReverseCounterTests
    {
        private static readonly Vector2 Anchor = Vector2.zero;

        private static Vector2 PositionAt(float degrees, float radius = 3f)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return Anchor + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        [Test]
        public void Tick_CompletesAfterDegreesInSpinDirection()
        {
            var counter = new SpiralReverseCounter(360f);
            Assert.IsFalse(counter.Tick(PositionAt(0f), Anchor, 1));
            for (int i = 1; i < 72; i++)
                Assert.IsFalse(counter.Tick(PositionAt(5f * i), Anchor, 1), $"adım {i}");
            Assert.IsTrue(counter.Tick(PositionAt(360f), Anchor, 1));
        }

        [Test]
        public void Tick_IgnoresMotionAgainstSpin()
        {
            var counter = new SpiralReverseCounter(360f);
            counter.Tick(PositionAt(0f), Anchor, 1);
            for (int i = 1; i <= 18; i++)
                Assert.IsFalse(counter.Tick(PositionAt(-5f * i), Anchor, 1));
            Assert.AreEqual(0f, counter.Progress, 0.0001f);

            for (int i = 1; i < 72; i++)
                Assert.IsFalse(counter.Tick(PositionAt(-90f + 5f * i), Anchor, 1), $"adım {i}");
            Assert.IsTrue(counter.Tick(PositionAt(270f), Anchor, 1));
        }

        [Test]
        public void Tick_ClockwiseSpin_CountsClockwiseMotion()
        {
            var counter = new SpiralReverseCounter(360f);
            counter.Tick(PositionAt(0f), Anchor, -1);
            for (int i = 1; i <= 36; i++)
                counter.Tick(PositionAt(-5f * i), Anchor, -1);
            Assert.AreEqual(0.5f, counter.Progress, 0.001f);
        }

        [Test]
        public void Reset_ClearsProgress()
        {
            var counter = new SpiralReverseCounter(360f);
            counter.Tick(PositionAt(0f), Anchor, 1);
            for (int i = 1; i <= 18; i++)
                counter.Tick(PositionAt(5f * i), Anchor, 1);
            Assert.Greater(counter.Progress, 0.2f);

            counter.Reset();

            Assert.AreEqual(0f, counter.Progress, 0.0001f);
            Assert.IsFalse(counter.Tick(PositionAt(200f), Anchor, 1), "Reset sonrası ilk Tick yalnızca yönü kaydeder");
            Assert.AreEqual(0f, counter.Progress, 0.0001f);
        }
    }
}
