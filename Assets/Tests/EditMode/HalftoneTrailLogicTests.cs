using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class HalftoneTrailLogicTests
    {
        // ---- Gecikmeli örnekleme ----

        [Test]
        public void History_SampleInsideRange_InterpolatesLinearly()
        {
            var h = new TrailHistory(0.5f);
            h.Record(0f, new Vector2(0f, 0f), 0f);
            h.Record(1f, new Vector2(10f, 0f), 90f);
            Assert.That(h.TrySample(0.25f, out var pos, out var rot));
            Assert.That(pos.x, Is.EqualTo(2.5f).Within(1e-4f));
            Assert.That(rot, Is.EqualTo(22.5f).Within(1e-3f));
        }

        [Test]
        public void History_DelayedSample_ReturnsPastPositionNotCurrent()
        {
            var h = new TrailHistory(0.5f);
            for (int i = 0; i <= 10; i++) h.Record(i * 0.02f, new Vector2(i * 0.2f, 0f), 0f); // 10 birim/sn
            float now = 0.2f;
            Assert.That(h.TrySample(now - 0.12f, out var pink, out _));
            Assert.That(h.TrySample(now - 0.06f, out var yellow, out _));
            Assert.That(h.TrySample(now, out var teal, out _));
            Assert.That(teal.x, Is.EqualTo(2f).Within(1e-3f));
            Assert.That(yellow.x, Is.EqualTo(1.4f).Within(1e-3f));
            Assert.That(pink.x, Is.EqualTo(0.8f).Within(1e-3f));
        }

        [Test]
        public void History_SampleOlderThanOldest_ClampsToOldest()
        {
            var h = new TrailHistory(0.5f);
            h.Record(1f, new Vector2(3f, 0f), 0f);
            h.Record(1.1f, new Vector2(4f, 0f), 0f);
            Assert.That(h.TrySample(0.2f, out var pos, out _));
            Assert.That(pos.x, Is.EqualTo(3f).Within(1e-4f));
        }

        [Test]
        public void History_Empty_ReturnsFalse()
        {
            Assert.That(new TrailHistory(0.5f).TrySample(1f, out _, out _), Is.False);
        }

        [Test]
        public void History_Prunes_OldSamples()
        {
            var h = new TrailHistory(0.3f);
            for (int i = 0; i < 100; i++) h.Record(i * 0.02f, Vector2.zero, 0f);
            Assert.That(h.Count, Is.LessThan(25));
        }

        // ---- Teleport ----

        [Test]
        public void IsTeleport_AboveThreshold_True()
        {
            Assert.That(HalftoneTrailMath.IsTeleport(Vector2.zero, new Vector2(1.6f, 0f), 1.5f), Is.True);
            Assert.That(HalftoneTrailMath.IsTeleport(Vector2.zero, new Vector2(1.4f, 0f), 1.5f), Is.False);
            Assert.That(HalftoneTrailMath.IsTeleport(Vector2.zero, new Vector2(1.2f, 1.2f), 1.5f), Is.True);
        }

        [Test]
        public void History_ClearAfterTeleport_NoSampleFromBeforeTeleport()
        {
            var h = new TrailHistory(0.5f);
            h.Record(0f, new Vector2(0f, 0f), 0f);
            h.Record(0.02f, new Vector2(0.1f, 0f), 0f);
            h.Clear();
            h.Record(0.04f, new Vector2(20f, 0f), 0f);
            Assert.That(h.TrySample(0.04f - 0.12f, out var pos, out _));
            Assert.That(pos.x, Is.EqualTo(20f).Within(1e-4f)); // gecikmeli katman ışınlanma öncesi konuma gitmez
        }

        // ---- Emisyon birikimi ----

        [Test]
        public void Accumulate_CarriesFractionAcrossFrames()
        {
            float acc = 0f;
            int total = 0;
            for (int i = 0; i < 10; i++) total += HalftoneTrailMath.Accumulate(ref acc, 0.1f, 1.2f);
            Assert.That(total, Is.EqualTo(1)); // 1.2 birim·1.2 = 1.2 -> 1, kalan 0.2
            Assert.That(acc, Is.EqualTo(0.2f).Within(1e-4f));
        }

        [Test]
        public void Accumulate_DistanceOneUnit_EmitsRoundedRate()
        {
            float acc = 0f;
            Assert.That(HalftoneTrailMath.Accumulate(ref acc, 5f, 1.2f), Is.EqualTo(6));
        }

        [Test]
        public void Accumulate_ZeroOrNegativeDistance_EmitsNothing()
        {
            float acc = 0f;
            Assert.That(HalftoneTrailMath.Accumulate(ref acc, 0f, 1.2f), Is.EqualTo(0));
            Assert.That(HalftoneTrailMath.Accumulate(ref acc, -3f, 1.2f), Is.EqualTo(0));
        }

        [Test]
        public void DensityPerUnit_ScalesWithWidthClamped()
        {
            Assert.That(HalftoneTrailMath.DensityPerUnit(1.2f, 2f, 3f), Is.EqualTo(1.2f).Within(1e-4f));
            Assert.That(HalftoneTrailMath.DensityPerUnit(1.2f, 4.5f, 3f), Is.EqualTo(1.8f).Within(1e-4f));
            Assert.That(HalftoneTrailMath.DensityPerUnit(1.2f, 30f, 3f), Is.EqualTo(2.4f).Within(1e-4f));
        }

        // ---- Üst sınır ----

        [Test]
        public void Budget_PerInkCapReached_SkipsSpawn()
        {
            var b = new TrailBudget(18, 54);
            for (int i = 0; i < 18; i++) Assert.That(b.TryReserve(TrailInk.Teal, 0f, 1f), Is.True);
            Assert.That(b.TryReserve(TrailInk.Teal, 0f, 1f), Is.False);
            Assert.That(b.TryReserve(TrailInk.Yellow, 0f, 1f), Is.True); // diğer mürekkep etkilenmez
        }

        [Test]
        public void Budget_TotalCap_Respected()
        {
            var b = new TrailBudget(18, 20);
            for (int i = 0; i < 18; i++) b.TryReserve(TrailInk.Teal, 0f, 1f);
            for (int i = 0; i < 2; i++) Assert.That(b.TryReserve(TrailInk.Yellow, 0f, 1f), Is.True);
            Assert.That(b.TryReserve(TrailInk.Pink, 0f, 1f), Is.False);
            Assert.That(b.TotalAlive(0f), Is.EqualTo(20));
        }

        [Test]
        public void Budget_ExpiredParticlesFreeSlots()
        {
            var b = new TrailBudget(2, 54);
            b.TryReserve(TrailInk.Teal, 0f, 1f);
            b.TryReserve(TrailInk.Teal, 0f, 1f);
            Assert.That(b.TryReserve(TrailInk.Teal, 0.5f, 1f), Is.False);
            Assert.That(b.TryReserve(TrailInk.Teal, 1.1f, 1f), Is.True);
        }

        [Test]
        public void Budget_Clear_EmptiesAll()
        {
            var b = new TrailBudget(2, 54);
            b.TryReserve(TrailInk.Pink, 0f, 5f);
            b.Clear();
            Assert.That(b.TotalAlive(0f), Is.EqualTo(0));
        }

        // ---- Doğum hücresi ----

        [Test]
        public void TryPickCell_RespectsRearAndLowerHalf_AndSitsOnLattice()
        {
            var rng = new System.Random(7);
            Vector2 size = new Vector2(4f, 1f);
            Vector2 half = size * 0.5f;
            float pitch = 0.42f;
            int hits = 0;
            foreach (TrailInk ink in new[] { TrailInk.Teal, TrailInk.Yellow, TrailInk.Pink })
            {
                for (int i = 0; i < 200; i++)
                {
                    // Platform +x'e gidiyor: arka = -x
                    if (!HalftoneTrailMath.TryPickCell(rng, ink, size, Vector2.up, Vector2.right, pitch, out var c, out var v))
                        continue;
                    hits++;
                    Assert.That(v, Is.GreaterThan(0.4f));
                    Assert.That(c.x / size.x, Is.LessThanOrEqualTo(-0.1f + 1e-4f)); // arka %40
                    Assert.That(Mathf.Abs(c.x), Is.LessThanOrEqualTo(half.x + 1e-4f));
                    Assert.That(Mathf.Abs(c.y), Is.LessThanOrEqualTo(half.y + 1e-4f));

                    // Kafes üstünde: merkez, kafes uzayında yarım tam sayı katı
                    float rad = HalftoneTrailMath.AngleDegrees(ink) * Mathf.Deg2Rad;
                    float qx = (Mathf.Cos(rad) * c.x + Mathf.Sin(rad) * c.y) / pitch - 0.5f;
                    float qy = (-Mathf.Sin(rad) * c.x + Mathf.Cos(rad) * c.y) / pitch - 0.5f;
                    Assert.That(qx, Is.EqualTo(Mathf.Round(qx)).Within(1e-3f));
                    Assert.That(qy, Is.EqualTo(Mathf.Round(qy)).Within(1e-3f));
                }
            }
            Assert.That(hits, Is.GreaterThan(300));
        }

        [Test]
        public void DiameterFor_MatchesShaderTable()
        {
            float p = 0.42f;
            Assert.That(HalftoneTrailMath.DiameterFor(TrailInk.Yellow, 1f, p), Is.EqualTo(p * 0.85f).Within(1e-5f));
            Assert.That(HalftoneTrailMath.DiameterFor(TrailInk.Pink, 0.1f, p), Is.EqualTo(0f));
            Assert.That(HalftoneTrailMath.DiameterFor(TrailInk.Pink, 0.5f, p), Is.EqualTo(p * 0.46f).Within(1e-5f));
            Assert.That(HalftoneTrailMath.DiameterFor(TrailInk.Teal, 0.3f, p), Is.EqualTo(0f));
            Assert.That(HalftoneTrailMath.DiameterFor(TrailInk.Teal, 1f, p), Is.EqualTo(p * 0.58f).Within(1e-5f));
        }

        // ---- Gecikmeli mürekkep değişimi ----

        [Test]
        public void History_RecordsShiftFront_DelayedSampleSeesPastFront()
        {
            var h = new TrailHistory(0.5f);
            h.Record(0.00f, Vector2.zero, 0f, 0f, Vector2.zero);
            h.Record(0.10f, Vector2.zero, 0f, 0.6f, new Vector2(1f, 2f));
            h.Record(0.20f, Vector2.zero, 0f, 1.2f, new Vector2(1f, 2f));
            Assert.That(h.TrySample(0.20f, out _, out _, out float teal, out _));
            Assert.That(h.TrySample(0.20f - 0.12f, out _, out _, out float pink, out Vector2 origin));
            Assert.That(teal, Is.EqualTo(1.2f).Within(1e-4f));
            Assert.That(pink, Is.LessThan(teal)); // gecikmeli katman dalgayı geriden görür
            Assert.That(origin, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void IsInShiftBandAt_UsesDistanceFromOrigin()
        {
            Assert.That(HalftoneTrailMath.IsInShiftBandAt(new Vector2(3f, 0f), Vector2.zero, 3.5f, 1.2f), Is.True);
            Assert.That(HalftoneTrailMath.IsInShiftBandAt(new Vector2(6f, 0f), Vector2.zero, 3.5f, 1.2f), Is.False);
        }
    }
}
