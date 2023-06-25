using System.Collections.Generic;
using UnityEngine;

public class PointMassesController : MonoBehaviour
{
    [SerializeField] private List<PointMass> m_PointMasses;
    [SerializeField] private Transform m_MidMass;

    [Tooltip("Rate of change of point masses angle deviation calculation.")]
    [SerializeField] private float m_DevRateOfChange;

    [Tooltip("PA = nRT, PressureConstant = nRT.")]
    [SerializeField] private float m_PressureConstant;

    [Tooltip("Strength of reference frame force as point mass moves away from it.")]
    [SerializeField] private AnimationCurve m_FrameForceByDistSqr;

    [SerializeField] private List<SpringJoint2D> m_BetweenPointMasses;
    [SerializeField] private List<SpringJoint2D> m_MassesToEdges;

    [Header("Point Masses Joint settings")] [Space(5)]
    [SerializeField] private float m_MassesDamping;
    [SerializeField] private float m_MassesFrequency;

    [Header("Point Masses to Edges Joint settings")] [Space(5)]
    [SerializeField] private float m_EdgesDamping;
    [SerializeField] private float m_EdgesFrequency;

    [Header("RigidBody settings")] [Space(5)]
    [SerializeField] private float m_MassesLinearDrag;
    [SerializeField] private bool m_UpdateSettingsEveryFrame;

    [SerializeField] private bool m_ShowRPoints;

    public float PointMassesDeviation = 0f;
    private float m_PointMassesDeviation = 0f;

    public Vector3 Position { 
        get {
            Vector3 position = Vector3.zero;

            for (int i = 0; i < m_PointMasses.Count; i++)
                position += m_PointMasses[i].Position;

            return position / m_PointMasses.Count;
        } 
    }

    private Rigidbody2D[] m_Rigidbodies;

    private List<float> m_ReferenceBaseAngles;
    private List<float> m_ReferenceRadii;
    private float m_ReferenceArea;
    private List<Vector3> m_DebugRPoints = new();

    private void Start() {
        m_Rigidbodies = GetComponentsInChildren<Rigidbody2D>();

        foreach (var joint in m_BetweenPointMasses) {
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
        }
        foreach (var joint in m_MassesToEdges) {
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
        }

        foreach (var comp in m_Rigidbodies)
            comp.drag = m_MassesLinearDrag;

        m_ReferenceArea = GetCurrArea(); //point masses must start as the original shape.

        InitBaseReferences();
    }

    private void InitBaseReferences() {
        m_ReferenceBaseAngles = new();
        m_ReferenceRadii = new();

        for (int i = 0; i < m_PointMasses.Count; i++) {
            var pos = m_PointMasses[i].Position - m_MidMass.position;
            var angle = Util.AngleFromVector(pos);
            m_ReferenceBaseAngles.Add(angle);


            m_DebugRPoints.Add(Vector3.zero);
        }

        for (int i = 0; i < m_PointMasses.Count; i++) {
            m_ReferenceRadii.Add((m_PointMasses[i].Position - m_MidMass.position).magnitude);
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

    private void Update()
    {
        if (!m_UpdateSettingsEveryFrame)
            return;

        foreach (var joint in m_BetweenPointMasses) {
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
        }
        foreach (var joint in m_MassesToEdges) {
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
        }

        foreach (var comp in m_Rigidbodies)
            comp.drag = m_MassesLinearDrag;
    }

    private void FixedUpdate() {
        ApplyNormalPressure();
        ApplyReferenceForces();
    }

    private void ApplyNormalPressure() {
        float area = GetCurrArea() / m_ReferenceArea;
        float pressure = m_PressureConstant / area;

        m_PointMassesDeviation = GetPointMassesDeviation();
        PointMassesDeviation = Mathf.LerpAngle(PointMassesDeviation, m_PointMassesDeviation, Time.fixedDeltaTime * m_DevRateOfChange);
        Vector2 normalForce;
        for (int i = 0; i < m_PointMasses.Count; i++) {
            float angle = m_ReferenceBaseAngles[i] + PointMassesDeviation;
            normalForce = Util.VectorFromAngle(angle);

            m_PointMasses[i].ApplyNormalForce(normalForce * pressure);
        }
    }

    private void ApplyReferenceForces() {
        for (int i = 0; i < m_PointMasses.Count; i++) {
            var pos = GetReferencePointPos(i);
            m_DebugRPoints[i] = pos;
            var massToRef = pos - m_PointMasses[i].Position;
            float forceStrength = m_FrameForceByDistSqr.Evaluate(massToRef.sqrMagnitude);
            m_PointMasses[i].ApplyReferenceFrameForce(massToRef.normalized * forceStrength);
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
        var pos = m_PointMasses[index].Position - m_MidMass.position;
        float angle = Util.AngleFromVector(pos);
        return Mathf.DeltaAngle(m_ReferenceBaseAngles[index], angle);
    }

    private Vector3 GetReferencePointPos(int index) {
        Vector3 disp = Util.VectorFromAngle(PointMassesDeviation + m_ReferenceBaseAngles[index]);
        return m_MidMass.position - (disp * m_ReferenceRadii[index]);
    }


    private void OnDrawGizmos() {
        
    }

}
