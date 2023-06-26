using UnityEngine;

public class SoftCarController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;
    [SerializeField] private PointMassesController m_LeftWheelController;
    [SerializeField] private PointMassesController m_RightWheelController;

    [SerializeField] private SpringJoint2D m_LeftWheelFixedJoint;
    [SerializeField] private SpringJoint2D m_RightWheelFixedJoint;

    private Vector2 GetLeftWheelPosition() {
        return (m_BodyController.GetReferencePointPos(10) +
            m_BodyController.GetReferencePointPos(11)) / 2f;
    }
    private Vector2 GetRightWheelPosition() {
        return (m_BodyController.GetReferencePointPos(8) +
            m_BodyController.GetReferencePointPos(9)) / 2f;
    }

    private void FixedUpdate() {
        m_LeftWheelFixedJoint.connectedAnchor = GetLeftWheelPosition();
        m_RightWheelFixedJoint.connectedAnchor = GetRightWheelPosition();
    }
}
