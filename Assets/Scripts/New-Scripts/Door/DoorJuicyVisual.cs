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

        [Header("Open Animation Settings")] [SerializeField]
        private float openTargetY = 3.5f;

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

        private void PlayOpenAnimation()
        {
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();

            Vector3 squashedScale = new Vector3(_initialLocalScale.x * 1.15f, _initialLocalScale.y * 0.8f, _initialLocalScale.z);
            Vector3 stretchedScale = new Vector3(_initialLocalScale.x * 0.85f, _initialLocalScale.y * 1.2f, _initialLocalScale.z);

            _activeSequence.Append(graphicTarget.DOScale(squashedScale, openDuration * 0.2f)
                .SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget
                .DOLocalMoveY(_initialLocalPosition.y + openTargetY, openDuration * 0.8f).SetEase(Ease.OutBack));

            _activeSequence.Join(graphicTarget.DOScale(stretchedScale, openDuration * 0.4f)
                .SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget.DOScale(_initialLocalScale, openDuration * 0.2f));
        }

        private void PlayCloseAnimation()
        {
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();

            Vector3 prepScale = new Vector3(_initialLocalScale.x * 0.9f, _initialLocalScale.y * 1.1f, _initialLocalScale.z);
            Vector3 impactScale = new Vector3(_initialLocalScale.x * 1.3f, _initialLocalScale.y * 0.6f, _initialLocalScale.z);
            Vector3 bounceScale = new Vector3(_initialLocalScale.x * 0.95f, _initialLocalScale.y * 1.05f, _initialLocalScale.z);

            _activeSequence.Append(graphicTarget.DOLocalMoveY(_initialLocalPosition.y, closeDuration)
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
    }
}