using UnityEngine;

public class PointMass : MonoBehaviour {

    [SerializeField] private bool m_ShowNormalGizmo;

    public Vector2 Position { 
        get {
            if (m_MassRb == null)
                return transform.position;
            return m_MassRb.position;
        }
    }

    private Rigidbody2D m_MassRb;
    public Rigidbody2D Rb => m_MassRb;

    private Vector3 DebugNormalForce;

    private void Awake()
    {
        m_MassRb = GetComponent<Rigidbody2D>();
    }

    public void ApplyNormalForce(Vector2 force) {
        DebugNormalForce = force;
        m_MassRb.AddForce(force);
    }

    private void OnDrawGizmos() {
        if (m_ShowNormalGizmo) { 
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Position, (Vector3)Position + (DebugNormalForce));
        }
    }
}
