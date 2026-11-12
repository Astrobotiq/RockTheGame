using NUnit.Framework;
using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>Çok oyunculu sonuç ekranının hesabı (docs/systems/multiplayer.md).</summary>
    public class RaceResultMathTests
    {
        [Test]
        public void KindOf_CoversAllOutcomes()
        {
            Assert.AreEqual(RaceResultKind.Waiting, RaceResultMath.KindOf(RaceMode.Race, false, 100f, 0f));
            Assert.AreEqual(RaceResultKind.Win, RaceResultMath.KindOf(RaceMode.Race, true, 100f, 120f));
            Assert.AreEqual(RaceResultKind.Lose, RaceResultMath.KindOf(RaceMode.Race, true, 120f, 100f));
            Assert.AreEqual(RaceResultKind.Draw, RaceResultMath.KindOf(RaceMode.Race, true, 100f, 100.04f));
            Assert.AreEqual(RaceResultKind.Coop, RaceResultMath.KindOf(RaceMode.Coop, true, 100f, 300f));
            Assert.AreEqual(RaceResultKind.Waiting, RaceResultMath.KindOf(RaceMode.Coop, false, 100f, 0f));
        }

        [Test]
        public void LoserHeight_IsTimeRatio_ClampedToMinAndOne()
        {
            Assert.AreEqual(0.8f, RaceResultMath.LoserHeight(80f, 100f), 1e-5f);
            Assert.AreEqual(RaceResultMath.MinHeight, RaceResultMath.LoserHeight(10f, 100f), 1e-5f);
            Assert.AreEqual(1f, RaceResultMath.LoserHeight(120f, 100f), 1e-5f);
            Assert.AreEqual(1f, RaceResultMath.LoserHeight(0f, 100f), 1e-5f);
        }

        [Test]
        public void Columns_LocalNotFinished_NothingShown()
        {
            RaceResultMath.Columns(RaceMode.Race, true, false, 0f, true, 90f, out var first, out var second);
            Assert.IsFalse(first.Present);
            Assert.IsFalse(second.Present);
        }

        [Test]
        public void Columns_HostIsFirst_ClientIsSecond()
        {
            RaceResultMath.Columns(RaceMode.Race, true, true, 80f, true, 100f, out var first, out var second);
            Assert.IsTrue(first.IsLocal);
            Assert.AreEqual(80f, first.Seconds);
            Assert.AreEqual(1f, first.Height, 1e-5f);
            Assert.IsFalse(second.IsLocal);
            Assert.AreEqual(0.8f, second.Height, 1e-5f);

            RaceResultMath.Columns(RaceMode.Race, false, true, 80f, true, 100f, out first, out second);
            Assert.IsFalse(first.IsLocal);
            Assert.AreEqual(100f, first.Seconds);
            Assert.IsTrue(second.IsLocal);
            Assert.AreEqual(1f, second.Height, 1e-5f);
        }

        [Test]
        public void Columns_Loser_IsShorter()
        {
            RaceResultMath.Columns(RaceMode.Race, true, true, 100f, true, 80f, out var first, out var second);
            Assert.AreEqual(0.8f, first.Height, 1e-5f);
            Assert.AreEqual(1f, second.Height, 1e-5f);
        }

        [Test]
        public void Columns_WaitingOpponent_OnlyLocalPresent()
        {
            RaceResultMath.Columns(RaceMode.Race, false, true, 90f, false, 0f, out var first, out var second);
            Assert.IsFalse(first.Present);
            Assert.IsTrue(second.Present);
            Assert.IsTrue(second.IsLocal);
        }

        [Test]
        public void Columns_CoopAndDraw_EqualHeights()
        {
            RaceResultMath.Columns(RaceMode.Coop, true, true, 100f, true, 300f, out var first, out var second);
            Assert.AreEqual(1f, first.Height);
            Assert.AreEqual(1f, second.Height);
            RaceResultMath.Columns(RaceMode.Race, true, true, 100f, true, 100f, out first, out second);
            Assert.AreEqual(1f, first.Height);
            Assert.AreEqual(1f, second.Height);
        }

        [Test]
        public void HeadlineAndLabel()
        {
            Assert.AreEqual("Kazandın!", RaceResultMath.Headline(RaceResultKind.Win));
            Assert.AreEqual("Kaybettin.", RaceResultMath.Headline(RaceResultKind.Lose));
            Assert.AreEqual("Berabere!", RaceResultMath.Headline(RaceResultKind.Draw));
            Assert.AreEqual("Birlikte bitirdiniz!", RaceResultMath.Headline(RaceResultKind.Coop));
            Assert.AreEqual("Bitirdin!", RaceResultMath.Headline(RaceResultKind.Waiting));
            Assert.AreEqual("SEN 02:14.8", RaceResultMath.Label(new RaceResultColumn(true, true, 134.8f, 1f)));
            Assert.AreEqual("RAKİP …", RaceResultMath.Label(new RaceResultColumn(false, false, 0f, 1f)));
        }
    }
}
