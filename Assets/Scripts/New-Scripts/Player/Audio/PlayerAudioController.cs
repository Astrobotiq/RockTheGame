using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Oyuncu GameObject'i üzerinde yer alan ve oyuncunun ses eylemlerini
    /// AudioManager'a ileten bağımsız kontrolcü bileşeni.
    /// </summary>
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;
        [Tooltip("Döngü seslerini (spiral) durdurmak için AudioManager'ın dinlediği SFX stop kanalı.")]
        [SerializeField] private AudioCueStopEventChannelSO sfxStopChannel;

        [Header("Audio Data")]
        [SerializeField] private PlayerAudioDataSO audioData;

        public void PlayJump()
        {
            PlaySound(audioData?.JumpCue);
        }

        public void PlayLand()
        {
            PlaySound(audioData?.LandCue);
        }

        public void PlayDash()
        {
            PlaySound(audioData?.DashCue);
        }

        public void PlayGrappleLaunch()
        {
            PlaySound(audioData?.GrappleLaunchCue);
        }

        public void PlayGrappleConnect()
        {
            PlaySound(audioData?.GrappleConnectCue);
        }

        public void PlaySlingshotLaunch()
        {
            PlaySound(audioData?.SlingshotLaunchCue);
        }

        public void PlaySlingshotAnticipation()
        {
            PlaySound(audioData?.SlingshotAnticipationCue);
        }

        public void PlaySpiralLoop()
        {
            PlaySound(audioData?.SpiralLoopCue);
        }

        public void StopSpiralLoop()
        {
            AudioCueSO cue = audioData?.SpiralLoopCue;
            if (sfxStopChannel != null && cue != null)
                sfxStopChannel.RaiseStopEvent(cue);
        }

        public void PlaySpiralOpposeGrab()
        {
            PlaySound(audioData?.SpiralOpposeGrabCue);
        }

        public void PlaySpiralReverse()
        {
            PlaySound(audioData?.SpiralReverseCue);
        }

        private void PlaySound(AudioCueSO cue)
        {
            if (sfxPlayChannel != null && cue != null)
            {
                sfxPlayChannel.RaisePlayEvent(cue, transform.position);
            }
        }
    }
}
