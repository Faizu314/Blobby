using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] private SpringJoint2D m_TailJoint;
    [SerializeField] private Rigidbody2D m_Hook;

    public void AttachTo(Rigidbody2D rb) {
        m_TailJoint.connectedBody = rb;
    }

    public void Shoot(Vector2 force) {
        m_Hook.AddForce(force * m_Hook.mass);
    }
}
