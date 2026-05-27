using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    /// <summary>
    /// Kamera gecislerini, boyutlandirmayi ve sinirlandirmayi asenkron olarak yoneten genisletilmis sozlesme.
    /// </summary>
    public interface ICameraTransitionHandler
    {
        void PrepareForTransition();

        UniTask PanAndZoomCameraAsync(Vector2 targetPosition, float targetSize, bool isOverridden, Collider2D newBounds,
            float duration, CancellationToken token);
        void FinalizeTransition(Collider2D newBounds, float targetSize, bool isOverridden);
        
        void SnapToRoomBounds(Collider2D newBounds, Vector2 targetPosition, float targetSize, bool isOverridden);
    }
}