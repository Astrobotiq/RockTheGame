using System.Linq;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Yerel oyuncunun olaylarına göre yarış kuralları: bayrak bir kez, 3'te ceza, birlikte modu, bitiş.
    /// docs/systems/multiplayer.md → "Kararlar".
    /// </summary>
    public class RaceSessionLocalTests
    {
        private static RaceSession RaceWithOpponentIn(int opponentRoom)
        {
            var session = new RaceSession(RaceMode.Race);
            session.Receive(new RoomEnteredMessage { RoomId = opponentRoom });
            return session;
        }

        [Test]
        public void RoomEntered_UpdatesLocalRoomAndSendsMessage()
        {
            var session = new RaceSession(RaceMode.Race);

            var sent = session.OnLocalRoomEntered(4);

            Assert.AreEqual(4, session.LocalRoomId);
            Assert.AreEqual(1, sent.Count);
            Assert.AreEqual(4, ((RoomEnteredMessage)sent[0]).RoomId);
        }

        [Test]
        public void RoomEntered_SameRoomAgain_SendsNothing()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalRoomEntered(4);

            Assert.AreEqual(0, session.OnLocalRoomEntered(4).Count);
        }

        [Test]
        public void FlagReached_Race_CountsAndSendsFlagCaptured()
        {
            var session = new RaceSession(RaceMode.Race);

            var sent = session.OnLocalFlagReached(2);

            Assert.AreEqual(1, session.LocalFlagCount);
            Assert.AreEqual(1, sent.Count);
            var flag = (FlagCapturedMessage)sent[0];
            Assert.AreEqual(2, flag.RoomId);
            Assert.AreEqual(1, flag.NewCount);
        }

        [Test]
        public void FlagReached_SameRoomTwice_IsIgnored()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalFlagReached(2);

            var sent = session.OnLocalFlagReached(2);

            Assert.AreEqual(0, sent.Count);
            Assert.AreEqual(1, session.LocalFlagCount);
        }

        [Test]
        public void ThirdFlag_SendsPenaltyForOpponentsPreviousRoom()
        {
            var session = RaceWithOpponentIn(7);
            session.OnLocalFlagReached(1);
            session.OnLocalFlagReached(2);

            var sent = session.OnLocalFlagReached(3);

            Assert.AreEqual(2, sent.Count);
            Assert.AreEqual(3, ((FlagCapturedMessage)sent[0]).NewCount);
            Assert.AreEqual(6, ((PenaltyMessage)sent[1]).TargetRoomId);
            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void ThirdFlag_OpponentInFirstRoom_StillSendsPenalty_ReceiverDecides()
        {
            // Gönderenin bildiği oda eski olabilir (RoomEntered yolda): cezayı yine yollar, boşa gidip gitmediğine alıcı karar verir.
            var session = RaceWithOpponentIn(RaceRules.FirstRoomId);
            session.OnLocalFlagReached(1);
            session.OnLocalFlagReached(2);

            var sent = session.OnLocalFlagReached(3);

            Assert.AreEqual(2, sent.Count);
            Assert.IsInstanceOf<PenaltyMessage>(sent[1]);
            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void ThirdFlag_OpponentRoomUnknown_StillSendsPenalty_ReceiverDecides()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalFlagReached(1);
            session.OnLocalFlagReached(2);

            var sent = session.OnLocalFlagReached(3);

            Assert.AreEqual(2, sent.Count);
            Assert.IsInstanceOf<PenaltyMessage>(sent[1]);
            Assert.GreaterOrEqual(((PenaltyMessage)sent[1]).TargetRoomId, 0);
            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void SixFlags_SendTwoPenalties()
        {
            var session = RaceWithOpponentIn(10);
            int penalties = 0;

            for (int room = 1; room <= 6; room++)
                penalties += session.OnLocalFlagReached(room).OfType<PenaltyMessage>().Count();

            Assert.AreEqual(2, penalties);
            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void Coop_FlagReached_SendsNothingAndDoesNotCount()
        {
            var session = new RaceSession(RaceMode.Coop);

            for (int room = 1; room <= 3; room++)
                Assert.AreEqual(0, session.OnLocalFlagReached(room).Count);

            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void LocalFinished_SendsFinishedOnce()
        {
            var session = new RaceSession(RaceMode.Race);

            var first = session.OnLocalFinished(812.5f);
            var second = session.OnLocalFinished(900f);

            Assert.IsTrue(session.LocalFinished);
            Assert.AreEqual(1, first.Count);
            Assert.AreEqual(812.5f, ((FinishedMessage)first[0]).ElapsedSeconds);
            Assert.AreEqual(0, second.Count);
        }

        [Test]
        public void AfterFinish_RoomAndFlagEventsAreIgnored()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalFinished(100f);

            Assert.AreEqual(0, session.OnLocalRoomEntered(5).Count);
            Assert.AreEqual(0, session.OnLocalFlagReached(5).Count);
            Assert.AreEqual(0, session.LocalFlagCount);
        }

        [Test]
        public void CreateProgressSync_ReflectsLocalState()
        {
            var session = RaceWithOpponentIn(20);
            session.OnLocalRoomEntered(9);
            session.OnLocalFlagReached(8);
            session.OnLocalFlagReached(3);

            var sync = session.CreateProgressSync(321.5f);

            Assert.AreEqual(9, sync.RoomId);
            Assert.AreEqual(2, sync.FlagCount);
            CollectionAssert.AreEqual(new[] { 3, 8 }, sync.CapturedRoomIds);
            Assert.AreEqual(321.5f, sync.ElapsedSeconds);
        }
    }
}
