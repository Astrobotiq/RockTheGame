/// <summary>
/// Karakterin etrafındaki kanca noktalarını tarayan, ContactFilter2D ile optimize edilmiş açısal (Dot Product) ve görüş açısı (Line of Sight) testleri yapan bağımsız sensör sınıfı.
/// </summary>
using Unity.Profiling;
using UnityEngine;

namespace New_Scripts.Player
{
    public class NodeDetector : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 15f;
        [SerializeField] private float minimumDotProduct = 0.5f;
        [SerializeField] private LayerMask nodeLayerMask;
        [SerializeField] private LayerMask obstacleLayerMask;

        private readonly Collider2D[] colliders = new Collider2D[10];
        private static readonly ProfilerMarker detectionProfilerMarker = new ProfilerMarker("NodeDetection");
        
        private ContactFilter2D nodeContactFilter;
        private ContactFilter2D obstacleContactFilter;

        private void Awake()
        {
            nodeContactFilter = new ContactFilter2D();
            nodeContactFilter.SetLayerMask(nodeLayerMask);
            nodeContactFilter.useLayerMask = true;
            nodeContactFilter.useTriggers = true;

            obstacleContactFilter = new ContactFilter2D();
            obstacleContactFilter.SetLayerMask(obstacleLayerMask);
            obstacleContactFilter.useLayerMask = true;
            obstacleContactFilter.useTriggers = true;
        }

        public bool TryFindBestNode(Vector2 aimDirection, Vector2 origin, out Vector2 bestNodePosition)
        {
            detectionProfilerMarker.Begin();
            
            bestNodePosition = Vector2.zero;
            
            if (aimDirection.sqrMagnitude < 0.01f)
            {
                detectionProfilerMarker.End();
                return false;
            }

            Vector2 normalizedAim = aimDirection.normalized;
            int count = Physics2D.OverlapCircle(origin, detectionRadius, nodeContactFilter, colliders);
            
            float highestDot = -1f;
            bool nodeFound = false;
            RaycastHit2D[] obstacleHits = new RaycastHit2D[1];

            for (int i = 0; i < count; i++)
            {
                Vector2 nodePos = colliders[i].transform.position;
                Vector2 directionToNode = (nodePos - origin).normalized;
                float dotProduct = Vector2.Dot(normalizedAim, directionToNode);

                if (dotProduct >= minimumDotProduct && dotProduct > highestDot)
                {
                    float distanceToNode = Vector2.Distance(origin, nodePos);
                    int obstacleCount = Physics2D.Raycast(origin, directionToNode, obstacleContactFilter, obstacleHits, distanceToNode);

                    if (obstacleCount == 0)
                    {
                        highestDot = dotProduct;
                        bestNodePosition = nodePos;
                        nodeFound = true;
                    }
                }
            }

            detectionProfilerMarker.End();
            return nodeFound;
        }
    }
}