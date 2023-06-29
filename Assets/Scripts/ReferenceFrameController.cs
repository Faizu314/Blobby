using System.Collections.Generic;
using UnityEngine;

public class ReferenceFrameController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_Controller;
    [SerializeField] private List<Rigidbody2D> m_ReferencePoints;
    [SerializeField] private List<SpringJoint2D> m_Joints;
    [SerializeField] private AnimationCurve m_FrequencyByDistance;

    private Rigidbody2D m_Rb;
    public Rigidbody2D Rb => m_Rb;

    private void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        m_Rb.rotation = m_Controller.PointMassesDeviation;
        UpdateJointFrequencies();
    }

    private void UpdateJointFrequencies() {
        for (int i = 0; i < m_ReferencePoints.Count; i++) {
            float distance = Vector2.Distance(m_ReferencePoints[i].position, m_Controller.PointPosition(i));
            m_Joints[i].frequency = GetFrequencyByDistance(distance);
        }
    }

    private float GetFrequencyByDistance(float distance) {
        return m_FrequencyByDistance.Evaluate(distance);
    }
}
