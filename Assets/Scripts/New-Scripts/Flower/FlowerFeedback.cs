using New_Scripts.Audio;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin ses ve titreşimi: uyarıda yükselen tıklar (kalan süre), tutunma, snap. <see cref="FlowerCycle"/>'ı dinler, durumu
    /// değiştirmez. Ses cue'ları <see cref="FlowerAudioSettingsSO"/>'da, titreşim profilleri <see cref="FlowerVibrationSettingsSO"/>'da;
    /// titreşim yalnızca oyuncu çiçeğe tutunurken çalar. Ayar ya da klip yoksa ilgili geri bildirim sessizce atlanır.
    /// </summary>
    [RequireComponent(typeof(FlowerCycle))]
    public class FlowerFeedback : MonoBehaviour
    {
        [SerializeField] private FlowerAudioSettingsSO audioSettings;
        [SerializeField] private FlowerVibrationSettingsSO vibrationSettings;
        [Tooltip("SFX-Play kanalı. Boşsa ses çalmaz.")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;

        private FlowerCycle _cycle;
        private int _ticksPlayed;

        private void OnEnable()
        {
            _cycle = GetComponent<FlowerCycle>();
            _cycle.Grabbed += OnGrabbed;
            _cycle.PhaseChanged += OnPhaseChanged;
            _ticksPlayed = 0;
        }

        private void OnDisable()
        {
            if (_cycle == null) return;
            _cycle.Grabbed -= OnGrabbed;
            _cycle.PhaseChanged -= OnPhaseChanged;
        }

        private void Update()
        {
            if (audioSettings == null || _cycle.Phase != FlowerPhase.Warning) return;

            int due = FlowerFeedbackMath.TicksDue(_cycle.Warn01, audioSettings.warnTickCount);
            while (_ticksPlayed < due)
            {
                PlayWarnTick(_ticksPlayed, audioSettings.warnTickCount);
                _ticksPlayed++;
            }
        }

        private void OnPhaseChanged(FlowerPhase phase)
        {
            _ticksPlayed = 0;
            if (phase != FlowerPhase.Closing) return;

            if (audioSettings != null) PlayCue(audioSettings.snapCue, transform.position, 1f);
            if (_cycle.IsHeld) Vibrate(vibrationSettings != null ? vibrationSettings.snapRumble : default);
        }

        private void OnGrabbed(ActiveArm arm, Vector2 point)
        {
            if (audioSettings != null) PlayCue(audioSettings.grabCue, point, 1f);
            Vibrate(vibrationSettings != null ? vibrationSettings.grabRumble : default);
        }

        private void PlayWarnTick(int index, int count)
        {
            PlayCue(audioSettings.warnTickCue, transform.position,
                FlowerFeedbackMath.TickPitch(index, count, audioSettings.warnTickPitch.x, audioSettings.warnTickPitch.y));
            if (_cycle.IsHeld) Vibrate(vibrationSettings != null ? vibrationSettings.warnTickRumble : default);
        }

        private void PlayCue(AudioCueSO cue, Vector3 position, float pitch)
        {
            // Klip yoksa boş cue: sessiz (AcidReaction ile aynı koruma).
            if (sfxPlayChannel == null || cue == null || cue.AudioClips == null || cue.AudioClips.Length == 0) return;
            var p = AudioCuePlayParams.Default;
            p.Position = position;
            p.Is3D = true;
            p.PitchMultiplier = pitch;
            sfxPlayChannel.RaisePlayEvent(cue, p);
        }

        private static void Vibrate(RumbleProfile profile)
        {
            if (profile.duration <= 0f || HapticManager.Instance == null) return;
            HapticManager.Instance.Vibrate(profile);
        }
    }
}
