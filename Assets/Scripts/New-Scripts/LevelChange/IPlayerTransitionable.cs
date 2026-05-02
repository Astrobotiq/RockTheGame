using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    /// <summary>
    /// Oyuncunun geçiş anındaki fiziksel durumunu ve girdilerini yönetmesi için gereken sözleşmeyi tanımlar.
    /// </summary>
    public interface IPlayerTransitionable
    {
        void FreezeForTransition();
        void UnfreezeFromTransition();
        void TeleportTo(Vector2 position);
        UniTask MoveToAsync(Vector2 targetPosition, float duration, CancellationToken token);
    }
}