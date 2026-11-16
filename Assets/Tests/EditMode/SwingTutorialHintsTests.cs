using New_Scripts.Tutorial;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    /// <summary>İpucu görünürlük tablosu (docs/swing-tutorial-plani.md): 1. ipucu Slowing+Aim, 2. ipucu Fire.</summary>
    public class SwingTutorialHintsTests
    {
        [TestCase(SwingTutorialStep.Waiting, false, false)]
        [TestCase(SwingTutorialStep.Slowing, true, false)]
        [TestCase(SwingTutorialStep.Aim, true, false)]
        [TestCase(SwingTutorialStep.Fire, false, true)]
        [TestCase(SwingTutorialStep.Done, false, false)]
        public void VisibleHints_MatchesStepTable(SwingTutorialStep step, bool arm, bool fire)
        {
            SwingTutorialHints.VisibleHints(step, out bool a, out bool f);

            Assert.AreEqual(arm, a, "kolu çevir ipucu");
            Assert.AreEqual(fire, f, "ateşle ipucu");
        }
    }
}
