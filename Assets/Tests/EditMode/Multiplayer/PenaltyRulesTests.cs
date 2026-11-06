using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Penalty;
using New_Scripts.Multiplayer.Session;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Cezanın hedef odası ve ne zaman uygulanabileceği. docs/superpowers/plans/2026-11-06-mp6-penalty.md → Task 1–2.
    /// </summary>
    public class PenaltyRulesTests
    {
        [Test]
        public void PreviousRoom_MainPath_IsIdMinusOne()
        {
            Assert.AreEqual(11, RaceRules.PreviousRoomId(12, new[] { 11, 13 }));
        }

        [Test]
        public void PreviousRoom_SideRoom_IsTheRoomItHangsOff()
        {
            // RoomNew-22, oda 11'den açılan yan oda: 21 komşusu değil, öncesi 11.
            Assert.AreEqual(11, RaceRules.PreviousRoomId(22, new[] { 11 }));
        }

        [Test]
        public void PreviousRoom_PicksLargestLowerNeighbor()
        {
            Assert.AreEqual(7, RaceRules.PreviousRoomId(30, new[] { 3, 7, 31 }));
        }

        [Test]
        public void PreviousRoom_NoNeighbors_FallsBackToIdMinusOne()
        {
            Assert.AreEqual(4, RaceRules.PreviousRoomId(5, new int[0]));
            Assert.AreEqual(4, RaceRules.PreviousRoomId(5, null));
        }

        [Test]
        public void TryGetPenaltyTarget_WithNeighbors_UsesRoomGraph()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalRoomEntered(22);

            Assert.IsTrue(session.TryGetPenaltyTarget(new[] { 11 }, out int target));
            Assert.AreEqual(11, target);
        }

        [Test]
        public void TryGetPenaltyTarget_WithNeighbors_FirstRoomOrCoop_IsFalse()
        {
            var first = new RaceSession(RaceMode.Race);
            Assert.IsFalse(first.TryGetPenaltyTarget(new[] { 2 }, out _));

            var coop = new RaceSession(RaceMode.Coop);
            coop.OnLocalRoomEntered(5);
            Assert.IsFalse(coop.TryGetPenaltyTarget(new[] { 4 }, out _));
        }

        [Test]
        public void IsPlayerBusy_DeadPausedOrInTransition()
        {
            Assert.IsFalse(PenaltyHandler.IsPlayerBusy(active: true, paused: false, inTransition: false));
            Assert.IsTrue(PenaltyHandler.IsPlayerBusy(active: false, paused: false, inTransition: false), "ölü");
            Assert.IsTrue(PenaltyHandler.IsPlayerBusy(active: true, paused: true, inTransition: false), "respawn / hitstop");
            Assert.IsTrue(PenaltyHandler.IsPlayerBusy(active: true, paused: false, inTransition: true), "oda geçişi");
        }
    }
}
