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

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void KeyCenters_AreInsideTheOuterRing(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 c = HudCounterPanel.KeyCenter(i, count); // y aşağı, sol-üst köşeden
                // Görünür çizim 18 piksel (gölge ve boşluk hariç); köşesi bile halka bandının içinde kalsın.
                float half = IconSize * 0.375f;
                float corner = half * Mathf.Sqrt(2f);
                Assert.GreaterOrEqual(c.magnitude - corner, HudCounterPanel.CircleRadius, $"anahtar {i}/{count} ananas dairesine biniyor");
                Assert.LessOrEqual(c.magnitude + corner, HudCounterPanel.RingOuterRadius, $"anahtar {i}/{count} halkanın dışında");
                Assert.GreaterOrEqual(c.x - half, 0f, $"anahtar {i}/{count} ekranın solundan taşıyor");
                Assert.GreaterOrEqual(c.y - half, 0f, $"anahtar {i}/{count} ekranın üstünden taşıyor");
            }
        }

        [Test]
        public void KeyCenters_DoNotOverlapEachOther()
        {
            for (int i = 0; i < HudCounterPanel.MaxKeys - 1; i++)
            {
                float distance = Vector2.Distance(
                    HudCounterPanel.KeyCenter(i, HudCounterPanel.MaxKeys), HudCounterPanel.KeyCenter(i + 1, HudCounterPanel.MaxKeys));
                Assert.GreaterOrEqual(distance, 18f, $"anahtar {i} ile {i + 1} çakışıyor (görünür ikon 18 piksel)");
            }
        }

        [Test]
        public void KeyCenters_AreSymmetricAroundTheDiagonal()
        {
            // 45° köşegen etrafında simetrik: her noktanın x'i ile y'si yer değiştirince toplam aynı kalır.
            for (int count = 1; count <= HudCounterPanel.MaxKeys; count++)
            {
                float sum = 0f;
                for (int i = 0; i < count; i++)
                {
                    Vector2 c = HudCounterPanel.KeyCenter(i, count);
                    sum += c.x - c.y;
                }
                Assert.AreEqual(0f, sum, 1e-3f, $"{count} anahtar köşegen etrafında ortalı değil");
            }
        }

        // 1080p: sanat ölçeği 4; ekran noktası sol-alt orijinli, köşe sol-üst (0, 1080).
        [TestCase(237f, 1011f, true, TestName = "oda 11'e düşerken: halkanın bandında")]
        [TestCase(0f, 1080f, true, TestName = "tam köşe")]
        [TestCase(553f, 695f, false, TestName = "oda 11 Start: temiz")]
        [TestCase(960f, 540f, false, TestName = "ekran ortası")]
        public void IsBehind_RingRadius_At1080p(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, HudCounterPanel.IsBehind(new Vector2(x, y), 1080f, 4f, HudCounterPanel.RingOuterRadius));
        }

        [Test]
        public void IsBehind_UsesMarginAndArtScale()
        {
            // Yarıçap + pay sanat pikseli kadar uzak: ölçek 4'te tam sınır içeride, bir piksel ötesi dışarıda.
            float reach = (HudCounterPanel.CircleRadius + HudCounterPanel.FadeMargin) * 4f;
            Assert.IsTrue(HudCounterPanel.IsBehind(new Vector2(reach, 1080f), 1080f, 4f, HudCounterPanel.CircleRadius));
            Assert.IsFalse(HudCounterPanel.IsBehind(new Vector2(reach + 1f, 1080f), 1080f, 4f, HudCounterPanel.CircleRadius));
            // Aynı ekran noktası ölçek 2'de (daha küçük HUD) dışarıda kalır.
            Assert.IsFalse(HudCounterPanel.IsBehind(new Vector2(reach, 1080f), 1080f, 2f, HudCounterPanel.CircleRadius));
        }

        [Test]
        public void OuterRing_IsWiderThanTheCounterCircle()
        {
            Assert.Greater(HudCounterPanel.RingOuterRadius, HudCounterPanel.CircleRadius + IconSize * 0.75f,
                "halka bandı 18 piksellik anahtara yetmiyor");
        }
    }
}
