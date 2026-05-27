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
        private Sequence _activeSequence;

        private void Awake()
        {
            if (graphicTarget == null) graphicTarget = transform;
            _initialLocalPosition = graphicTarget.localPosition;
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

            _activeSequence.Append(graphicTarget.DOScale(new Vector3(1.15f, 0.8f, 1f), openDuration * 0.2f)
                .SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget
                .DOLocalMoveY(_initialLocalPosition.y + openTargetY, openDuration * 0.8f).SetEase(Ease.OutBack));

            _activeSequence.Join(graphicTarget.DOScale(new Vector3(0.85f, 1.2f, 1f), openDuration * 0.4f)
                .SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget.DOScale(Vector3.one, openDuration * 0.2f));
        }

        private void PlayCloseAnimation()
        {
            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence();

            _activeSequence.Append(graphicTarget.DOLocalMoveY(_initialLocalPosition.y, closeDuration)
                .SetEase(Ease.InCubic));

            _activeSequence.Join(graphicTarget.DOScale(new Vector3(0.9f, 1.1f, 1f), closeDuration * 0.5f));

            _activeSequence.AppendCallback(() =>
            {
                TriggerImpactVFX();
            });

            _activeSequence.Append(graphicTarget.DOScale(new Vector3(1.3f, 0.6f, 1f), 0.08f).SetEase(Ease.OutQuad));

            _activeSequence.Append(graphicTarget.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.08f).SetEase(Ease.InOutQuad));
            _activeSequence.Append(graphicTarget.DOScale(Vector3.one, 0.05f));
        }

        private void TriggerImpactVFX()
        {
            Debug.Log("[Door Juice] Kapı yere çarptı! Ağır çarpma hissi tetiklendi.");
        }
    }
}