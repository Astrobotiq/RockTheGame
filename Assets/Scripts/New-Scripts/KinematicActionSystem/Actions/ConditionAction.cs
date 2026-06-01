using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Bir tetikleyici (KinematicTrigger) aktif olana kadar sekansı bekleten (await) mantıksal eylem.
    /// </summary>
    [System.Serializable]
    public class ConditionAction : ActionNode
    {
        [Header("Condition Settings")]
        public bool autoResetTrigger = true;

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            // Editör önizlemesinde bekleme yapılmaz.
        }

        public override async UniTask ExecuteAsync(Transform target, IKinematicSolver solver, CancellationToken cancellationToken)
        {
            if (!isEnabled) return;

            KinematicTrigger trigger = target.GetComponent<KinematicTrigger>();
            if (trigger == null)
            {
                trigger = target.GetComponentInChildren<KinematicTrigger>();
            }

            if (trigger == null)
            {
                Debug.LogWarning($"KinematicTrigger not found on {target.name} for ConditionAction! Waiting skipped.");
                return;
            }

            await UniTask.WaitUntil(() => trigger.IsTriggered, cancellationToken: cancellationToken);

            if (autoResetTrigger)
            {
                trigger.ResetTrigger();
            }
        }
    }
}
