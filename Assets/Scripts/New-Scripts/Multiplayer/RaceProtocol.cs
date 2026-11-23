namespace New_Scripts.Multiplayer
{
    /// <summary>
    /// Ağ protokolünün sabitleri. Sürüm, mesaj biçimi değiştiğinde artırılır;
    /// farklı sürümdeki iki oyun birbirine bağlanmaz (docs/systems/multiplayer.md).
    /// </summary>
    public static class RaceProtocol
    {
        public const ushort Version = 6; // 2: ReadyMessage (MP 7), 3: Hello.Resume (MP 8), 4: Snapshot taşıyıcı/node + DeathMessage (MPG2), 5: LeaveMessage (MPP2), 6: Snapshot.InBubble (BB6)
        public const int MaxPlayerNameLength = 32;
    }
}
