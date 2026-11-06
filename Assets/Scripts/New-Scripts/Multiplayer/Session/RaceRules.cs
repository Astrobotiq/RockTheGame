namespace New_Scripts.Multiplayer.Session
{
    /// <summary>Yarış kurallarının sabitleri (docs/systems/multiplayer.md → "Kararlar").</summary>
    public static class RaceRules
    {
        /// <summary>Bu kadar bayrak toplayan oyuncu rakibini bir oda geri gönderir; sayaç sıfırlanır.</summary>
        public const int FlagsPerPenalty = 3;

        /// <summary>İlk odanın RoomId'si. Bu odadaki oyuncuya ceza işlemez.</summary>
        public const int FirstRoomId = 1;

        /// <summary>
        /// Cezada gidilecek oda: RoomId − 1 komşuysa o (ana yol); değilse komşular içinde bu odadan küçük en büyük kimlik
        /// (yan oda: RoomNew-22'nin öncesi 11). Komşu bilgisi yoksa RoomId − 1.
        /// </summary>
        public static int PreviousRoomId(int currentRoomId, System.Collections.Generic.IEnumerable<int> neighborRoomIds)
        {
            int best = 0;
            if (neighborRoomIds != null)
                foreach (int id in neighborRoomIds)
                {
                    if (id == currentRoomId - 1) return id;
                    if (id < currentRoomId && id > best) best = id;
                }
            return best > 0 ? best : currentRoomId - 1;
        }
    }
}
