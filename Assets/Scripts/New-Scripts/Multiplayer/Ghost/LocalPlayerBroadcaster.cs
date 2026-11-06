using System.Collections.Generic;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using New_Scripts.Player;
using New_Scripts.Player.States;
using New_Scripts.Player.Visual;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Yerel oyuncunun durumunu saniyede SnapshotRate kez güvenilmez kanaldan yollar ve oda girişlerini
    /// RaceSession'a verip dönen mesajları güvenilir kanaldan yollar. Tek kişilik kodu yalnız okur.
    /// </summary>
    public sealed class LocalPlayerBroadcaster : MonoBehaviour
    {
        private readonly SnapshotBuilder _builder = new SnapshotBuilder();
        private readonly List<int> _neighborIds = new List<int>();
        private readonly List<(Room room, UnityEngine.Events.UnityAction action)> _subscriptions = new List<(Room, UnityEngine.Events.UnityAction)>();

        private PlayerController _player;
        private SpriteRenderer _bodySprite;
        private RaceSession _session;
        private IRaceTransport _transport;
        private float _sendAccumulator;

        public int LocalRoomId { get; private set; }
        public IReadOnlyCollection<int> LocalNeighborRoomIds => _neighborIds;

        public static LocalPlayerBroadcaster Create(PlayerController player, RaceSession session, IRaceTransport transport)
        {
            var broadcaster = new GameObject("[MP Broadcaster]").AddComponent<LocalPlayerBroadcaster>();
            broadcaster._player = player;
            broadcaster._session = session;
            broadcaster._transport = transport;
            var animation = player.GetComponentInChildren<PlayerAnimationController>(true);
            broadcaster._bodySprite = animation != null ? animation.GetComponent<SpriteRenderer>() : null;
            broadcaster.SubscribeToRooms();
            return broadcaster;
        }

        private void SubscribeToRooms()
        {
            // LevelBootstrapper.Start'tan önce abone olunur; ilk odanın SetAsCurrent'ı da yakalanır.
            foreach (var room in FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (room.OnRoomEntered == null) continue;
                Room captured = room;
                UnityEngine.Events.UnityAction action = () => OnRoomEntered(captured);
                room.OnRoomEntered.AddListener(action);
                _subscriptions.Add((room, action));
            }
        }

        private void OnRoomEntered(Room room)
        {
            LocalRoomId = room.RoomId;
            _neighborIds.Clear();
            if (room.NeighborRooms != null)
                foreach (var neighbor in room.NeighborRooms)
                    if (neighbor != null) _neighborIds.Add(neighbor.RoomId);

            foreach (var message in _session.OnLocalRoomEntered(room.RoomId))
                _transport.Send(RaceMessageCodec.Encode(message), reliable: true);
        }

        private void Update()
        {
            if (_player == null || _transport == null) return;

            _sendAccumulator += Time.unscaledDeltaTime;
            float interval = 1f / GhostTiming.SnapshotRate;
            if (_sendAccumulator < interval) return;
            _sendAccumulator %= interval;

            var snapshot = _builder.Build(ReadPose(), Time.realtimeSinceStartupAsDouble);
            if (_transport.State == TransportState.Connected)
                _transport.Send(RaceMessageCodec.Encode(snapshot), reliable: false);
        }

        private PlayerPoseInput ReadPose()
        {
            IPlayerState state = _player.CurrentState;
            return new PlayerPoseInput
            {
                RoomId = LocalRoomId,
                Position = _player.transform.position,
                Velocity = _player.Velocity,
                FacingRight = _bodySprite == null || !_bodySprite.flipX,
                IsGrounded = _player.IsGrounded,
                IsSwinging = state is SwingingState || state is DualSwingingState,
                IsDashing = state is DashState,
                IsWallClimbing = state is WallClimbingState,
                IsWallSliding = state is WallSlidingState,
                LeftAnchor = _player.LeftAnchor,
                RightAnchor = _player.RightAnchor
            };
        }

        private void OnDestroy()
        {
            foreach (var (room, action) in _subscriptions)
                if (room != null) room.OnRoomEntered.RemoveListener(action);
            _subscriptions.Clear();
        }
    }
}
