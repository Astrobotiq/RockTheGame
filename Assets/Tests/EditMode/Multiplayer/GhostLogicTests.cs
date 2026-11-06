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

            var first = builder.Build(input);
            var second = builder.Build(input);
            var third = builder.Build(input);

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
                var snapshot = builder.Build(new PlayerPoseInput { IsDashing = dashing[i] });
                Assert.AreEqual(expected[i], snapshot.DashCount, $"adım {i}");
                Assert.AreEqual(dashing[i], snapshot.IsDashing);
            }
        }

        [Test]
        public void Build_JumpOver3Units_SetsTeleportedOnce()
        {
            var builder = new SnapshotBuilder();
            float[] xs = { 0f, 0.5f, 10f, 10.2f };
            bool[] expected = { false, false, true, false };

            for (int i = 0; i < xs.Length; i++)
                Assert.AreEqual(expected[i], builder.Build(new PlayerPoseInput { Position = new Vector2(xs[i], 0f) }).Teleported, $"adım {i}");
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
