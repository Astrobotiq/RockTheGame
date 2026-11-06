using System;
using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Transport;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Bellek içi taşıyıcının IRaceTransport sözleşmesi: bağlanma, sıralı teslim, kopma.
    /// docs/systems/multiplayer.md → "Bileşenler".
    /// </summary>
    public class InMemoryTransportTests
    {
        private InMemoryTransport _host;
        private InMemoryTransport _client;
        private readonly List<byte[]> _hostReceived = new List<byte[]>();
        private readonly List<byte[]> _clientReceived = new List<byte[]>();

        [SetUp]
        public void SetUp()
        {
            (_host, _client) = InMemoryTransport.CreatePair();
            _hostReceived.Clear();
            _clientReceived.Clear();
            _host.MessageReceived += m => _hostReceived.Add(m);
            _client.MessageReceived += m => _clientReceived.Add(m);
        }

        [TearDown]
        public void TearDown()
        {
            _host.Dispose();
            _client.Dispose();
        }

        private void Connect()
        {
            _host.Host();
            _client.Join("ignored");
            _host.Poll();
            _client.Poll();
        }

        [Test]
        public void HostAndJoin_BothConnectAfterPoll()
        {
            int hostConnected = 0, clientConnected = 0;
            _host.Connected += () => hostConnected++;
            _client.Connected += () => clientConnected++;

            _host.Host();
            _client.Join("ignored");
            Assert.AreEqual(TransportState.Connecting, _host.State);
            Assert.AreEqual(TransportState.Connecting, _client.State);

            _host.Poll(); _client.Poll();
            _host.Poll(); _client.Poll();

            Assert.AreEqual(TransportState.Connected, _host.State);
            Assert.AreEqual(TransportState.Connected, _client.State);
            Assert.AreEqual(1, hostConnected);
            Assert.AreEqual(1, clientConnected);
        }

        [Test]
        public void OnlyHost_StaysConnecting()
        {
            _host.Host();
            _host.Poll(); _client.Poll();
            _host.Poll(); _client.Poll();

            Assert.AreEqual(TransportState.Connecting, _host.State);
            Assert.AreEqual(TransportState.Disconnected, _client.State);
        }

        [Test]
        public void Message_ArrivesOnlyAfterReceiverPolls()
        {
            Connect();

            Assert.IsTrue(_host.Send(new byte[] { 42 }, reliable: true));
            Assert.AreEqual(0, _clientReceived.Count);

            _client.Poll();

            Assert.AreEqual(1, _clientReceived.Count);
            CollectionAssert.AreEqual(new byte[] { 42 }, _clientReceived[0]);
        }

        [Test]
        public void Messages_ArriveInSendOrder([Values(true, false)] bool reliable)
        {
            Connect();

            _host.Send(new byte[] { 1 }, reliable);
            _host.Send(new byte[] { 2 }, reliable);
            _host.Send(new byte[] { 3 }, reliable);
            _client.Poll();

            Assert.AreEqual(3, _clientReceived.Count);
            Assert.AreEqual(1, _clientReceived[0][0]);
            Assert.AreEqual(2, _clientReceived[1][0]);
            Assert.AreEqual(3, _clientReceived[2][0]);
        }

        [Test]
        public void Send_CopiesPayload()
        {
            Connect();
            byte[] buffer = { 10, 20 };

            _host.Send(buffer, reliable: false);
            buffer[0] = 99;
            _client.Poll();

            CollectionAssert.AreEqual(new byte[] { 10, 20 }, _clientReceived[0]);
        }

        [Test]
        public void Send_BeforeConnect_ReturnsFalse()
        {
            Assert.IsFalse(_host.Send(new byte[] { 1 }, reliable: true));

            _host.Host();
            Assert.IsFalse(_host.Send(new byte[] { 1 }, reliable: true));
        }

        [Test]
        public void Disconnect_RaisesLocalRequestAndRemoteLeft()
        {
            Connect();
            DisconnectReason? hostReason = null, clientReason = null;
            _host.Disconnected += r => hostReason = r;
            _client.Disconnected += r => clientReason = r;

            _host.Disconnect();
            _host.Poll();
            _client.Poll();

            Assert.AreEqual(DisconnectReason.LocalRequest, hostReason);
            Assert.AreEqual(DisconnectReason.RemoteLeft, clientReason);
            Assert.AreEqual(TransportState.Disconnected, _host.State);
            Assert.AreEqual(TransportState.Disconnected, _client.State);
        }

        [Test]
        public void Disconnect_DropsPendingMessages()
        {
            Connect();
            _host.Send(new byte[] { 1 }, reliable: true);

            _host.Disconnect();
            _client.Poll();

            Assert.AreEqual(0, _clientReceived.Count);
        }

        [Test]
        public void Send_AfterDisconnect_ReturnsFalse()
        {
            Connect();
            _host.Disconnect();
            _host.Poll();
            _client.Poll();

            Assert.IsFalse(_host.Send(new byte[] { 1 }, reliable: true));
            Assert.IsFalse(_client.Send(new byte[] { 1 }, reliable: true));
        }

        [Test]
        public void ReplyInsideHandler_IsDeliveredOnNextPoll()
        {
            Connect();
            _client.MessageReceived += m => _client.Send(new byte[] { (byte)(m[0] + 1) }, reliable: true);

            _host.Send(new byte[] { 5 }, reliable: true);
            Assert.DoesNotThrow(() => _client.Poll());
            _host.Poll();

            Assert.AreEqual(1, _hostReceived.Count);
            Assert.AreEqual(6, _hostReceived[0][0]);
        }

        [Test]
        public void DisconnectInsideHandler_StopsBatchAndRaisesOnNextPoll()
        {
            Connect();
            DisconnectReason? clientReason = null, hostReason = null;
            _client.Disconnected += r => clientReason = r;
            _host.Disconnected += r => hostReason = r;
            _client.MessageReceived += m => { if (m[0] == 1) _client.Disconnect(); };

            _host.Send(new byte[] { 1 }, true);
            _host.Send(new byte[] { 2 }, true);
            Assert.DoesNotThrow(() => _client.Poll());

            Assert.AreEqual(1, _clientReceived.Count, "Disconnect'ten sonra kalan mesajlar teslim edilmemeli");
            Assert.IsNull(clientReason, "Disconnected olayı bir sonraki Poll'da gelmeli");

            _client.Poll();
            _host.Poll();
            Assert.AreEqual(DisconnectReason.LocalRequest, clientReason);
            Assert.AreEqual(DisconnectReason.RemoteLeft, hostReason);
        }

        [Test]
        public void SendInsideOwnHandler_DoesNotThrowAndArrivesNextPoll()
        {
            // Aynı uç kendi Poll'u sırasında hem alır hem karşıya yollar; karşı uç da aynı turda yanıtlar.
            Connect();
            int pingPongs = 0;
            _host.MessageReceived += m => { pingPongs++; if (m[0] < 3) _host.Send(new byte[] { (byte)(m[0] + 1) }, true); };
            _client.MessageReceived += m => { pingPongs++; if (m[0] < 3) _client.Send(new byte[] { (byte)(m[0] + 1) }, true); };

            _host.Send(new byte[] { 0 }, true);
            for (int i = 0; i < 4; i++) { _client.Poll(); _host.Poll(); }

            Assert.AreEqual(4, pingPongs);
        }

        [Test]
        public void CodecOverTransport_EndToEnd()
        {
            Connect();
            var raceId = Guid.NewGuid();

            _host.Send(RaceMessageCodec.Encode(new HelloMessage
            {
                ProtocolVersion = 1, Mode = RaceMode.Race, RaceId = raceId, PlayerName = "Eren"
            }), reliable: true);
            _client.Poll();

            Assert.IsTrue(RaceMessageCodec.TryDecode(_clientReceived[0], out IRaceMessage decoded));
            var hello = (HelloMessage)decoded;
            Assert.AreEqual(raceId, hello.RaceId);
            Assert.AreEqual("Eren", hello.PlayerName);
        }
    }
}
