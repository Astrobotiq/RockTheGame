using New_Scripts.Pause;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class PauseStateTests
    {
        [Test]
        public void TryPause_WhenAllowed_Pauses_AndRaisesChangedOnce()
        {
            var state = new PauseState();
            int calls = 0;
            bool last = false;
            state.Changed += p => { calls++; last = p; };

            Assert.IsTrue(state.TryPause(allowed: true));
            Assert.IsTrue(state.IsPaused);
            Assert.AreEqual(1, calls);
            Assert.IsTrue(last);

            Assert.IsFalse(state.TryPause(allowed: true), "zaten duraklı");
            Assert.AreEqual(1, calls, "ikinci çağrı olay üretmez");
        }

        [Test]
        public void TryPause_WhenNotAllowed_DoesNothing()
        {
            var state = new PauseState();
            Assert.IsFalse(state.TryPause(allowed: false));
            Assert.IsFalse(state.IsPaused);
        }

        [Test]
        public void Resume_WorksEvenWhenPauseWouldNowBeBlocked()
        {
            var state = new PauseState();
            state.TryPause(true);
            Assert.IsTrue(state.Resume(), "devam için izin gerekmez");
            Assert.IsFalse(state.IsPaused);
            Assert.IsFalse(state.Resume(), "duraklı değilken devam bir şey yapmaz");
        }

        [Test]
        public void Toggle_FlipsState_AndRespectsAllowedOnlyWhenPausing()
        {
            var state = new PauseState();
            Assert.IsFalse(state.Toggle(allowed: false), "izin yok, duraklamaz");
            Assert.IsTrue(state.Toggle(allowed: true));
            Assert.IsTrue(state.IsPaused);
            Assert.IsTrue(state.Toggle(allowed: false), "duraklıyken izin yokken de çıkılır");
            Assert.IsFalse(state.IsPaused);
        }

        [Test]
        public void CanPause_RequiresActiveIdleUnblockedPlayer()
        {
            Assert.IsTrue(PauseRules.CanPause(playerActive: true, playerBusy: false, inTransition: false, blocked: false));
            Assert.IsFalse(PauseRules.CanPause(false, false, false, false), "ölü / oyuncu yok");
            Assert.IsFalse(PauseRules.CanPause(true, true, false, false), "respawn ya da hit-stop");
            Assert.IsFalse(PauseRules.CanPause(true, false, true, false), "oda geçişi");
            Assert.IsFalse(PauseRules.CanPause(true, false, false, true), "çok oyunculu yarış");
        }
    }
}
