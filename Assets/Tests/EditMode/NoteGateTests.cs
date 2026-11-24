using New_Scripts.Audio;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class NoteGateTests
    {
        // zincir penceresi 1.5, min aralık 0.08, en fazla 3 ses, ses süresi 1.2
        private static NoteGate Gate() => new NoteGate(1.5f, 0.08f, 3, 1.2f);

        [Test]
        public void FirstNote_IsAccepted_AndNotChained()
        {
            Assert.That(Gate().TryAcquire(10f, out bool chained), Is.True);
            Assert.That(chained, Is.False);
        }

        [Test]
        public void SecondNoteWithinWindow_IsChained()
        {
            var g = Gate();
            g.TryAcquire(10f, out _);
            Assert.That(g.TryAcquire(10.6f, out bool chained), Is.True);
            Assert.That(chained, Is.True);
        }

        [Test]
        public void NoteAfterWindow_IsNotChained()
        {
            var g = Gate();
            g.TryAcquire(10f, out _);
            Assert.That(g.TryAcquire(11.6f, out bool chained), Is.True);
            Assert.That(chained, Is.False);
        }

        [Test]
        public void NoteWithinMinInterval_IsRejected_AndDoesNotRestartTheInterval()
        {
            var g = Gate();
            g.TryAcquire(10f, out _);
            Assert.That(g.TryAcquire(10.05f, out _), Is.False);
            // Reddedilen vuruş aralığı sıfırlamamalı: 0.09 sn sonra (ilk notadan) kabul edilir.
            Assert.That(g.TryAcquire(10.09f, out _), Is.True);
        }

        [Test]
        public void FourthSimultaneousNote_IsRejected_UntilOldestEnds()
        {
            var g = Gate();
            Assert.That(g.TryAcquire(10.0f, out _), Is.True);
            Assert.That(g.TryAcquire(10.2f, out _), Is.True);
            Assert.That(g.TryAcquire(10.4f, out _), Is.True);
            Assert.That(g.TryAcquire(10.6f, out _), Is.False, "3 ses doluyken 4. atlanmalı");
            Assert.That(g.TryAcquire(11.3f, out _), Is.True, "ilk nota (10.0) 1.2 sn sonra bitti");
        }

        [Test]
        public void RejectedNote_DoesNotBreakTheChain()
        {
            var g = Gate();
            g.TryAcquire(10.0f, out _);
            g.TryAcquire(10.2f, out _);
            g.TryAcquire(10.4f, out _);
            g.TryAcquire(10.6f, out _); // reddedilir
            g.TryAcquire(11.3f, out bool chained);
            Assert.That(chained, Is.True, "son kabul 10.4, 0.9 sn önce, hâlâ pencerede");
        }

        [Test]
        public void ClockGoingBackwards_ResetsMemory()
        {
            var g = Gate();
            g.TryAcquire(100f, out _);
            g.TryAcquire(100.2f, out _);
            g.TryAcquire(100.4f, out _);
            // yeni Play oturumu: saat 0'a döndü
            Assert.That(g.TryAcquire(0.5f, out bool chained), Is.True);
            Assert.That(chained, Is.False);
        }
    }
}
