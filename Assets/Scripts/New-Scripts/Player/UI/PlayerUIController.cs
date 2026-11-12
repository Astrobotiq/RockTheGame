using New_Scripts.UI.Hud;
using UnityEngine;

namespace New_Scripts.Player.UI
{
    /// <summary>
    /// Karakterin duvar dayanıklılığı göstergesinin köprüsü: <see cref="PlayerController"/> çağrılarını kırılan stamina halkasına
    /// (<see cref="HudStaminaRing"/>, Overlay canvas) iletir. Eski World Space çubuk (<c>StaminaCanvas</c>) kapatılır;
    /// sahne referansı bozulmasın diye alan tutulur.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [Tooltip("Eski World Space çubuk. Halka onun yerini aldı; Awake'te kapatılır.")]
        [SerializeField] private CanvasGroup staminaBarGroup;

        private HudStaminaRing _ring;

        private void Awake()
        {
            if (staminaBarGroup != null) staminaBarGroup.gameObject.SetActive(false);

            _ring = gameObject.AddComponent<HudStaminaRing>();
            _ring.Bind(transform, GetComponentInParent<BoxCollider2D>());
        }

        public void ShowStaminaBar() => _ring.Show();

        public void UpdateStamina(float current, float max, float warningThreshold) =>
            _ring.SetStamina(current, max, warningThreshold);

        public void RefillAndHideStaminaBar() => _ring.RefillAndHide();
    }
}
