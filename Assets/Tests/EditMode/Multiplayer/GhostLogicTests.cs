using System;
using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Hayaletin saf mantığı: snapshot üretimi, ~0.1 sn geriden ara değer, görünürlük.
    /// docs/superpowers/plans/2026-11-06-mp4-ghost.md → Task 1.
    /// </summary>
    public class GhostLogicTests
    {
        private const double Offset = 1.0;

        private static SnapshotMessage Snap(uint seq, float x, bool teleported = false, Vector2? left = null)
            => new SnapshotMessage { Sequence = seq, RoomId = 1, Position = new Vector2(x, 0f), Teleported = teleported, LeftAnchor = left };

        private static double Arrival(uint seq) => seq / (double)GhostTiming.SnapshotRate + Offset;

        // ---- SnapshotBuilder ----

        [Test]
        public void Build_MapsPoseAndCountsSequence()
        {
            var builder = new SnapshotBuilder();
            var input = new PlayerPoseInput { RoomId = 4, Position = new Vector2(1f, 2f), Velocity = new Vector2(-3f, 5f), FacingRight = false, IsGrounded = true };

            var first = builder.Build(input, 0.0);
            var second = builder.Build(input, 0.05);
            var third = builder.Build(input, 0.10);

            Assert.AreEqual(0u, first.Sequence);
            Assert.AreEqual(1u, second.Sequence);
            Assert.AreEqual(2u, third.Sequence);
            Assert.AreEqual(4, first.RoomId);
            Assert.AreEqual(new Vector2(1f, 2f), first.Position);
            Assert.AreEqual(3f, first.Speed);
            Assert.AreEqual(5f, first.VerticalVelocity);
            Assert.IsFalse(first.FacingRight);
            Assert.IsTrue(first.IsGrounded);
        }

        [Test]
        public void Build_DashCountIncrementsOncePerDashEntry()
        {
            var builder = new SnapshotBuilder();
            bool[] dashing = { false, true, true, false, true };
            byte[] expected = { 0, 1, 1, 1, 2 };

            for (int i = 0; i < dashing.Length; i++)
            {
                var snapshot = builder.Build(new PlayerPoseInput { IsDashing = dashing[i] }, i * 0.05);
                Assert.AreEqual(expected[i], snapshot.DashCount, $"adım {i}");
                Assert.AreEqual(dashing[i], snapshot.IsDashing);
            }
        }

        [Test]
        public void Build_LargeJump_SetsTeleportedOnce()
        {
            var builder = new SnapshotBuilder();
            float[] xs = { 0f, 0.5f, 10f, 10.2f };
            bool[] expected = { false, false, true, false };

            for (int i = 0; i < xs.Length; i++)
                Assert.AreEqual(expected[i], builder.Build(new PlayerPoseInput { Position = new Vector2(xs[i], 0f) }, i * 0.05).Teleported, $"adım {i}");
        }

        [Test]
        public void Build_FastLegitMovement_IsNotTeleport()
        {
            // Sapan en yüksek hız 65 b/sn: 50 ms'de 3.25 birim; 15 FPS'te 66 ms'de 4.3 birim. İkisi de ışınlanma değil.
            var builder = new SnapshotBuilder();
            builder.Build(new PlayerPoseInput { Position = Vector2.zero }, 0.0);
            Assert.IsFalse(builder.Build(new PlayerPoseInput { Position = new Vector2(3.25f, 0f) }, 0.05).Teleported);
            Assert.IsFalse(builder.Build(new PlayerPoseInput { Position = new Vector2(3.25f + 4.3f, 0f) }, 0.116).Teleported);
        }

        [Test]
        public void Build_ExplicitTeleportFlag_SetsTeleported()
        {
            var builder = new SnapshotBuilder();
            builder.Build(new PlayerPoseInput { Position = Vector2.zero }, 0.0);
            Assert.IsTrue(builder.Build(new PlayerPoseInput { Position = new Vector2(0.5f, 0f), Teleported = true }, 0.05).Teleported);
        }

        [Test]
        public void Build_HiddenWhileDead_ThenTeleportsOnReappear()
        {
            // Ölünce oyuncu kapanır (gizli), respawn'da yeni yerde açılır: açıldığı ilk snapshot ışınlanmadır,
            // mesafe eşiğin altında olsa bile (aynı odada yakın checkpoint).
            var builder = new SnapshotBuilder();
            var alive = builder.Build(new PlayerPoseInput { Position = Vector2.zero }, 0.0);
            var dead = builder.Build(new PlayerPoseInput { Position = Vector2.zero, IsHidden = true }, 0.05);
            var back = builder.Build(new PlayerPoseInput { Position = new Vector2(1f, 0f) }, 1.5);
            var next = builder.Build(new PlayerPoseInput { Position = new Vector2(1.1f, 0f) }, 1.55);

            Assert.IsFalse(alive.IsHidden);
            Assert.IsTrue(dead.IsHidden);
            Assert.IsFalse(back.IsHidden);
            Assert.IsTrue(back.Teleported, "yeniden belirdiği snapshot ışınlanma olmalı");
            Assert.IsFalse(next.Teleported);
        }

        [Test]
        public void Build_SequenceFollowsRealTime_SkipsAfterHitch()
        {
            // 150 ms'lik takılmadan sonra sıra numarası gerçek zamana göre atlar; zaman ekseni geride kalmaz.
            var builder = new SnapshotBuilder();
            double[] times = { 0.0, 0.05, 0.10, 0.30 };
            uint[] expected = { 0, 1, 2, 6 };

            for (int i = 0; i < times.Length; i++)
                Assert.AreEqual(expected[i], builder.Build(default, times[i]).Sequence, $"adım {i}");
        }

        // ---- GhostInterpolator ----

        [Test]
        public void Sample_NoSnapshots_ReturnsFalse()
        {
            Assert.IsFalse(new GhostInterpolator().TrySample(5.0, out _));
        }

        [Test]
        public void Sample_RendersDelayBehind_InterpolatesMidpoint()
        {
            var interp = new GhostInterpolator();
            for (uint seq = 0; seq <= 2; seq++) interp.Add(Snap(seq, seq), Arrival(seq));

            // Çizim saati = now - offset - 0.1 = 0.075 → seq1 (0.05) ile seq2 (0.10) arası yarı.
            Assert.IsTrue(interp.TrySample(1.175, out GhostPose pose));
            Assert.AreEqual(1.5f, pose.Position.x, 1e-3f);
        }

        [Test]
        public void Sample_OlderOrDuplicateSequence_IsIgnored()
        {
            var interp = new GhostInterpolator();
            interp.Add(Snap(5, 5f), Arrival(5));
            interp.Add(Snap(4, 99f), Arrival(5) + 0.01);
            interp.Add(Snap(5, 77f), Arrival(5) + 0.02);

            Assert.IsTrue(interp.TrySample(Arrival(5) + 1.0, out GhostPose pose));
            Assert.AreEqual(5f, pose.Position.x, 1e-4f);
        }

        [Test]
        public void Sample_NoNewPackets_HoldsLastPose()
        {
            var interp = new GhostInterpolator();
            for (uint seq = 0; seq <= 3; seq++) interp.Add(Snap(seq, seq * 2f), Arrival(seq));

            Assert.IsTrue(interp.TrySample(Arrival(3) + 2.0, out GhostPose pose));
            Assert.AreEqual(6f, pose.Position.x, 1e-4f);
        }

        [Test]
        public void Sample_Teleported_SnapsWithoutBlending()
        {
            var interp = new GhostInterpolator();
            interp.Add(Snap(0, 0f), Arrival(0));
            interp.Add(Snap(1, 1f), Arrival(1));
            interp.Add(Snap(2, 50f, teleported: true), Arrival(2));

            // Işınlanmadan sonra her örnek 50'dir; 1 ile 50 arasında bir değer görülmemeli.
            for (double now = Arrival(2); now < Arrival(2) + 0.3; now += 0.01)
            {
                Assert.IsTrue(interp.TrySample(now, out GhostPose pose));
                Assert.AreEqual(50f, pose.Position.x, 1e-4f, $"now={now:0.000}");
            }
        }

        [Test]
        public void Sample_NetworkJitter_DoesNotShake()
        {
            var interp = new GhostInterpolator();
            var random = new System.Random(7);
            for (uint seq = 0; seq < 40; seq++)
                interp.Add(Snap(seq, seq), Arrival(seq) + (random.NextDouble() - 0.5) * 0.04);

            float previous = float.MinValue;
            for (double now = Arrival(4); now < Arrival(39); now += 1.0 / 60.0)
            {
                Assert.IsTrue(interp.TrySample(now, out GhostPose pose));
                Assert.GreaterOrEqual(pose.Position.x, previous - 1e-4f, $"geri gitti, now={now:0.000}");
                previous = pose.Position.x;
            }
        }

        [Test]
        public void Sample_SkippedSequences_StillInterpolates()
        {
            var interp = new GhostInterpolator();
            for (uint seq = 0; seq <= 19; seq++) interp.Add(Snap(seq, seq), Arrival(seq));
            for (uint seq = 22; seq <= 30; seq++) interp.Add(Snap(seq, seq), Arrival(seq));

            // Çizim saati 1.0 sn → seq19 (0.95) ile seq22 (1.10) arası: x 19 ile 22 arasında, uçlarda değil.
            Assert.IsTrue(interp.TrySample(Offset + 0.1 + 1.0, out GhostPose pose));
            Assert.Greater(pose.Position.x, 19f);
            Assert.Less(pose.Position.x, 22f);
        }

        [Test]
        public void Sample_ReceiverClockRunsFaster_KeepsInterpolatingInLongSession()
        {
            // Alıcının saati %0.2 hızlı: 120 sn sonra 0.24 sn kayma. Saat farkı tahmini yükselemezse
            // çizim saati en yeni snapshot'ı geçer ve hayalet 20 Hz'de basamaklı oynar.
            var interp = new GhostInterpolator();
            const double skew = 1.002;
            uint last = 120 * GhostTiming.SnapshotRate;
            for (uint seq = 0; seq <= last; seq++)
                interp.Add(Snap(seq, seq * 0.1f), seq / (double)GhostTiming.SnapshotRate * skew + Offset);

            double now = last / (double)GhostTiming.SnapshotRate * skew + Offset;
            Assert.IsTrue(interp.TrySample(now, out GhostPose pose));
            Assert.Less(pose.Position.x, last * 0.1f - 1e-3f, "en yeni snapshot'ta takılı kaldı, ara değer yok");
        }

        [Test]
        public void Sample_HiddenFlag_PassesThrough()
        {
            var interp = new GhostInterpolator();
            interp.Add(new SnapshotMessage { Sequence = 0, IsHidden = true }, Arrival(0));

            Assert.IsTrue(interp.TrySample(Arrival(0) + 1.0, out GhostPose pose));
            Assert.IsTrue(pose.IsHidden);
        }

        [Test]
        public void Sample_AnchorNullOnOneSide_UsesNewer()
        {
            var interp = new GhostInterpolator();
            interp.Add(Snap(0, 0f, left: null), Arrival(0));
            interp.Add(Snap(1, 1f, left: new Vector2(3f, 4f)), Arrival(1));

            // İki snapshot'ın arasında: kanca bir tarafta yok, yeni snapshot'ınki kullanılır.
            Assert.IsTrue(interp.TrySample(Offset + 0.1 + 0.025, out GhostPose pose));
            Assert.AreEqual(new Vector2(3f, 4f), pose.LeftAnchor);
        }

        // ---- GhostVisibility ----

        private static readonly IReadOnlyCollection<int> Neighbors = new[] { 4, 6 };

        [Test]
        public void Visible_SameRoom() => Assert.IsTrue(GhostVisibility.IsVisible(5, 5, Neighbors));

        [Test]
        public void Visible_NeighborRoom() => Assert.IsTrue(GhostVisibility.IsVisible(6, 5, Neighbors));

        [Test]
        public void Hidden_FarRoom() => Assert.IsFalse(GhostVisibility.IsVisible(9, 5, Neighbors));

        [Test]
        public void Hidden_UnknownOpponentRoom() => Assert.IsFalse(GhostVisibility.IsVisible(0, 5, Neighbors));
    }
}
