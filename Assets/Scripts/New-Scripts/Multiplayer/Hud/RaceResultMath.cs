using System;
using New_Scripts.Multiplayer.Messages;

namespace New_Scripts.Multiplayer.Hud
{
    public enum RaceResultKind
    {
        /// <summary>Yerel oyuncu bitirdi, rakip henüz bitirmedi.</summary>
        Waiting,
        Win,
        Lose,
        Draw,
        Coop
    }

    /// <summary>Sonuç ekranının bir sütunu: 1. oyuncu silindir, 2. oyuncu küp.</summary>
    public readonly struct RaceResultColumn
    {
        public readonly bool Present;
        public readonly bool IsLocal;
        public readonly float Seconds;
        /// <summary>0..1; kazanan (ya da berabere/birlikte) 1, kaybeden süre oranında kısa.</summary>
        public readonly float Height;

        public RaceResultColumn(bool present, bool isLocal, float seconds, float height)
        {
            Present = present;
            IsLocal = isLocal;
            Seconds = seconds;
            Height = height;
        }
    }

    /// <summary>Sonuç ekranının hesabı (saf, EditMode'da test edilir). 1. oyuncu = host, 2. oyuncu = istemci.</summary>
    public static class RaceResultMath
    {
        /// <summary>Kaybeden sütun bundan kısa olmaz (süre farkı büyükse de okunur kalsın).</summary>
        public const float MinHeight = 0.45f;
        public const float DrawToleranceSeconds = 0.05f;

        public static RaceResultKind KindOf(RaceMode mode, bool opponentFinished, float localSeconds, float opponentSeconds)
        {
            if (!opponentFinished) return RaceResultKind.Waiting;
            if (mode == RaceMode.Coop) return RaceResultKind.Coop;
            if (Math.Abs(localSeconds - opponentSeconds) < DrawToleranceSeconds) return RaceResultKind.Draw;
            return localSeconds < opponentSeconds ? RaceResultKind.Win : RaceResultKind.Lose;
        }

        /// <summary>Kaybedenin yüksekliği: <c>kazanan süre / kaybeden süre</c>, <see cref="MinHeight"/> ile 1 arasına sıkıştırılır.</summary>
        public static float LoserHeight(float winnerSeconds, float loserSeconds)
        {
            if (loserSeconds <= 0f || winnerSeconds <= 0f) return 1f;
            float ratio = winnerSeconds / loserSeconds;
            if (ratio < MinHeight) return MinHeight;
            return ratio > 1f ? 1f : ratio;
        }

        /// <summary>
        /// İki sütunu (1. ve 2. oyuncu) verir. <paramref name="localIsFirst"/>: yerel oyuncu host (1. oyuncu) mu.
        /// Yerel bitirmediyse iki sütun da yok.
        /// </summary>
        public static void Columns(RaceMode mode, bool localIsFirst, bool localFinished, float localSeconds,
            bool opponentFinished, float opponentSeconds, out RaceResultColumn first, out RaceResultColumn second)
        {
            first = second = default;
            if (!localFinished) return;

            var kind = KindOf(mode, opponentFinished, localSeconds, opponentSeconds);
            float localHeight = 1f, opponentHeight = 1f;
            if (kind == RaceResultKind.Win) opponentHeight = LoserHeight(localSeconds, opponentSeconds);
            else if (kind == RaceResultKind.Lose) localHeight = LoserHeight(opponentSeconds, localSeconds);

            var local = new RaceResultColumn(true, true, localSeconds, localHeight);
            var opponent = new RaceResultColumn(opponentFinished, false, opponentSeconds, opponentHeight);
            first = localIsFirst ? local : opponent;
            second = localIsFirst ? opponent : local;
        }

        public static string Headline(RaceResultKind kind)
        {
            switch (kind)
            {
                case RaceResultKind.Win: return "Kazandın!";
                case RaceResultKind.Lose: return "Kaybettin.";
                case RaceResultKind.Draw: return "Berabere!";
                case RaceResultKind.Coop: return "Birlikte bitirdiniz!";
                default: return "Bitirdin!";
            }
        }

        /// <summary>Sütunun üstündeki etiket: "SEN 02:14.8" / "RAKİP 02:19.3" / "RAKİP …" (henüz bitirmedi).</summary>
        public static string Label(RaceResultColumn column) =>
            (column.IsLocal ? "SEN" : "RAKİP") + " " + (column.Present ? RaceHudText.FormatTime(column.Seconds) : "…");
    }
}
