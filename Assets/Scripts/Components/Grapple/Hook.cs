using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] private RopeNode m_RopeNode;
    [SerializeField] private Rigidbody2D m_Hook;
    [SerializeField] private LayerMask m_StickyLayer;

    private bool m_IsStuck = false;
    public bool IsStuck => m_IsStuck;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!Util.IsInLayerMask(m_StickyLayer, collision.gameObject.layer))
            return;

        m_Hook.isKinematic = true;
        m_Hook.inertia = 0f;
        m_Hook.angularVelocity = 0f;
        m_Hook.velocity = Vector2.zero;
        m_IsStuck = true;
    }

    public void AttachTo(Rigidbody2D rb) {
        m_RopeNode.AttachTo(rb, this);
    }

    public void Shoot(Vector2 force) {
        m_Hook.AddForce(force * m_Hook.mass);
    }

}
