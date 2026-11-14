using NUnit.Framework;
using New_Scripts.Multiplayer.Hud;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>Yarış HUD'u pist penceresi (mercek). docs/ui-research/race-hud-track.md → "Mercek".</summary>
    public class RaceTrackWindowTests
    {
        static void AssertRange(RoomRange r, int lo, int hi)
        {
            Assert.AreEqual(lo, r.Lo, "Lo");
            Assert.AreEqual(hi, r.Hi, "Hi");
        }

        [Test]
        public void SingleWindow_CenteredOnMidpoint_Is14Rooms()
        {
            var l = RaceTrackWindow.Compute(22, 11, 9);
            Assert.IsFalse(l.IsSplit);
            AssertRange(l.First, 4, 17);
            Assert.AreEqual(14, l.First.Count);
        }

        [Test]
        public void SingleWindow_ClampsToStartAndFinish()
        {
            AssertRange(RaceTrackWindow.Compute(22, 2, 1).First, 1, 14);
            AssertRange(RaceTrackWindow.Compute(22, 22, 21).First, 9, 22);
        }

        [Test]
        public void SingleWindow_ShortRaceShowsWholeTrack()
        {
            AssertRange(RaceTrackWindow.Compute(10, 4, 6).First, 1, 10);
        }

        [Test]
        public void SingleWindow_SameRoom_AndLocalOrRivalAhead_GiveSameWindow()
        {
            AssertRange(RaceTrackWindow.Compute(22, 8, 8).First, 2, 15);
            var a = RaceTrackWindow.Compute(22, 11, 9);
            var b = RaceTrackWindow.Compute(22, 9, 11);
            AssertRange(a.First, b.First.Lo, b.First.Hi);
        }

        [Test]
        public void Split_OnlyWhenGapExceedsThreshold()
        {
            Assert.IsFalse(RaceTrackWindow.Compute(100, 13, 3).IsSplit, "fark 10: bölünmez");
            Assert.IsTrue(RaceTrackWindow.Compute(100, 14, 3).IsSplit, "fark 11: bölünür");
        }

        [Test]
        public void Split_PiecesAroundEachPlayer_WithHiddenCount()
        {
            var l = RaceTrackWindow.Compute(100, 50, 30);
            Assert.IsTrue(l.IsSplit);
            AssertRange(l.First, 28, 33);
            AssertRange(l.Second, 48, 53);
            Assert.AreEqual(14, l.HiddenRooms);
        }

        [Test]
        public void Split_OrderedByPositionNotByWho()
        {
            var a = RaceTrackWindow.Compute(100, 50, 30);
            var b = RaceTrackWindow.Compute(100, 30, 50);
            AssertRange(b.First, a.First.Lo, a.First.Hi);
            AssertRange(b.Second, a.Second.Lo, a.Second.Hi);
        }

        [Test]
        public void Split_PiecesClampAtStartAndFinish()
        {
            var start = RaceTrackWindow.Compute(100, 40, 1);
            AssertRange(start.First, 1, 6);
            AssertRange(start.Second, 38, 43);
            Assert.AreEqual(31, start.HiddenRooms);

            var end = RaceTrackWindow.Compute(100, 100, 50);
            AssertRange(end.First, 48, 53);
            AssertRange(end.Second, 95, 100);
            Assert.AreEqual(41, end.HiddenRooms);
        }

        [Test]
        public void Compute_CustomWidthAndThreshold()
        {
            var l = RaceTrackWindow.Compute(50, 20, 14, windowWidth: 8, splitGap: 12);
            Assert.IsFalse(l.IsSplit);
            AssertRange(l.First, 14, 21);
        }

        [Test]
        public void GapText_LeaderAndLevel()
        {
            Assert.AreEqual("you +2", RaceTrackWindow.GapText(9, 7));
            Assert.AreEqual("rival +3", RaceTrackWindow.GapText(4, 7));
            Assert.AreEqual("level", RaceTrackWindow.GapText(5, 5));
        }
    }
}
