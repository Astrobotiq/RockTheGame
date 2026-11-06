using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Rakipten gelen mesajların RaceSession'a etkisi ve iki oturumun taşıyıcı üzerinden uçtan uca konuşması.
    /// docs/systems/multiplayer.md → "Kararlar", "Mesajlar".
    /// </summary>
    public class RaceSessionRemoteTests
    {
        [Test]
        public void RemoteRoomEntered_UpdatesOpponentRoomAndRaisesEvent()
        {
            var session = new RaceSession(RaceMode.Race);
            int raised = 0;
            session.OpponentRoomChanged += room => raised = room;

            session.Receive(new RoomEnteredMessage { RoomId = 3 });

            Assert.AreEqual(3, session.OpponentRoomId);
            Assert.AreEqual(3, raised);
        }

        [Test]
        public void RemoteSnapshot_DoesNotChangeOpponentRoom()
        {
            var session = new RaceSession(RaceMode.Race);
            session.Receive(new RoomEnteredMessage { RoomId = 3 });

            session.Receive(new SnapshotMessage { RoomId = 9, Position = Vector2.zero });

            Assert.AreEqual(3, session.OpponentRoomId);
        }

        [Test]
        public void RemoteFlagCaptured_UpdatesOpponentCount()
        {
            var session = new RaceSession(RaceMode.Race);
            int raised = -1;
            session.OpponentFlagCountChanged += count => raised = count;

            session.Receive(new FlagCapturedMessage { RoomId = 4, NewCount = 2 });

            Assert.AreEqual(2, session.OpponentFlagCount);
            Assert.AreEqual(2, raised);
        }

        [Test]
        public void RemotePenalty_RaisesWithLocalPreviousRoom()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalRoomEntered(5);
            int? target = null;
            session.PenaltyReceived += room => target = room;

            session.Receive(new PenaltyMessage { TargetRoomId = 9 });

            Assert.AreEqual(4, target);
        }

        [Test]
        public void RemotePenalty_LocalInFirstRoom_IsIgnored()
        {
            var session = new RaceSession(RaceMode.Race);
            bool raised = false;
            session.PenaltyReceived += _ => raised = true;

            session.Receive(new PenaltyMessage { TargetRoomId = 0 });

            Assert.IsFalse(raised);
        }

        [Test]
        public void RemotePenalty_InCoop_IsIgnored()
        {
            var session = new RaceSession(RaceMode.Coop);
            session.OnLocalRoomEntered(5);
            bool raised = false;
            session.PenaltyReceived += _ => raised = true;

            session.Receive(new PenaltyMessage { TargetRoomId = 4 });

            Assert.IsFalse(raised);
        }

        [Test]
        public void RemoteFinished_SetsOpponentFinishedAndRaises()
        {
            var session = new RaceSession(RaceMode.Race);
            float? raised = null;
            session.OpponentFinishedRace += t => raised = t;

            session.Receive(new FinishedMessage { ElapsedSeconds = 812.5f });

            Assert.IsTrue(session.OpponentFinished);
            Assert.AreEqual(812.5f, session.OpponentElapsedSeconds);
            Assert.AreEqual(812.5f, raised);
        }

        [Test]
        public void RemoteProgressSync_ReplacesOpponentState()
        {
            var session = new RaceSession(RaceMode.Race);
            session.Receive(new RoomEnteredMessage { RoomId = 3 });
            session.Receive(new FlagCapturedMessage { RoomId = 2, NewCount = 2 });

            session.Receive(new ProgressSyncMessage { RoomId = 8, FlagCount = 0, CapturedRoomIds = new[] { 2, 5 }, ElapsedSeconds = 50f });

            Assert.AreEqual(8, session.OpponentRoomId);
            Assert.AreEqual(0, session.OpponentFlagCount);
        }

        [Test]
        public void RemoteProgressSync_NullCapturedRooms_DoesNotThrow()
        {
            var session = new RaceSession(RaceMode.Race);

            Assert.DoesNotThrow(() => session.Receive(new ProgressSyncMessage { RoomId = 4, FlagCount = 1, CapturedRoomIds = null }));
            Assert.AreEqual(4, session.OpponentRoomId);
        }

        [Test]
        public void TwoSessions_ThreeFlagsOverInMemoryTransport_DeliverPenalty()
        {
            var (hostTransport, clientTransport) = InMemoryTransport.CreatePair();
            var a = new RaceSession(RaceMode.Race);
            var b = new RaceSession(RaceMode.Race);
            Wire(hostTransport, a);
            Wire(clientTransport, b);
            hostTransport.Host();
            clientTransport.Join("ignored");
            hostTransport.Poll();
            clientTransport.Poll();

            int? bTarget = null;
            b.PenaltyReceived += room => bTarget = room;

            Send(clientTransport, b.OnLocalRoomEntered(6));
            hostTransport.Poll();
            Assert.AreEqual(6, a.OpponentRoomId);

            for (int room = 1; room <= 3; room++)
                Send(hostTransport, a.OnLocalFlagReached(room));
            clientTransport.Poll();

            Assert.AreEqual(5, bTarget);
            Assert.AreEqual(3, b.OpponentFlagCount);

            hostTransport.Dispose();
            clientTransport.Dispose();
        }

        private static void Wire(IRaceTransport transport, RaceSession session)
        {
            transport.MessageReceived += bytes =>
            {
                if (RaceMessageCodec.TryDecode(bytes, out IRaceMessage message)) session.Receive(message);
            };
        }

        private static void Send(IRaceTransport transport, IReadOnlyList<IRaceMessage> messages)
        {
            foreach (var message in messages) transport.Send(RaceMessageCodec.Encode(message), reliable: true);
        }
    }
}
