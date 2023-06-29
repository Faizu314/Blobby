using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float RotationTorque;
    [SerializeField] private float Speed;

    private Rigidbody2D m_Rb;

    private void Start() {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if (Mathf.Abs(m_Rb.angularVelocity) > Speed)
            return;
        m_Rb.AddTorque(RotationTorque);
    }
}
