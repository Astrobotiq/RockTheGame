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
    /// Her uygulama şu sözleşmeye uyar (bellek içi taşıyıcı daha sıkı davranır, ama oyun kodu ona güvenmemeli):
    /// <list type="bullet">
    /// <item>Olaylar yalnızca Poll() içinde, onu çağıran thread'de (ana thread) tetiklenir.</item>
    /// <item>Sıra: güvenilir mesajlar kendi aralarında sıralı ve kayıpsızdır; güvenilmez mesajlar kendi aralarında
    /// sıralıdır ama kaybolabilir. İki kanal birbirine göre sıralı DEĞİLDİR (örn. yeni odanın Snapshot'ı
    /// RoomEntered'dan önce gelebilir).</item>
    /// <item>MessageReceived'a verilen dizi alıcınındır; taşıyıcı onu yeniden kullanmaz, saklanabilir.</item>
    /// <item>Disconnect() çağıran uçta bir sonraki Poll'da Disconnected(LocalRequest) tetiklenir; alttaki kütüphane
    /// yerel bir olay vermiyorsa adaptör bunu kendisi üretir.</item>
    /// <item>Host()/Join() başarısız olursa (adres yok, reddedildi, zaman aşımı) hata, bir sonraki Poll'larda
    /// Disconnected(Rejected) ya da Disconnected(Timeout) olarak gelir; bu metotlar istisna atmaz.</item>
    /// <item>Karşı uç ayrılınca bu uç Disconnected olur ve dinlemeye kendiliğinden dönmez; yeniden bağlanmak için
    /// oturum katmanı Host()/Join()'i tekrar çağırır.</item>
    /// </list>
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
