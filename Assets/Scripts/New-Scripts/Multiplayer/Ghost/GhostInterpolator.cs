using System;
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
        public bool IsHidden;
        public Vector2? LeftAnchor;
        public Vector2? RightAnchor;
    }

    /// <summary>
    /// Gelen snapshot'ları gönderenin zaman ekseninde (Sequence / SnapshotRate) tutar ve InterpolationDelaySeconds
    /// geriden ara değer üretir. Alıcı ile gönderen saati arasındaki fark, son OffsetWindowSeconds içinde görülen en
    /// küçük değerle tahmin edilir: düşüşü hemen alınır (hayalet ileri gider), yükselişi en fazla OffsetMaxRiseRate
    /// hızla izlenir (çizim saati hiç geri gitmez). Böylece ağ titremesi hayaleti sarsmaz, iki makinenin saat farkı
    /// uzun bir yarışta birikmez. Eski ya da tekrar gelen snapshot atılır, paket gelmezse son durumda
    /// beklenir (ileri tahmin yok), ışınlanmada tampon temizlenir.
    /// </summary>
    public sealed class GhostInterpolator
    {
        private const int MaxBuffered = 32;
        private const double OffsetWindowSeconds = 2.0;
        private const double OffsetMaxRiseRate = 0.02; // çizim saati en fazla %2 yavaşlar

        private readonly List<(SnapshotMessage snapshot, double senderTime)> _buffer = new List<(SnapshotMessage, double)>();
        private readonly Queue<(double receiveTime, double offset)> _recentOffsets = new Queue<(double, double)>();
        private uint? _lastSequence;
        private double? _clockOffset;
        private double _lastReceiveTime;

        public void Add(SnapshotMessage snapshot, double receiveTime)
        {
            if (snapshot == null) return;
            if (_lastSequence.HasValue && snapshot.Sequence <= _lastSequence.Value) return;
            _lastSequence = snapshot.Sequence;

            double senderTime = snapshot.Sequence / (double)GhostTiming.SnapshotRate;
            UpdateClockOffset(receiveTime, receiveTime - senderTime);

            if (snapshot.Teleported) _buffer.Clear();
            _buffer.Add((snapshot, senderTime));
            if (_buffer.Count > MaxBuffered) _buffer.RemoveAt(0);
        }

        private void UpdateClockOffset(double receiveTime, double sample)
        {
            _recentOffsets.Enqueue((receiveTime, sample));
            while (_recentOffsets.Count > 1 && receiveTime - _recentOffsets.Peek().receiveTime > OffsetWindowSeconds)
                _recentOffsets.Dequeue();

            double windowMin = double.MaxValue;
            foreach (var (_, offset) in _recentOffsets) if (offset < windowMin) windowMin = offset;

            if (!_clockOffset.HasValue || windowMin < _clockOffset.Value)
                _clockOffset = windowMin;
            else
                _clockOffset = Math.Min(windowMin, _clockOffset.Value + (receiveTime - _lastReceiveTime) * OffsetMaxRiseRate);

            _lastReceiveTime = receiveTime;
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
            IsHidden = s.IsHidden,
            LeftAnchor = s.LeftAnchor,
            RightAnchor = s.RightAnchor
        };
    }
}
