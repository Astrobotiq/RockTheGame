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

        private ICameraTransitionHandler cameraHandler;
        private CancellationTokenSource transitionCts;

        // SISTEMI KORUYAN KILIT (STATE LOCK)
        private bool isTransitioning = false;

        private void Awake()
        {
            cameraHandler = GetComponent<ICameraTransitionHandler>();
        }

        // Coordinator icindeki TransitionRoutineAsync imzasi ve cagrisi:

        public void ExecuteTransition(IPlayerTransitionable player, Collider2D newBounds, Vector2 spawnPosition,
            float targetSize, bool overrideZoom)
        {
            if (isTransitioning)
                return;
            transitionCts?.Cancel();
            transitionCts?.Dispose();
            transitionCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            TransitionRoutineAsync(player, newBounds, spawnPosition, targetSize, overrideZoom, transitionCts.Token)
                .Forget();
        }

        private async UniTask TransitionRoutineAsync(
            IPlayerTransitionable player,
            Collider2D newBounds,
            Vector2 spawnPosition,
            float targetSize,
            bool overrideZoom,
            CancellationToken token)
        {
            isTransitioning = true;
            player.FreezeForTransition();
            cameraHandler.PrepareForTransition();

            // Kamera ve oyuncu aynı anda aynı hedefe doğru hareket eder
            await UniTask.WhenAll(
                cameraHandler.PanAndZoomCameraAsync(
                    spawnPosition, targetSize, overrideZoom, newBounds, transitionDuration, token),
                player.MoveToAsync(
                    spawnPosition, transitionDuration, token)
            );

            cameraHandler.FinalizeTransition(newBounds, targetSize, overrideZoom);
            player.UnfreezeFromTransition();
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