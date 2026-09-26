using DG.Tweening;
using TMPro;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Toplanan altın/çilek sayısını ekranda güncelleyen ve toplanma anında
    /// DOTween kullanarak tatlı bir ölçeklenme (punch) efekti uygulayan UI Kontrolcüsü.
    /// </summary>
    public class CollectibleUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CollectibleInventorySO inventory;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private RectTransform iconTransform;

        [Header("Animation Settings")]
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private Vector3 punchScale = new Vector3(0.2f, 0.2f, 0f);
        [SerializeField] private int punchVibrato = 10;

        private Tween punchTween;
        private Tween textTween;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnCoinsChanged += HandleCoinsChanged;
                UpdateUI(inventory.TotalCoins, animate: false);
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnCoinsChanged -= HandleCoinsChanged;
            }
            KillTweens();
        }

        private void HandleCoinsChanged(int newCount)
        {
            UpdateUI(newCount, animate: true);
        }

        private void UpdateUI(int count, bool animate)
        {
            if (countText != null)
            {
                countText.text = count.ToString("D2"); // "00", "01", etc.
            }

            if (animate)
            {
                KillTweens();

                // Simgenin ölçeğini sars (punch)
                if (iconTransform != null)
                {
                    iconTransform.localScale = Vector3.one;
                    punchTween = iconTransform.DOPunchScale(punchScale, punchDuration, punchVibrato).SetUpdate(true);
                }

                // Metnin ölçeğini sars (punch)
                if (countText != null)
                {
                    countText.transform.localScale = Vector3.one;
                    textTween = countText.transform.DOPunchScale(punchScale, punchDuration, punchVibrato).SetUpdate(true);
                }
            }
        }

        private void KillTweens()
        {
            punchTween?.Kill();
            textTween?.Kill();

            if (iconTransform != null) iconTransform.localScale = Vector3.one;
            if (countText != null) countText.transform.localScale = Vector3.one;
        }
    }
}
