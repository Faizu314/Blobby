using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance => m_Instance;
    private static InputManager m_Instance;
    private InputActions m_Input;

    public delegate void HookAction(Vector2 mousePos);
    public event HookAction OnJump;

    public PlayerInputs playerInputs = new();

    private void Awake() {
        if (m_Instance != null)
            Destroy(this);
        m_Instance = this;
    }

    private void Start() {
        m_Input = new InputActions();

        m_Input.Movement.Enable();
        m_Input.Movement.Click.performed += _ => OnJump?.Invoke(GetMouseWorldPos());
    }

    private Vector2 GetMouseWorldPos() {
        var mousePos = m_Input.Movement.MousePos.ReadValue<Vector2>();
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void Update() {
        ResetState();
        UpdateState();
    }

    private void ResetState() {
        playerInputs.MovingForward = playerInputs.MovingBackward = false;
    }

    private void UpdateState() {
        if (m_Input.Movement.Forward.IsInProgress())
            playerInputs.MovingForward = true;
        else if (m_Input.Movement.Backward.IsPressed())
            playerInputs.MovingBackward = true;
    }

    [Serializable]
    public class PlayerInputs {
        public bool MovingForward;
        public bool MovingBackward;
    }
}
