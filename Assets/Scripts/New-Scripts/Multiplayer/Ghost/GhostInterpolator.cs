using System.Collections.Generic;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayaletin bir anlık çizim durumu.</summary>
    public struct GhostPose
    {
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
        public Vector2? LeftAnchor;
        public Vector2? RightAnchor;
    }

    /// <summary>
    /// Gelen snapshot'ları gönderenin zaman ekseninde (Sequence / SnapshotRate) tutar ve InterpolationDelaySeconds
    /// geriden ara değer üretir. Alıcı ile gönderen saati arasındaki fark, görülen en küçük değerle tahmin edilir;
    /// böylece ağ titremesi hayaleti sarsmaz. Eski ya da tekrar gelen snapshot atılır, paket gelmezse son durumda
    /// beklenir (ileri tahmin yok), ışınlanmada tampon temizlenir.
    /// </summary>
    public sealed class GhostInterpolator
    {
        private const int MaxBuffered = 32;

        private readonly List<(SnapshotMessage snapshot, double senderTime)> _buffer = new List<(SnapshotMessage, double)>();
        private uint? _lastSequence;
        private double? _clockOffset;

        public void Add(SnapshotMessage snapshot, double receiveTime)
        {
            if (snapshot == null) return;
            if (_lastSequence.HasValue && snapshot.Sequence <= _lastSequence.Value) return;
            _lastSequence = snapshot.Sequence;

            double senderTime = snapshot.Sequence / (double)GhostTiming.SnapshotRate;
            double offset = receiveTime - senderTime;
            if (!_clockOffset.HasValue || offset < _clockOffset.Value) _clockOffset = offset;

            if (snapshot.Teleported) _buffer.Clear();
            _buffer.Add((snapshot, senderTime));
            if (_buffer.Count > MaxBuffered) _buffer.RemoveAt(0);
        }

        public bool TrySample(double now, out GhostPose pose)
        {
            pose = default;
            if (_buffer.Count == 0) return false;

            double renderTime = now - _clockOffset.Value - GhostTiming.InterpolationDelaySeconds;

            if (renderTime <= _buffer[0].senderTime)
            {
                pose = ToPose(_buffer[0].snapshot);
                return true;
            }

            int last = _buffer.Count - 1;
            if (renderTime >= _buffer[last].senderTime)
            {
                pose = ToPose(_buffer[last].snapshot);
                return true;
            }

            for (int i = 0; i < last; i++)
            {
                var older = _buffer[i];
                var newer = _buffer[i + 1];
                if (renderTime < older.senderTime || renderTime >= newer.senderTime) continue;

                float t = (float)((renderTime - older.senderTime) / (newer.senderTime - older.senderTime));
                pose = Blend(older.snapshot, newer.snapshot, t);
                return true;
            }

            pose = ToPose(_buffer[last].snapshot);
            return true;
        }

        private static GhostPose Blend(SnapshotMessage older, SnapshotMessage newer, float t)
        {
            GhostPose pose = ToPose(older);
            pose.Position = Vector2.Lerp(older.Position, newer.Position, t);
            pose.Speed = Mathf.Lerp(older.Speed, newer.Speed, t);
            pose.VerticalVelocity = Mathf.Lerp(older.VerticalVelocity, newer.VerticalVelocity, t);
            pose.LeftAnchor = BlendAnchor(older.LeftAnchor, newer.LeftAnchor, t);
            pose.RightAnchor = BlendAnchor(older.RightAnchor, newer.RightAnchor, t);
            return pose;
        }

        // Kanca bir tarafta yoksa (yeni atıldı ya da bırakıldı) yeni snapshot'ınki kullanılır.
        private static Vector2? BlendAnchor(Vector2? older, Vector2? newer, float t)
        {
            if (older.HasValue && newer.HasValue) return Vector2.Lerp(older.Value, newer.Value, t);
            return newer;
        }

        private static GhostPose ToPose(SnapshotMessage s) => new GhostPose
        {
            Position = s.Position,
            FacingRight = s.FacingRight,
            Speed = s.Speed,
            IsGrounded = s.IsGrounded,
            VerticalVelocity = s.VerticalVelocity,
            IsSwinging = s.IsSwinging,
            IsDashing = s.IsDashing,
            IsWallClimbing = s.IsWallClimbing,
            IsWallSliding = s.IsWallSliding,
            DashCount = s.DashCount,
            LeftAnchor = s.LeftAnchor,
            RightAnchor = s.RightAnchor
        };
    }
}
