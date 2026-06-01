using New_Scripts.KinematicActionSystem.Core;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Actions
{
    /// <summary>
    /// Projedeki özel KinematicSplinePath bileşenini kullanarak pürüzsüz bir rota üzerinde hareket eylemi.
    /// </summary>
    [System.Serializable]
    public class SplineMoveAction : ActionNode
    {
        [Header("Spline Settings")]
        public KinematicSplinePath splinePath;
        public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override void Evaluate(Transform target, IKinematicSolver solver, float localTime)
        {
            if (duration <= 0) return;

            KinematicSplinePath path = splinePath;
            if (path == null)
            {
                path = target.GetComponent<KinematicSplinePath>();
            }

            if (path == null)
            {
                Debug.LogWarning($"KinematicSplinePath bulunamadı! SplineMoveAction yürütülemedi. ({target.name})");
                return;
            }

            float t = Mathf.Clamp01(localTime / duration);
            float curveVal = speedCurve.Evaluate(t);

            Vector3 worldPos = path.GetPoint(curveVal);

            if (solver != null)
            {
                float dt = Application.isPlaying ? Time.fixedDeltaTime : Time.deltaTime;
                solver.UpdateSolver(worldPos, dt);
            }
            else
            {
                target.position = worldPos;
            }
        }
    }
}
