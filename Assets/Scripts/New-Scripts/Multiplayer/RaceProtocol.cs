namespace New_Scripts.Multiplayer
{
    /// <summary>
    /// Ağ protokolünün sabitleri. Sürüm, mesaj biçimi değiştiğinde artırılır;
    /// farklı sürümdeki iki oyun birbirine bağlanmaz (docs/systems/multiplayer.md).
    /// </summary>
    public static class RaceProtocol
    {
        public const ushort Version = 2; // 2: ReadyMessage (MP 7)
        public const int MaxPlayerNameLength = 32;
    }
}
