using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerBloomMathTests
    {
        private const int Layers = 2;
        private const int Count = 8;

        private static float P(float open, int layer, int index) =>
            FlowerBloomMath.PetalProgress(open, layer, Layers, index, 0.25f, 0.15f);

        [Test]
        public void ClosedFlower_AllPetalsAtZero()
        {
            for (int l = 0; l < Layers; l++)
                for (int i = 0; i < Count; i++)
                    Assert.That(P(0f, l, i), Is.EqualTo(0f).Within(1e-6f));
        }

        [Test]
        public void FullyOpenFlower_AllPetalsAtOne()
        {
            for (int l = 0; l < Layers; l++)
                for (int i = 0; i < Count; i++)
                    Assert.That(P(1f, l, i), Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void InnerLayer_OpensAfterOuterLayer()
        {
            Assert.That(P(0.5f, 0, 0), Is.GreaterThan(P(0.5f, 1, 0)));
        }

        [Test]
        public void Closing_InnerLayerStartsClosingFirst()
        {
            // open 1'den düşerken iç katman (gecikmeli) ilk eksilen olur, dış katman hâlâ tam açıktır.
            Assert.That(P(0.8f, 0, 0), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(P(0.8f, 1, 0), Is.LessThan(1f));
        }

        [Test]
        public void SamePetal_ProgressIsMonotonic()
        {
            float prev = 0f;
            for (float o = 0f; o <= 1f; o += 0.05f)
            {
                float v = P(o, 1, 5);
                Assert.That(v, Is.GreaterThanOrEqualTo(prev - 1e-6f));
                prev = v;
            }
        }

        [Test]
        public void HugeDelays_StillReachFullyOpen()
        {
            Assert.That(FlowerBloomMath.PetalProgress(1f, 3, 4, 7, 0.6f, 0.5f), Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void NeighbouringPetals_AreNotInLockstep()
        {
            // Altın oran adımı: komşu yapraklar aynı sırayı paylaşmaz.
            Assert.That(FlowerBloomMath.PetalRank(0), Is.Not.EqualTo(FlowerBloomMath.PetalRank(1)).Within(0.05f));
            Assert.That(FlowerBloomMath.PetalRank(1), Is.Not.EqualTo(FlowerBloomMath.PetalRank(2)).Within(0.05f));
        }

        [Test]
        public void PetalRank_StaysInUnitRange()
        {
            for (int i = 0; i < 40; i++)
                Assert.That(FlowerBloomMath.PetalRank(i), Is.InRange(0f, 1f));
        }
    }
}
