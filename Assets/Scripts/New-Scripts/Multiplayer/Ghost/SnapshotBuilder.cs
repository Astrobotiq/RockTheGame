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
    }

    /// <summary>
    /// Oyuncu durumundan SnapshotMessage üretir. Sıra numarasını, dash sayacını (DashState'e her girişte +1)
    /// ve ışınlanmayı (iki snapshot arasında TeleportDistance'tan büyük sıçrama) kendisi takip eder.
    /// Animasyon değerleri PlayerAnimationController'ın animator'a yazdıklarıyla aynı.
    /// </summary>
    public sealed class SnapshotBuilder
    {
        private uint _nextSequence;
        private byte _dashCount;
        private bool _wasDashing;
        private Vector2? _lastPosition;

        public SnapshotMessage Build(PlayerPoseInput input)
        {
            if (input.IsDashing && !_wasDashing) _dashCount++;
            _wasDashing = input.IsDashing;

            bool teleported = _lastPosition.HasValue &&
                              (input.Position - _lastPosition.Value).sqrMagnitude > GhostTiming.TeleportDistance * GhostTiming.TeleportDistance;
            _lastPosition = input.Position;

            return new SnapshotMessage
            {
                Sequence = _nextSequence++,
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
