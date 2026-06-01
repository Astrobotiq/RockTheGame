using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Nesneyi aktif veya pasif duruma getiren eylem.
    /// </summary>
    [System.Serializable]
    public class ToggleActiveAction : ActionNode
    {
        [Header("Toggle Settings")]
        public bool activeState;

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (localTime >= 0)
            {
                target.gameObject.SetActive(activeState);
            }
        }
    }
}
