using System;

namespace New_Scripts.Multiplayer.Transport
{
    /// <summary>
    /// Arkadaş davetiyle bağlanma (Steam). Multiplayer assembly'si Steam'i bilmez; Steam assembly'si açılışta kendini
    /// <see cref="InviteServices.Current"/>'a kaydeder. Kayıt yoksa ya da IsAvailable false ise lobi IP ile bağlanır.
    /// </summary>
    public interface IInviteService
    {
        string DisplayName { get; }
        bool IsAvailable { get; }
        string LocalPlayerName { get; }

        /// <summary>Bu servisin taşıyıcısı; Join'e verilen adres JoinRequested'dan gelir.</summary>
        IRaceTransport CreateTransport();

        /// <summary>Host olduktan sonra: arkadaş davet arayüzünü açar.</summary>
        void InviteFriends();

        /// <summary>Daveti kapatır (Vazgeç).</summary>
        void CloseInvites();

        /// <summary>Davet kabul edildi; argüman host'un adresi (Steam'de SteamID).</summary>
        event Action<string> JoinRequested;

        /// <summary>Lobi açılmadan önce gelen davet (oyun davetle başlatıldıysa); bir kez döner.</summary>
        string TakePendingJoin();
    }

    public static class InviteServices
    {
        public static IInviteService Current { get; set; }

        public static bool Available => Current != null && Current.IsAvailable;
    }
}
