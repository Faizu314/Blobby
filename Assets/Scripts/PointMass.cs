using UnityEngine;

public class PointMass : MonoBehaviour {

    [SerializeField] private bool m_ShowNormalGizmo;
    [SerializeField] private bool m_ShowFrameGizmo;

    public Vector2 Position { 
        get {
            if (m_MassRb == null)
                return transform.position;
            return m_MassRb.position;
        }
    }

    private Rigidbody2D m_MassRb;
    public Rigidbody2D Rb => m_MassRb;
    private CircleCollider2D m_Collider;

    private Vector3 DebugNormalForce;
    private Vector3 DebugFrameForce;

    private void Awake()
    {
        m_MassRb = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<CircleCollider2D>();
    }

    public void ApplyNormalForce(Vector2 force) {
        DebugNormalForce = force;
        m_MassRb.AddForce(force);
    }

    public void SetCollision(bool enabled) {
        m_Collider.enabled = enabled;
    }

    private void OnDrawGizmos() {
        if (m_ShowNormalGizmo) { 
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Position, (Vector3)Position + (DebugNormalForce));
        }
        if (m_ShowFrameGizmo) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Position, (Vector3)Position + (DebugFrameForce));
        }
    }
}
