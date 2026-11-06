namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayalet senkronunun sabitleri (docs/systems/multiplayer.md → "Mesajlar").</summary>
    public static class GhostTiming
    {
        /// <summary>Saniyedeki snapshot sayısı; gönderenin zaman ekseni Sequence / SnapshotRate.</summary>
        public const int SnapshotRate = 20;

        /// <summary>Hayalet bu kadar geriden çizilir: 20 Hz'de bir kayıp paketi takılmadan atlatır.</summary>
        public const float InterpolationDelaySeconds = 0.1f;

        /// <summary>
        /// Işınlanma eşiği hız olarak: iki snapshot arasındaki sıçrama TeleportSpeed * geçen süre + TeleportMargin'i
        /// aşarsa ışınlanma sayılır. Oyundaki en yüksek hız sapan (65 b/sn); 100 bununla güvenli payı bırakır.
        /// </summary>
        public const float TeleportSpeed = 100f;
        public const float TeleportMargin = 1f;
    }
}
