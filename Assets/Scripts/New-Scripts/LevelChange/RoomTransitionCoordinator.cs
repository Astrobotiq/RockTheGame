using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    /// <summary>
    /// Gecis anindaki senkronizasyonu saglayan ve cift tetiklenmeleri (Double Trigger) bloke eden asenkron koordinator sinifidir.
    /// </summary>
    public class RoomTransitionCoordinator : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] float physicsCooldownDelay = 0.5f;
        [SerializeField] private RoomManager roomManager;

        private ICameraTransitionHandler cameraHandler;
        private CancellationTokenSource transitionCts;

        private bool isTransitioning = false;

        private void Awake()
        {
            cameraHandler = GetComponent<ICameraTransitionHandler>();
        }
        
        public void ExecuteTransition(
            IPlayerTransitionable player,
            Room targetRoom,
            Collider2D newBounds,
            Vector2 spawnPosition,
            TransitionDirection direction,
            float targetSize,
            bool overrideZoom)
        {
            if (isTransitioning) return;
            transitionCts?.Cancel();
            transitionCts?.Dispose();
            transitionCts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());

            TransitionRoutineAsync(
                player, targetRoom, newBounds, spawnPosition,
                direction, targetSize, overrideZoom,
                transitionCts.Token
            ).Forget();
        }

        private async UniTask TransitionRoutineAsync(
            IPlayerTransitionable player,
            Room targetRoom,
            Collider2D newBounds,
            Vector2 spawnPosition,
            TransitionDirection direction,
            float targetSize,
            bool overrideZoom,
            CancellationToken token)
        {
            isTransitioning = true;

            player.FreezeForTransition();
            cameraHandler.PrepareForTransition();

            await cameraHandler.PanAndZoomCameraAsync(
                spawnPosition, targetSize, overrideZoom,
                newBounds, transitionDuration, token);

            cameraHandler.FinalizeTransition(newBounds, targetSize, overrideZoom);

            roomManager.TransitionToRoom(targetRoom);

            player.UnfreezeFromTransition(direction);

            isTransitioning = false;

            await UniTask.Delay(
                TimeSpan.FromSeconds(physicsCooldownDelay),
                cancellationToken: token);
        }

        private void OnDestroy()
        {
            if (transitionCts != null)
            {
                transitionCts.Cancel();
                transitionCts.Dispose();
            }
        }
    }
}