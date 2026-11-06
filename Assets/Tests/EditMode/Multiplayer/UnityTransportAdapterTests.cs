using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Unity Transport adaptörünün IRaceTransport sözleşmesine uyduğu, gerçek localhost bağlantısıyla.
    /// docs/systems/multiplayer.md → "Bileşenler" (UnityTransportAdapter).
    /// </summary>
    public class UnityTransportAdapterTests
    {
        private readonly List<IRaceTransport> _created = new List<IRaceTransport>();

        [TearDown]
        public void TearDown()
        {
            foreach (var t in _created) t.Dispose();
            _created.Clear();
        }

        private static ushort FreePort()
        {
            using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            return (ushort)((IPEndPoint)probe.Client.LocalEndPoint).Port;
        }

        private UnityTransportAdapter Create(ushort port)
        {
            var settings = UnityTransportSettings.Default;
            settings.Port = port;
            settings.ConnectTimeoutMS = 100;
            settings.MaxConnectAttempts = 5;
            var t = new UnityTransportAdapter(settings);
            _created.Add(t);
            return t;
        }

        private static bool PumpUntil(Func<bool> condition, params IRaceTransport[] transports)
        {
            for (int i = 0; i < 400; i++)
            {
                foreach (var t in transports) t.Poll();
                if (condition()) return true;
                Thread.Sleep(5);
            }
            return false;
        }

        private (UnityTransportAdapter host, UnityTransportAdapter client) ConnectedPair()
        {
            ushort port = FreePort();
            var host = Create(port);
            var client = Create(port);
            host.Host();
            client.Join("127.0.0.1");
            Assert.IsTrue(PumpUntil(() => host.State == TransportState.Connected && client.State == TransportState.Connected, host, client),
                $"Bağlanamadı: host {host.State}, client {client.State}");
            return (host, client);
        }

        [Test]
        public void HostAndJoin_Localhost_BothConnect()
        {
            int hostConnected = 0, clientConnected = 0;
            ushort port = FreePort();
            var host = Create(port);
            var client = Create(port);
            host.Connected += () => hostConnected++;
            client.Connected += () => clientConnected++;

            host.Host();
            client.Join($"127.0.0.1:{port}");

            Assert.IsTrue(PumpUntil(() => hostConnected == 1 && clientConnected == 1, host, client));
            Assert.AreEqual(TransportState.Connected, host.State);
            Assert.AreEqual(TransportState.Connected, client.State);
        }

        [Test]
        public void ReliableMessages_ArriveInOrder()
        {
            var (host, client) = ConnectedPair();
            var received = new List<byte>();
            client.MessageReceived += m => received.Add(m[0]);

            for (byte i = 1; i <= 20; i++) Assert.IsTrue(host.Send(new[] { i }, reliable: true));

            Assert.IsTrue(PumpUntil(() => received.Count == 20, host, client), $"{received.Count}/20 geldi");
            for (int i = 0; i < 20; i++) Assert.AreEqual(i + 1, received[i]);
        }

        [Test]
        public void UnreliableMessage_Arrives()
        {
            var (host, client) = ConnectedPair();
            byte[] got = null;
            host.MessageReceived += m => got = m;

            Assert.IsTrue(client.Send(new byte[] { 7, 8, 9 }, reliable: false));

            Assert.IsTrue(PumpUntil(() => got != null, host, client));
            CollectionAssert.AreEqual(new byte[] { 7, 8, 9 }, got);
        }

        [Test]
        public void Send_CopiesPayload()
        {
            var (host, client) = ConnectedPair();
            byte[] got = null;
            client.MessageReceived += m => got = m;
            byte[] buffer = { 10, 20 };

            host.Send(buffer, reliable: true);
            buffer[0] = 99;

            Assert.IsTrue(PumpUntil(() => got != null, host, client));
            CollectionAssert.AreEqual(new byte[] { 10, 20 }, got);
        }

        [Test]
        public void Send_BeforeConnect_ReturnsFalse()
        {
            var host = Create(FreePort());
            Assert.IsFalse(host.Send(new byte[] { 1 }, reliable: true));

            host.Host();
            Assert.IsFalse(host.Send(new byte[] { 1 }, reliable: true));
        }

        [Test]
        public void Disconnect_RaisesLocalRequestAndRemoteLeft()
        {
            var (host, client) = ConnectedPair();
            DisconnectReason? hostReason = null, clientReason = null;
            host.Disconnected += r => hostReason = r;
            client.Disconnected += r => clientReason = r;

            client.Disconnect();

            Assert.IsTrue(PumpUntil(() => hostReason.HasValue && clientReason.HasValue, host, client),
                $"host {hostReason}, client {clientReason}");
            Assert.AreEqual(DisconnectReason.LocalRequest, clientReason);
            Assert.AreEqual(DisconnectReason.RemoteLeft, hostReason);
            Assert.AreEqual(TransportState.Disconnected, host.State);
            Assert.AreEqual(TransportState.Disconnected, client.State);
            Assert.IsFalse(host.Send(new byte[] { 1 }, reliable: true));
        }

        [Test]
        public void Join_ClosedPort_RaisesTimeoutWithoutThrowing()
        {
            var client = Create(FreePort());
            DisconnectReason? reason = null;
            client.Disconnected += r => reason = r;

            Assert.DoesNotThrow(() => client.Join("127.0.0.1"));

            Assert.IsTrue(PumpUntil(() => reason.HasValue, client), $"durum {client.State}");
            Assert.AreEqual(DisconnectReason.Timeout, reason);
            Assert.AreEqual(TransportState.Disconnected, client.State);
        }

        [Test]
        public void SecondClient_IsRejected_FirstStaysConnected()
        {
            ushort port = FreePort();
            var host = Create(port);
            var first = Create(port);
            var second = Create(port);
            host.Host();
            first.Join("127.0.0.1");
            Assert.IsTrue(PumpUntil(() => host.State == TransportState.Connected && first.State == TransportState.Connected, host, first));

            DisconnectReason? secondReason = null;
            second.Disconnected += r => secondReason = r;
            second.Join("127.0.0.1");

            Assert.IsTrue(PumpUntil(() => secondReason.HasValue, host, first, second), $"ikinci {second.State}");
            Assert.AreEqual(TransportState.Connected, host.State);
            Assert.AreEqual(TransportState.Connected, first.State);

            byte[] got = null;
            first.MessageReceived += m => got = m;
            host.Send(new byte[] { 5 }, reliable: true);
            Assert.IsTrue(PumpUntil(() => got != null, host, first));
        }

        [Test]
        public void Host_PortInUse_RaisesRejectedWithoutThrowing()
        {
            ushort port = FreePort();
            using var blocker = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            var host = Create(port);
            DisconnectReason? reason = null;
            host.Disconnected += r => reason = r;
            // Unity Transport bağlanamayınca kendisi bir hata logu yazar; davranış (Rejected) test edilir.
            LogAssert.Expect(LogType.Error, new Regex("Failed to bind UDP socket"));

            Assert.DoesNotThrow(() => host.Host());

            Assert.IsTrue(PumpUntil(() => reason.HasValue, host), $"durum {host.State}");
            Assert.AreEqual(DisconnectReason.Rejected, reason);
        }

        [Test]
        public void HundredReliableMessages_AllArriveInOrder()
        {
            var (host, client) = ConnectedPair();
            var received = new List<int>();
            client.MessageReceived += m => received.Add(BitConverter.ToInt32(m, 0));

            for (int i = 0; i < 100; i++) Assert.IsTrue(host.Send(BitConverter.GetBytes(i), reliable: true));

            Assert.IsTrue(PumpUntil(() => received.Count == 100, host, client), $"{received.Count}/100 geldi");
            for (int i = 0; i < 100; i++) Assert.AreEqual(i, received[i]);
        }

        [Test]
        public void CodecAndRaceSession_OverUnityTransport_DeliverPenalty()
        {
            var (hostTransport, clientTransport) = ConnectedPair();
            var a = new RaceSession(RaceMode.Race);
            var b = new RaceSession(RaceMode.Race);
            hostTransport.MessageReceived += bytes => { if (RaceMessageCodec.TryDecode(bytes, out var m)) a.Receive(m); };
            clientTransport.MessageReceived += bytes => { if (RaceMessageCodec.TryDecode(bytes, out var m)) b.Receive(m); };
            bool penalty = false;
            b.PenaltyReceived += () => penalty = true;

            foreach (var m in b.OnLocalRoomEntered(6)) clientTransport.Send(RaceMessageCodec.Encode(m), true);
            Assert.IsTrue(PumpUntil(() => a.OpponentRoomId == 6, hostTransport, clientTransport));

            for (int room = 1; room <= 3; room++)
                foreach (var m in a.OnLocalFlagReached(room)) hostTransport.Send(RaceMessageCodec.Encode(m), true);

            Assert.IsTrue(PumpUntil(() => penalty, hostTransport, clientTransport));
            Assert.IsTrue(b.TryGetPenaltyTarget(out int target));
            Assert.AreEqual(5, target);
        }
    }
}
