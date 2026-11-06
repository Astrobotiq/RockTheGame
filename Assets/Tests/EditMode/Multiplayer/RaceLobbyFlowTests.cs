using System;
using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.Multiplayer;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Save;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Lobi el sıkışması ve ortak başlangıç: modu host belirler, sürüm uyuşmazsa reddedilir, iki taraf da hazır olunca
    /// 3 sn geri sayım. docs/superpowers/plans/2026-11-06-mp7-lobby-hud.md → Task 2.
    /// </summary>
    public class RaceLobbyFlowTests
    {
        private static readonly Guid RaceId = new Guid("aaaaaaaa-0000-4000-8000-000000000007");

        private sealed class Peer
        {
            public InMemoryTransport Transport;
            public RaceLobbyFlow Flow;
            public double Now;
            public int LoadRequests;
            public int RaceStarts;

            public void Send(IEnumerable<IRaceMessage> messages)
            {
                foreach (var m in messages) Transport.Send(RaceMessageCodec.Encode(m), reliable: true);
            }
        }

        private Peer _host;
        private Peer _client;

        private static Peer MakePeer(InMemoryTransport transport, RaceLobbyFlow flow)
        {
            var peer = new Peer { Transport = transport, Flow = flow };
            flow.LoadSceneRequested += () => peer.LoadRequests++;
            flow.RaceStarted += () => peer.RaceStarts++;
            transport.Connected += () => peer.Send(flow.OnConnected());
            transport.MessageReceived += bytes =>
            {
                Assert.IsTrue(RaceMessageCodec.TryDecode(bytes, out var message));
                peer.Send(flow.Receive(message, peer.Now));
            };
            return peer;
        }

        private void Pump(int rounds = 4)
        {
            for (int i = 0; i < rounds; i++)
            {
                _host.Transport.Poll();
                _client.Transport.Poll();
            }
        }

        private void Connect(RaceMode hostMode, RaceMode clientPreference)
        {
            var (h, c) = InMemoryTransport.CreatePair();
            _host = MakePeer(h, new RaceLobbyFlow(isHost: true, hostMode, "Host", RaceId));
            _client = MakePeer(c, new RaceLobbyFlow(isHost: false, clientPreference, "Client", RaceId));
            h.Host();
            c.Join("ignored");
            Pump();
        }

        [Test]
        public void Handshake_ClientUsesHostMode_BothAskToLoadScene()
        {
            Connect(RaceMode.Race, clientPreference: RaceMode.Coop);

            Assert.AreEqual(RaceMode.Race, _host.Flow.Session.Mode);
            Assert.AreEqual(RaceMode.Race, _client.Flow.Session.Mode, "istemci host'un modunu kullanmalı");
            Assert.AreEqual(1, _host.LoadRequests);
            Assert.AreEqual(1, _client.LoadRequests);
            Assert.AreEqual(LobbyPhase.Loading, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Loading, _client.Flow.Phase);
        }

        [Test]
        public void Handshake_ExchangesProgress_OpponentRoomKnown()
        {
            Connect(RaceMode.Race, RaceMode.Race);

            Assert.AreEqual(RaceRules.FirstRoomId, _host.Flow.Session.OpponentRoomId);
            Assert.AreEqual(RaceRules.FirstRoomId, _client.Flow.Session.OpponentRoomId);
        }

        [Test]
        public void Hello_WrongProtocolVersion_Fails()
        {
            var flow = new RaceLobbyFlow(isHost: true, RaceMode.Race, "Host", RaceId);
            flow.OnConnected();
            string reason = null;
            flow.Failed += r => reason = r;

            var replies = flow.Receive(new HelloMessage
            {
                ProtocolVersion = (ushort)(RaceProtocol.Version - 1), Mode = RaceMode.Race, RaceId = RaceId, PlayerName = "Eski"
            }, 0);

            Assert.AreEqual(LobbyPhase.Failed, flow.Phase);
            Assert.IsEmpty(replies);
            StringAssert.Contains("sürüm", reason.ToLowerInvariant());
        }

        [Test]
        public void Countdown_StartsOnlyWhenBothReady_ThenRaceStarts()
        {
            Connect(RaceMode.Race, RaceMode.Race);

            _host.Now = 10;
            _host.Send(_host.Flow.OnSceneReady(10));
            Pump();
            Assert.AreEqual(LobbyPhase.WaitingForOpponent, _host.Flow.Phase, "rakip yüklenmeden geri sayım yok");

            _client.Now = 15;
            _host.Now = 15;
            _client.Send(_client.Flow.OnSceneReady(15));
            Pump();

            Assert.AreEqual(LobbyPhase.Countdown, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Countdown, _client.Flow.Phase);
            Assert.AreEqual(RaceLobbyFlow.CountdownSeconds, _host.Flow.CountdownRemaining(15), 1e-4);

            _host.Flow.Tick(17.9);
            Assert.AreEqual(0, _host.RaceStarts);
            _host.Flow.Tick(18.0);
            _client.Flow.Tick(18.0);
            Assert.AreEqual(1, _host.RaceStarts);
            Assert.AreEqual(1, _client.RaceStarts);
            Assert.AreEqual(LobbyPhase.Racing, _host.Flow.Phase);

            _host.Flow.Tick(30);
            Assert.AreEqual(1, _host.RaceStarts, "başlangıç bir kez");
        }

        [Test]
        public void SceneReadyBeforeHandshake_SendsReadyWhenHandshakeCompletes()
        {
            // Geliştirici başlatıcısı: sahne zaten yüklü, akış bağlanmadan önce hazır.
            var (h, c) = InMemoryTransport.CreatePair();
            _host = MakePeer(h, new RaceLobbyFlow(true, RaceMode.Race, "Host", RaceId));
            _client = MakePeer(c, new RaceLobbyFlow(false, RaceMode.Race, "Client", RaceId));
            _host.Flow.LoadSceneRequested += () => _host.Send(_host.Flow.OnSceneReady(_host.Now));
            _client.Flow.LoadSceneRequested += () => _client.Send(_client.Flow.OnSceneReady(_client.Now));

            h.Host();
            c.Join("ignored");
            Pump(6);

            Assert.AreEqual(LobbyPhase.Countdown, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Countdown, _client.Flow.Phase);
        }

        [Test]
        public void GameMessages_ForwardedToSession_AfterHandshake()
        {
            Connect(RaceMode.Race, RaceMode.Race);

            _client.Send(_client.Flow.Session.OnLocalRoomEntered(4));
            Pump();

            Assert.AreEqual(4, _host.Flow.Session.OpponentRoomId);
        }

        // ---------------- Devam etme (MP 8) ----------------

        private static readonly Guid SavedRace = new Guid("cccccccc-0000-4000-8000-000000000008");

        private static RaceSaveData Save(Guid id, int room, int flags, params int[] captured)
        {
            var session = new RaceSession(RaceMode.Race);
            session.RestoreLocal(room, flags, captured);
            return RaceSaveData.From(session, id, "rakip", 42f);
        }

        private void ConnectWith(RaceLobbyFlow hostFlow, RaceLobbyFlow clientFlow)
        {
            var (h, c) = InMemoryTransport.CreatePair();
            _host = MakePeer(h, hostFlow);
            _client = MakePeer(c, clientFlow);
            h.Host();
            c.Join("ignored");
            Pump();
        }

        [Test]
        public void NewRace_ClientAdoptsHostRaceId()
        {
            Connect(RaceMode.Race, RaceMode.Race);
            Assert.AreNotEqual(Guid.Empty, _host.Flow.RaceId);
            Assert.AreEqual(_host.Flow.RaceId, _client.Flow.RaceId);
        }

        [Test]
        public void Resume_SameRace_RestoresEachSideAndSharesProgress()
        {
            ConnectWith(new RaceLobbyFlow(true, RaceMode.Coop, "Host", Guid.NewGuid(), resume: Save(SavedRace, 7, 2, 5, 6)),
                        new RaceLobbyFlow(false, RaceMode.Coop, "Client", Guid.Empty, resume: Save(SavedRace, 4, 1, 3)));

            Assert.AreEqual(LobbyPhase.Loading, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Loading, _client.Flow.Phase);
            Assert.AreEqual(RaceMode.Race, _client.Flow.Session.Mode, "mod kayıttan");
            Assert.AreEqual(7, _host.Flow.Session.LocalRoomId);
            Assert.AreEqual(2, _host.Flow.Session.LocalFlagCount);
            Assert.AreEqual(4, _client.Flow.Session.LocalRoomId);
            Assert.AreEqual(4, _host.Flow.Session.OpponentRoomId);
            Assert.AreEqual(7, _client.Flow.Session.OpponentRoomId);
            Assert.AreEqual(SavedRace, _client.Flow.RaceId);
        }

        [Test]
        public void Resume_HostResumesClientNew_BothFailWithReason()
        {
            ConnectWith(new RaceLobbyFlow(true, RaceMode.Race, "Host", Guid.NewGuid(), resume: Save(SavedRace, 7, 0)),
                        new RaceLobbyFlow(false, RaceMode.Race, "Client", Guid.Empty));

            Assert.AreEqual(LobbyPhase.Failed, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Failed, _client.Flow.Phase);
            StringAssert.Contains("Devam et", _host.Flow.FailureReason);
            StringAssert.Contains("Devam et", _client.Flow.FailureReason);
        }

        [Test]
        public void Resume_DifferentSaves_BothFail()
        {
            ConnectWith(new RaceLobbyFlow(true, RaceMode.Race, "Host", Guid.NewGuid(), resume: Save(SavedRace, 7, 0)),
                        new RaceLobbyFlow(false, RaceMode.Race, "Client", Guid.Empty, resume: Save(Guid.NewGuid(), 4, 0)));

            Assert.AreEqual(LobbyPhase.Failed, _host.Flow.Phase);
            Assert.AreEqual(LobbyPhase.Failed, _client.Flow.Phase);
            StringAssert.Contains("farklı", _host.Flow.FailureReason);
        }

        [Test]
        public void Reconnect_WithExistingSessions_KeepsStateAndCountsDownAgain()
        {
            var hostSession = new RaceSession(RaceMode.Race);
            hostSession.RestoreLocal(9, 1, new[] { 8 });
            var clientSession = new RaceSession(RaceMode.Race);
            clientSession.RestoreLocal(6, 0, new int[0]);
            var hostFlow = new RaceLobbyFlow(true, RaceMode.Race, "Host", SavedRace, existingSession: hostSession);
            var clientFlow = new RaceLobbyFlow(false, RaceMode.Race, "Client", SavedRace, existingSession: clientSession);
            hostFlow.LoadSceneRequested += () => _host.Send(hostFlow.OnSceneReady(_host.Now));
            clientFlow.LoadSceneRequested += () => _client.Send(clientFlow.OnSceneReady(_client.Now));

            ConnectWith(hostFlow, clientFlow);
            Pump();

            Assert.AreSame(hostSession, hostFlow.Session);
            Assert.AreSame(clientSession, clientFlow.Session);
            Assert.AreEqual(LobbyPhase.Countdown, hostFlow.Phase);
            Assert.AreEqual(LobbyPhase.Countdown, clientFlow.Phase);
            Assert.AreEqual(6, hostSession.OpponentRoomId);
        }

        [Test]
        public void GameMessage_BeforeClientKnowsMode_IsDroppedSafely()
        {
            var flow = new RaceLobbyFlow(isHost: false, RaceMode.Race, "Client", RaceId);
            Assert.IsNull(flow.Session);
            Assert.DoesNotThrow(() => flow.Receive(new RoomEnteredMessage { RoomId = 3 }, 0));
        }
    }
}
