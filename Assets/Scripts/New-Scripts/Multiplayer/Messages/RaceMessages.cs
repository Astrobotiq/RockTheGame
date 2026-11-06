using System;
using UnityEngine;

namespace New_Scripts.Multiplayer.Messages
{
    /// <summary>
    /// Kablodaki ilk bayt. Değerler protokolün parçası: değiştirmek RaceProtocol.Version'ı artırmayı gerektirir.
    /// </summary>
    public enum RaceMessageType : byte
    {
        Hello = 1,
        Snapshot = 2,
        RoomEntered = 3,
        FlagCaptured = 4,
        Penalty = 5,
        ProgressSync = 6,
        Finished = 7
    }

    public enum RaceMode : byte
    {
        Race = 0,
        Coop = 1
    }

    public interface IRaceMessage
    {
        RaceMessageType Type { get; }
    }

    /// <summary>Bağlanınca ilk mesaj: protokol sürümü, mod, yarış kimliği ve oyuncu adı.</summary>
    public sealed class HelloMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Hello;
        public ushort ProtocolVersion;
        public RaceMode Mode;
        public Guid RaceId;
        public string PlayerName;
    }

    /// <summary>
    /// Hayaletin saniyede ~20 kez gelen anlık durumu (güvenilmez kanal).
    /// DashCount her dash'te bir artar; kayıp paket olsa da alıcı değişimi görüp animasyonu oynatır.
    /// </summary>
    public sealed class SnapshotMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Snapshot;
        public uint Sequence;
        public int RoomId;
        public Vector2 Position;
        public bool FacingRight;
        public float Speed;
        public bool IsGrounded;
        public float VerticalVelocity;
        public bool IsSwinging;
        public bool IsDashing;
        public bool IsWallClimbing;
        public bool IsWallSliding;
        public byte DashCount;
        public bool Teleported;
        public Vector2? LeftAnchor;
        public Vector2? RightAnchor;
    }

    public sealed class RoomEnteredMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.RoomEntered;
        public int RoomId;
    }

    public sealed class FlagCapturedMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.FlagCaptured;
        public int RoomId;
        public int NewCount;
    }

    public sealed class PenaltyMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Penalty;
        public int TargetRoomId;
    }

    /// <summary>Bağlanınca ya da yeniden bağlanınca oyuncunun tüm ilerlemesi.</summary>
    public sealed class ProgressSyncMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.ProgressSync;
        public int RoomId;
        public int FlagCount;
        public int[] CapturedRoomIds;
        public float ElapsedSeconds;
    }

    public sealed class FinishedMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Finished;
        public float ElapsedSeconds;
    }
}
