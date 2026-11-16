using System;

namespace New_Scripts.Player.IFramePauseable
{
    /// <summary>
    /// Hit Stop mekanigine dahil olacak ve calismasi dondurulacak bilesenlerin uygulamasi gereken arayuz.
    /// </summary>
    public interface IFramePausable
    {
        void OnPauseStarted();
        void OnPauseEnded();
    }

    /// <summary>
    /// Hit Stop sistemi icin bilesenler arasi iletisimi saglayan ve bagimliliklari ortadan kaldiran statik olay sinifi.
    /// </summary>
    public static class HitStopEvents
    {
        /// <summary>Hit-stop sürerken true. Platform sürücüleri <c>FixedUpdate</c>'te buna bakıp durur. Yalnızca <c>HitStopManager</c> yazar.</summary>
        public static bool IsActive { get; internal set; }

        public static Action<float> RequestHitStop;
        public static Action HitStopStarted;
        public static Action HitStopEnded;
    }
}