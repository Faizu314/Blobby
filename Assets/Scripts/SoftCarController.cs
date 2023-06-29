using UnityEngine;

public class SoftCarController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;
    [SerializeField] private PointMassesController m_LeftWheelController;
    [SerializeField] private PointMassesController m_RightWheelController;

    [SerializeField] private float Speed;

    private InputActions m_Input;
    private Coroutine m_MovementCoroutine = null;

    private void Start() {
        m_Input = new InputActions();

        m_Input.Movement.Enable();
    }

    private void FixedUpdate() {
        if (m_Input.Movement.Forward.IsPressed()) {
            MoveForward(Speed);
        }
        else if (m_Input.Movement.Backward.IsPressed())
            MoveForward(-Speed);
    }


    private void MoveForward(float amount) {
        m_LeftWheelController.ApplyTorque(amount);
        m_RightWheelController.ApplyTorque(amount);
    }

}
