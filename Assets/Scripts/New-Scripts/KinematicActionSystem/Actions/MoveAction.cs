using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// A noktasından B noktasına belirli bir interpolasyon eğrisi kullanarak gitme eylemi.
    /// </summary>
    [System.Serializable]
    public class MoveAction : ActionNode
    {
        [Header("Movement Settings")]
        public Vector3 startPos;
        public Vector3 endPos;
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (duration <= 0) return;
            
            float t = Mathf.Clamp01(localTime / duration);
            float curveVal = movementCurve.Evaluate(t);
            Vector3 targetPos = Vector3.Lerp(startPos, endPos, curveVal);

            if (solver != null)
            {
                float dt = Application.isPlaying ? Time.fixedDeltaTime : Time.deltaTime;
                solver.UpdateSolver(targetPos, dt);
            }
            else
            {
                target.position = targetPos;
            }
        }
    }
}
