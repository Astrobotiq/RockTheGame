using New_Scripts.Player.Visual;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class StaminaFlashStateTests
    {
        private const float Threshold = 1.5f;
        private const float HzStart = 7f;
        private const float HzZero = 12f;

        private static bool Step(StaminaFlashState s, float dt, float remaining, float threshold = Threshold)
            => s.Step(dt, remaining, threshold, HzStart, HzZero);

        [Test]
        public void FlashHz_AtThreshold_IsStartHz()
        {
            Assert.That(StaminaFlashState.FlashHz(1.5f, Threshold, HzStart, HzZero), Is.EqualTo(7f).Within(0.001f));
            Assert.That(StaminaFlashState.FlashHz(0f, Threshold, HzStart, HzZero), Is.EqualTo(12f).Within(0.001f));
            Assert.That(StaminaFlashState.FlashHz(0.75f, Threshold, HzStart, HzZero), Is.EqualTo(9.5f).Within(0.001f));
            Assert.That(StaminaFlashState.FlashHz(3f, Threshold, HzStart, HzZero), Is.EqualTo(7f).Within(0.001f));
        }

        [Test]
        public void FlashHz_NonPositiveThreshold_ReturnsStartHz()
            => Assert.That(StaminaFlashState.FlashHz(1f, 0f, HzStart, HzZero), Is.EqualTo(7f).Within(0.001f));

        [Test]
        public void Step_NotActive_NeverFlashes()
            => Assert.That(Step(new StaminaFlashState(), 0.016f, 1f), Is.False);

        [Test]
        public void Step_ActiveAboveThreshold_NoFlash()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Assert.That(Step(s, 0.016f, 2f), Is.False);
        }

        [Test]
        public void Step_ActiveInWarning_FirstFrameIsOn()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Assert.That(Step(s, 0.001f, 1f), Is.True);
        }

        [Test]
        public void Step_ActiveInWarning_TogglesOverTime()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            bool sawOn = false, sawOff = false;
            for (int i = 0; i < 20; i++)
            {
                if (Step(s, 0.05f, 1f)) sawOn = true; else sawOff = true;
            }
            Assert.That(sawOn, Is.True);
            Assert.That(sawOff, Is.True);
        }

        [Test]
        public void Step_ZeroRemaining_NoFlash()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Assert.That(Step(s, 0.016f, 0f), Is.False);
        }

        [Test]
        public void Step_NonPositiveThreshold_NoFlash()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Assert.That(Step(s, 0.016f, 1f, 0f), Is.False);
        }

        [Test]
        public void Step_HugeDt_WrapsPhase()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Assert.DoesNotThrow(() => Step(s, 0.5f, 1f));
            Assert.DoesNotThrow(() => Step(s, 0.001f, 1f));
        }

        [Test]
        public void Stop_ClearsFlashAndPhase()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            for (int i = 0; i < 3; i++) Step(s, 0.04f, 1f);
            s.Stop();
            Assert.That(Step(s, 0.001f, 1f), Is.False);
            s.SetActive(true);
            Assert.That(Step(s, 0.001f, 1f), Is.True);
        }

        [Test]
        public void ReGrab_InWarning_FlashesImmediately()
        {
            var s = new StaminaFlashState();
            s.SetActive(true);
            Step(s, 0.04f, 1f);
            s.SetActive(false);
            Assert.That(Step(s, 0.001f, 1f), Is.False);
            s.SetActive(true);
            Assert.That(Step(s, 0.001f, 1f), Is.True);
        }
    }
}
