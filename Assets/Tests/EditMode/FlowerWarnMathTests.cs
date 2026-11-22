using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerWarnMathTests
    {
        private const int N = 5;

        [Test]
        public void Fold_NothingFoldedWhenWarnIsZero_EverythingFoldedAtOne()
        {
            for (int i = 0; i < N; i++)
            {
                Assert.That(FlowerWarnMath.Fold(0f, i, N, 0.5f), Is.EqualTo(0f).Within(1e-6f), $"yaprak {i} başta açık");
                Assert.That(FlowerWarnMath.Fold(1f, i, N, 0.5f), Is.EqualTo(1f).Within(1e-6f), $"yaprak {i} sonda katlı");
            }
        }

        [Test]
        public void Fold_PetalsFoldInIndexOrder()
        {
            // Her anda daha düşük indeksli yaprak daha çok (ya da eşit) katlanmış olmalı.
            for (int s = 0; s <= 40; s++)
            {
                float warn = s / 40f;
                for (int i = 1; i < N; i++)
                    Assert.That(FlowerWarnMath.Fold(warn, i - 1, N, 0.5f), Is.GreaterThanOrEqualTo(FlowerWarnMath.Fold(warn, i, N, 0.5f) - 1e-6f),
                        $"warn {warn:F2}: yaprak {i - 1} yaprak {i}'den önce katlanmalı");
            }
        }

        [Test]
        public void Fold_IsMonotonicInWarn()
        {
            for (int i = 0; i < N; i++)
            {
                float prev = 0f;
                for (int s = 0; s <= 40; s++)
                {
                    float f = FlowerWarnMath.Fold(s / 40f, i, N, 0.5f);
                    Assert.That(f, Is.GreaterThanOrEqualTo(prev - 1e-6f));
                    prev = f;
                }
            }
        }

        [Test]
        public void Fold_CountReadsAsRemainingTime()
        {
            // Ardışık pencerelerde (overlap 0) uyarının i/N'inde tam i yaprak katlıdır: sayaç okunur.
            for (int k = 1; k < N; k++)
            {
                float warn = k / (float)N;
                int folded = 0;
                for (int i = 0; i < N; i++)
                    if (FlowerWarnMath.Fold(warn, i, N, 0f) > 0.99f) folded++;
                Assert.AreEqual(k, folded, $"warn {warn:F2}");
            }
        }

        [Test]
        public void Fold_SinglePetalFoldsOverWholeWarning()
        {
            Assert.That(FlowerWarnMath.Fold(0f, 0, 1, 0.5f), Is.EqualTo(0f));
            Assert.That(FlowerWarnMath.Fold(0.5f, 0, 1, 0.5f), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(FlowerWarnMath.Fold(1f, 0, 1, 0.5f), Is.EqualTo(1f));
        }

        [Test]
        public void StartBurst_FullAtStartFadesToZero()
        {
            Assert.That(FlowerWarnMath.StartBurst(0f, 0.2f), Is.EqualTo(1f));
            Assert.That(FlowerWarnMath.StartBurst(0.1f, 0.2f), Is.EqualTo(0.25f).Within(1e-6f));
            Assert.That(FlowerWarnMath.StartBurst(0.2f, 0.2f), Is.EqualTo(0f));
            Assert.That(FlowerWarnMath.StartBurst(-1f, 0.2f), Is.EqualTo(0f), "darbe yokken (age < 0) sıfır");
            Assert.That(FlowerWarnMath.StartBurst(0.05f, 0f), Is.EqualTo(0f), "süre 0 ise darbe yok");
        }
    }
}
