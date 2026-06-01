using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Nesnenin boyutunu (Scale) interpolasyon eğrisi ile esnetip basıklaştıran eylem.
    /// </summary>
    [System.Serializable]
    public class SquashStretchAction : ActionNode
    {
        [Header("Scale Settings")]
        public Vector3 baseScale = Vector3.one;
        public Vector3 targetScale = new Vector3(0.8f, 1.2f, 1.0f);
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (duration <= 0) return;

            float t = Mathf.Clamp01(localTime / duration);
            float curveVal = scaleCurve.Evaluate(t);
            target.localScale = Vector3.Lerp(baseScale, targetScale, curveVal);
        }
    }
}
