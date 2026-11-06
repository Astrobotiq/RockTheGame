using System;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>Multiplayer HUD'unun metinleri (saf, test edilir).</summary>
    public static class RaceHudText
    {
        public static string LocalFlags(int count) => $"Bayrak {count}/{RaceRules.FlagsPerPenalty}";

        public static string Opponent(RaceMode mode, int roomId, int flagCount)
        {
            if (roomId <= 0) return "Rakip: ?";
            return mode == RaceMode.Race
                ? $"Rakip: Oda {roomId} · Bayrak {flagCount}/{RaceRules.FlagsPerPenalty}"
                : $"Rakip: Oda {roomId}";
        }

        /// <summary>Kalan süre yukarı yuvarlanır (2.01 → "3"); sıfırda "BAŞLA!".</summary>
        public static string Countdown(float remainingSeconds) =>
            remainingSeconds <= 0f ? "BAŞLA!" : ((int)Math.Ceiling(remainingSeconds - 1e-4f)).ToString();

        /// <summary>Lobideki durum satırı; <paramref name="phase"/> null ise henüz oturum yok.</summary>
        public static string LobbyStatus(LobbyPhase? phase, bool isHost, string failureReason, int port = 7777)
        {
            if (phase == null) return "Mod seç, host ol ya da katıl";
            switch (phase.Value)
            {
                case LobbyPhase.Failed: return failureReason ?? "Bağlantı kurulamadı";
                case LobbyPhase.Idle:
                case LobbyPhase.Handshake: return isHost ? $"Rakip bekleniyor… (port {port})" : "Bağlanılıyor…";
                default: return "Yükleniyor…";
            }
        }

        public static string Status(LobbyPhase phase, TransportState transport, string failureReason)
        {
            if (phase == LobbyPhase.Failed) return failureReason ?? "Bağlantı kurulamadı";
            if (transport == TransportState.Disconnected) return "Bağlantı koptu";
            if (phase == LobbyPhase.WaitingForOpponent) return "Rakip yükleniyor…";
            return "";
        }
    }
}
