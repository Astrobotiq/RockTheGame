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
        [SerializeField] private AudioCueSO slingshotAnticipationCue;
        [SerializeField] private AudioCueSO slingshotLaunchCue;

        [Header("Spiral Node Sounds")]
        [Tooltip("Spirale tutunurken çalan döngü (cue loop olmalı). Perde sabit.")]
        [SerializeField] private AudioCueSO spiralLoopCue;
        [Tooltip("Spirale tersine giderken tutunma anı.")]
        [SerializeField] private AudioCueSO spiralOpposeGrabCue;
        [Tooltip("Ters dönen spiral yön değiştirdiğinde.")]
        [SerializeField] private AudioCueSO spiralReverseCue;

        public AudioCueSO JumpCue => jumpCue;
        public AudioCueSO LandCue => landCue;
        public AudioCueSO DashCue => dashCue;
        public AudioCueSO GrappleLaunchCue => grappleLaunchCue;
        public AudioCueSO GrappleConnectCue => grappleConnectCue;
        public AudioCueSO SlingshotAnticipationCue => slingshotAnticipationCue;
        public AudioCueSO SlingshotLaunchCue => slingshotLaunchCue;
        public AudioCueSO SpiralLoopCue => spiralLoopCue;
        public AudioCueSO SpiralOpposeGrabCue => spiralOpposeGrabCue;
        public AudioCueSO SpiralReverseCue => spiralReverseCue;
    }
}
