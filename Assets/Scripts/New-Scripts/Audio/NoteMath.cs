using System;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Bounce pad notaları için saf matematik: gam derecesi → MIDI, MIDI → frekans/pitch oranı, en yakın örnek bölgesi.
    /// Unity'ye bağımlı değildir (EditMode testlenebilir).
    /// </summary>
    public static class NoteMath
    {
        /// <summary>Bir örneğin, kök notasından en fazla bu kadar yarım ses kaydırılması önerilir (ses-ve-nota.md).</summary>
        public const int MaxZoneShiftSemitones = 4;

        /// <summary>
        /// Gam derece indeksini MIDI notasına çevirir. İndeks gam uzunluğunu aşarsa bir sonraki oktava geçer
        /// (5 notalı gamda indeks 5 = bir oktav yukarıdaki kök).
        /// </summary>
        public static int Midi(int rootMidi, int[] scaleSteps, int degreeIndex)
        {
            if (scaleSteps == null || scaleSteps.Length == 0)
                throw new ArgumentException("scaleSteps boş olamaz.", nameof(scaleSteps));
            if (degreeIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(degreeIndex));

            int octave = degreeIndex / scaleSteps.Length;
            return rootMidi + octave * 12 + scaleSteps[degreeIndex % scaleSteps.Length];
        }

        /// <summary>Gam derece indeksinin gam içindeki sırası (0..gamUzunluğu-1); renk tablosunu seçer.</summary>
        public static int DegreeClass(int degreeIndex, int scaleLength) => degreeIndex % scaleLength;

        /// <summary>Gam derece indeksinin oktav numarası (0 = ilk oktav).</summary>
        public static int Octave(int degreeIndex, int scaleLength) => degreeIndex / scaleLength;

        /// <summary>MIDI notasının frekansı (Hz). A4 (69) = 440 Hz.</summary>
        public static float Frequency(int midi) => 440f * (float)Math.Pow(2.0, (midi - 69) / 12.0);

        /// <summary><c>AudioCuePlayParams.PitchMultiplier</c> için oran: 2^(yarımSes/12).</summary>
        public static float PitchRatio(int semitones) => (float)Math.Pow(2.0, semitones / 12.0);

        /// <summary>
        /// Hedef MIDI notasına kök notası en yakın olan örnek bölgesinin indeksi. Eşitlikte düşük indeks kazanır.
        /// Bölge yoksa -1.
        /// </summary>
        public static int NearestZone(int[] zoneRootMidis, int targetMidi)
        {
            if (zoneRootMidis == null || zoneRootMidis.Length == 0) return -1;

            int best = 0;
            int bestDistance = Math.Abs(zoneRootMidis[0] - targetMidi);
            for (int i = 1; i < zoneRootMidis.Length; i++)
            {
                int d = Math.Abs(zoneRootMidis[i] - targetMidi);
                if (d < bestDistance)
                {
                    best = i;
                    bestDistance = d;
                }
            }
            return best;
        }
    }
}
