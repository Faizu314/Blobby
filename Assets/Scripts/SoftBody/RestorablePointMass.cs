using UnityEngine;

public class RestorablePointMass : PointMass {

    private SpringJoint2D[] m_SpringJoints;
    private Vector2[] m_BaseDirToOther;
    private PointMassesController m_Controller;

    protected override void Awake() {
        base.Awake();

        m_SpringJoints = GetComponents<SpringJoint2D>();
        m_BaseDirToOther = new Vector2[m_SpringJoints.Length];
        m_Controller = GetComponentInParent<PointMassesController>();
    }
    private void Start() {

        for (int i = 0; i < m_BaseDirToOther.Length; i++) {
            m_BaseDirToOther[i] = (m_SpringJoints[i].connectedBody.transform.localPosition - transform.localPosition).normalized;
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
        }
    }
}
