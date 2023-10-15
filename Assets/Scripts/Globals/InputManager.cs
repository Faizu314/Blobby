using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance => m_Instance;
    private static InputManager m_Instance;

    private InputActions m_Inputs;

    public GlobalInputs globalInputs = new();

    private void Awake() {
        if (m_Instance != null)
            Destroy(this);
        m_Instance = this;
    }

    private void Start() {
        m_Inputs = new InputActions();

        m_Inputs.Movement.Enable();
    }

    private void Update() {
        ResetState();
        UpdateState();
    }

    private void ResetState() {
        globalInputs.MoveDir = Vector2.zero;
        globalInputs.CursorPos = Vector2.one / 2f;
        globalInputs.PrimaryAction = globalInputs.SecondaryAction = false;
    }

    private void UpdateState() {
        globalInputs.MoveDir = m_Inputs.Movement.Direction.ReadValue<Vector2>();
        globalInputs.CursorPos = ToViewPortSpace(m_Inputs.Movement.MousePos.ReadValue<Vector2>());
        globalInputs.PrimaryAction = m_Inputs.Movement.PrimaryAction.IsPressed();
        globalInputs.SecondaryAction = m_Inputs.Movement.SecondaryAction.IsPressed();
    }

    private Vector2 ToViewPortSpace(Vector2 pos) {
        return Camera.main.ScreenToViewportPoint(pos);
    }

    [Serializable]
    public class GlobalInputs {
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
        public bool PrimaryAction;
        /// <summary>
        /// Was pressed this frame.
        /// </summary>
        public bool SecondaryAction;
    }
}
