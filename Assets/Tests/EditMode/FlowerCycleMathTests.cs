using New_Scripts.Flower;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class FlowerCycleMathTests
    {
        private static readonly FlowerCycleTimings Tm = FlowerCycleTimings.Default;   // 1.8 / 0.5 / 2.2 / 0.7 / 0.15

        [Test]
        public void PhasesFollowOneAnother()
        {
            Assert.That(FlowerCycleMath.Evaluate(0.5f, Tm).Phase, Is.EqualTo(FlowerPhase.Closed));
            Assert.That(FlowerCycleMath.Evaluate(1.9f, Tm).Phase, Is.EqualTo(FlowerPhase.Opening));
            Assert.That(FlowerCycleMath.Evaluate(3.0f, Tm).Phase, Is.EqualTo(FlowerPhase.Open));
            Assert.That(FlowerCycleMath.Evaluate(4.6f, Tm).Phase, Is.EqualTo(FlowerPhase.Warning));
            Assert.That(FlowerCycleMath.Evaluate(5.3f, Tm).Phase, Is.EqualTo(FlowerPhase.Closing));
        }

        [Test]
        public void Cycle_Wraps()
        {
            float len = Tm.CycleLength;
            Assert.That(FlowerCycleMath.Evaluate(len + 0.5f, Tm).Phase, Is.EqualTo(FlowerPhase.Closed));
            Assert.That(FlowerCycleMath.Evaluate(-0.1f, Tm).Phase, Is.EqualTo(FlowerPhase.Closing));
        }

        [Test]
        public void Closed_IsNotGrabbable()
        {
            Assert.That(FlowerCycleMath.Evaluate(0.5f, Tm).Grabbable, Is.False);
        }

        [Test]
        public void Opening_BecomesGrabbableAfterGrabAt()
        {
            Assert.That(FlowerCycleMath.Evaluate(1.8f + 0.1f, Tm).Grabbable, Is.False);
            Assert.That(FlowerCycleMath.Evaluate(1.8f + 0.35f, Tm).Grabbable, Is.True);
        }

        [Test]
        public void OpenAndWarning_AreGrabbable()
        {
            Assert.That(FlowerCycleMath.Evaluate(3.0f, Tm).Grabbable, Is.True);
            Assert.That(FlowerCycleMath.Evaluate(4.6f, Tm).Grabbable, Is.True);
        }

        [Test]
        public void Snap_IsGrabbableOnlyDuringCoyote()
        {
            float snapStart = Tm.closed + Tm.opening + Tm.open + Tm.warning;
            Assert.That(FlowerCycleMath.Evaluate(snapStart + 0.05f, Tm).Grabbable, Is.True);
            Assert.That(FlowerCycleMath.Evaluate(snapStart + 0.12f, Tm).Grabbable, Is.False);
        }

        [Test]
        public void Warning_RampsFromZeroToOne()
        {
            float warningStart = Tm.closed + Tm.opening + Tm.open;
            Assert.That(FlowerCycleMath.Evaluate(warningStart + 0.001f, Tm).Warn01, Is.EqualTo(0f).Within(0.01f));
            Assert.That(FlowerCycleMath.Evaluate(warningStart + Tm.warning * 0.5f, Tm).Warn01, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(FlowerCycleMath.Evaluate(3.0f, Tm).Warn01, Is.EqualTo(0f));
        }

        [Test]
        public void Hint_OnlyInLastPartOfClosedPhase()
        {
            Assert.That(FlowerCycleMath.Evaluate(0.5f, Tm).Hint01, Is.EqualTo(0f));
            Assert.That(FlowerCycleMath.Evaluate(1.8f - 0.2f, Tm).Hint01, Is.EqualTo(0.5f).Within(0.01f));
        }
    }
}
