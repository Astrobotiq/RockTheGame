using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Kinematik eylemlerin türeyeceği temel sınıf.
    /// [SerializeReference] ile polimorfik olarak serileştirilebilmesi için düz bir sınıf (class) olarak tanımlanmıştır.
    /// </summary>
    [System.Serializable]
    public abstract class ActionNode
    {
        public string name;
        public float startTime;
        public float duration;
        public bool isEnabled = true;

        /// <summary>
        /// Eylemin durumunu belirli bir yerel zamanda hesaplar ve uygular.
        /// </summary>
        public abstract void Evaluate(Transform target, IKinematicSolver solver, float localTime);

        /// <summary>
        /// Oyun içi runtime asenkron çalıştırma.
        /// </summary>
        public virtual async UniTask ExecuteAsync(Transform target, IKinematicSolver solver, CancellationToken cancellationToken)
        {
            if (!isEnabled) return;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Evaluate(target, solver, elapsed);
                
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
                elapsed += Time.fixedDeltaTime;
            }
            Evaluate(target, solver, duration);
        }
    }
}
