namespace New_Scripts.Multiplayer.Session
{
    /// <summary>Yarış kurallarının sabitleri (docs/systems/multiplayer.md → "Kararlar").</summary>
    public static class RaceRules
    {
        /// <summary>Bu kadar bayrak toplayan oyuncu rakibini bir oda geri gönderir; sayaç sıfırlanır.</summary>
        public const int FlagsPerPenalty = 3;

        /// <summary>İlk odanın RoomId'si. Bu odadaki oyuncuya ceza işlemez.</summary>
        public const int FirstRoomId = 1;
    }
}
