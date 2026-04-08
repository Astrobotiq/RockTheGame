using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Player.UI
{
    /// <summary>
    /// Karakterin stamina bari gibi dinamik UI bilesenlerinin gorunurluk, dolum ve kaybolma animasyonlarini yoneten kontrolcu.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup staminaBarGroup;
        [SerializeField] private Image staminaFillImage;
            
        private Tween fadeTween;
        private Tween scaleTween;
        private Sequence hideSequence;
    
        private bool isVisible;

        private void Awake()
        {
            staminaBarGroup.alpha = 0f;
            staminaBarGroup.transform.localScale = Vector3.zero;
        }

        public void ShowStaminaBar()
        {
            if (isVisible) return;
            isVisible = true;

            hideSequence?.Kill();
            fadeTween?.Kill();
            scaleTween?.Kill();

            fadeTween = staminaBarGroup.DOFade(1f, 0.2f).SetUpdate(true);
            scaleTween = staminaBarGroup.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void UpdateStamina(float current, float max)
        {
            staminaFillImage.fillAmount = current / max;
        }

        public void RefillAndHideStaminaBar()
        {
            if (!isVisible) return;
            isVisible = false;

            hideSequence?.Kill();
            fadeTween?.Kill();
            scaleTween?.Kill();

            hideSequence = DOTween.Sequence().SetUpdate(true);
            hideSequence.Append(staminaFillImage.DOFillAmount(1f, 0.1f));
            hideSequence.AppendInterval(0.3f);
            hideSequence.Append(staminaBarGroup.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            hideSequence.Join(staminaBarGroup.DOFade(0f, 0.2f));
        }
    }
}