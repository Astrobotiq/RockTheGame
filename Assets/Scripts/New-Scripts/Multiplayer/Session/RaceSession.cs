using System;
using System.Collections.Generic;
using System.Linq;
using New_Scripts.Multiplayer.Messages;

namespace New_Scripts.Multiplayer.Session
{
    /// <summary>
    /// Yarış ve birlikte modlarının kuralları; ağ ve Unity bilmez.
    /// Yerel olay metotları karşıya gidecek mesajları liste olarak döndürür; karşıdan gelenler Receive ile verilir.
    /// </summary>
    public sealed class RaceSession
    {
        private readonly HashSet<int> _capturedRooms = new HashSet<int>();

        public RaceMode Mode { get; }
        public int LocalRoomId { get; private set; }
        public int LocalFlagCount { get; private set; }
        public IReadOnlyCollection<int> LocalCapturedRoomIds => _capturedRooms;
        public bool LocalFinished { get; private set; }

        /// <summary>0 = henüz bilinmiyor.</summary>
        public int OpponentRoomId { get; private set; }
        public int OpponentFlagCount { get; private set; }
        public bool OpponentFinished { get; private set; }
        public float OpponentElapsedSeconds { get; private set; }

        public RaceSession(RaceMode mode, int startRoomId = RaceRules.FirstRoomId)
        {
            Mode = mode;
            LocalRoomId = startRoomId;
        }

        public IReadOnlyList<IRaceMessage> OnLocalRoomEntered(int roomId)
        {
            if (LocalFinished || roomId == LocalRoomId) return Array.Empty<IRaceMessage>();

            LocalRoomId = roomId;
            return new IRaceMessage[] { new RoomEnteredMessage { RoomId = roomId } };
        }

        public IReadOnlyList<IRaceMessage> OnLocalFlagReached(int roomId)
        {
            if (LocalFinished || Mode != RaceMode.Race || !_capturedRooms.Add(roomId))
                return Array.Empty<IRaceMessage>();

            LocalFlagCount++;
            var sent = new List<IRaceMessage> { new FlagCapturedMessage { RoomId = roomId, NewCount = LocalFlagCount } };

            if (LocalFlagCount >= RaceRules.FlagsPerPenalty)
            {
                LocalFlagCount = 0;
                // Rakip ilk odadaysa ya da odası henüz bilinmiyorsa ceza boşa gider.
                if (OpponentRoomId > RaceRules.FirstRoomId)
                    sent.Add(new PenaltyMessage { TargetRoomId = OpponentRoomId - 1 });
            }

            return sent;
        }

        public IReadOnlyList<IRaceMessage> OnLocalFinished(float elapsedSeconds)
        {
            if (LocalFinished) return Array.Empty<IRaceMessage>();

            LocalFinished = true;
            return new IRaceMessage[] { new FinishedMessage { ElapsedSeconds = elapsedSeconds } };
        }

        public ProgressSyncMessage CreateProgressSync(float elapsedSeconds)
        {
            return new ProgressSyncMessage
            {
                RoomId = LocalRoomId,
                FlagCount = LocalFlagCount,
                CapturedRoomIds = _capturedRooms.OrderBy(id => id).ToArray(),
                ElapsedSeconds = elapsedSeconds
            };
        }

        public void Receive(IRaceMessage message)
        {
            switch (message)
            {
                case RoomEnteredMessage room:
                    OpponentRoomId = room.RoomId;
                    break;
            }
        }
    }
}
