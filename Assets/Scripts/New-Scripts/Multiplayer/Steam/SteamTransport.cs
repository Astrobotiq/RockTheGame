using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using New_Scripts.Multiplayer.Transport;
using Steamworks;
using Steamworks.Data;

namespace New_Scripts.Multiplayer.Steam
{
    /// <summary>
    /// IRaceTransport'un Steam karşılığı: SteamNetworkingSockets relay bağlantısı (NAT ve güvenlik duvarı sorunu yok).
    /// Host CreateRelaySocket ile dinler, istemci host'un SteamID'sine ConnectRelay ile bağlanır. Join'in adresi SteamID'dir.
    /// Sözleşme UnityTransportAdapter'la aynı: olaylar yalnız Poll içinde, ikinci istemci reddedilir, yerel Disconnect bir
    /// sonraki Poll'da LocalRequest verir, karşı uç ayrılınca host yeniden dinlemeye geçmez.
    /// Gönderimler Reliable|NoNagle / Unreliable|NoNagle.
    /// </summary>
    public sealed class SteamTransport : IRaceTransport
    {
        private readonly Queue<Action> _pending = new Queue<Action>();
        private SocketManager _socket;
        private ConnectionManager _client;
        private Connection _peer;
        private bool _hasPeer;
        private bool _isHost;
        private bool _wasConnected;

        public TransportState State { get; private set; } = TransportState.Disconnected;

        public event Action Connected;
        public event Action<DisconnectReason> Disconnected;
        public event Action<byte[]> MessageReceived;

        /// <summary>Bağlantı kurulunca (Steam lobisinden çıkmak için).</summary>
        public Action ConnectedHook;

        public void Host()
        {
            if (State != TransportState.Disconnected) return;
            _isHost = true;
            _wasConnected = false;
            try
            {
                _socket = SteamNetworkingSockets.CreateRelaySocket(0, new HostHandler(this));
            }
            catch (Exception)
            {
                _socket = null;
            }

            if (_socket == null)
            {
                _pending.Enqueue(() => Disconnected?.Invoke(DisconnectReason.Rejected));
                return;
            }
            State = TransportState.Connecting;
        }

        public void Join(string address)
        {
            if (State != TransportState.Disconnected) return;
            _isHost = false;
            _wasConnected = false;
            if (!ulong.TryParse(address, out ulong steamId))
            {
                _pending.Enqueue(() => Disconnected?.Invoke(DisconnectReason.Rejected));
                return;
            }

            try
            {
                _client = SteamNetworkingSockets.ConnectRelay(new SteamId { Value = steamId }, 0, new ClientHandler(this));
            }
            catch (Exception)
            {
                _client = null;
            }

            if (_client == null)
            {
                _pending.Enqueue(() => Disconnected?.Invoke(DisconnectReason.Rejected));
                return;
            }
            State = TransportState.Connecting;
        }

        public void Disconnect()
        {
            if (State == TransportState.Disconnected && _socket == null && _client == null) return;
            CloseAll();
            _pending.Enqueue(() => Disconnected?.Invoke(DisconnectReason.LocalRequest));
        }

        public bool Send(byte[] payload, bool reliable)
        {
            if (State != TransportState.Connected || payload == null) return false;
            var type = (reliable ? SendType.Reliable : SendType.Unreliable) | SendType.NoNagle;
            Connection connection = _isHost ? _peer : _client.Connection;
            Result result = connection.SendMessage(payload, type);
            // Güvenilmez mesaj kuyruk doluyken düşebilir; sözleşmeye göre kaybolabilir.
            return result == Result.OK || (!reliable && result == Result.LimitExceeded);
        }

        public void Poll()
        {
            if (SteamClient.IsValid) SteamClient.RunCallbacks();
            _socket?.Receive();
            _client?.Receive();

            // Yalnız bu Poll'da birikenler; handler'ın yenileri bir sonraki Poll'a kalır.
            int count = _pending.Count;
            for (int i = 0; i < count; i++) _pending.Dequeue().Invoke();
        }

        public void Dispose()
        {
            CloseAll();
            _pending.Clear();
        }

        // ---------------- Steam geri çağrıları (Poll içinden) ----------------

        private void OnPeerConnecting(Connection connection)
        {
            if (_hasPeer) connection.Close(); // ikinci istemci
            else connection.Accept();
        }

        private void OnPeerConnected(Connection connection)
        {
            if (_hasPeer) return;
            _peer = connection;
            _hasPeer = true;
            SetConnected();
        }

        private void OnPeerDisconnected(Connection connection, ConnectionInfo info)
        {
            if (!_hasPeer || connection.Id != _peer.Id) return;
            Lost(info);
        }

        private void SetConnected()
        {
            State = TransportState.Connected;
            _wasConnected = true;
            _pending.Enqueue(() => Connected?.Invoke());
            ConnectedHook?.Invoke();
        }

        private void Lost(ConnectionInfo info)
        {
            DisconnectReason reason = info.State == ConnectionState.ProblemDetectedLocally
                ? DisconnectReason.Timeout
                : _wasConnected ? DisconnectReason.RemoteLeft : DisconnectReason.Rejected;
            CloseAll();
            _pending.Enqueue(() => Disconnected?.Invoke(reason));
        }

        private void Deliver(IntPtr data, int size)
        {
            if (State != TransportState.Connected || size <= 0) return;
            var bytes = new byte[size];
            Marshal.Copy(data, bytes, 0, size);
            _pending.Enqueue(() => MessageReceived?.Invoke(bytes));
        }

        private void CloseAll()
        {
            try
            {
                if (_hasPeer) _peer.Close();
                _socket?.Close();
                _client?.Close();
            }
            catch (Exception)
            {
                // Steam kapanmışsa kapatma hatası önemsiz.
            }
            _hasPeer = false;
            _socket = null;
            _client = null;
            State = TransportState.Disconnected;
        }

        private sealed class HostHandler : ISocketManager
        {
            private readonly SteamTransport _owner;
            public HostHandler(SteamTransport owner) => _owner = owner;

            public void OnConnecting(Connection connection, ConnectionInfo info) => _owner.OnPeerConnecting(connection);
            public void OnConnected(Connection connection, ConnectionInfo info) => _owner.OnPeerConnected(connection);
            public void OnDisconnected(Connection connection, ConnectionInfo info) => _owner.OnPeerDisconnected(connection, info);

            public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                if (_owner._hasPeer && connection.Id == _owner._peer.Id) _owner.Deliver(data, size);
            }
        }

        private sealed class ClientHandler : IConnectionManager
        {
            private readonly SteamTransport _owner;
            public ClientHandler(SteamTransport owner) => _owner = owner;

            public void OnConnecting(ConnectionInfo info) { }
            public void OnConnected(ConnectionInfo info) => _owner.SetConnected();
            public void OnDisconnected(ConnectionInfo info) => _owner.Lost(info);
            public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel) => _owner.Deliver(data, size);
        }
    }
}
