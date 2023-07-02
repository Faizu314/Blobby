using UnityEngine;

public class RopeNode : MonoBehaviour {

    [SerializeField] private SpringJoint2D m_TailJoint;
    [SerializeField] private GameObject m_RopeNodePrefab;
    [Tooltip("Must be larger than the rest length of the spring joint.")]
    [SerializeField] private float m_MaxSqrDistance;
    [SerializeField] private int m_Index;

    private Rigidbody2D m_Rb;
    private Rigidbody2D m_CurrConnected = null;
    private bool m_IsLast;

    private void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    public void AttachTo(Rigidbody2D rb) {
        m_TailJoint.connectedBody = rb;
        m_CurrConnected = rb;
        m_IsLast = true;
    }

    public void Detach() {
        m_TailJoint.connectedBody = null;
    }

    private void FixedUpdate() {
        if (m_CurrConnected == null)
            return;
        if (m_Index < 1)
            return;

        if (Vector2.SqrMagnitude(m_Rb.position - m_CurrConnected.position) > m_MaxSqrDistance) {
            var dir = (m_CurrConnected.position - m_Rb.position).normalized;
            var newNode = Instantiate(m_RopeNodePrefab, m_Rb.position + (dir * m_TailJoint.distance), Quaternion.identity);
            var newRopeNode = newNode.GetComponent<RopeNode>();
            newRopeNode.m_Index = m_Index - 1;
            newRopeNode.AttachTo(m_CurrConnected);
            AttachTo(newRopeNode.m_Rb);
            if (m_IsLast)
                newRopeNode.m_IsLast = true;

            m_IsLast = false;
        }
    }

    private void OnDestroy() {
        if (m_CurrConnected == null)
            return;
        if (m_IsLast)
            return;

        Destroy(m_TailJoint);
        Destroy(m_CurrConnected.gameObject);
    }

}