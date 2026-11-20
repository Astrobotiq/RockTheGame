using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Duvar dayanıklılığı uyarısı: tırmanırken kalan süre <c>StaminaWarningThreshold</c> altına inince karakterin gövdesi
    /// kırmızıya yanıp söner (Celeste gibi). Mantık <see cref="StaminaFlashState"/>'te, burası onu her karede sürer ve
    /// sonucu <see cref="PlayerColorController.SetFlash"/> ile shader'a yazar. Unscaled zaman: hitstop/pause'da da akar.
    /// Değerler <c>PlayerStatsSO.StaminaFlash*</c> alanlarında (başlangıç önerisi; gamepad ile ayarlanır).
    /// </summary>
    [AddComponentMenu("RockTheGame/Player/Stamina Warning Flash")]
    public sealed class StaminaWarningFlash : MonoBehaviour
    {
        private readonly StaminaFlashState _state = new StaminaFlashState();

        private PlayerColorController _colors;
        private PlayerStatsSO _stats;
        private Color _red = Color.red;
        private float _remaining;
        private float _threshold;
        private bool _flashing;

        public void Bind(PlayerColorController colors, PlayerStatsSO stats)
        {
            _colors = colors;
            _stats = stats;
        }

        /// <summary>Tırmanma başladı/bitti. Bitince flash anında söner.</summary>
        public void SetActive(bool active)
        {
            if (active) _state.SetActive(true);
            else Stop();
        }

        public void SetStamina(float current, float threshold)
        {
            _remaining = current;
            _threshold = threshold;
        }

        /// <summary>Durur ve gövdeyi normale döndürür (zaten durmuş olsa da; ölüm/respawn sonrası temizlik).</summary>
        public void Stop()
        {
            _state.Stop();
            _flashing = false;
            if (_colors != null) _colors.SetFlash(0f, _red);
        }

        private void Awake()
        {
            var theme = UiThemeSO.Load();
            if (theme != null) _red = theme.Red;
        }

        private void OnDisable() => Stop();

        private void Update()
        {
            if (_colors == null || _stats == null) return;

            bool on = _state.Step(Time.unscaledDeltaTime, _remaining, _threshold,
                _stats.StaminaFlashHzAtThreshold, _stats.StaminaFlashHzAtZero);
            if (on == _flashing) return;

            _flashing = on;
            _colors.SetFlash(on ? _stats.StaminaFlashAmount : 0f, _red);
        }
    }
}
