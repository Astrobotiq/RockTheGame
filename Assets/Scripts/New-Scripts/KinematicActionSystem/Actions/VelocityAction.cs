using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Kinematik çözücüye ivme/momentum pompalayan eylem.
    /// </summary>
    [System.Serializable]
    public class VelocityAction : ActionNode
    {
        [Header("Velocity Settings")]
        public Vector2 impulseVelocity;

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (localTime == 0 && solver != null)
            {
                solver.ApplyVelocity(impulseVelocity);
            }
        }
    }
}
