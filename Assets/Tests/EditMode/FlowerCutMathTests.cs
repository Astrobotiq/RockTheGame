using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerCutMathTests
    {
        private static float Seed(int i, int n) => (i + 0.5f) / n;

        [Test]
        public void SameSeed_GivesSameCut()
        {
            var a = FlowerCutMath.ForSeed(0.37f);
            var b = FlowerCutMath.ForSeed(0.37f);
            Assert.That(b.Length, Is.EqualTo(a.Length));
            Assert.That(b.Width, Is.EqualTo(a.Width));
            Assert.That(b.TipSkew, Is.EqualTo(a.TipSkew));
        }

        [Test]
        public void AllCuts_StayInsideTheirLimits()
        {
            for (int i = 0; i < 2000; i++)
            {
                var c = FlowerCutMath.ForSeed(Seed(i, 2000));
                Assert.That(c.Length, Is.InRange(1f - FlowerCutMath.LengthJitter, 1f + FlowerCutMath.LengthJitter));
                Assert.That(c.Width, Is.InRange(1f - FlowerCutMath.WidthJitter, 1f + FlowerCutMath.WidthJitter));
                Assert.That(c.TipSkew, Is.InRange(-FlowerCutMath.TipSkewMax, FlowerCutMath.TipSkewMax));
            }
        }

        [Test]
        public void SeedOutOfRange_IsClamped()
        {
            var lo = FlowerCutMath.ForSeed(-5f);
            var hi = FlowerCutMath.ForSeed(5f);
            Assert.That(lo.Length, Is.InRange(1f - FlowerCutMath.LengthJitter, 1f + FlowerCutMath.LengthJitter));
            Assert.That(hi.Width, Is.InRange(1f - FlowerCutMath.WidthJitter, 1f + FlowerCutMath.WidthJitter));
        }

        [Test]
        public void MeanMultipliers_AreOne_AndSkewIsBalanced()
        {
            const int n = 4000;
            double length = 0, width = 0, skew = 0;
            for (int i = 0; i < n; i++)
            {
                var c = FlowerCutMath.ForSeed(Seed(i, n));
                length += c.Length;
                width += c.Width;
                skew += c.TipSkew;
            }

            Assert.That(length / n, Is.EqualTo(1.0).Within(0.01), "Ortalama boy çarpanı 1 olmalı (çiçek boyutu değişmesin).");
            Assert.That(width / n, Is.EqualTo(1.0).Within(0.01));
            Assert.That(skew / n, Is.EqualTo(0.0).Within(0.01), "Uçlar iki yana da eşit yatmalı.");
        }

        [Test]
        public void NeighbouringPetalSeeds_GiveDifferentCuts()
        {
            // FlowerBloom'un ürettiği tipik komşu seed'ler: birbirine yakın olmayan ama art arda gelen yapraklar.
            float[] seeds = { 0.12f, 0.55f, 0.83f, 0.31f, 0.67f };
            for (int i = 0; i < seeds.Length; i++)
            {
                for (int j = i + 1; j < seeds.Length; j++)
                {
                    var a = FlowerCutMath.ForSeed(seeds[i]);
                    var b = FlowerCutMath.ForSeed(seeds[j]);
                    bool differs = System.Math.Abs(a.Length - b.Length) > 1e-4f
                                   || System.Math.Abs(a.Width - b.Width) > 1e-4f
                                   || System.Math.Abs(a.TipSkew - b.TipSkew) > 1e-4f;
                    Assert.That(differs, Is.True, $"seed {seeds[i]} ve {seeds[j]} aynı kesimi verdi.");
                }
            }
        }

        [Test]
        public void Channels_AreNotLockedTogether()
        {
            // Üç değer aynı seed'den çıksa da biri diğerini belirlememeli: uzun yaprak hep geniş olmasın.
            int longAndWide = 0, longAndNarrow = 0;
            for (int i = 0; i < 1000; i++)
            {
                var c = FlowerCutMath.ForSeed(Seed(i, 1000));
                if (c.Length > 1f && c.Width > 1f) longAndWide++;
                if (c.Length > 1f && c.Width < 1f) longAndNarrow++;
            }

            Assert.That(longAndWide, Is.GreaterThan(150));
            Assert.That(longAndNarrow, Is.GreaterThan(150));
        }

        [Test]
        public void NoneCut_IsIdentity()
        {
            var c = FlowerCutMath.Cut.None;
            Assert.That(c.Length, Is.EqualTo(1f));
            Assert.That(c.Width, Is.EqualTo(1f));
            Assert.That(c.TipSkew, Is.EqualTo(0f));
        }
    }
}
