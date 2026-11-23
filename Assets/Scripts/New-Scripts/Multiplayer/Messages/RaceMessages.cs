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
        Finished = 7,
        Ready = 8,
        Death = 9,
        Leave = 10
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
        /// <summary>Kayıtlı yarışa devam ediliyor (protokol v3). Yeni yarışta istemci RaceId olarak Guid.Empty yollar.</summary>
        public bool Resume;
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
        /// <summary>Oyuncu görünmüyor (öldü, respawn bekliyor): hayalet gizlenir.</summary>
        public bool IsHidden;
        /// <summary>Gönderen ceza balonunda taşınıyor (protokol v6): alıcı hayaletin çevresine balon çizer, hız sıçraması ışınlanma sayılmaz.</summary>
        public bool InBubble;
        public Vector2? LeftAnchor;
        public Vector2? RightAnchor;
        /// <summary>Gönderenin üstünde durduğu ya da tutunduğu hareketli yüzey (kimlik + gönderenin dünyasındaki poz), yoksa null.</summary>
        public SurfacePose? Carrier;
        /// <summary>Sol kolun tutunduğu hareketli node'un (ya da onu taşıyan sürücünün) kimliği ve gönderenin dünyasındaki pozu.</summary>
        public SurfacePose? LeftNode;
        public SurfacePose? RightNode;
    }

    /// <summary>
    /// Gönderenin dünyasındaki bir hareketli nesnenin kimliği (<c>SceneObjectId</c>) ve o andaki pozu. Alıcı kendi dünyasındaki
    /// nesneyi değil, nesnenin gönderenin dünyasındaki yerini çizer; çünkü platform fazı her oyuncuda ayrı gider.
    /// </summary>
    public readonly struct SurfacePose
    {
        public readonly uint Id;
        public readonly Vector2 Position;
        /// <summary>Derece, z ekseni.</summary>
        public readonly float Rotation;

        public SurfacePose(uint id, Vector2 position, float rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }
    }

    /// <summary>
    /// Oyuncu hareketli bir tehlikeye çarpıp öldü (güvenilir kanal). Alıcı tehlikeyi ölüm anındaki pozunda kısa süre gösterir.
    /// Öldüren bilinmiyorsa ya da sabitse mesaj yollanmaz.
    /// </summary>
    public sealed class DeathMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Death;
        public SurfacePose Killer;
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

    /// <summary>
    /// Oyuncu pause menüsünden yarıştan bilerek çıkıyor (güvenilir kanal). Alıcı 30 sn "koptu, bekleniyor" beklemeden lobiye döner;
    /// bağlantının kendiliğinden kopmasından (ağ, çökme) ayrılır.
    /// </summary>
    public sealed class LeaveMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Leave;
    }

    /// <summary>Sahne yüklendi, oyuncu donuk bekliyor. İki taraf da Ready görünce ortak geri sayım başlar.</summary>
    public sealed class ReadyMessage : IRaceMessage
    {
        public RaceMessageType Type => RaceMessageType.Ready;
    }
}
