using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Oyuncunun tetikleyebileceği tüm seslerin (zıplama, dash, yere iniş vb.)
    /// referanslarını bir arada tutan ScriptableObject veri sınıfı.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerAudioData", menuName = "Player/Player Audio Data")]
    public class PlayerAudioDataSO : ScriptableObject
    {
        [Header("Movement Sounds")]
        [SerializeField] private AudioCueSO jumpCue;
        [SerializeField] private AudioCueSO landCue;
        [SerializeField] private AudioCueSO dashCue;

        [Header("Grapple & Slingshot Sounds")]
        [SerializeField] private AudioCueSO grappleLaunchCue;
        [SerializeField] private AudioCueSO grappleConnectCue;
        [SerializeField] private AudioCueSO slingshotLaunchCue;

        public AudioCueSO JumpCue => jumpCue;
        public AudioCueSO LandCue => landCue;
        public AudioCueSO DashCue => dashCue;
        public AudioCueSO GrappleLaunchCue => grappleLaunchCue;
        public AudioCueSO GrappleConnectCue => grappleConnectCue;
        public AudioCueSO SlingshotLaunchCue => slingshotLaunchCue;
    }
}
