using System;

namespace New_Scripts.Audio
{
    /// <summary>Sequencer'ın seçtiği nota: gam derece indeksi (oktav dahil) ve enstrüman indeksi.</summary>
    public readonly struct NoteChoice
    {
        public readonly int Degree;
        public readonly int Instrument;

        public NoteChoice(int degree, int instrument)
        {
            Degree = degree;
            Instrument = instrument;
        }
    }

    /// <summary>
    /// Bounce pad nota seçimi (saf mantık). Zincir dışında tekrarsız rastgele yürüyüş (±1 %60, ±2 %30, ±3 %10),
    /// zincirde (yere basmadan ardışık pad) gamda +1 ya da +2 derece tırmanış; tepeyi aşınca köke döner.
    /// Enstrüman rastgele seçilir ama aynısı arka arkaya gelmez. Rastgelelik dışarıdan verilir (testlenebilir).
    /// </summary>
    public sealed class NoteSequencer
    {
        private const float StepOneWeight = 0.6f;
        private const float StepTwoWeight = 0.3f; // geri kalan ±3

        private readonly int degreeCount;
        private readonly int instrumentCount;
        private readonly Func<float> random; // [0,1)

        private int lastDegree = -1;
        private int lastInstrument = -1;

        public int LastDegree => lastDegree;
        public int LastInstrument => lastInstrument;

        public NoteSequencer(int degreeCount, int instrumentCount, Func<float> random)
        {
            if (degreeCount < 1) throw new ArgumentOutOfRangeException(nameof(degreeCount));
            if (instrumentCount < 1) throw new ArgumentOutOfRangeException(nameof(instrumentCount));
            this.degreeCount = degreeCount;
            this.instrumentCount = instrumentCount;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>Sıradaki notayı seçer. <paramref name="chained"/>: oyuncu zincirde mi (yere basmadan pad'den pad'e)?</summary>
        public NoteChoice Next(bool chained)
        {
            int degree;
            if (lastDegree < 0) degree = Pick(degreeCount);
            else if (chained) degree = Climb(lastDegree);
            else degree = Walk(lastDegree);

            int instrument = NextInstrument();
            lastDegree = degree;
            lastInstrument = instrument;
            return new NoteChoice(degree, instrument);
        }

        /// <summary>Hafızayı sıfırlar (oda değişimi gibi); sonraki nota rastgele başlar.</summary>
        public void Reset()
        {
            lastDegree = -1;
            lastInstrument = -1;
        }

        private int Climb(int from)
        {
            int next = from + (random() < 0.5f ? 1 : 2);
            return next >= degreeCount ? 0 : next;
        }

        private int Walk(int from)
        {
            if (degreeCount < 2) return 0;

            float r = random();
            int step = r < StepOneWeight ? 1 : r < StepOneWeight + StepTwoWeight ? 2 : 3;
            int dir = random() < 0.5f ? -1 : 1;

            int target = from + dir * step;
            if (target < 0 || target >= degreeCount) target = from - dir * step; // sınırda yön değiştir
            target = Math.Max(0, Math.Min(degreeCount - 1, target));
            if (target == from) target = from > 0 ? from - 1 : from + 1; // dar aralıkta tekrar olmasın
            return target;
        }

        private int NextInstrument()
        {
            if (instrumentCount < 2) return 0;
            if (lastInstrument < 0) return Pick(instrumentCount);

            int pick = Pick(instrumentCount - 1);
            return pick >= lastInstrument ? pick + 1 : pick; // son enstrümanı atla
        }

        private int Pick(int count) => Math.Min(count - 1, (int)(random() * count));
    }
}
