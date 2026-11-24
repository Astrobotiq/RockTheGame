using New_Scripts.Audio;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class NoteMathTests
    {
        private static readonly int[] MinorPentatonic = { 0, 3, 5, 7, 10 };

        [Test]
        public void Midi_FirstOctave_FollowsScaleSteps()
        {
            // A4 C5 D5 E5 G5
            int[] expected = { 69, 72, 74, 76, 79 };
            for (int i = 0; i < 5; i++)
                Assert.That(NoteMath.Midi(69, MinorPentatonic, i), Is.EqualTo(expected[i]));
        }

        [Test]
        public void Midi_IndexPastScale_WrapsToNextOctave()
        {
            Assert.That(NoteMath.Midi(69, MinorPentatonic, 5), Is.EqualTo(81)); // A5
            Assert.That(NoteMath.Midi(69, MinorPentatonic, 9), Is.EqualTo(91)); // G6
        }

        [Test]
        public void DegreeClassAndOctave_SplitTheIndex()
        {
            Assert.That(NoteMath.DegreeClass(7, 5), Is.EqualTo(2));
            Assert.That(NoteMath.Octave(7, 5), Is.EqualTo(1));
        }

        [Test]
        public void Frequency_A4Is440_AndOctaveDoubles()
        {
            Assert.That(NoteMath.Frequency(69), Is.EqualTo(440f).Within(1e-3f));
            Assert.That(NoteMath.Frequency(81), Is.EqualTo(880f).Within(1e-2f));
            Assert.That(NoteMath.Frequency(60), Is.EqualTo(261.626f).Within(1e-2f));
        }

        [Test]
        public void PitchRatio_ZeroIsOne_OctaveIsTwo_SemitoneIsTwelfthRoot()
        {
            Assert.That(NoteMath.PitchRatio(0), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(NoteMath.PitchRatio(12), Is.EqualTo(2f).Within(1e-5f));
            Assert.That(NoteMath.PitchRatio(-12), Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(NoteMath.PitchRatio(1), Is.EqualTo(1.05946f).Within(1e-4f));
        }

        [Test]
        public void NearestZone_PicksClosestRoot()
        {
            int[] roots = { 60, 67, 74 };
            Assert.That(NoteMath.NearestZone(roots, 61), Is.EqualTo(0));
            Assert.That(NoteMath.NearestZone(roots, 66), Is.EqualTo(1));
            Assert.That(NoteMath.NearestZone(roots, 80), Is.EqualTo(2));
        }

        [Test]
        public void NearestZone_Tie_PicksLowerIndex()
        {
            Assert.That(NoteMath.NearestZone(new[] { 60, 70 }, 65), Is.EqualTo(0));
        }

        [Test]
        public void NearestZone_NoZones_ReturnsMinusOne()
        {
            Assert.That(NoteMath.NearestZone(new int[0], 60), Is.EqualTo(-1));
            Assert.That(NoteMath.NearestZone(null, 60), Is.EqualTo(-1));
        }

        [Test]
        public void ThreeZonesTwoOctaves_EveryNoteStaysWithinMaxShift()
        {
            // A4–G6 (MIDI 69–91, 22 yarım ses) 3 bölgeyle ±4'ü geçmeden kapanabilir, ama kökler uçlara göre seçilmeli:
            // C5/G5/D6 (72/79/86) olsaydı G6 5 yarım ses uzakta kalırdı. Bu, örnek kaydı task'ının kök seçim kuralı.
            int[] roots = { 73, 80, 87 };
            for (int d = 0; d < 10; d++)
            {
                int midi = NoteMath.Midi(69, MinorPentatonic, d);
                int zone = NoteMath.NearestZone(roots, midi);
                Assert.That(System.Math.Abs(midi - roots[zone]), Is.LessThanOrEqualTo(NoteMath.MaxZoneShiftSemitones),
                    $"derece {d} (midi {midi})");
            }
        }
    }
}
