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
        /// <summary>Açık ışınlanma (respawn, ceza): hayalet kaydırmadan yerleşsin.</summary>
        public bool Teleported;
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
        private Vector2? _lastPosition;

        public SnapshotMessage Build(PlayerPoseInput input, double realtimeSeconds)
        {
            uint sequence = (uint)Math.Floor(realtimeSeconds * GhostTiming.SnapshotRate);
            if (_lastSequence.HasValue && sequence <= _lastSequence.Value) sequence = _lastSequence.Value + 1;

            if (input.IsDashing && !_wasDashing) _dashCount++;
            _wasDashing = input.IsDashing;

            bool teleported = input.Teleported;
            if (_lastPosition.HasValue)
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
                LeftAnchor = input.LeftAnchor,
                RightAnchor = input.RightAnchor
            };
        }
    }
}
