using UnityEngine;
using UnityEngine.InputSystem;

namespace New_Scripts.Player
{
    /// <summary>
    /// Unity New Input System kullanarak oyuncu girdilerini okuyan ve IInputReader arayüzünü uygulayan bağımsız sistem.
    /// </summary>
    public class InputHandler : MonoBehaviour, IInputReader
    {
        [SerializeField] private InputActionReference leftStickAction;
        [SerializeField] private InputActionReference rightStickAction;
        [SerializeField] private InputActionReference leftTriggerAction;
        [SerializeField] private InputActionReference rightTriggerAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference leftBumperAction;
        [SerializeField] private InputActionReference rightBumperAction;
        [SerializeField] private float triggerThreshold = 0.1f;

        public Vector2 LeftStick => leftStickAction.action.ReadValue<Vector2>();
        public Vector2 RightStick => rightStickAction.action.ReadValue<Vector2>();
        public bool IsLeftTriggerHeld => leftTriggerAction.action.ReadValue<float>() >= triggerThreshold;
        public bool IsRightTriggerHeld => rightTriggerAction.action.ReadValue<float>() >= triggerThreshold;
        public bool IsJumpPressed => jumpAction.action.WasPressedThisFrame();
        public bool IsDashPressed => dashAction.action.WasPressedThisFrame();
        public bool IsLeftBumperHeld => leftBumperAction.action.IsPressed();
        public bool IsRightBumperHeld => rightBumperAction.action.IsPressed();

        private void OnEnable()
        {
            leftStickAction.action.Enable();
            rightStickAction.action.Enable();
            leftTriggerAction.action.Enable();
            rightTriggerAction.action.Enable();
            jumpAction.action.Enable();
            dashAction.action.Enable();
            leftBumperAction.action.Enable();
            rightBumperAction.action.Enable();
        }

        private void OnDisable()
        {
            leftStickAction.action.Disable();
            rightStickAction.action.Disable();
            leftTriggerAction.action.Disable();
            rightTriggerAction.action.Disable();
            jumpAction.action.Disable();
            dashAction.action.Disable();
            leftBumperAction.action.Disable();
            rightBumperAction.action.Disable();
        }
    }
}