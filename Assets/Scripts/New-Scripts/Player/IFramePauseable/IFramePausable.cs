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
        public static Action<float> RequestHitStop;
        public static Action HitStopStarted;
        public static Action HitStopEnded;
    }
}