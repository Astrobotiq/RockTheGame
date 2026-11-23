using System.Collections.Generic;
using New_Scripts.Death;
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
        private PlayerHealth _health;
        private MovingSurfaceRegistry _surfaces;
        private SpriteRenderer _bodySprite;
        private RaceSession _session;
        private IRaceTransport _transport;
        private float _sendAccumulator;

        /// <summary>Sahnedeki hareketli nesne kimlikleri; hayalet konturu aynı tabloyu kullanır.</summary>
        public MovingSurfaceRegistry Surfaces => _surfaces;

        /// <summary>Yerel oyuncu ceza balonunda mı (<c>PenaltyHandler.IsInBubble</c>); boşsa hiçbir zaman.</summary>
        public System.Func<bool> InBubbleSource { get; set; }

        public int LocalRoomId { get; private set; }
        public IReadOnlyCollection<int> LocalNeighborRoomIds => _neighborIds;

        public static LocalPlayerBroadcaster Create(PlayerController player, RaceSession session, IRaceTransport transport)
        {
            var broadcaster = new GameObject("[MP Broadcaster]").AddComponent<LocalPlayerBroadcaster>();
            broadcaster._player = player;
            broadcaster._session = session;
            broadcaster._transport = transport;
            // Hareketli nesne kimlikleri sahne yüklenince bir kez kurulur (çalışırken yaratılanlar kimlik almaz).
            broadcaster._surfaces = MovingSurfaceRegistry.BuildFromScene();
            broadcaster._health = player.GetComponent<PlayerHealth>();
            if (broadcaster._health != null) broadcaster._health.OnDeath += broadcaster.OnPlayerDeath;
            var animation = player.GetComponentInChildren<PlayerAnimationController>(true);
            broadcaster._bodySprite = animation != null ? animation.GetComponent<SpriteRenderer>() : null;
            broadcaster.SubscribeToRooms();
            broadcaster.EnterRoomContainingPlayer();
            return broadcaster;
        }

        /// <summary>
        /// Sahne önceden yüklüyse (geliştirici başlatıcısı) ilk odanın OnRoomEntered'ı kaçmıştır: oyuncunun içinde durduğu
        /// oda ilk oda sayılır. Lobi yolunda LevelBootstrapper ardından gerçek başlangıç odasını yine bildirir.
        /// </summary>
        private void EnterRoomContainingPlayer()
        {
            Vector2 position = _player.transform.position;
            foreach (var (room, _) in _subscriptions)
            {
                if (room.RoomBounds == null || !room.RoomBounds.OverlapPoint(position)) continue;
                OnRoomEntered(room);
                return;
            }
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

        /// <summary>Hareketli bir tehlike öldürdüyse rakibe bildirir; sabit tehlike ya da bilinmeyen ölümde mesaj yok.</summary>
        private void OnPlayerDeath()
        {
            if (_transport == null || _transport.State != TransportState.Connected) return;
            if (_health.LastKiller == null || !_surfaces.TryGetPose(_health.LastKiller, out SurfacePose killer)) return;
            _transport.Send(RaceMessageCodec.Encode(new DeathMessage { Killer = killer }), reliable: true);
        }

        private SurfacePose? ReadCarrier()
        {
            var carrier = _player.PhysicsHandler != null ? _player.PhysicsHandler.CurrentCarrier as Component : null;
            return carrier != null && _surfaces.TryGetPose(carrier.transform, out SurfacePose pose) ? pose : (SurfacePose?)null;
        }

        private SurfacePose? ReadNode(Transform node) =>
            node != null && _surfaces.TryGetPose(node, out SurfacePose pose) ? pose : (SurfacePose?)null;

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
                RightAnchor = _player.RightAnchor,
                Carrier = ReadCarrier(),
                LeftNode = ReadNode(_player.LeftAnchorNode),
                RightNode = ReadNode(_player.RightAnchorNode),
                // PlayerHealth ölünce oyuncu nesnesini kapatır, RespawnManager yeni yerde açar.
                IsHidden = !_player.gameObject.activeInHierarchy,
                InBubble = InBubbleSource != null && InBubbleSource()
            };
        }

        private void OnDestroy()
        {
            if (_health != null) _health.OnDeath -= OnPlayerDeath;
            foreach (var (room, action) in _subscriptions)
                if (room != null) room.OnRoomEntered.RemoveListener(action);
            _subscriptions.Clear();
        }
    }
}
