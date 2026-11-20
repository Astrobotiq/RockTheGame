using New_Scripts.Player.Visual;
using UnityEngine;

namespace New_Scripts.Player.UI
{
    /// <summary>
    /// Karakterin duvar dayanıklılığı uyarısının köprüsü: <see cref="PlayerController"/> çağrılarını karakter yanıp sönmesine
    /// (<see cref="StaminaWarningFlash"/>) iletir; ekranda arayüz öğesi yok. Genel API halka döneminden kaldı
    /// (<c>ShowStaminaBar</c> adı yanıltıcı, çağıranlar değişmesin diye korundu). Eski World Space çubuk (<c>StaminaCanvas</c>)
    /// kapatılır; sahne referansı bozulmasın diye alan tutulur.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [Tooltip("Eski World Space çubuk. Karakter yanıp sönmesi onun yerini aldı; Awake'te kapatılır.")]
        [SerializeField] private CanvasGroup staminaBarGroup;

        private StaminaWarningFlash _flash;

        private void Awake()
        {
            if (staminaBarGroup != null) staminaBarGroup.gameObject.SetActive(false);

            _flash = gameObject.AddComponent<StaminaWarningFlash>();
        }

        private void Start()
        {
            var player = GetComponentInParent<PlayerController>();
            if (player != null) _flash.Bind(player.ColorController, player.Stats);
        }

        /// <summary>Tırmanma başladı: uyarı eşiğine inince yanıp sönme açılır.</summary>
        public void ShowStaminaBar() => _flash.SetActive(true);

        public void UpdateStamina(float current, float max, float warningThreshold) =>
            _flash.SetStamina(current, warningThreshold);

        /// <summary>Enerji doldu / respawn: yanıp sönme durur, gövde normale döner.</summary>
        public void RefillAndHideStaminaBar() => _flash.Stop();

        /// <summary>Duvardan ayrılınca (<c>WallClimbingState.ExitState</c>): yanıp sönme durur.</summary>
        public void StopStaminaWarning() => _flash.Stop();
    }
}
