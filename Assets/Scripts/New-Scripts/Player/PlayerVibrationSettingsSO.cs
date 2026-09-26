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
    }
}
