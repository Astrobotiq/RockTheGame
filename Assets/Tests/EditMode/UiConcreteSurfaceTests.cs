using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace RockTheGame.Tests
{
    public class UiConcreteSurfaceTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void SurfaceColor_GoesFromPaperToToneWithBalance()
        {
            var theme = ScriptableObject.CreateInstance<UiThemeSO>();
            try
            {
                Assert.AreEqual(theme.Paper, UiConcreteSurface.SurfaceColor(theme, ConcreteTone.Wall, 0f));
                Assert.AreEqual(theme.ConcreteWall, UiConcreteSurface.SurfaceColor(theme, ConcreteTone.Wall, 1f));
                Assert.AreEqual(theme.ConcreteMid, UiConcreteSurface.SurfaceColor(theme, ConcreteTone.Mid, 0.7f), "0,625 üstünde tam beton");
                Assert.AreEqual(theme.ConcreteSlab, UiConcreteSurface.SurfaceColor(theme, ConcreteTone.Slab, 1f));
            }
            finally { Object.DestroyImmediate(theme); }
        }

        [Test]
        public void ThemeAsset_HasConcreteMaterialWithShader()
        {
            var theme = Resources.Load<UiThemeSO>(UiThemeSO.ResourcePath);
            Assert.IsNotNull(theme.ConcreteMaterial, "UiTheme'de ConcreteMaterial yok");
            Assert.AreEqual("RockTheGame/UI/Concrete", theme.ConcreteMaterial.shader.name);
            Assert.IsTrue(theme.ConcreteMaterial.shader.isSupported);
        }

        [Test]
        public void Apply_SetsMaterialBalanceAndConcreteColor()
        {
            _go = new GameObject("surface", typeof(RectTransform), typeof(Image));
            var surface = _go.AddComponent<UiConcreteSurface>();
            surface.Screen = UiScreen.Pause; // Apply'i tetikler
            var theme = UiThemeSO.Load();
            var image = _go.GetComponent<Image>();

            Assert.IsNotNull(surface.Instance, "materyal örneği oluşmadı");
            Assert.AreSame(surface.Instance, image.material);
            Assert.AreEqual(theme.BalanceFor(UiScreen.Pause), surface.Instance.GetFloat("_Balance"), 1e-6f);
            Assert.AreEqual(1f, surface.Instance.GetFloat("_Concrete"));
            Assert.AreEqual(UiConcreteSurface.SurfaceColor(theme, ConcreteTone.Wall, theme.BalanceFor(UiScreen.Pause)), image.color);
        }
    }
}
