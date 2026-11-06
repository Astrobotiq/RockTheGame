using System;

namespace New_Scripts.Multiplayer.Transport
{
    public enum TransportState
    {
        Disconnected,
        Connecting,
        Connected
    }

    public enum DisconnectReason
    {
        LocalRequest,
        RemoteLeft,
        Timeout,
        Rejected
    }

    /// <summary>
    /// Oyun kodunun ağla konuştuğu tek arayüz. Arkasında bellek içi, Unity Transport ya da Steam taşıyıcısı olabilir.
    /// Olaylar yalnızca Poll() içinde, onu çağıran thread'de (ana thread) tetiklenir.
    /// </summary>
    public interface IRaceTransport : IDisposable
    {
        TransportState State { get; }

        event Action Connected;
        event Action<DisconnectReason> Disconnected;
        event Action<byte[]> MessageReceived;

        void Host();

        /// <summary>Unity Transport'ta "ip:port", Steam'de SteamID. Bellek içi taşıyıcı yok sayar.</summary>
        void Join(string address);

        void Disconnect();

        /// <summary>Payload kopyalanır; çağıran tamponu hemen yeniden kullanabilir. Bağlı değilse false döner.</summary>
        bool Send(byte[] payload, bool reliable);

        void Poll();
    }
}
