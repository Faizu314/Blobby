using UnityEngine;

public class JoystickInput : MonoBehaviour
{
    private enum JoystickType {
        Primary,
        Secondary
    }

    [SerializeField] private JoystickType m_JoystickType;
    [SerializeField] private Joystick m_Joystick;

    private InputManager.InterfaceInputs m_Inputs;

    private void Start() {
        m_Inputs = InputManager.Instance.interfaceInputs;
    }

    private void Update() {
        if (m_JoystickType == JoystickType.Primary) {
            m_Inputs.PrimaryJoyStick = m_Joystick.Direction;
        }
        else if (m_JoystickType == JoystickType.Secondary) {
            m_Inputs.SecondaryJoyStick = m_Joystick.Direction;
        }
    }
}
