using UnityEngine;

public class ReferenceFramePositioner : MonoBehaviour
{
    [SerializeField] private PointMassesController m_Controller;

    private Transform m_Transform;

    private void Awake() {
        m_Transform = transform;
    }

    private void FixedUpdate() {
        m_Transform.position = m_Controller.Position;
        m_Transform.localEulerAngles = new(0f, 0f, m_Controller.PointMassesDeviation);
    }
}
