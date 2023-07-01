using UnityEngine;

public class SoftCarController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;

    [SerializeField] private Rigidbody2D m_ForwardEdge;
    [SerializeField] private Rigidbody2D m_RearEdge;
    [SerializeField] private GameObject m_HookPrefab;

    [SerializeField] private float Speed;
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float HookForce;

    private InputActions m_Input;
    private GameObject m_HookObj;

    private void Start() {
        m_Input = new InputActions();

        m_Input.Movement.Enable();
        m_Input.Movement.Jump.performed += _ => OnJump();
    }

    private void FixedUpdate() {
        if (m_Input.Movement.Forward.IsInProgress()) {
            MoveForward(Speed);
        }
        else if (m_Input.Movement.Backward.IsPressed())
            MoveForward(-Speed);

        if (m_Input.Movement.RotateC.IsPressed())
            Rotate(RotationSpeed);
        else if (m_Input.Movement.RotateAC.IsPressed())
            Rotate(-RotationSpeed);
    }


    private void MoveForward(float amount) {
        m_BodyController.ApplyTorque(amount);
    }

    private void Rotate(float amount) {
        m_BodyController.ApplyTorque(amount);
    }

    private void OnJump() {
        if (m_HookObj == null) {
            float rot = m_BodyController.PointMassesDeviation;
            Vector2 dir = Util.VectorFromAngle(rot);

            m_HookObj = Instantiate(m_HookPrefab, m_ForwardEdge.position + dir, Quaternion.identity);
            var hook = m_HookObj.GetComponent<Hook>();
            hook.AttachTo(m_ForwardEdge);
            hook.Shoot(dir * HookForce);
        }
        else
            Destroy(m_HookObj);
    }
}
