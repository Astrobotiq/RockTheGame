using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerFeedbackMathTests
    {
        [Test]
        public void TicksDue_NoWarningMeansNoTick()
        {
            Assert.AreEqual(0, FlowerFeedbackMath.TicksDue(0f, 4));
            Assert.AreEqual(0, FlowerFeedbackMath.TicksDue(0.5f, 0));
        }

        [Test]
        public void TicksDue_FirstTickRightAtWarnStart_AllPlayedBeforeTheEnd()
        {
            Assert.AreEqual(1, FlowerFeedbackMath.TicksDue(0.001f, 4));
            Assert.AreEqual(4, FlowerFeedbackMath.TicksDue(1f, 4));
            Assert.AreEqual(4, FlowerFeedbackMath.TicksDue(0.75f, 4), "son tık son çeyrekte");
        }

        [Test]
        public void TicksDue_IsMonotonicAndEvenlySpaced()
        {
            int prev = 0;
            var firstSeen = new float[5];
            for (int s = 1; s <= 100; s++)
            {
                float warn = s / 100f;
                int due = FlowerFeedbackMath.TicksDue(warn, 4);
                Assert.GreaterOrEqual(due, prev);
                if (due > prev) firstSeen[due] = warn;
                prev = due;
            }
            Assert.AreEqual(0.25f, firstSeen[2], 0.011f);
            Assert.AreEqual(0.5f, firstSeen[3], 0.011f);
            Assert.AreEqual(0.75f, firstSeen[4], 0.011f);
        }

        [Test]
        public void TickPitch_RisesFromMinToMax()
        {
            Assert.AreEqual(0.9f, FlowerFeedbackMath.TickPitch(0, 4, 0.9f, 1.4f), 1e-6f);
            Assert.AreEqual(1.4f, FlowerFeedbackMath.TickPitch(3, 4, 0.9f, 1.4f), 1e-6f);
            float prev = 0f;
            for (int i = 0; i < 4; i++)
            {
                float p = FlowerFeedbackMath.TickPitch(i, 4, 0.9f, 1.4f);
                Assert.Greater(p, prev);
                prev = p;
            }
            Assert.AreEqual(0.9f, FlowerFeedbackMath.TickPitch(0, 1, 0.9f, 1.4f), 1e-6f, "tek tık: min");
        }
    }
}
