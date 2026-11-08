using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class UiThemeTests
    {
        [Test]
        public void BorderWidth_IsZeroBelowThreshold_AndScalesUp()
        {
            Assert.AreEqual(0f, UiBalance.BorderWidth(3f, 0.05f));
            Assert.AreEqual(3f * (0.35f + 0.65f * 0.55f), UiBalance.BorderWidth(3f, 0.55f), 1e-4f);
            Assert.AreEqual(3f, UiBalance.BorderWidth(3f, 1f), 1e-4f, "denge 1'de taban kalınlık");
        }

        [Test]
        public void ShadowOffset_IsWholePixels_AndZeroAtBauhaus()
        {
            Assert.AreEqual(0, UiBalance.ShadowOffset(4f, 0f));
            Assert.AreEqual(4, UiBalance.ShadowOffset(4f, 0.55f), "round(4*0,55*1,6)=round(3,52)");
            Assert.AreEqual(6, UiBalance.ShadowOffset(4f, 1f));
        }

        [Test]
        public void ExtrusionDepth_ScalesWithBalance()
        {
            Assert.AreEqual(0, UiBalance.ExtrusionDepth(8f, 0f));
            Assert.AreEqual(7, UiBalance.ExtrusionDepth(8f, 0.55f), "round(8*0,55*1,5)=round(6,6)");
            Assert.AreEqual(12, UiBalance.ExtrusionDepth(8f, 1f));
        }

        [Test]
        public void ConcreteBlend_ReachesFullConcreteAt0625()
        {
            Assert.AreEqual(0f, UiBalance.ConcreteBlend(0f));
            Assert.AreEqual(0.88f, UiBalance.ConcreteBlend(0.55f), 1e-4f);
            Assert.AreEqual(1f, UiBalance.ConcreteBlend(0.625f), 1e-4f);
            Assert.AreEqual(1f, UiBalance.ConcreteBlend(1f));
        }

        [Test]
        public void SurfaceDetails_AppearAtMockupThresholds()
        {
            Assert.IsFalse(UiBalance.HasTexture(0.12f));
            Assert.IsTrue(UiBalance.HasTexture(0.13f));
            Assert.IsFalse(UiBalance.HasHazardStripes(0.29f));
            Assert.IsTrue(UiBalance.HasHazardStripes(0.3f));
            Assert.IsFalse(UiBalance.HasFormworkLines(0.4f));
            Assert.IsTrue(UiBalance.HasFormworkLines(0.41f));
            Assert.IsFalse(UiBalance.HasTieHoles(0.6f));
            Assert.IsTrue(UiBalance.HasTieHoles(0.61f));
        }

        [Test]
        public void Balance_IsClamped()
        {
            Assert.AreEqual(0, UiBalance.ShadowOffset(4f, -5f));
            Assert.AreEqual(UiBalance.ShadowOffset(4f, 1f), UiBalance.ShadowOffset(4f, 5f));
        }

        [Test]
        public void Theme_DefaultsMatchPaletteDecision()
        {
            var t = ScriptableObject.CreateInstance<UiThemeSO>();
            try
            {
                Assert.AreEqual(0.55f, t.Balance, 1e-6f);
                Assert.AreEqual((Color)new Color32(0x14, 0x0C, 0x1C, 255), t.Ink);
                Assert.AreEqual((Color)new Color32(0xF1, 0xE6, 0xC7, 255), t.Paper);
                Assert.AreEqual((Color)new Color32(0xC9, 0xC6, 0xBF, 255), t.ConcreteWall, "beton tonları mockup'unki");
            }
            finally { Object.DestroyImmediate(t); }
        }

        [Test]
        public void ThemeAsset_FontsAreAssigned_AndCoverTurkishLetters()
        {
            var t = Resources.Load<UiThemeSO>(UiThemeSO.ResourcePath);
            Assert.IsNotNull(t, "Resources/UI/UiTheme.asset yok");
            foreach (var font in new[] { t.TitleFont, t.MonoFont, t.MonoBoldFont })
            {
                Assert.IsNotNull(font, "UiTheme'de font atanmamış");
                foreach (char c in "çÇğĞıİöÖşŞüÜ")
                    Assert.IsTrue(font.HasCharacter(c), $"{font.name} '{c}' içermiyor");
            }
        }

        [Test]
        public void BalanceFor_UsesScreenOverride_ElseGlobal()
        {
            var t = ScriptableObject.CreateInstance<UiThemeSO>();
            try
            {
                t.ScreenOverrides = new[] { new UiThemeSO.ScreenBalance { Screen = UiScreen.Hud, Balance = 0.2f } };
                Assert.AreEqual(0.2f, t.BalanceFor(UiScreen.Hud), 1e-6f);
                Assert.AreEqual(0.55f, t.BalanceFor(UiScreen.Pause), 1e-6f);
                Assert.AreEqual(UiBalance.ShadowOffset(4f, 0.2f), t.ShadowOffsetFor(UiScreen.Hud));
            }
            finally { Object.DestroyImmediate(t); }
        }
    }
}
