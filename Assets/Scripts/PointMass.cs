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

    private Transform m_Transform;
    private Rigidbody2D m_MassRb;

    private Vector3 DebugNormalForce;

    private void Awake()
    {
        m_Transform = transform;
        m_MassRb = m_Transform.GetComponent<Rigidbody2D>();
    }

    public void ApplyNormalForce(Vector2 force) {
        DebugNormalForce = force;
        m_MassRb.AddForce(force);
    }

    public void ApplyReferenceFrameForce(Vector2 force) {
        m_MassRb.AddForce(force);
    }

    public void TranslateRb(Vector2 displacement) {
        m_MassRb.position += displacement;
    }

    private void OnDrawGizmos() {
        if (!m_ShowNormalGizmo) 
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Position, (Vector3)Position + (DebugNormalForce));
    }
}
