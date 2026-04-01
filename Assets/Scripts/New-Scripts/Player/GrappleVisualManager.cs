using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Ana bağlamdaki kanca verilerini okuyan, ipin mermi gibi fırlatılmasını (Lerp), sönümlü sinüs dalgası ve zarf fonksiyonu kullanarak kıvrılmasını hesaplayan bağımsız görsel sistem.
    /// </summary>
    public class GrappleVisualManager : MonoBehaviour
    {
        [Header("Dependencies")] 
        [SerializeField] private PlayerController playerContext;

        [Header("Renderers")] 
        [SerializeField] private LineRenderer leftRopeRenderer;
        [SerializeField] private LineRenderer rightRopeRenderer;

        [Header("Animation Settings")] [SerializeField]
        private int resolution = 30;

        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private float waveSize = 1.5f;
        [SerializeField] private float waveSpeed = 40f;
        [SerializeField] private int waveCount = 3;

        private RopeData leftRopeData = new RopeData();
        private RopeData rightRopeData = new RopeData();

        private class RopeData
        {
            public bool isActive;
            public float animationTimer;
            public Vector2? currentAnchor;
        }

        private void Awake()
        {
            InitializeRope(leftRopeRenderer);
            InitializeRope(rightRopeRenderer);
        }

        private void LateUpdate()
        {
            UpdateRopeVisuals(leftRopeRenderer, leftRopeData, playerContext.LeftArm.transform.position,
                playerContext.LeftAnchor);
            UpdateRopeVisuals(rightRopeRenderer, rightRopeData, playerContext.RightArm.transform.position,
                playerContext.RightAnchor);
        }

        private void InitializeRope(LineRenderer rope)
        {
            if (rope != null)
            {
                rope.positionCount = resolution;
                rope.enabled = false;
                rope.useWorldSpace = true;
            }
        }

        private void UpdateRopeVisuals(LineRenderer rope, RopeData data, Vector3 startPosition, Vector2? targetAnchor)
        {
            if (rope == null) return;

            if (targetAnchor.HasValue)
            {
                if (!data.isActive || data.currentAnchor != targetAnchor)
                {
                    data.isActive = true;
                    data.animationTimer = 0f;
                    data.currentAnchor = targetAnchor;
                    rope.enabled = true;
                    rope.positionCount = resolution;
                }

                data.animationTimer += Time.deltaTime;
                float percent = Mathf.Clamp01(data.animationTimer / animationDuration);

                DrawRopeWave(rope, startPosition, targetAnchor.Value, percent);
            }
            else
            {
                if (data.isActive)
                {
                    data.isActive = false;
                    data.currentAnchor = null;
                    rope.enabled = false;
                }
            }
        }

        private void DrawRopeWave(LineRenderer rope, Vector2 start, Vector2 target, float percent)
        {
            Vector2 currentEnd = Vector2.Lerp(start, target, percent);
            Vector2 direction = (currentEnd - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float currentWaveSize = waveSize * (1f - percent);

            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / (resolution - 1);
                Vector2 pointPos = Vector2.Lerp(start, currentEnd, t);

                if (percent < 1f)
                {
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float sineWave = Mathf.Sin(t * waveCount * Mathf.PI * 2f - Time.time * waveSpeed);
                    Vector2 offset = perpendicular * (sineWave * currentWaveSize * envelope);
                    pointPos += offset;
                }

                rope.SetPosition(i, pointPos);
            }
        }
    }
}