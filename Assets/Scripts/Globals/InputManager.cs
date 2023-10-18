using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class InputManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private bool m_TestUnityRemote;
#endif

    public static InputManager Instance => m_Instance;
    private static InputManager m_Instance;

    private InputActions m_Inputs;

    public HardwareInputs hardwareInputs = new();
    public InterfaceInputs interfaceInputs = new();

    private void Awake() {
        if (m_Instance != null)
            Destroy(this);
        m_Instance = this;
    }

    private void Start() {
        EnhancedTouchSupport.Enable();

        m_Inputs = new InputActions();

        m_Inputs.Movement.Enable();
    }


#if UNITY_EDITOR
    private void Update() {
        ResetState();

        if (m_TestUnityRemote)
            UpdateStateOldInput();
        else
            UpdateState();
    }

    private void UpdateStateOldInput() {
        if (Input.touchCount < 1)
            return;
        UnityEngine.Touch primaryTouch = Input.GetTouch(0);
        hardwareInputs.MoveDir = primaryTouch.deltaPosition.normalized;
        hardwareInputs.CursorPos = new(primaryTouch.position.x / Screen.width, primaryTouch.position.y / Screen.height);
        hardwareInputs.PrimaryAction = true;
        hardwareInputs.PrimaryActionDown = primaryTouch.phase == TouchPhase.Began;
    }

#else
    private void Update() {
        ResetState();
        UpdateState();
    }
#endif

    private void ResetState() {
        hardwareInputs.MoveDir = Vector2.one / 2f;
        hardwareInputs.CursorPos = Vector2.one / 2f;
    }

    private void UpdateState() {
        hardwareInputs.MoveDir = m_Inputs.Movement.Direction.ReadValue<Vector2>();
        hardwareInputs.CursorPos = ToViewPortSpace(m_Inputs.Movement.CursorPos.ReadValue<Vector2>());
        if (SystemInfo.deviceType == DeviceType.Handheld) {
            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (touches.Count == 1)
                hardwareInputs.CursorPos = ToViewPortSpace(touches[0].screenPosition);
        }

        hardwareInputs.PrimaryAction = m_Inputs.Movement.PrimaryAction.IsPressed();
        hardwareInputs.PrimaryActionDown = m_Inputs.Movement.PrimaryAction.WasPressedThisFrame();
    }

    private Vector2 ToViewPortSpace(Vector2 pos) {
        return new(pos.x / Screen.width, pos.y / Screen.height);
    }

    [Serializable]
    public class HardwareInputs {
        /// <summary>
        /// Normalized.
        /// </summary>
        public Vector2 MoveDir;
        /// <summary>
        /// In Viewport Space.
        /// </summary>
        public Vector2 CursorPos;
        /// <summary>
        /// Was pressed this frame.
        /// </summary>
        public bool PrimaryActionDown;
        /// <summary>
        /// Is down this frame.
        /// </summary>
        public bool PrimaryAction;
    }

    [Serializable]
    public class InterfaceInputs {
        public Vector2 PrimaryJoyStick;
        public Vector2 SecondaryJoyStick;
        public bool PrimaryButton;
    }
}
