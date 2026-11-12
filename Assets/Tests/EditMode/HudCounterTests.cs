using New_Scripts.UI.Hud;
using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class UiPixelScaleTests
    {
        [TestCase(1920, 1080, 4)]
        [TestCase(2560, 1440, 5)]
        [TestCase(3840, 2160, 8)]
        [TestCase(1280, 720, 2)]
        [TestCase(1366, 768, 2)]
        [TestCase(1920, 1200, 4, TestName = "16:10: genişlik 4, yükseklik 4")]
        [TestCase(2560, 1080, 4, TestName = "ultrawide: yükseklik sınırlar")]
        [TestCase(800, 600, 1, TestName = "küçük pencere: en az 1")]
        [TestCase(960, 1080, 2, TestName = "dar pencere: genişlik sınırlar")]
        public void ArtScaleFor_PicksLargestIntegerThatFits(int w, int h, int expected)
        {
            Assert.AreEqual(expected, UiPixelScale.ArtScaleFor(w, h));
        }

        [Test]
        public void ScaleFactor_KeepsFourCanvasUnitsPerArtPixel()
        {
            // 1080p: 4 ekran pikseli / sanat pikseli, 4 canvas birimi → çarpan 1.
            Assert.AreEqual(1f, UiPixelScale.ScaleFactorFor(4), 1e-6f);
            // 1440p: 5 ekran pikseli / sanat pikseli → 1 canvas birimi = 1,25 ekran pikseli.
            Assert.AreEqual(1.25f, UiPixelScale.ScaleFactorFor(5), 1e-6f);
        }
    }

    public class HudCounterLayoutTests
    {
        private const float IconSize = 24f; // sanat pikseli, ikon dosyası

        [Test]
        public void KeyCenters_AreInsideTheQuarterCircle()
        {
            for (int i = 0; i < HudCounterPanel.MaxKeys; i++)
            {
                Vector2 c = HudCounterPanel.KeyCenter(i);
                // İkonun en uzak köşesi (görünür çizim 18 piksel: gölge ve boşluk hariç) daireyi aşmasın.
                float far = (c + Vector2.one * (IconSize * 0.375f)).magnitude;
                Assert.LessOrEqual(far, HudCounterPanel.CircleRadius, $"anahtar {i} daire dışında");
                Assert.GreaterOrEqual(c.x - IconSize * 0.375f, 0f, $"anahtar {i} ekranın solundan taşıyor");
            }
        }

        [Test]
        public void KeyCenters_DoNotOverlapEachOther()
        {
            for (int i = 0; i < HudCounterPanel.MaxKeys - 1; i++)
            {
                float distance = Vector2.Distance(HudCounterPanel.KeyCenter(i), HudCounterPanel.KeyCenter(i + 1));
                Assert.GreaterOrEqual(distance, 18f, $"anahtar {i} ile {i + 1} çakışıyor (görünür ikon 18 piksel)");
            }
        }

        [Test]
        public void KeyCenters_StayBelowTheCounterRow()
        {
            // Sayaç satırı (ananas + yazı) y ≈ 4..30 sanat pikseli; anahtarlar onun altında başlar.
            for (int i = 0; i < HudCounterPanel.MaxKeys; i++)
                Assert.GreaterOrEqual(HudCounterPanel.KeyCenter(i).y - IconSize * 0.375f, 28f, $"anahtar {i} sayaç satırına biniyor");
        }
    }
}
