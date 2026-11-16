using New_Scripts.Player;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Havada duvar carpmasi: hiz sifirlanir, duvar kisa surede biterse geri verilir (docs/systems/movement-core.md).
    /// </summary>
    public class WallSpeedRetentionTests
    {
        private const float Dt = 0.02f;
        private const float Window = 0.06f;

        [Test]
        public void Step_NoWall_KeepsVelocity()
        {
            var r = new WallSpeedRetention();
            Assert.AreEqual(25f, r.Step(25f, false, false, Window, Dt));
            Assert.IsFalse(r.IsRetaining);
        }

        [Test]
        public void Step_HitsRightWall_ZeroesVelocity()
        {
            var r = new WallSpeedRetention();
            Assert.AreEqual(0f, r.Step(25f, false, true, Window, Dt));
            Assert.IsTrue(r.IsRetaining);
        }

        [Test]
        public void Step_HitsLeftWall_ZeroesVelocity()
        {
            var r = new WallSpeedRetention();
            Assert.AreEqual(0f, r.Step(-25f, true, false, Window, Dt));
        }

        [Test]
        public void Step_MovingAwayFromTouchedWall_KeepsVelocity()
        {
            var r = new WallSpeedRetention();
            Assert.AreEqual(-10f, r.Step(-10f, false, true, Window, Dt));
            Assert.IsFalse(r.IsRetaining);
        }

        [Test]
        public void Step_WallEndsInsideWindow_ReturnsRetainedSpeed()
        {
            var r = new WallSpeedRetention();
            r.Step(25f, false, true, Window, Dt);
            Assert.AreEqual(0f, r.Step(0f, false, true, Window, Dt));
            Assert.AreEqual(25f, r.Step(0f, false, false, Window, Dt));
            Assert.IsFalse(r.IsRetaining);
        }

        [Test]
        public void Step_WallEndsAfterWindow_DoesNotReturnSpeed()
        {
            var r = new WallSpeedRetention();
            r.Step(25f, false, true, Window, Dt);
            // 0.06 sn = 3 adim; pencere dolana kadar duvar duruyor. Dolunca bos hizla yeni carpma yok.
            for (int i = 0; i < 4; i++) r.Step(0f, false, true, Window, Dt);
            Assert.IsFalse(r.IsRetaining);
            Assert.AreEqual(0f, r.Step(0f, false, false, Window, Dt));
        }

        [Test]
        public void Step_PlayerReversesInsideWindow_CancelsReturn()
        {
            var r = new WallSpeedRetention();
            r.Step(25f, false, true, Window, Dt);
            Assert.AreEqual(-3f, r.Step(-3f, false, false, Window, Dt));
            Assert.IsFalse(r.IsRetaining);
        }

        [Test]
        public void Step_PushingIntoWallInsideWindow_KeepsFirstImpactSpeed()
        {
            var r = new WallSpeedRetention();
            r.Step(25f, false, true, Window, Dt);
            // Stick duvara itiyor, hiz kucuk pozitife cikti: yine sifirlanir ama retained 25 kalir.
            Assert.AreEqual(0f, r.Step(0.4f, false, true, Window, Dt));
            Assert.AreEqual(25f, r.Step(0f, false, false, Window, Dt));
        }

        [Test]
        public void Step_ZeroRetentionTime_ZeroesButNeverReturns()
        {
            var r = new WallSpeedRetention();
            // 0 = geri verme kapali (secenek a): carpan hiz sifirlanir, kose cikisinda geri gelmez.
            Assert.AreEqual(0f, r.Step(25f, false, true, 0f, Dt));
            Assert.AreEqual(0f, r.Step(0f, false, false, 0f, Dt));
            Assert.IsFalse(r.IsRetaining);
        }
    }
}
