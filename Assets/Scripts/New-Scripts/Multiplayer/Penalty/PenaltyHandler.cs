using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Death;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Rakibin cezasını uygular: yerel oyuncu bir önceki odanın başlangıç checkpoint'ine gider. Dizi RespawnManager'ınkiyle
    /// aynı (iris kapanır, sıfırlama, ışınlama, kamera ve oda, iris açılır); checkpoint de etkinleşir, sonraki ölüm oraya döner.
    /// Oyuncu ölü, duraklatılmış ya da oda geçişindeyken ceza bekler; hedef uygulama anındaki odadan hesaplanır.
    /// Tek kişilik kodu yalnız çağırır, değiştirmez.
    /// </summary>
    public sealed class PenaltyHandler : MonoBehaviour
    {
        public const string ObjectName = "[MP Penalty]";
        private const float CameraCatchUpSeconds = 0.5f;

        private RaceSession _session;
        private PlayerController _player;
        private RoomManager _roomManager;
        private TransitionManager _transitionManager;
        private readonly List<Room> _rooms = new List<Room>();
        private CancellationTokenSource _cts;

        public int Pending { get; private set; }
        public bool IsApplying { get; private set; }

        /// <summary>Uygulanan her cezadan sonra; argüman gidilen oda.</summary>
        public event Action<int> PenaltyApplied;

        public static bool IsPlayerBusy(bool active, bool paused, bool inTransition) => !active || paused || inTransition;

        public static PenaltyHandler Create(RaceSession session, PlayerController player)
        {
            var handler = new GameObject(ObjectName).AddComponent<PenaltyHandler>();
            handler._session = session;
            handler._player = player;
            handler._roomManager = FindFirstObjectByType<RoomManager>(FindObjectsInactive.Include);
            handler._transitionManager = FindFirstObjectByType<TransitionManager>(FindObjectsInactive.Include);
            handler._rooms.AddRange(FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            return handler;
        }

        public void Enqueue() => Pending++;

        private void OnEnable() => _cts = new CancellationTokenSource();

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void Update()
        {
            if (Pending == 0 || IsApplying || _player == null) return;
            if (IsPlayerBusy(_player.gameObject.activeInHierarchy, _player.IsPaused, _player.CurrentState is PlayerTransitionState)) return;

            Pending--;
            Room current = FindRoom(_session.LocalRoomId);
            var neighbors = new List<int>();
            if (current != null && current.NeighborRooms != null)
                foreach (var n in current.NeighborRooms) if (n != null) neighbors.Add(n.RoomId);

            if (!_session.TryGetPenaltyTarget(neighbors, out int targetId))
            {
                Debug.Log("[MP] Ceza işlemedi (ilk oda, birlikte modu ya da yarış bitti)");
                return;
            }

            Room target = FindRoom(targetId);
            if (target == null)
            {
                Debug.LogWarning($"[MP] Ceza hedefi oda {targetId} sahnede yok");
                return;
            }

            ApplyAsync(target, _cts.Token).Forget();
        }

        private async UniTaskVoid ApplyAsync(Room target, CancellationToken token)
        {
            IsApplying = true;
            try
            {
                Debug.Log($"[MP] Ceza: oda {_session.LocalRoomId} → {target.RoomId}");
                _player.OnStartRespawn();

                if (_transitionManager != null)
                    await _transitionManager.PlayCloseTransitionAsync(_player.transform.position, token);

                if (LevelResetManager.Instance != null) LevelResetManager.Instance.ResetAll();

                Vector3 spawn = target.InitialCheckpoint != null ? target.InitialCheckpoint.transform.position : target.transform.position;
                _player.transform.position = new Vector3(spawn.x, spawn.y, _player.transform.position.z);
                _player.Velocity = Vector2.zero;
                if (target.InitialCheckpoint != null) target.InitialCheckpoint.ActivateCheckpoint();

                if (CameraController.Instance is ICameraTransitionHandler cameraHandler && target.RoomBounds != null)
                    cameraHandler.SnapToRoomBounds(target.RoomBounds, spawn, target.OverrideCameraSize, target.OverrideDynamicZoom);
                // SetAsCurrent → Room.OnRoomEntered → yayıncı RoomEntered'ı yollar, RaceSession'ın odası güncellenir.
                if (_roomManager != null) _roomManager.TransitionToRoom(target);

                await UniTask.Delay(TimeSpan.FromSeconds(CameraCatchUpSeconds), cancellationToken: token);

                if (_transitionManager != null)
                    await _transitionManager.PlayOpenTransitionAsync(spawn, token);

                _player.OnEndRespawn();
                PenaltyApplied?.Invoke(target.RoomId);
            }
            catch (OperationCanceledException)
            {
                // Başlatıcı kapandı ya da sahne değişti; oyuncu kaldıysa serbest bırakılır.
                if (_player != null) _player.OnEndRespawn();
            }
            finally
            {
                IsApplying = false;
            }
        }

        private Room FindRoom(int id) => _rooms.Find(r => r != null && r.RoomId == id);
    }
}
