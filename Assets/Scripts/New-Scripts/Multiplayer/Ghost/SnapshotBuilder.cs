using System;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Yerel oyuncunun bir karelik durumu; PlayerController ve animasyon sprite'ından doldurulur.</summary>
    public struct PlayerPoseInput
    {
        public int RoomId;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool FacingRight;
        public bool IsGrounded;
        public bool IsSwinging;
        public bool IsDashing;
        public bool IsWallClimbing;
        public bool IsWallSliding;
        public Vector2? LeftAnchor;
        public Vector2? RightAnchor;
        public SurfacePose? Carrier;
        public SurfacePose? LeftNode;
        public SurfacePose? RightNode;
        /// <summary>Açık ışınlanma (respawn, ceza): hayalet kaydırmadan yerleşsin.</summary>
        public bool Teleported;
        /// <summary>Oyuncu nesnesi kapalı (öldü, respawn bekliyor).</summary>
        public bool IsHidden;
        /// <summary>Oyuncu ceza balonunda taşınıyor (PenaltyHandler); uzun odada balon 100 b/sn'yi aşar.</summary>
        public bool InBubble;
    }

    /// <summary>
    /// Oyuncu durumundan SnapshotMessage üretir. Sıra numarası gönderenin gerçek saatinden gelir
    /// (floor(zaman * SnapshotRate)); takılmada atlar, böylece alıcının zaman ekseni geride kalmaz.
    /// Dash sayacı DashState'e her girişte +1; ışınlanma, açık bayrak ya da hıza göre ölçeklenmiş sıçrama eşiği.
    /// Animasyon değerleri PlayerAnimationController'ın animator'a yazdıklarıyla aynı.
    /// </summary>
    public sealed class SnapshotBuilder
    {
        private uint? _lastSequence;
        private double _lastTime;
        private byte _dashCount;
        private bool _wasDashing;
        private bool _wasHidden;
        private Vector2? _lastPosition;

        public SnapshotMessage Build(PlayerPoseInput input, double realtimeSeconds)
        {
            uint sequence = (uint)Math.Floor(realtimeSeconds * GhostTiming.SnapshotRate);
            if (_lastSequence.HasValue && sequence <= _lastSequence.Value) sequence = _lastSequence.Value + 1;

            if (input.IsDashing && !_wasDashing) _dashCount++;
            _wasDashing = input.IsDashing;

            // Gizliyken (ölü) yeniden belirdiği ilk snapshot ışınlanmadır: respawn noktası yakın olsa da kaymasın.
            bool teleported = input.Teleported || (_wasHidden && !input.IsHidden);
            _wasHidden = input.IsHidden;
            // Balonda hız sıçraması ışınlanma değildir: hayalet balonla birlikte yumuşakça süzülür.
            if (_lastPosition.HasValue && !input.InBubble)
            {
                double elapsed = Math.Max(realtimeSeconds - _lastTime, 1.0 / GhostTiming.SnapshotRate);
                double limit = GhostTiming.TeleportSpeed * elapsed + GhostTiming.TeleportMargin;
                teleported |= (input.Position - _lastPosition.Value).magnitude > limit;
            }

            _lastPosition = input.Position;
            _lastSequence = sequence;
            _lastTime = realtimeSeconds;

            return new SnapshotMessage
            {
                Sequence = sequence,
                RoomId = input.RoomId,
                Position = input.Position,
                FacingRight = input.FacingRight,
                Speed = Mathf.Abs(input.Velocity.x),
                IsGrounded = input.IsGrounded,
                VerticalVelocity = input.Velocity.y,
                IsSwinging = input.IsSwinging,
                IsDashing = input.IsDashing,
                IsWallClimbing = input.IsWallClimbing,
                IsWallSliding = input.IsWallSliding,
                DashCount = _dashCount,
                Teleported = teleported,
                IsHidden = input.IsHidden,
                InBubble = input.InBubble,
                LeftAnchor = input.LeftAnchor,
                RightAnchor = input.RightAnchor,
                Carrier = input.Carrier,
                LeftNode = input.LeftNode,
                RightNode = input.RightNode
            };
        }
    }
}
