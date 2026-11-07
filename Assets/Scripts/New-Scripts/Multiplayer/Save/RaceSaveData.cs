using System;
using System.Linq;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;

namespace New_Scripts.Multiplayer.Save
{
    /// <summary>
    /// Bir oyuncunun yarış kaydı (JsonUtility ile yazılır). Her oyuncu yalnız kendi ilerlemesini tutar; rakibin son bilinen
    /// odası ve sayacı lobide özet göstermek için. Tek kişilik kayıttan bağımsız.
    /// </summary>
    [Serializable]
    public sealed class RaceSaveData
    {
        public string RaceIdText;
        public int ModeValue;
        public string OpponentName;
        public int RoomId;
        public int FlagCount;
        public int[] CapturedRoomIds = Array.Empty<int>();
        public float ElapsedSeconds;
        public int OpponentRoomId;
        public int OpponentFlagCount;
        public string SavedAtUtc;

        public Guid Id => Guid.TryParse(RaceIdText, out var id) ? id : Guid.Empty;
        public RaceMode RaceMode => (RaceMode)ModeValue;

        public static RaceSaveData From(RaceSession session, Guid raceId, string opponentName, float elapsedSeconds)
        {
            return new RaceSaveData
            {
                RaceIdText = raceId.ToString(),
                ModeValue = (int)session.Mode,
                OpponentName = opponentName ?? "",
                RoomId = session.LocalRoomId,
                FlagCount = session.LocalFlagCount,
                CapturedRoomIds = session.LocalCapturedRoomIds.OrderBy(id => id).ToArray(),
                ElapsedSeconds = elapsedSeconds,
                OpponentRoomId = session.OpponentRoomId,
                OpponentFlagCount = session.OpponentFlagCount,
                SavedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        /// <summary>Kayıttaki yerel durumla yeni bir RaceSession kurar.</summary>
        public RaceSession CreateSession()
        {
            var session = new RaceSession(RaceMode);
            session.RestoreLocal(RoomId, FlagCount, CapturedRoomIds);
            return session;
        }

        public bool IsValid => Id != Guid.Empty && RoomId > 0 && Enum.IsDefined(typeof(RaceMode), (byte)ModeValue);
    }
}
