using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace New_Scripts.Multiplayer.Transport
{
    public struct UnityTransportSettings
    {
        public ushort Port;
        public int DisconnectTimeoutMS;
        public int HeartbeatTimeoutMS;
        public int ConnectTimeoutMS;
        public int MaxConnectAttempts;

        /// <summary>Oyun ayarları: kopma 5 sn'de algılanır (UTP varsayılanı 30 sn).</summary>
        public static UnityTransportSettings Default => new UnityTransportSettings
        {
            Port = 7777,
            DisconnectTimeoutMS = 5000,
            HeartbeatTimeoutMS = 500,
            ConnectTimeoutMS = 1000,
            MaxConnectAttempts = 10
        };
    }

    /// <summary>
    /// Unity Transport (NGO'suz) ile tek bağlantılı host/istemci. Localhost, yerel ağ ve doğrudan IP.
    /// IRaceTransport sözleşmesine uyar: olaylar yalnız Poll'da; yerel Disconnect ve Host/Join hataları
    /// bir sonraki Poll'da Disconnected olarak gelir; karşı uç ayrılınca sürücü kapanır, dinlemeye dönülmez.
    /// </summary>
    public sealed class UnityTransportAdapter : IRaceTransport
    {
        private readonly UnityTransportSettings _settings;
        private readonly Queue<byte[]> _pendingReliable = new Queue<byte[]>();
        private readonly Queue<DisconnectReason> _pendingDisconnects = new Queue<DisconnectReason>();

        private NetworkDriver _driver;
        private NetworkPipeline _reliablePipeline;
        private NetworkPipeline _unreliablePipeline;
        private NetworkConnection _connection;
        private bool _isHost;

        public TransportState State { get; private set; } = TransportState.Disconnected;

        public event Action Connected;
        public event Action<DisconnectReason> Disconnected;
        public event Action<byte[]> MessageReceived;

        public UnityTransportAdapter(UnityTransportSettings settings)
        {
            _settings = settings;
        }

        public void Host()
        {
            if (State != TransportState.Disconnected) return;

            CreateDriver();
            _isHost = true;
            if (_driver.Bind(NetworkEndpoint.AnyIpv4.WithPort(_settings.Port)) != 0 || _driver.Listen() != 0)
            {
                Fail(DisconnectReason.Rejected);
                return;
            }

            State = TransportState.Connecting;
        }

        public void Join(string address)
        {
            if (State != TransportState.Disconnected) return;

            if (!TryParseAddress(address, _settings.Port, out NetworkEndpoint endpoint))
            {
                _pendingDisconnects.Enqueue(DisconnectReason.Rejected);
                return;
            }

            CreateDriver();
            _isHost = false;
            _connection = _driver.Connect(endpoint);
            State = TransportState.Connecting;
        }

        public void Disconnect()
        {
            if (!_driver.IsCreated) return;

            if (_connection.IsCreated)
            {
                _driver.Disconnect(_connection);
                // Kopma paketi ScheduleFlushSend ile değil, sürücü güncellemesiyle gider; yoksa karşı uç 5 sn bekleyip Timeout görür.
                _driver.ScheduleUpdate().Complete();
            }

            DisposeDriver();
            _pendingDisconnects.Enqueue(DisconnectReason.LocalRequest);
        }

        public bool Send(byte[] payload, bool reliable)
        {
            if (State != TransportState.Connected || payload == null) return false;

            byte[] copy = (byte[])payload.Clone();
            if (reliable && _pendingReliable.Count > 0)
            {
                // Sıra bozulmasın: kuyrukta bekleyen varsa yenisi de arkasına girer.
                _pendingReliable.Enqueue(copy);
                return true;
            }

            int result = TrySend(copy, reliable);
            if (result == (int)StatusCode.NetworkSendQueueFull)
            {
                if (reliable) _pendingReliable.Enqueue(copy);
                return true; // güvenilmez mesaj kuyruk doluyken düşer; sözleşmeye göre kaybolabilir
            }

            return result == (int)StatusCode.Success;
        }

        public void Poll()
        {
            while (_pendingDisconnects.Count > 0)
                Disconnected?.Invoke(_pendingDisconnects.Dequeue());

            if (!_driver.IsCreated) return;

            _driver.ScheduleUpdate().Complete();

            if (_isHost) AcceptConnections();

            NetworkEvent.Type type;
            while (_driver.IsCreated && (type = _driver.PopEvent(out NetworkConnection connection, out DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                if (connection != _connection) continue; // reddedilen ikinci istemcinin olayları

                switch (type)
                {
                    case NetworkEvent.Type.Connect:
                        SetConnected();
                        break;
                    case NetworkEvent.Type.Data:
                        var buffer = new NativeArray<byte>(reader.Length, Allocator.Temp);
                        reader.ReadBytes(buffer);
                        byte[] bytes = buffer.ToArray();
                        buffer.Dispose();
                        MessageReceived?.Invoke(bytes);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        var reason = (Unity.Networking.Transport.Error.DisconnectReason)reader.ReadByte();
                        DisposeDriver();
                        Disconnected?.Invoke(Map(reason));
                        return;
                }
            }

            FlushPendingReliable();
        }

        public void Dispose()
        {
            Disconnect();
            _pendingDisconnects.Clear();
        }

        private void AcceptConnections()
        {
            NetworkConnection incoming;
            while ((incoming = _driver.Accept()) != default)
            {
                if (_connection.IsCreated)
                {
                    // Yalnız bir rakip: ikinci bağlantı hemen kapatılır.
                    _driver.Disconnect(incoming);
                    continue;
                }

                _connection = incoming;
                SetConnected();
            }
        }

        private void SetConnected()
        {
            if (State == TransportState.Connected) return;
            State = TransportState.Connected;
            Connected?.Invoke();
        }

        private int TrySend(byte[] payload, bool reliable)
        {
            NetworkPipeline pipeline = reliable ? _reliablePipeline : _unreliablePipeline;
            int status = _driver.BeginSend(pipeline, _connection, out DataStreamWriter writer);
            if (status != 0) return status;

            var data = new NativeArray<byte>(payload, Allocator.Temp);
            writer.WriteBytes(data);
            data.Dispose();

            status = _driver.EndSend(writer);
            if (status < 0) return status;

            // Gönderimi bir kare beklemeden yolla.
            _driver.ScheduleFlushSend().Complete();
            return 0;
        }

        private void FlushPendingReliable()
        {
            while (_driver.IsCreated && State == TransportState.Connected && _pendingReliable.Count > 0)
            {
                int result = TrySend(_pendingReliable.Peek(), reliable: true);
                if (result == (int)StatusCode.NetworkSendQueueFull) return;
                _pendingReliable.Dequeue();
            }
        }

        private void CreateDriver()
        {
            var settings = new NetworkSettings(Allocator.Temp);
            settings.WithNetworkConfigParameters(
                connectTimeoutMS: _settings.ConnectTimeoutMS,
                maxConnectAttempts: _settings.MaxConnectAttempts,
                disconnectTimeoutMS: _settings.DisconnectTimeoutMS,
                heartbeatTimeoutMS: _settings.HeartbeatTimeoutMS);
            _driver = NetworkDriver.Create(settings);
            settings.Dispose();

            _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            _unreliablePipeline = _driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
            _connection = default;
        }

        private void DisposeDriver()
        {
            if (_driver.IsCreated) _driver.Dispose();
            _driver = default;
            _connection = default;
            _pendingReliable.Clear();
            State = TransportState.Disconnected;
        }

        private void Fail(DisconnectReason reason)
        {
            DisposeDriver();
            _pendingDisconnects.Enqueue(reason);
        }

        private static DisconnectReason Map(Unity.Networking.Transport.Error.DisconnectReason reason)
        {
            switch (reason)
            {
                case Unity.Networking.Transport.Error.DisconnectReason.ClosedByRemote:
                    return DisconnectReason.RemoteLeft;
                case Unity.Networking.Transport.Error.DisconnectReason.Timeout:
                case Unity.Networking.Transport.Error.DisconnectReason.MaxConnectionAttempts:
                    return DisconnectReason.Timeout;
                default:
                    return DisconnectReason.Rejected;
            }
        }

        /// <summary>"ip" ya da "ip:port".</summary>
        private static bool TryParseAddress(string address, ushort defaultPort, out NetworkEndpoint endpoint)
        {
            endpoint = default;
            if (string.IsNullOrWhiteSpace(address)) return false;

            string host = address.Trim();
            ushort port = defaultPort;
            int colon = host.LastIndexOf(':');
            if (colon > 0)
            {
                if (!ushort.TryParse(host.Substring(colon + 1), out port)) return false;
                host = host.Substring(0, colon);
            }

            return NetworkEndpoint.TryParse(host, port, out endpoint);
        }
    }
}
