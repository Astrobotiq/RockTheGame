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
        [SerializeField] private float transitionDelay = 0.5f;

        private ICameraTransitionHandler cameraHandler;
        private CancellationTokenSource transitionCts;
    
        // SISTEMI KORUYAN KILIT (STATE LOCK)
        private bool isTransitioning = false;

        private void Awake()
        {
            cameraHandler = GetComponent<ICameraTransitionHandler>();
        }

        public void ExecuteTransition(IPlayerTransitionable player, Collider2D newBounds, Vector2 spawnPosition)
        {
            // Eger halihazirda bir gecis yapiyorsak, fizik motorundan gelen tum ekstra carpismalari YOK SAY.
            if (isTransitioning) return;

            transitionCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            TransitionRoutineAsync(player, newBounds, spawnPosition, transitionCts.Token).Forget();
        }

        private async UniTask TransitionRoutineAsync(IPlayerTransitionable player, Collider2D newBounds, Vector2 spawnPosition, CancellationToken token)
        {
            // Kilidi kapat
            isTransitioning = true;

            player.FreezeForTransition();
            cameraHandler.PrepareForTransition();

            player.TeleportTo(spawnPosition);

            await UniTask.Delay(TimeSpan.FromSeconds(transitionDelay), cancellationToken: token);

            
            cameraHandler.FinalizeTransition(newBounds);
            player.UnfreezeFromTransition();
            

            // Kilidi geri ac
            isTransitioning = false;
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