using System;
using New_Scripts.Audio;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class NoteSequencerTests
    {
        private const int Degrees = 10; // 5 nota × 2 oktav
        private const int Instruments = 4;

        private static NoteSequencer Seeded(int seed)
        {
            var rng = new Random(seed);
            return new NoteSequencer(Degrees, Instruments, () => (float)rng.NextDouble());
        }

        [Test]
        public void Unchained_NeverRepeatsSameDegree()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var seq = Seeded(seed);
                int previous = -1;
                for (int i = 0; i < 500; i++)
                {
                    int degree = seq.Next(false).Degree;
                    Assert.That(degree, Is.Not.EqualTo(previous), $"seed {seed}, adım {i}");
                    previous = degree;
                }
            }
        }

        [Test]
        public void Unchained_StaysInRange_AndMovesAtMostThree()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var seq = Seeded(seed);
                int previous = seq.Next(false).Degree;
                for (int i = 0; i < 500; i++)
                {
                    int degree = seq.Next(false).Degree;
                    Assert.That(degree, Is.InRange(0, Degrees - 1));
                    Assert.That(Math.Abs(degree - previous), Is.InRange(1, 3), $"seed {seed}, adım {i}");
                    previous = degree;
                }
            }
        }

        [Test]
        public void Instrument_NeverRepeatsBackToBack_AndUsesAll()
        {
            var seq = Seeded(7);
            var seen = new bool[Instruments];
            int previous = -1;
            for (int i = 0; i < 400; i++)
            {
                int instrument = seq.Next(false).Instrument;
                Assert.That(instrument, Is.InRange(0, Instruments - 1));
                Assert.That(instrument, Is.Not.EqualTo(previous));
                seen[instrument] = true;
                previous = instrument;
            }
            Assert.That(seen, Is.All.True);
        }

        [Test]
        public void Chained_ClimbsByOneOrTwo_UntilTopThenWrapsToRoot()
        {
            var seq = Seeded(3);
            int previous = seq.Next(false).Degree;
            bool wrapped = false;
            for (int i = 0; i < 200; i++)
            {
                int degree = seq.Next(true).Degree;
                if (degree == 0)
                {
                    wrapped = true;
                    Assert.That(previous + 1, Is.GreaterThanOrEqualTo(Degrees - 1), "yalnız tepe aşılınca köke dönmeli");
                }
                else
                {
                    Assert.That(degree - previous, Is.InRange(1, 2));
                }
                previous = degree;
            }
            Assert.That(wrapped, Is.True);
        }

        [Test]
        public void Chained_ForcedRandomValues_StepOneThenTwo()
        {
            // random: ilk Pick(10) → 4, sonra climb 0.1 → +1, climb 0.9 → +2
            var values = new System.Collections.Generic.Queue<float>(new[] { 0.45f, 0.5f, 0.1f, 0.5f, 0.9f, 0.5f });
            var seq = new NoteSequencer(Degrees, Instruments, () => values.Dequeue());
            Assert.That(seq.Next(false).Degree, Is.EqualTo(4));
            Assert.That(seq.Next(true).Degree, Is.EqualTo(5));
            Assert.That(seq.Next(true).Degree, Is.EqualTo(7));
        }

        [Test]
        public void Walk_AtBottomEdge_FlipsDirectionUpward()
        {
            // İlk nota derece 0 (random 0.0); sonra adım 1 (0.0 < 0.6), yön aşağı (0.0 < 0.5) → -1 sınır dışı → yukarı döner.
            var values = new System.Collections.Generic.Queue<float>(new[] { 0.0f, 0.5f, 0.0f, 0.0f, 0.5f });
            var seq = new NoteSequencer(Degrees, Instruments, () => values.Dequeue());
            Assert.That(seq.Next(false).Degree, Is.EqualTo(0));
            Assert.That(seq.Next(false).Degree, Is.EqualTo(1));
        }

        [Test]
        public void Reset_StartsFreshAndForgetsInstrument()
        {
            var seq = Seeded(1);
            seq.Next(false);
            seq.Reset();
            Assert.That(seq.LastDegree, Is.EqualTo(-1));
            Assert.That(seq.LastInstrument, Is.EqualTo(-1));
        }

        [Test]
        public void SingleInstrument_AlwaysZero_AndSingleDegree_NoCrash()
        {
            var rng = new Random(0);
            var seq = new NoteSequencer(1, 1, () => (float)rng.NextDouble());
            for (int i = 0; i < 20; i++)
            {
                var choice = seq.Next(i % 2 == 0);
                Assert.That(choice.Degree, Is.EqualTo(0));
                Assert.That(choice.Instrument, Is.EqualTo(0));
            }
        }

        [Test]
        public void RandomReturningOne_DoesNotOverflowIndices()
        {
            var seq = new NoteSequencer(Degrees, Instruments, () => 0.9999999f);
            for (int i = 0; i < 10; i++)
            {
                var choice = seq.Next(false);
                Assert.That(choice.Degree, Is.InRange(0, Degrees - 1));
                Assert.That(choice.Instrument, Is.InRange(0, Instruments - 1));
            }
        }
    }
}
