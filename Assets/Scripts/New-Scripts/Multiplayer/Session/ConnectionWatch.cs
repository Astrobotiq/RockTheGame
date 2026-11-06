using System;

namespace New_Scripts.Multiplayer.Session
{
    /// <summary>
    /// Taşıyıcıdan bağımsız kopma algısı: SilenceSeconds boyunca rakipten hiç paket gelmezse rakip koptu sayılır.
    /// Kopunca ReconnectWindowSeconds beklenir; dolarsa yarış kaydedilip lobiye dönülür.
    /// </summary>
    public sealed class ConnectionWatch
    {
        public const double SilenceSeconds = 3.0;
        public const double ReconnectWindowSeconds = 30.0;

        private double _lastPacketAt = double.NaN;
        private double _deadline;

        public bool IsWaitingForReconnect { get; private set; }

        public void OnPacket(double now) => _lastPacketAt = now;

        public void Reset() => _lastPacketAt = double.NaN;

        /// <summary>Hiç paket görülmediyse sessiz sayılmaz (bağlantı yeni kuruluyor olabilir).</summary>
        public bool IsSilent(double now) => !double.IsNaN(_lastPacketAt) && now - _lastPacketAt > SilenceSeconds;

        public void BeginReconnectWait(double now)
        {
            IsWaitingForReconnect = true;
            _deadline = now + ReconnectWindowSeconds;
        }

        public void EndReconnectWait()
        {
            IsWaitingForReconnect = false;
            Reset();
        }

        public int SecondsLeft(double now) => IsWaitingForReconnect ? (int)Math.Ceiling(Math.Max(0, _deadline - now)) : 0;

        public bool HasExpired(double now) => IsWaitingForReconnect && now >= _deadline;
    }
}
