using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.KinematicActionSystem.Core;
using New_Scripts.Player.IFramePauseable;
using UnityEngine;
using Unity.Cinemachine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Kritik vuruş hissi veya hareket bitişlerindeki etkiyi artırmak için
    /// oyunu donduran (Hitstop) veya kamerayı sarsan (Screen Shake) eylem.
    /// </summary>
    [System.Serializable]
    public class HitstopShakeAction : ActionNode
    {
        [Header("Hitstop Settings")]
        public bool triggerHitstop = true;
        public float hitstopDuration = 0.1f;

        [Header("Camera Shake Settings")]
        public bool triggerShake = true;
        public float shakeForce = 1f;
        public Vector3 shakeVelocity = new Vector3(0f, -1f, 0f);

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            // Editör önizlemesinde atlanır
        }

        public override async UniTask ExecuteAsync(Transform target, IKinematicSolver solver, CancellationToken cancellationToken)
        {
            if (!isEnabled) return;

            if (triggerHitstop)
            {
                HitStopEvents.RequestHitStop?.Invoke(hitstopDuration);
            }

            if (triggerShake)
            {
                var impulse = target.GetComponent<CinemachineImpulseSource>();
                if (impulse == null)
                {
                    impulse = target.GetComponentInChildren<CinemachineImpulseSource>();
                }

                if (impulse != null)
                {
                    impulse.GenerateImpulseWithVelocity(shakeVelocity * shakeForce);
                }
                else
                {
                    Debug.LogWarning($"CinemachineImpulseSource not found on {target.name} for HitstopShakeAction!");
                }
            }

            await UniTask.CompletedTask;
        }
    }
}
