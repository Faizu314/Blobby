using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_MidMass;

    [SerializeField] private float m_JumpStrength;

    private InputActions m_Input;
    private void Start()
    {
        m_Input = new InputActions();
        m_Input.Movement.Enable();

        m_Input.Movement.Jump.performed += _ => OnJump();
    }

    private void OnJump() {
        m_MidMass.AddForce(Vector2.up * m_JumpStrength, ForceMode2D.Force);
    }
}
