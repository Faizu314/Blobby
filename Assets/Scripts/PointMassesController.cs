using System.Collections.Generic;
using UnityEngine;

public class PointMassesController : MonoBehaviour {

    [SerializeField] private List<PointMass> m_PointMasses;
    [SerializeField] private PointMass m_MidMass;
    [SerializeField] private ReferenceFrameController m_FrameController;

    [Tooltip("Rate of change of point masses angle deviation calculation.")]
    [SerializeField] private float m_DevRateOfChange;
    [SerializeField] private float m_MaxRotationByTorque;

    [Tooltip("PA = nRT, PressureConstant = nRT.")]
    [SerializeField] private float m_PressureConstant;
    [Tooltip("P = AnimCurve(Area/BaseArea) * PressureConstant")]
    [SerializeField] private AnimationCurve m_PressureByArea;

    [SerializeField] public List<SpringJoint2D> m_BetweenPointMasses;
    [SerializeField] public List<SpringJoint2D> m_MassesToEdges;

    [Header("Point Masses Joint settings")][Space(5)]
    [SerializeField] private float m_MassesDamping;
    [SerializeField] private float m_MassesFrequency;

    [Header("Point Masses to Edges Joint settings")][Space(5)]
    [SerializeField] private float m_EdgesDamping;
    [SerializeField] private float m_EdgesFrequency;

    [Header("RigidBody settings")][Space(5)]
    [SerializeField] private float m_MassesLinearDrag;
    [SerializeField] private float m_MassesGravityScale;

    [Header("Debug")][Space(5)]
    [SerializeField] private bool m_UpdateSettingsEveryFrame;


    public float PointMassesDeviation = 0f;
    private float m_PointMassesDeviation = 0f;
    private float m_PrevPointMassesDeviation = 0f;

    public Vector3 PointPosition(int i) {
        //Vector2 position = Vector2.zero;

        //for (int i = 0; i < m_PointMasses.Count; i++)
        //    position += m_PointMasses[i].Position;

        //return position / m_PointMasses.Count;

        return m_PointMasses[i].Position;
    }

    private Rigidbody2D[] m_Rigidbodies;

    private List<float> m_ReferenceBaseAngles;
    private List<float> m_ReferenceRadii;
    private float m_ReferenceArea;

    private void Start() {
        m_Rigidbodies = GetComponentsInChildren<Rigidbody2D>();

        foreach (var joint in m_BetweenPointMasses) {
            joint.enableCollision = false;
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
            joint.autoConfigureDistance = false;
        }
        foreach (var joint in m_MassesToEdges) {
            joint.enableCollision = false;
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
            joint.autoConfigureDistance = false;
        }

        foreach (var comp in m_Rigidbodies) {
            comp.drag = m_MassesLinearDrag;
            comp.gravityScale = m_MassesGravityScale;
        }

        m_ReferenceArea = GetCurrArea(); //point masses must start as the original shape.

        InitBaseReferences();
    }

    private void InitBaseReferences() {
        m_ReferenceBaseAngles = new();
        m_ReferenceRadii = new();

        for (int i = 0; i < m_PointMasses.Count; i++) {
            var pos = m_PointMasses[i].Position - m_MidMass.Position;
            var angle = Util.AngleFromVector(pos);
            m_ReferenceBaseAngles.Add(angle);
        }

        for (int i = 0; i < m_PointMasses.Count; i++)
            m_ReferenceRadii.Add((m_PointMasses[i].Position - m_MidMass.Position).magnitude);
    }

    private void ApplyConfiguration() {
        foreach (var joint in m_BetweenPointMasses) {
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
        }
        foreach (var joint in m_MassesToEdges) {
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
        }

        foreach (var comp in m_Rigidbodies) {
            comp.drag = m_MassesLinearDrag;
            comp.gravityScale = m_MassesGravityScale;
        }
    }

    private void Update() {
        if (!m_UpdateSettingsEveryFrame)
            return;

        ApplyConfiguration();
    }

    private void FixedUpdate() {
        ApplyNormalPressure();
    }

    public void ApplyTorque(float torque) {
        if (Mathf.Abs((m_PointMassesDeviation - m_PrevPointMassesDeviation) / Time.fixedDeltaTime) > m_MaxRotationByTorque)
            return;

        float torquePerMass = -torque / m_PointMasses.Count;

        for (int i = 0; i < m_PointMasses.Count; i++) {
            Vector2 force = Vector3.Cross(m_PointMasses[i].Position - m_MidMass.Position, Vector3.back);
            m_PointMasses[i].Rb.AddForce(force.normalized * torquePerMass / force.magnitude);
        }
    }
    
    private float GetCurrArea() {
        float area = 0f;

        for (int i = 0; i < m_PointMasses.Count; i++) {
            Vector2 a;
            Vector2 b = m_PointMasses[i].Position;

            if (i == m_PointMasses.Count - 1)
                a = m_PointMasses[0].Position;
            else
                a = m_PointMasses[i + 1].Position;

            area += (a.x - b.x) * (a.y + b.y) / 2;
        }

        return area;
    }

    private void ApplyNormalPressure() {
        float area = GetCurrArea() / m_ReferenceArea;
        if (Mathf.Abs(area) < 0.01f)
            area = 0.01f * Mathf.Sign(area);
        float pressure = m_PressureConstant * m_PressureByArea.Evaluate(area);

        m_PrevPointMassesDeviation = m_PointMassesDeviation;
        m_PointMassesDeviation = GetPointMassesDeviation();
        PointMassesDeviation = Mathf.LerpAngle(PointMassesDeviation, m_PointMassesDeviation, Time.fixedDeltaTime * m_DevRateOfChange);
        Vector2 normalForce;
        for (int i = 0; i < m_PointMasses.Count; i++) {
            float angle = m_ReferenceBaseAngles[i] + PointMassesDeviation;
            normalForce = Util.VectorFromAngle(angle);

            m_PointMasses[i].ApplyNormalForce(normalForce * pressure);
        }
    }

    private float GetPointMassesDeviation() {
        Vector2 sumDeltaAngleAsVec = Vector2.zero;

        for (int i = 0; i < m_ReferenceBaseAngles.Count; i++) {
            var angle = GetPointMassDeviation(i);
            sumDeltaAngleAsVec += Util.VectorFromAngle(angle);
        }

        return Util.AngleFromVector(sumDeltaAngleAsVec);
    }

    private float GetPointMassDeviation(int index) {
        var pos = m_PointMasses[index].Position - m_MidMass.Position;
        float angle = Util.AngleFromVector(pos);
        return Mathf.DeltaAngle(m_ReferenceBaseAngles[index], angle);
    }
}
