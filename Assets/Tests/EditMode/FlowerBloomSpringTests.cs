using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// FlowerBloom'un varsayılan yay katsayılarının (açılış 180/14,8, snap 900/48) hissettirmesi planlanan davranışı verdiğini
    /// ölçer: açılışta ~%8-20 aşım ve yarım saniyede yerleşme, anticipation'da küçük bir geri dip, snap'te çok kısa ve az aşımlı.
    /// </summary>
    public class FlowerBloomSpringTests
    {
        private const float OpenK = 180f, OpenC = 14.8f, CloseK = 900f, CloseC = 48f;
        private const float DipTarget = -0.2f, AnticipationTime = 0.12f;

        private static void Simulate(UiSpring s, float seconds, out float min, out float max, out float settleTime)
        {
            const float dt = 1f / 240f;
            min = s.Value;
            max = s.Value;
            settleTime = 0f;
            for (float t = 0f; t < seconds; t += dt)
            {
                s.Step(dt);
                min = Mathf.Min(min, s.Value);
                max = Mathf.Max(max, s.Value);
                if (Mathf.Abs(s.Value - s.Target) > 0.02f) settleTime = t + dt;
            }
        }

        [Test]
        public void OpenSpring_OvershootsALittleAndSettlesInHalfASecond()
        {
            var s = new UiSpring(OpenK, OpenC) { Value = 0f, Target = 1f };
            Simulate(s, 2f, out _, out float max, out float settle);
            Assert.That(max, Is.InRange(1.08f, 1.20f), "overshoot");
            Assert.That(settle, Is.LessThan(0.8f), "settle time");
        }

        [Test]
        public void OpenSpring_AnticipationDipsBelowZeroBrieflyThenOvershootsAsBefore()
        {
            // Hedef önce DipTarget'a çekilir, AnticipationTime sonra 1'e fırlar (FlowerBloom'daki sıra).
            var s = new UiSpring(OpenK, OpenC) { Value = 0f, Target = DipTarget };
            const float dt = 1f / 240f;
            float min = 0f, max = 0f;
            for (float t = 0f; t < 1.5f; t += dt)
            {
                if (t >= AnticipationTime) s.Target = 1f;
                s.Step(dt);
                min = Mathf.Min(min, s.Value);
                max = Mathf.Max(max, s.Value);
            }
            Assert.That(min, Is.InRange(-0.14f, -0.07f), "anticipation dip");
            Assert.That(max, Is.InRange(1.08f, 1.22f), "overshoot after the dip");
        }

        [Test]
        public void CloseSpring_SnapsShutQuicklyWithoutBouncing()
        {
            var s = new UiSpring(CloseK, CloseC) { Value = 1f, Target = 0f };
            Simulate(s, 1f, out float min, out _, out float settle);
            Assert.That(settle, Is.LessThan(0.3f), "snap time");
            Assert.That(min, Is.GreaterThan(-0.08f), "undershoot");
        }
    }
}
