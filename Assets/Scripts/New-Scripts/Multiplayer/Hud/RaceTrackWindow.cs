using System;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>Oda aralığı, iki uç dahil.</summary>
    public readonly struct RoomRange
    {
        public readonly int Lo;
        public readonly int Hi;
        public RoomRange(int lo, int hi) { Lo = lo; Hi = hi; }
        public int Count => Hi - Lo + 1;
    }

    /// <summary>
    /// Pist şeridinin gösterdiği odalar. Bölünmemişse yalnız <see cref="First"/> dolu;
    /// bölünmüşse <see cref="First"/> geride olanın, <see cref="Second"/> öndekinin parçasıdır.
    /// </summary>
    public readonly struct RaceTrackLayout
    {
        public readonly bool IsSplit;
        public readonly RoomRange First;
        public readonly RoomRange Second;
        /// <summary>Bölünmüşken iki parça arasında gösterilmeyen oda sayısı; bölünmemişse 0.</summary>
        public readonly int HiddenRooms;

        public RaceTrackLayout(RoomRange window) { IsSplit = false; First = window; Second = default; HiddenRooms = 0; }

        public RaceTrackLayout(RoomRange low, RoomRange high, int hidden)
        {
            IsSplit = true; First = low; Second = high; HiddenRooms = hidden;
        }
    }

    /// <summary>Yarış HUD'u pist penceresi (mercek), saf ve test edilir. docs/ui-research/race-hud-track.md → "Mercek".</summary>
    public static class RaceTrackWindow
    {
        public const int DefaultWindowWidth = 14;
        public const int DefaultSplitGap = 10;
        /// <summary>Bölünmüşken her oyuncunun etrafındaki parça genişliği.</summary>
        public const int SplitPieceWidth = 6;

        /// <summary>Odalar 1..<paramref name="totalRooms"/>. Fark <paramref name="splitGap"/>'ten büyükse iki parça, değilse tek pencere.</summary>
        public static RaceTrackLayout Compute(int totalRooms, int localRoom, int rivalRoom,
            int windowWidth = DefaultWindowWidth, int splitGap = DefaultSplitGap)
        {
            int lowRoom = Math.Min(localRoom, rivalRoom);
            int highRoom = Math.Max(localRoom, rivalRoom);

            if (highRoom - lowRoom <= splitGap)
                return new RaceTrackLayout(Clamp((lowRoom + highRoom) / 2 - (windowWidth - 1) / 2, windowWidth, totalRooms));

            // Parça: oyuncudan 2 oda geri, geri kalanı ileri (6 oda → r-2..r+3).
            int back = (SplitPieceWidth - 1) / 2;
            var low = Clamp(lowRoom - back, SplitPieceWidth, totalRooms);
            var high = Clamp(highRoom - back, SplitPieceWidth, totalRooms);
            return new RaceTrackLayout(low, high, high.Lo - low.Hi - 1);
        }

        /// <summary>Şeridin sağ altındaki fark yazısı: öndeki ve oda farkı, eşitse "level".</summary>
        public static string GapText(int localRoom, int rivalRoom)
        {
            int diff = localRoom - rivalRoom;
            if (diff == 0) return "level";
            return diff > 0 ? $"you +{diff}" : $"rival +{-diff}";
        }

        static RoomRange Clamp(int lo, int width, int totalRooms)
        {
            width = Math.Min(width, totalRooms);
            lo = Math.Max(1, Math.Min(lo, totalRooms - width + 1));
            return new RoomRange(lo, lo + width - 1);
        }
    }
}
