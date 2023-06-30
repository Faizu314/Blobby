using System.Collections.Generic;
using UnityEngine;

public class ReferenceFrameController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_Controller;
    [SerializeField] private List<Rigidbody2D> m_ReferencePoints;
    [SerializeField] private List<SpringJoint2D> m_Joints;
    [SerializeField] private AnimationCurve m_FrequencyByDistance;
    [SerializeField] private float m_Multiplier;

    private Rigidbody2D m_Rb;
    public Rigidbody2D Rb => m_Rb;

    private List<float> m_CorrectionForce = new();

    private void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();
        for (int i = 0; i < m_ReferencePoints.Count; i++)
            m_CorrectionForce.Add(0f);
    }

    private void FixedUpdate() {
        m_Rb.rotation = m_Controller.PointMassesDeviation;
        UpdateJointFrequencies();
    }

    private void UpdateJointFrequencies() {
        for (int i = 0; i < m_ReferencePoints.Count; i++) {
            float distance = Vector2.Distance(m_ReferencePoints[i].position, m_Controller.PointPosition(i));
            if (distance > 0.5f)
                m_CorrectionForce[i] += Time.fixedDeltaTime;
            else if (m_CorrectionForce[i] > 0f && distance < 0.1f)
                m_CorrectionForce[i] = 0f;

            m_Joints[i].frequency = GetFrequencyByDistance(distance) + (m_CorrectionForce[i] * m_Multiplier);
        }
    }

    private float GetFrequencyByDistance(float distance) {
        return m_FrequencyByDistance.Evaluate(distance);
    }
}
