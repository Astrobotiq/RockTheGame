using UnityEngine;

namespace New_Scripts.Player
{
    [CreateAssetMenu(fileName = "PlayerVibrationSettings", menuName = "Player/Vibration Settings")]
    public class PlayerVibrationSettingsSO : ScriptableObject
    {
        [Header("One-Shot Rumble Profiles")]
        public RumbleProfile Dash;
        public RumbleProfile HighImpact;
        public RumbleProfile Death;
        public RumbleProfile Jump;
        public RumbleProfile Land;

        [Header("Continuous Wall Slide Profiles")]
        public ContinuousRumbleProfile WallSlideLeft;
        public ContinuousRumbleProfile WallSlideRight;

        [Header("Continuous Wall Climb Profiles")]
        public ContinuousRumbleProfile WallClimbLeft;
        public ContinuousRumbleProfile WallClimbRight;

        [Header("Spiral Node")]
        [Tooltip("Spirale tutunurken sürekli titreşim.")]
        public ContinuousRumbleProfile SpiralOrbit;
        [Tooltip("Spirale tersine giderek tutunma anı.")]
        public RumbleProfile SpiralOpposeGrab;
        [Tooltip("Ters dönen spiral yön değiştirdiğinde.")]
        public RumbleProfile SpiralReverse;
        [Tooltip("Spiralden tetiği bırakarak hızlı çıkış (SP8). Kısa, HighImpact'ten hafif.")]
        public RumbleProfile SpiralRelease;
    }
}
