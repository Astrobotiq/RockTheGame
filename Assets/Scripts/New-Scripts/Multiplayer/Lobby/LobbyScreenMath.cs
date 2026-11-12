using New_Scripts.Multiplayer.Messages;

namespace New_Scripts.Multiplayer.Lobby
{
    public enum LobbySide
    {
        Host,
        Join
    }

    /// <summary>
    /// Lobi ekranının (düzen B: iki yarı) saf kuralları: hangi yarı seçili, hangi yazı çıkar, A neye basar.
    /// Unity ve ağ bilmez, EditMode'da test edilir. Metinler İngilizce (UR-12, 2026-11-12).
    /// </summary>
    public static class LobbyScreenMath
    {
        public const float StickDeadzone = 0.5f;

        /// <summary>Sol çubuk / d-pad yatay girdisi: sola host, sağa join; ölü bölgede seçim değişmez.</summary>
        public static LobbySide Move(LobbySide current, float horizontal)
        {
            if (horizontal <= -StickDeadzone) return LobbySide.Host;
            if (horizontal >= StickDeadzone) return LobbySide.Join;
            return current;
        }

        public static string ModeName(RaceMode mode) => mode == RaceMode.Race ? "race" : "co-op";

        /// <summary>Host yarısının alt yazısı. İstemci olarak bağlanırken boş.</summary>
        public static string HostCaption(bool active, bool isHost, bool useInvite)
        {
            if (!active) return "START A ROOM";
            if (!isHost) return string.Empty;
            return useInvite ? "INVITE SENT" : "ROOM OPEN";
        }

        /// <summary>Join yarısının alt yazısı.</summary>
        public static string JoinCaption(bool active, bool isHost, bool opponentPresent, bool useInvite)
        {
            if (!active) return useInvite ? "ACCEPT AN INVITE" : "ENTER A ROOM";
            if (!isHost) return "JOINING";
            return opponentPresent ? "RIVAL" : "WAITING FOR P2";
        }

        /// <summary>A'ya basınca ne olur. Bağlanırken (active) A bir şey yapmaz, B vazgeçer; davet kipinde join davetten gelir.</summary>
        public static bool CanConfirm(bool active, LobbySide side, bool useInvite)
        {
            if (active) return false;
            return side == LobbySide.Host || !useInvite;
        }

        public static string ConnectionHint(bool useInvite, string inviteName) =>
            "Y  CONNECTION: " + (useInvite ? (string.IsNullOrEmpty(inviteName) ? "STEAM" : inviteName.ToUpperInvariant()) : "IP");

        public static string ResumeHint(bool on, int roomId, int flagCount) =>
            on ? $"X  CONTINUE: ROOM {roomId}  FLAG {flagCount}/3" : "X  CONTINUE: OFF";
    }
}
