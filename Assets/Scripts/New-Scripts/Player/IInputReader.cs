using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Sistemin dış dünyadan (oyuncu veya yapay zeka) alacağı girdi verilerinin sözleşmesini tanımlar.
    /// </summary>

    public interface IInputReader
    {
        Vector2 LeftStick { get; }
        Vector2 RightStick { get; }
        bool IsLeftTriggerHeld { get; }
        bool IsRightTriggerHeld { get; }
        bool IsJumpPressed { get; }
        bool IsDashPressed { get; }
    }
}