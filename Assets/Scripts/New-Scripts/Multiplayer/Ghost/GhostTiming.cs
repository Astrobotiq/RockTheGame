namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayalet senkronunun sabitleri (docs/systems/multiplayer.md → "Mesajlar").</summary>
    public static class GhostTiming
    {
        /// <summary>Saniyedeki snapshot sayısı; gönderenin zaman ekseni Sequence / SnapshotRate.</summary>
        public const int SnapshotRate = 20;

        /// <summary>Hayalet bu kadar geriden çizilir: 20 Hz'de bir kayıp paketi takılmadan atlatır.</summary>
        public const float InterpolationDelaySeconds = 0.1f;

        /// <summary>İki ardışık snapshot arasında bundan büyük sıçrama ışınlanma sayılır (ölüm, ceza).</summary>
        public const float TeleportDistance = 3f;
    }
}
