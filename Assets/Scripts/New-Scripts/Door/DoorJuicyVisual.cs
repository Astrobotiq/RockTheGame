using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace New_Scripts.Door
{
    public class DoorJuicyVisual : MonoBehaviour
    {
        [FormerlySerializedAs("doorContext")]
        [Header("Dependency")] 
        [SerializeField]
        private RoomDoor roomDoorContext;

        [Header("Visual Settings")] [SerializeField]
        private Transform graphicTarget;

        [Header("Open Animation Settings")]
        [SerializeField] private float openTargetX = 0f;
        [SerializeField] private float openTargetY = 3.5f;

        [SerializeField] private float openDuration = 0.4f;

        [Header("Close Animation Settings")] [SerializeField]
        private float closeDuration = 0.25f;

        private Vector3 _initialLocalPosition;
        private Vector3 _initialLocalScale;
        private Sequence _activeSequence;

        private void Awake()
        {
            if (graphicTarget == null) graphicTarget = transform;
            _initialLocalPosition = graphicTarget.localPosition;
            _initialLocalScale = graphicTarget.localScale;
        }

        private void OnEnable()
        {
            if (roomDoorContext != null)
            {
                roomDoorContext.OnOpened += PlayOpenAnimation;
                roomDoorContext.OnClosed += PlayCloseAnimation;
                
                SnapToCurrentState();
            }
        }

        private void OnDisable()
        {
            if (roomDoorContext != null)
            {
                roomDoorContext.OnOpened -= PlayOpenAnimation;
                roomDoorContext.OnClosed -= PlayCloseAnimation;
            }
        }

        private void SnapToCurrentState()
        {
            if (roomDoorContext != null && graphicTarget != null)
            {
                if (roomDoorContext.IsOpen)
                {
                    graphicTarget.localPosition = new Vector3(_initialLocalPosition.x + openTargetX, _initialLocalPosition.y + openTargetY, _initialLocalPosition.z);
                }
                else
                {
                    graphicTarget.localPosition = _initialLocalPosition;
                }
            }
        }

        private void PlayOpenAnimation()
        {
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();

            bool isHorizontal = Mathf.Abs(openTargetX) > Mathf.Abs(openTargetY);
            Vector3 squashedScale;
            Vector3 stretchedScale;

            if (isHorizontal)
            {
                squashedScale = new Vector3(_initialLocalScale.x * 0.8f, _initialLocalScale.y * 1.15f, _initialLocalScale.z);
                stretchedScale = new Vector3(_initialLocalScale.x * 1.2f, _initialLocalScale.y * 0.85f, _initialLocalScale.z);
            }
            else
            {
                squashedScale = new Vector3(_initialLocalScale.x * 1.15f, _initialLocalScale.y * 0.8f, _initialLocalScale.z);
                stretchedScale = new Vector3(_initialLocalScale.x * 0.85f, _initialLocalScale.y * 1.2f, _initialLocalScale.z);
            }

            _activeSequence.Append(graphicTarget.DOScale(squashedScale, openDuration * 0.2f)
                .SetEase(Ease.OutQuad));

            Vector3 targetPosition = new Vector3(_initialLocalPosition.x + openTargetX, _initialLocalPosition.y + openTargetY, _initialLocalPosition.z);
            _activeSequence.Append(graphicTarget
                .DOLocalMove(targetPosition, openDuration * 0.8f).SetEase(Ease.OutBack));

            _activeSequence.Join(graphicTarget.DOScale(stretchedScale, openDuration * 0.4f)
                .SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget.DOScale(_initialLocalScale, openDuration * 0.2f));
        }

        private void PlayCloseAnimation()
        {
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();

            bool isHorizontal = Mathf.Abs(openTargetX) > Mathf.Abs(openTargetY);
            Vector3 prepScale;
            Vector3 impactScale;
            Vector3 bounceScale;

            if (isHorizontal)
            {
                prepScale = new Vector3(_initialLocalScale.x * 1.1f, _initialLocalScale.y * 0.9f, _initialLocalScale.z);
                impactScale = new Vector3(_initialLocalScale.x * 0.6f, _initialLocalScale.y * 1.3f, _initialLocalScale.z);
                bounceScale = new Vector3(_initialLocalScale.x * 1.05f, _initialLocalScale.y * 0.95f, _initialLocalScale.z);
            }
            else
            {
                prepScale = new Vector3(_initialLocalScale.x * 0.9f, _initialLocalScale.y * 1.1f, _initialLocalScale.z);
                impactScale = new Vector3(_initialLocalScale.x * 1.3f, _initialLocalScale.y * 0.6f, _initialLocalScale.z);
                bounceScale = new Vector3(_initialLocalScale.x * 0.95f, _initialLocalScale.y * 1.05f, _initialLocalScale.z);
            }

            _activeSequence.Append(graphicTarget.DOLocalMove(_initialLocalPosition, closeDuration)
                .SetEase(Ease.InCubic));

            _activeSequence.Join(graphicTarget.DOScale(prepScale, closeDuration * 0.5f));

            _activeSequence.AppendCallback(() =>
            {
                TriggerImpactVFX();
            });

            _activeSequence.Append(graphicTarget.DOScale(impactScale, 0.08f).SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget.DOScale(bounceScale, 0.08f).SetEase(Ease.InOutQuad));
            _activeSequence.Append(graphicTarget.DOScale(_initialLocalScale, 0.05f));
        }

        private void TriggerImpactVFX()
        {
            Debug.Log("[Door Juice] Kapı yere çarptı! Ağır çarpma hissi tetiklendi.");
        }

        private void OnDrawGizmosSelected()
        {
            if (graphicTarget == null) return;

            Transform parent = graphicTarget.parent != null ? graphicTarget.parent : transform;
            Vector3 localPos = Application.isPlaying ? _initialLocalPosition : graphicTarget.localPosition;
            Vector3 openLocalPos = new Vector3(localPos.x + openTargetX, localPos.y + openTargetY, localPos.z);
            Vector3 openWorldPos = parent.TransformPoint(openLocalPos);
            Vector3 currentWorldPos = parent.TransformPoint(localPos);

            // Draw a line from start to end position
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(currentWorldPos, openWorldPos);

            // Try to draw a box corresponding to the size of the sprite/renderer if available
            var spriteRenderer = graphicTarget.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Bounds bounds = spriteRenderer.bounds;
                Vector3 boundsOffset = openWorldPos - graphicTarget.position;
                Vector3 openBoundsCenter = bounds.center + boundsOffset;
                
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                Gizmos.DrawWireCube(openBoundsCenter, bounds.size);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(openWorldPos, 0.5f);
            }
        }
    }
}