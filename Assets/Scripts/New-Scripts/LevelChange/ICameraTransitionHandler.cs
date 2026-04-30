using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// Kamera bileseninin gecis durumlarini ve sinirlarini dinamik olarak guncelleyen sozlesmeyi tanimlar.
/// </summary>
public interface ICameraTransitionHandler
{
    void PrepareForTransition();
    UniTask PanCameraToAsync(Vector2 targetPosition, Collider2D newBounds, float duration, CancellationToken token);
    void FinalizeTransition(Collider2D newBounds);
}