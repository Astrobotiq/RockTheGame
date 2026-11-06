using System;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Yarış mesajlarının byte'a yazılıp aynen geri okunması ve bozuk veride çökmeme.
    /// docs/systems/multiplayer.md → "Mesajlar".
    /// </summary>
    public class RaceMessageCodecTests
    {
        private static readonly Guid FixedRaceId = new Guid("6f1c2a9e-3b4d-4e5f-8a7b-0c1d2e3f4a5b");

        private static T RoundTrip<T>(IRaceMessage message) where T : class, IRaceMessage
        {
            byte[] bytes = RaceMessageCodec.Encode(message);
            Assert.IsTrue(RaceMessageCodec.TryDecode(bytes, out IRaceMessage decoded), "TryDecode false döndü");
            Assert.IsInstanceOf<T>(decoded);
            return (T)decoded;
        }

        private static SnapshotMessage FullSnapshot() => new SnapshotMessage
        {
            Sequence = 4000000000u,
            RoomId = 17,
            Position = new Vector2(123.25f, -45.5f),
            FacingRight = true,
            Speed = 7.75f,
            IsGrounded = false,
            VerticalVelocity = -12.5f,
            IsSwinging = true,
            IsDashing = false,
            IsWallClimbing = true,
            IsWallSliding = false,
            DashCount = 255,
            Teleported = false,
            LeftAnchor = new Vector2(1.5f, 2.5f),
            RightAnchor = new Vector2(-3.25f, 4.75f)
        };

        [Test]
        public void Hello_RoundTrips()
        {
            var hello = new HelloMessage
            {
                ProtocolVersion = 1, Mode = RaceMode.Coop, RaceId = FixedRaceId, PlayerName = "Çağrı Öz"
            };

            var result = RoundTrip<HelloMessage>(hello);

            Assert.AreEqual(RaceMessageType.Hello, result.Type);
            Assert.AreEqual(1, result.ProtocolVersion);
            Assert.AreEqual(RaceMode.Coop, result.Mode);
            Assert.AreEqual(FixedRaceId, result.RaceId);
            Assert.AreEqual("Çağrı Öz", result.PlayerName);
        }

        [Test]
        public void Hello_ResumeFlag_RoundTrips([Values(false, true)] bool resume)
        {
            var hello = new HelloMessage { ProtocolVersion = 3, Mode = RaceMode.Race, RaceId = FixedRaceId, PlayerName = "A", Resume = resume };
            Assert.AreEqual(resume, RoundTrip<HelloMessage>(hello).Resume);
        }

        [Test]
        public void Snapshot_RoundTrips_WithAnchors()
        {
            var s = FullSnapshot();

            var r = RoundTrip<SnapshotMessage>(s);

            Assert.AreEqual(s.Sequence, r.Sequence);
            Assert.AreEqual(s.RoomId, r.RoomId);
            Assert.AreEqual(s.Position, r.Position);
            Assert.AreEqual(s.FacingRight, r.FacingRight);
            Assert.AreEqual(s.Speed, r.Speed);
            Assert.AreEqual(s.IsGrounded, r.IsGrounded);
            Assert.AreEqual(s.VerticalVelocity, r.VerticalVelocity);
            Assert.AreEqual(s.IsSwinging, r.IsSwinging);
            Assert.AreEqual(s.IsDashing, r.IsDashing);
            Assert.AreEqual(s.IsWallClimbing, r.IsWallClimbing);
            Assert.AreEqual(s.IsWallSliding, r.IsWallSliding);
            Assert.AreEqual(s.DashCount, r.DashCount);
            Assert.AreEqual(s.Teleported, r.Teleported);
            Assert.AreEqual(s.LeftAnchor, r.LeftAnchor);
            Assert.AreEqual(s.RightAnchor, r.RightAnchor);
        }

        [Test]
        public void Snapshot_RoundTrips_InvertedFlags()
        {
            // Her bool'un kendi bitine yazıldığını görmek için bayrakları tersine çevirip tekrar dene.
            var s = FullSnapshot();
            s.FacingRight = !s.FacingRight; s.IsGrounded = !s.IsGrounded; s.IsSwinging = !s.IsSwinging;
            s.IsDashing = !s.IsDashing; s.IsWallClimbing = !s.IsWallClimbing; s.IsWallSliding = !s.IsWallSliding;
            s.DashCount = 7; s.Teleported = !s.Teleported; s.IsHidden = !s.IsHidden;

            var r = RoundTrip<SnapshotMessage>(s);

            Assert.AreEqual(s.FacingRight, r.FacingRight);
            Assert.AreEqual(s.IsGrounded, r.IsGrounded);
            Assert.AreEqual(s.IsSwinging, r.IsSwinging);
            Assert.AreEqual(s.IsDashing, r.IsDashing);
            Assert.AreEqual(s.IsWallClimbing, r.IsWallClimbing);
            Assert.AreEqual(s.IsWallSliding, r.IsWallSliding);
            Assert.AreEqual(s.DashCount, r.DashCount);
            Assert.AreEqual(s.Teleported, r.Teleported);
            Assert.AreEqual(s.IsHidden, r.IsHidden);
        }

        [Test]
        public void Snapshot_RoundTrips_NullAnchors()
        {
            var s = FullSnapshot();
            s.LeftAnchor = null;
            s.RightAnchor = null;

            var r = RoundTrip<SnapshotMessage>(s);

            Assert.IsNull(r.LeftAnchor);
            Assert.IsNull(r.RightAnchor);
            Assert.AreEqual(s.Position, r.Position);
        }

        [Test]
        public void Snapshot_RoundTrips_OnlyRightAnchor()
        {
            var s = FullSnapshot();
            s.LeftAnchor = null;

            var r = RoundTrip<SnapshotMessage>(s);

            Assert.IsNull(r.LeftAnchor);
            Assert.AreEqual(s.RightAnchor, r.RightAnchor);
        }

        [Test]
        public void Snapshot_EncodedSize_IsAtMost50Bytes()
        {
            byte[] bytes = RaceMessageCodec.Encode(FullSnapshot());
            Assert.LessOrEqual(bytes.Length, 50);
        }

        [Test]
        public void RoomEntered_FlagCaptured_Penalty_Finished_RoundTrip()
        {
            Assert.AreEqual(9, RoundTrip<RoomEnteredMessage>(new RoomEnteredMessage { RoomId = 9 }).RoomId);

            var flag = RoundTrip<FlagCapturedMessage>(new FlagCapturedMessage { RoomId = 12, NewCount = 3 });
            Assert.AreEqual(12, flag.RoomId);
            Assert.AreEqual(3, flag.NewCount);

            Assert.AreEqual(11, RoundTrip<PenaltyMessage>(new PenaltyMessage { TargetRoomId = 11 }).TargetRoomId);

            Assert.AreEqual(5432.125f,
                RoundTrip<FinishedMessage>(new FinishedMessage { ElapsedSeconds = 5432.125f }).ElapsedSeconds);
        }

        [Test]
        public void Ready_RoundTrips_AsSingleByte()
        {
            byte[] bytes = RaceMessageCodec.Encode(new ReadyMessage());
            Assert.AreEqual(1, bytes.Length);
            Assert.IsNotNull(RoundTrip<ReadyMessage>(new ReadyMessage()));
        }

        [Test]
        public void ProgressSync_RoundTrips([Values(0, 3)] int capturedCount)
        {
            int[] captured = new int[capturedCount];
            for (int i = 0; i < capturedCount; i++) captured[i] = 4 + i * 2;
            var msg = new ProgressSyncMessage
            {
                RoomId = 14, FlagCount = 2, CapturedRoomIds = captured, ElapsedSeconds = 987.5f
            };

            var r = RoundTrip<ProgressSyncMessage>(msg);

            Assert.AreEqual(14, r.RoomId);
            Assert.AreEqual(2, r.FlagCount);
            Assert.IsNotNull(r.CapturedRoomIds);
            CollectionAssert.AreEqual(captured, r.CapturedRoomIds);
            Assert.AreEqual(987.5f, r.ElapsedSeconds);
        }

        [Test]
        public void PlayerName_LongerThan32_IsTruncated()
        {
            string longName = new string('a', 20) + new string('ş', 20);
            var hello = new HelloMessage { ProtocolVersion = 1, Mode = RaceMode.Race, RaceId = FixedRaceId, PlayerName = longName };

            var r = RoundTrip<HelloMessage>(hello);

            Assert.AreEqual(longName.Substring(0, 32), r.PlayerName);
        }

        [Test]
        public void TryDecode_Empty_ReturnsFalse()
        {
            Assert.IsFalse(RaceMessageCodec.TryDecode(new byte[0], out _));
            Assert.IsFalse(RaceMessageCodec.TryDecode(null, out _));
        }

        [Test]
        public void TryDecode_UnknownType_ReturnsFalse()
        {
            Assert.IsFalse(RaceMessageCodec.TryDecode(new byte[] { 99, 0, 0 }, out _));
        }

        [Test]
        public void TryDecode_TrailingBytes_ReturnsFalse()
        {
            byte[] bytes = RaceMessageCodec.Encode(new RoomEnteredMessage { RoomId = 3 });
            byte[] padded = new byte[bytes.Length + 1];
            Array.Copy(bytes, padded, bytes.Length);

            Assert.IsFalse(RaceMessageCodec.TryDecode(padded, out _));
        }

        [Test]
        public void TryDecode_Truncated_ReturnsFalseWithoutThrowing()
        {
            var messages = new IRaceMessage[]
            {
                new HelloMessage { ProtocolVersion = 1, Mode = RaceMode.Race, RaceId = FixedRaceId, PlayerName = "Eren" },
                new ProgressSyncMessage { RoomId = 5, FlagCount = 1, CapturedRoomIds = new[] { 2, 3, 4 }, ElapsedSeconds = 10f },
                FullSnapshot()
            };

            foreach (var message in messages)
            {
                byte[] full = RaceMessageCodec.Encode(message);
                for (int length = 1; length < full.Length; length++)
                {
                    byte[] prefix = new byte[length];
                    Array.Copy(full, prefix, length);
                    bool ok = true;
                    Assert.DoesNotThrow(() => ok = RaceMessageCodec.TryDecode(prefix, out _),
                        $"{message.Type} {length}/{full.Length} baytta istisna attı");
                    Assert.IsFalse(ok, $"{message.Type} {length}/{full.Length} baytta true döndü");
                }
            }
        }

        [Test]
        public void TryDecode_HelloWithMalformedNameLength_ReturnsFalseWithoutThrowing()
        {
            // Tip, sürüm, mod, 16 bayt Guid, ardından 5 devam baytlı geçersiz 7-bit uzunluk öneki:
            // BinaryReader.ReadString burada FormatException atar (gözden geçirme bulgusu #1).
            // Düzen: [0] tip, [1-2] sürüm, [3] mod, [4-19] Guid, [20-24] uzunluk öneki.
            byte[] bytes = new byte[25];
            bytes[0] = (byte)RaceMessageType.Hello;
            bytes[1] = 1;
            for (int i = 20; i < 25; i++) bytes[i] = 0xFF;

            bool ok = true;
            Assert.DoesNotThrow(() => ok = RaceMessageCodec.TryDecode(bytes, out _));
            Assert.IsFalse(ok);
        }

        [Test]
        public void TryDecode_ProgressSyncWithHugeArrayLength_ReturnsFalse()
        {
            // Tip baytı + RoomId + FlagCount + dizi uzunluğu = int.MaxValue, sonrası yok.
            byte[] bytes = new byte[13];
            bytes[0] = (byte)RaceMessageType.ProgressSync;
            BitConverter.GetBytes(int.MaxValue).CopyTo(bytes, 9);

            Assert.IsFalse(RaceMessageCodec.TryDecode(bytes, out _));
        }
    }
}
