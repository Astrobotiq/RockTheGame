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
        private const byte IsHiddenBit = 1 << 7;

        private const byte LeftAnchorBit = 1 << 0;
        private const byte RightAnchorBit = 1 << 1;

        private const byte CarrierBit = 1 << 0;
        private const byte LeftNodeBit = 1 << 1;
        private const byte RightNodeBit = 1 << 2;
        // İkinci bayrak baytı yüzeylerden artanı da taşır (v6): ayrı bayt eklemek her paketi büyütürdü.
        private const byte InBubbleBit = 1 << 3;

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
                    writer.Write(hello.Resume);
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
                case ReadyMessage _:
                case LeaveMessage _:
                    break;
                case DeathMessage death:
                    WritePose(writer, death.Killer);
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
                        hello.Resume = reader.ReadBoolean();
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
                    case RaceMessageType.Ready:
                        message = new ReadyMessage();
                        break;
                    case RaceMessageType.Death:
                        message = new DeathMessage { Killer = ReadPose(reader) };
                        break;
                    case RaceMessageType.Leave:
                        message = new LeaveMessage();
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
            catch (Exception)
            {
                // Ağdan gelen veri güvenilmez: BinaryReader EndOfStream, IO, Argument ve bozuk uzunluk önekinde
                // FormatException atabilir. Sözleşme "asla istisna atmaz" olduğu için hepsi false'a döner.
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
            if (s.IsHidden) flags |= IsHiddenBit;
            writer.Write(flags);
            writer.Write(s.DashCount);

            byte anchors = 0;
            if (s.LeftAnchor.HasValue) anchors |= LeftAnchorBit;
            if (s.RightAnchor.HasValue) anchors |= RightAnchorBit;
            writer.Write(anchors);
            if (s.LeftAnchor.HasValue) { writer.Write(s.LeftAnchor.Value.x); writer.Write(s.LeftAnchor.Value.y); }
            if (s.RightAnchor.HasValue) { writer.Write(s.RightAnchor.Value.x); writer.Write(s.RightAnchor.Value.y); }

            byte surfaces = 0;
            if (s.Carrier.HasValue) surfaces |= CarrierBit;
            if (s.LeftNode.HasValue) surfaces |= LeftNodeBit;
            if (s.RightNode.HasValue) surfaces |= RightNodeBit;
            if (s.InBubble) surfaces |= InBubbleBit;
            writer.Write(surfaces);
            if (s.Carrier.HasValue) WritePose(writer, s.Carrier.Value);
            if (s.LeftNode.HasValue) WritePose(writer, s.LeftNode.Value);
            if (s.RightNode.HasValue) WritePose(writer, s.RightNode.Value);
        }

        private static void WritePose(BinaryWriter writer, SurfacePose pose)
        {
            writer.Write(pose.Id);
            writer.Write(pose.Position.x);
            writer.Write(pose.Position.y);
            writer.Write(pose.Rotation);
        }

        private static SurfacePose ReadPose(BinaryReader reader) =>
            new SurfacePose(reader.ReadUInt32(), new Vector2(reader.ReadSingle(), reader.ReadSingle()), reader.ReadSingle());

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
            s.IsHidden = (flags & IsHiddenBit) != 0;
            s.DashCount = reader.ReadByte();

            byte anchors = reader.ReadByte();
            if ((anchors & LeftAnchorBit) != 0) s.LeftAnchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            if ((anchors & RightAnchorBit) != 0) s.RightAnchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());

            byte surfaces = reader.ReadByte();
            s.InBubble = (surfaces & InBubbleBit) != 0;
            if ((surfaces & CarrierBit) != 0) s.Carrier = ReadPose(reader);
            if ((surfaces & LeftNodeBit) != 0) s.LeftNode = ReadPose(reader);
            if ((surfaces & RightNodeBit) != 0) s.RightNode = ReadPose(reader);
            return s;
        }
    }
}
