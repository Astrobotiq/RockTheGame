using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Belirli bir Euler açısına veya dönme miktarına interpolasyon ile dönme eylemi.
    /// </summary>
    [System.Serializable]
    public class RotateAction : ActionNode
    {
        [Header("Rotation Settings")]
        public Vector3 startRotationEuler;
        public Vector3 endRotationEuler;
        public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (duration <= 0) return;

            float t = Mathf.Clamp01(localTime / duration);
            float curveVal = rotationCurve.Evaluate(t);
            
            Quaternion startRot = Quaternion.Euler(startRotationEuler);
            Quaternion endRot = Quaternion.Euler(endRotationEuler);
            
            target.rotation = Quaternion.Slerp(startRot, endRot, curveVal);
        }
    }
}
