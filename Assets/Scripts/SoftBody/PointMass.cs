using UnityEngine;

public class PointMass : MonoBehaviour {

    [SerializeField] private float m_StickyBreakForce;
    [SerializeField] private bool m_ShowNormalGizmo;
    [SerializeField] private bool m_ShowGroundedGizmo;

    public Vector2 Position { 
        get {
            if (m_MassRb == null)
                return transform.position;
            return m_MassRb.position;
        }
    }

    private Rigidbody2D m_MassRb;
    public Rigidbody2D Rb => m_MassRb;
    public bool IsGrounded { get; private set; }

    private Vector3 DebugNormalForce;
    private RigidbodyConstraints2D m_OriginalConstraints;
    private StickyMoveable m_GroundedSurface;
    private DistanceJoint2D m_StickyJoint;
    private bool m_IsFrozen = false;

    protected virtual void Awake()
    {
        m_MassRb = GetComponent<Rigidbody2D>();
        m_OriginalConstraints = m_MassRb.constraints;
    }
    public void ApplyNormalForce(Vector2 force) {
        DebugNormalForce = force;
        m_MassRb.AddForce(force);
    }
    public void OnGrounded(StickyMoveable surface, bool isGrounded) {
        m_GroundedSurface = surface;
        IsGrounded = isGrounded;
    }
    public void SetFreeze(bool freeze) {
        if (m_IsFrozen == freeze)
            return;

        if (freeze)
            Freeze();
        else
            UnFreeze();

        m_IsFrozen = freeze;
    }

    private void Freeze() {
        if (m_GroundedSurface == null)
            m_MassRb.constraints = RigidbodyConstraints2D.FreezeAll;
        else {
            m_StickyJoint = gameObject.AddComponent<DistanceJoint2D>();
            m_StickyJoint.connectedBody = m_GroundedSurface.Rb;
            m_StickyJoint.connectedAnchor = m_GroundedSurface.transform.InverseTransformPoint(m_MassRb.position);
            m_StickyJoint.autoConfigureDistance = false;
            m_StickyJoint.distance = 0f;
            m_StickyJoint.breakForce = m_StickyBreakForce;
        }
    }

    private void OnJointBreak2D(Joint2D joint) {
        if (joint != m_StickyJoint)
            return;
        UnFreeze();
        m_IsFrozen = false;
    }

    private void UnFreeze() {
        if (m_StickyJoint == null)
            m_MassRb.constraints = m_OriginalConstraints;
        else {
            m_StickyJoint.connectedBody = null;
            m_StickyJoint.enabled = false;
            Destroy(m_StickyJoint);
            m_StickyJoint = null;
        }
    }

    private void OnDrawGizmos() {
        if (m_StickyJoint != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        if (m_ShowNormalGizmo) { 
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Position, (Vector3)Position + (DebugNormalForce));
        }
        if (m_ShowGroundedGizmo && IsGrounded) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_MassRb.position, 0.1f);
        }
    }
}
