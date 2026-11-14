using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace New_Scripts.Multiplayer.Lobby
{
    public enum LobbySide
    {
        Host,
        Join
    }

    /// <summary>
    /// Lobi ekranının (düzen B: iki yarı) saf kuralları: hangi yarı seçili, hangi yazı çıkar, A neye basar.
    /// Unity bileşeni ve ağ bilmez (yalnız Vector2/Mathf), EditMode'da test edilir. Metinler İngilizce (UR-12, 2026-11-12).
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

        /// <summary>Host oda açtı, rakip henüz gelmedi: sağ yarı boş beton, P2 kesik çizgili.</summary>
        public static bool WaitingForRival(bool active, bool isHost, bool opponentPresent) =>
            active && isHost && !opponentPresent;

        /// <summary>Host'un beklemesi bitti: rakip bu karede geldi (vazgeçmek ya da kopmak değil). Küp düşer, sağ yarı boyanır.</summary>
        public static bool RivalArrived(bool wasWaiting, bool active, bool isHost, bool opponentPresent) =>
            wasWaiting && active && isHost && opponentPresent;

        /// <summary>
        /// Sağ yarının boya süpürmesi: çapraz sınırdan başlayıp sağa ilerleyen dışbükey çokgen (sanat pikseli, y aşağı).
        /// <paramref name="t"/> 0 → boş dizi, 1 → sağ yarının tamamı. Sınır (<paramref name="boundaryTopX"/>, y = 0) ile
        /// (<paramref name="boundaryBottomX"/>, y = <paramref name="frameH"/>) arasındaki doğrunun uzantısıdır; <paramref name="pad"/>
        /// çerçeve dışına taşan boya payıdır.
        /// </summary>
        public static Vector2[] RightSweep(float t, float frameW, float frameH, float pad, float boundaryTopX, float boundaryBottomX)
        {
            t = Mathf.Clamp01(t);
            if (t <= 0f) return new Vector2[0];

            float top = -pad, bottom = frameH + pad;
            float slope = (boundaryBottomX - boundaryTopX) / frameH;
            float xTop = boundaryTopX + slope * top;
            float xBottom = boundaryTopX + slope * bottom;
            float edge = Mathf.Lerp(xBottom, frameW + pad, t);

            if (edge >= xTop)
                return new[] { new Vector2(xTop, top), new Vector2(edge, top), new Vector2(edge, bottom), new Vector2(xBottom, bottom) };

            float yCross = (edge - boundaryTopX) / slope;
            return new[] { new Vector2(edge, yCross), new Vector2(edge, bottom), new Vector2(xBottom, bottom) };
        }
    }
}
