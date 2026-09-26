using UnityEngine;
using New_Scripts.Player;

namespace New_Scripts.LevelChange
{
    [CreateAssetMenu(fileName = "TransitionVibrationSettings", menuName = "Platform/Transition Vibration Settings")]
    public class TransitionVibrationSettingsSO : ScriptableObject
    {
        [Header("Room Transition Profiles")]
        public RumbleProfile TransitionLeft;
        public RumbleProfile TransitionRight;
        public RumbleProfile TransitionUp;
        public RumbleProfile TransitionDown;
    }
}
