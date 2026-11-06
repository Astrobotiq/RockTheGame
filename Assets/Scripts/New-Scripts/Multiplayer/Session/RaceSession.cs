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

        /// <summary>
        /// Rakip ceza yolladı ve ceza bu oyuncuya işliyor. Hedef oda burada verilmez: ceza oda geçişi ya da respawn
        /// bitene kadar bekletilebilir; uygulayan taraf o an TryGetPenaltyTarget ile hedefi okur.
        /// </summary>
        public event Action PenaltyReceived;
        public event Action<int> OpponentRoomChanged;
        public event Action<int> OpponentFlagCountChanged;
        /// <summary>Rakip yarışı bitirdi; argüman rakibin süresi.</summary>
        public event Action<float> OpponentFinishedRace;

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
                // Ceza her zaman yollanır; boşa gidip gitmediğine alıcı karar verir (ilk odadaysa işlemez).
                // Burada bilinen oda eski olabilir (RoomEntered yolda); TargetRoomId yalnız bilgi.
                sent.Add(new PenaltyMessage { TargetRoomId = Math.Max(0, OpponentRoomId - 1) });
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

        /// <summary>
        /// Rakipten gelen mesajı işler. Rakibin odası yalnız güvenilir mesajlardan (RoomEntered, ProgressSync) alınır:
        /// Snapshot güvenilmez kanalda gelir ve RoomEntered'a göre sırasız olabilir. Hello ve Snapshot burada yok sayılır.
        /// </summary>
        public void Receive(IRaceMessage message)
        {
            switch (message)
            {
                case RoomEnteredMessage room:
                    SetOpponentRoom(room.RoomId);
                    break;
                case FlagCapturedMessage flag:
                    // Gönderen 3. bayrakta kendi sayacını sıfırladı; buradaki kopya da sıfırlanır (HUD önce 3'ü görür).
                    SetOpponentFlagCount(flag.NewCount);
                    if (flag.NewCount >= RaceRules.FlagsPerPenalty) SetOpponentFlagCount(0);
                    break;
                case PenaltyMessage _:
                    // Hedefi alıcı belirler: gecikme sırasında oda değişmiş olabilir, gönderenin TargetRoomId'si yalnız bilgi.
                    if (TryGetPenaltyTarget(out _)) PenaltyReceived?.Invoke();
                    break;
                case ProgressSyncMessage sync:
                    SetOpponentRoom(sync.RoomId);
                    SetOpponentFlagCount(sync.FlagCount);
                    break;
                case FinishedMessage finished:
                    if (OpponentFinished) break;
                    OpponentFinished = true;
                    OpponentElapsedSeconds = finished.ElapsedSeconds;
                    OpponentFinishedRace?.Invoke(finished.ElapsedSeconds);
                    break;
            }
        }

        /// <summary>
        /// Cezanın şu an uygulanacağı oda: yerel oyuncunun o anki odasının bir öncesi. Birlikte modunda, yarışı bitirmişken
        /// ya da ilk odadayken ceza işlemez ve false döner.
        /// </summary>
        public bool TryGetPenaltyTarget(out int roomId)
        {
            roomId = LocalRoomId - 1;
            return Mode == RaceMode.Race && !LocalFinished && LocalRoomId > RaceRules.FirstRoomId;
        }

        private void SetOpponentRoom(int roomId)
        {
            if (roomId == OpponentRoomId) return;
            OpponentRoomId = roomId;
            OpponentRoomChanged?.Invoke(roomId);
        }

        private void SetOpponentFlagCount(int count)
        {
            if (count == OpponentFlagCount) return;
            OpponentFlagCount = count;
            OpponentFlagCountChanged?.Invoke(count);
        }
    }
}
