using UnityEngine;
using New_Scripts.Platform;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Kinematik eylemlerin fizik çözücü ile haberleşmesini sağlayan arayüz.
    /// </summary>
    public interface IKinematicSolver : IMovingSurface
    {
        void Initialize(GameObject owner);
        void UpdateSolver(Vector3 targetPosition, float deltaTime);
        void ApplyVelocity(Vector2 velocity);
        void ResetSolver();
    }
}
