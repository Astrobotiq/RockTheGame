using System.Collections.Generic;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Hayalet, rakip yerel oyuncunun odasında ya da komşu odalarından birindeyse görünür (komşular zaten aktif).
    /// Rakibin odası bilinmiyorsa (0) gizli.
    /// </summary>
    public static class GhostVisibility
    {
        public static bool IsVisible(int opponentRoomId, int localRoomId, IReadOnlyCollection<int> localNeighborRoomIds)
        {
            if (opponentRoomId <= 0) return false;
            if (opponentRoomId == localRoomId) return true;
            if (localNeighborRoomIds == null) return false;
            foreach (int id in localNeighborRoomIds)
                if (id == opponentRoomId) return true;
            return false;
        }
    }
}
