using System.Collections.Generic;
using UnityEngine;

public class PointMass : MonoBehaviour {

    [SerializeField] private bool m_ShowNormalGizmo;
    [SerializeField] private bool m_ShowGroundedGizmo;

    public List<float> DEBUG = new();

    public Vector2 Position { 
        get {
            if (m_MassRb == null)
                return transform.position;
            return m_MassRb.position;
        }
    }

    private Rigidbody2D m_MassRb;
    public Rigidbody2D Rb => m_MassRb;
    public bool IsGrounded { get; set; }

    private Vector3 DebugNormalForce;
    private RigidbodyConstraints2D m_OriginalConstraints;
    private SpringJoint2D[] m_SpringJoints;
    private Vector2[] m_BaseDirToOther;
    private List<float> m_BaseFrequencies = new();
    private PointMassesController m_Controller;

    private void Awake()
    {
        m_MassRb = GetComponent<Rigidbody2D>();
        m_OriginalConstraints = m_MassRb.constraints;
        m_SpringJoints = GetComponents<SpringJoint2D>();
        m_BaseDirToOther = new Vector2[m_SpringJoints.Length];
        m_Controller = GetComponentInParent<PointMassesController>();
    }

    private void Start() {
        
        for (int i = 0; i < m_BaseDirToOther.Length; i++) {
            m_BaseDirToOther[i] = (m_SpringJoints[i].connectedBody.transform.localPosition - transform.localPosition).normalized;
            DEBUG.Add(0f);
        }
    }

    private void FixedUpdate() {
        Vector2 temp;
        float dot;
        Quaternion playerRot = Quaternion.Euler(0f, 0f, -m_Controller.PointMassesDeviation);

        for (int i = 0; i < m_SpringJoints.Length; i++) {
            temp = (m_SpringJoints[i].connectedBody.transform.localPosition - transform.localPosition).normalized;
            temp = playerRot * temp;
            dot = Vector2.Dot(temp, m_BaseDirToOther[i]);

            if (dot < 0f) {
                m_SpringJoints[i].frequency = 0.1f;
            }
            else {
                m_SpringJoints[i].frequency = m_Controller.MassesFrequency;
            }

            DEBUG[i] = dot;
        }
    }

    public void ApplyNormalForce(Vector2 force) {
        DebugNormalForce = force;
        m_MassRb.AddForce(force);
    }
    public void SetFreeze(bool freeze) {
        if (freeze)
            m_MassRb.constraints = RigidbodyConstraints2D.FreezeAll;
        else
            m_MassRb.constraints = m_OriginalConstraints;
    }

    private void OnDrawGizmos() {
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
