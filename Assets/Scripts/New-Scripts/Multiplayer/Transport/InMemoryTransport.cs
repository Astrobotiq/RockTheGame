using System;
using System.Collections.Generic;

namespace New_Scripts.Multiplayer.Transport
{
    /// <summary>
    /// Aynı süreçteki iki ucu birbirine bağlayan taşıyıcı. EditMode testlerinde gerçek ağ olmadan
    /// uçtan uca akışı denemek için. Güvenilir ve güvenilmez mesajlar aynı şekilde, kayıpsız ve sırayla teslim edilir.
    /// </summary>
    public sealed class InMemoryTransport : IRaceTransport
    {
        private InMemoryTransport _peer;
        private bool _wantsConnection;
        private readonly Queue<byte[]> _inbox = new Queue<byte[]>();
        private DisconnectReason? _pendingDisconnect;

        public TransportState State { get; private set; } = TransportState.Disconnected;

        public event Action Connected;
        public event Action<DisconnectReason> Disconnected;
        public event Action<byte[]> MessageReceived;

        private InMemoryTransport() { }

        public static (InMemoryTransport host, InMemoryTransport client) CreatePair()
        {
            var host = new InMemoryTransport();
            var client = new InMemoryTransport();
            host._peer = client;
            client._peer = host;
            return (host, client);
        }

        public void Host() => BeginConnecting();

        public void Join(string address) => BeginConnecting();

        public void Disconnect()
        {
            if (State == TransportState.Disconnected && !_wantsConnection) return;

            DropConnection(DisconnectReason.LocalRequest);
            _peer.OnRemoteLeft();
        }

        public bool Send(byte[] payload, bool reliable)
        {
            if (State != TransportState.Connected || payload == null) return false;

            _peer._inbox.Enqueue((byte[])payload.Clone());
            return true;
        }

        public void Poll()
        {
            if (_pendingDisconnect.HasValue)
            {
                DisconnectReason reason = _pendingDisconnect.Value;
                _pendingDisconnect = null;
                Disconnected?.Invoke(reason);
                return;
            }

            if (State == TransportState.Connecting && _peer._wantsConnection)
            {
                State = TransportState.Connected;
                Connected?.Invoke();
            }

            if (State != TransportState.Connected || _inbox.Count == 0) return;

            // İşleyiciler bu sırada Send/Disconnect çağırabilir: anlık kopya üzerinde dön.
            byte[][] batch = _inbox.ToArray();
            _inbox.Clear();
            foreach (byte[] message in batch)
            {
                if (State != TransportState.Connected) break;
                MessageReceived?.Invoke(message);
            }
        }

        public void Dispose() => Disconnect();

        private void BeginConnecting()
        {
            if (State != TransportState.Disconnected) return;

            _wantsConnection = true;
            _pendingDisconnect = null;
            State = TransportState.Connecting;
        }

        private void OnRemoteLeft()
        {
            if (State == TransportState.Disconnected && !_wantsConnection) return;
            DropConnection(DisconnectReason.RemoteLeft);
        }

        private void DropConnection(DisconnectReason reason)
        {
            _wantsConnection = false;
            State = TransportState.Disconnected;
            _inbox.Clear();
            _pendingDisconnect = reason;
        }
    }
}
