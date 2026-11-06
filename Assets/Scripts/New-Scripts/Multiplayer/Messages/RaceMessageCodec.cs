using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace New_Scripts.Multiplayer.Messages
{
    /// <summary>
    /// Yarış mesajlarını byte dizisine yazar ve geri okur.
    /// Biçim: ilk bayt RaceMessageType, ardından alanlar tanımlandıkları sırayla (little-endian, BinaryWriter).
    /// TryDecode bozuk, kesik ya da fazla baytlı veride istisna atmadan false döner: ağdan ne gelirse gelsin oyun çökmesin.
    /// </summary>
    public static class RaceMessageCodec
    {
        // SnapshotMessage bool'larının bayrak baytındaki bitleri.
        private const byte FacingRightBit = 1 << 0;
        private const byte IsGroundedBit = 1 << 1;
        private const byte IsSwingingBit = 1 << 2;
        private const byte IsDashingBit = 1 << 3;
        private const byte IsWallClimbingBit = 1 << 4;
        private const byte IsWallSlidingBit = 1 << 5;
        private const byte TeleportedBit = 1 << 6;

        private const byte LeftAnchorBit = 1 << 0;
        private const byte RightAnchorBit = 1 << 1;

        public static byte[] Encode(IRaceMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using var stream = new MemoryStream(64);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)message.Type);

            switch (message)
            {
                case HelloMessage hello:
                    writer.Write(hello.ProtocolVersion);
                    writer.Write((byte)hello.Mode);
                    writer.Write(hello.RaceId.ToByteArray());
                    writer.Write(TruncateName(hello.PlayerName));
                    break;
                case SnapshotMessage s:
                    WriteSnapshot(writer, s);
                    break;
                case RoomEnteredMessage room:
                    writer.Write(room.RoomId);
                    break;
                case FlagCapturedMessage flag:
                    writer.Write(flag.RoomId);
                    writer.Write(flag.NewCount);
                    break;
                case PenaltyMessage penalty:
                    writer.Write(penalty.TargetRoomId);
                    break;
                case ProgressSyncMessage sync:
                    writer.Write(sync.RoomId);
                    writer.Write(sync.FlagCount);
                    int[] captured = sync.CapturedRoomIds ?? Array.Empty<int>();
                    writer.Write(captured.Length);
                    foreach (int id in captured) writer.Write(id);
                    writer.Write(sync.ElapsedSeconds);
                    break;
                case FinishedMessage finished:
                    writer.Write(finished.ElapsedSeconds);
                    break;
                default:
                    throw new ArgumentException($"Bilinmeyen mesaj tipi: {message.GetType().Name}", nameof(message));
            }

            writer.Flush();
            return stream.ToArray();
        }

        public static bool TryDecode(byte[] data, out IRaceMessage message)
        {
            message = null;
            if (data == null || data.Length == 0) return false;

            try
            {
                using var stream = new MemoryStream(data, writable: false);
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                var type = (RaceMessageType)reader.ReadByte();

                switch (type)
                {
                    case RaceMessageType.Hello:
                    {
                        var hello = new HelloMessage { ProtocolVersion = reader.ReadUInt16() };
                        byte mode = reader.ReadByte();
                        if (!Enum.IsDefined(typeof(RaceMode), mode)) return false;
                        hello.Mode = (RaceMode)mode;
                        byte[] guid = reader.ReadBytes(16);
                        if (guid.Length != 16) return false;
                        hello.RaceId = new Guid(guid);
                        hello.PlayerName = reader.ReadString();
                        message = hello;
                        break;
                    }
                    case RaceMessageType.Snapshot:
                        message = ReadSnapshot(reader);
                        break;
                    case RaceMessageType.RoomEntered:
                        message = new RoomEnteredMessage { RoomId = reader.ReadInt32() };
                        break;
                    case RaceMessageType.FlagCaptured:
                        message = new FlagCapturedMessage { RoomId = reader.ReadInt32(), NewCount = reader.ReadInt32() };
                        break;
                    case RaceMessageType.Penalty:
                        message = new PenaltyMessage { TargetRoomId = reader.ReadInt32() };
                        break;
                    case RaceMessageType.ProgressSync:
                    {
                        var sync = new ProgressSyncMessage { RoomId = reader.ReadInt32(), FlagCount = reader.ReadInt32() };
                        int count = reader.ReadInt32();
                        // Uzunluk kalan bayttan büyükse dev bir dizi ayırmadan reddet.
                        if (count < 0 || count > (stream.Length - stream.Position) / sizeof(int)) return false;
                        sync.CapturedRoomIds = new int[count];
                        for (int i = 0; i < count; i++) sync.CapturedRoomIds[i] = reader.ReadInt32();
                        sync.ElapsedSeconds = reader.ReadSingle();
                        message = sync;
                        break;
                    }
                    case RaceMessageType.Finished:
                        message = new FinishedMessage { ElapsedSeconds = reader.ReadSingle() };
                        break;
                    default:
                        return false;
                }

                if (stream.Position != stream.Length)
                {
                    message = null;
                    return false;
                }

                return true;
            }
            catch (Exception e) when (e is EndOfStreamException || e is IOException || e is ArgumentException)
            {
                message = null;
                return false;
            }
        }

        private static string TruncateName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return name.Length <= RaceProtocol.MaxPlayerNameLength
                ? name
                : name.Substring(0, RaceProtocol.MaxPlayerNameLength);
        }

        private static void WriteSnapshot(BinaryWriter writer, SnapshotMessage s)
        {
            writer.Write(s.Sequence);
            writer.Write(s.RoomId);
            writer.Write(s.Position.x);
            writer.Write(s.Position.y);
            writer.Write(s.Speed);
            writer.Write(s.VerticalVelocity);

            byte flags = 0;
            if (s.FacingRight) flags |= FacingRightBit;
            if (s.IsGrounded) flags |= IsGroundedBit;
            if (s.IsSwinging) flags |= IsSwingingBit;
            if (s.IsDashing) flags |= IsDashingBit;
            if (s.IsWallClimbing) flags |= IsWallClimbingBit;
            if (s.IsWallSliding) flags |= IsWallSlidingBit;
            if (s.Teleported) flags |= TeleportedBit;
            writer.Write(flags);
            writer.Write(s.DashCount);

            byte anchors = 0;
            if (s.LeftAnchor.HasValue) anchors |= LeftAnchorBit;
            if (s.RightAnchor.HasValue) anchors |= RightAnchorBit;
            writer.Write(anchors);
            if (s.LeftAnchor.HasValue) { writer.Write(s.LeftAnchor.Value.x); writer.Write(s.LeftAnchor.Value.y); }
            if (s.RightAnchor.HasValue) { writer.Write(s.RightAnchor.Value.x); writer.Write(s.RightAnchor.Value.y); }
        }

        private static SnapshotMessage ReadSnapshot(BinaryReader reader)
        {
            var s = new SnapshotMessage
            {
                Sequence = reader.ReadUInt32(),
                RoomId = reader.ReadInt32(),
                Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                Speed = reader.ReadSingle(),
                VerticalVelocity = reader.ReadSingle()
            };

            byte flags = reader.ReadByte();
            s.FacingRight = (flags & FacingRightBit) != 0;
            s.IsGrounded = (flags & IsGroundedBit) != 0;
            s.IsSwinging = (flags & IsSwingingBit) != 0;
            s.IsDashing = (flags & IsDashingBit) != 0;
            s.IsWallClimbing = (flags & IsWallClimbingBit) != 0;
            s.IsWallSliding = (flags & IsWallSlidingBit) != 0;
            s.Teleported = (flags & TeleportedBit) != 0;
            s.DashCount = reader.ReadByte();

            byte anchors = reader.ReadByte();
            if ((anchors & LeftAnchorBit) != 0) s.LeftAnchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            if ((anchors & RightAnchorBit) != 0) s.RightAnchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            return s;
        }
    }
}
