using System.Collections.Generic;
using UnityEngine;

public class PointMassesController : MonoBehaviour {

    [SerializeField] private List<PointMass> m_PointMasses;
    [SerializeField] private List<BoxCollider2D> m_EdgeColliders;
    [SerializeField] private PointMass m_MidMass;
    [SerializeField] private Rigidbody2D m_FrameController;

    [Tooltip("Rate of change of point masses angle deviation calculation.")]
    [SerializeField] private float m_DevRateOfChange;
    [SerializeField] private float m_MaxRotationByTorque;

    [Tooltip("PA = nRT, PressureConstant = nRT.")]
    [SerializeField] private float m_PressureConstant;
    [Tooltip("P = AnimCurve(Area/BaseArea) * PressureConstant")]
    [SerializeField] private AnimationCurve m_PressureByArea;

    [Header("Spring Joints")][Space(5)]
    [SerializeField] public List<SpringJoint2D> m_MassesToMasses;
    [SerializeField] public List<SpringJoint2D> m_MassesToEdges;
    [SerializeField] public List<SpringJoint2D> m_EdgesToMasses;
    [SerializeField] public List<SpringJoint2D> m_FrameToMasses;

    [Header("Point Masses Joint Settings")][Space(5)]
    [SerializeField] private float m_MassesDamping;
    [SerializeField] private float m_MassesFrequency;

    [Header("Point Masses to Edges Joint Settings")][Space(5)]
    [SerializeField] private float m_EdgesDamping;
    [SerializeField] private float m_EdgesFrequency;

    [Header("Reference Points to Point Masses Joint Settings")]
    [SerializeField] private float m_FrameDamping;
    [SerializeField] private float m_FrameFrequency;

    [Header("RigidBody settings")][Space(5)]
    [SerializeField] private float m_MassesLinearDrag;
    [SerializeField] private float m_MassesGravityScale;

    public float Scale = 1f;

    [Header("Debug")][Space(5)]
    [SerializeField] private bool m_UpdateSettingsEveryFrame;


    public float PointMassesDeviation = 0f;
    private float m_PointMassesDeviation = 0f;
    private float m_PrevPointMassesDeviation = 0f;

    public Vector2 AvgPosition { 
        get {
            var pos = Vector2.zero;
            foreach (var point in m_PointMasses)
                pos += point.Position;

            return pos / m_PointMasses.Count;
        } 
    }

    public Vector2 Position => m_MidMass.Position;

    public Vector3 PointPosition(int i) {
        return m_PointMasses[i].Position;
    }

    private Rigidbody2D[] m_Rigidbodies;

    private List<float> m_ReferenceBaseAngles;
    private List<float> m_BaseJointDist;
    private List<float> m_BaseColLength;
    private List<Vector2> m_BaseEdgeAnchors;
    private List<float> m_ReferenceRadii;
    private float m_ReferenceArea;

    private void Start() {
        m_Rigidbodies = GetComponentsInChildren<Rigidbody2D>();

        m_BaseJointDist = new();
        foreach (var joint in m_MassesToMasses) {
            joint.enableCollision = false;
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
            joint.autoConfigureDistance = false;
            m_BaseJointDist.Add(joint.distance);
        }
        foreach (var joint in m_MassesToEdges) {
            joint.enableCollision = false;
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
            joint.autoConfigureDistance = false;
        }

        m_BaseColLength = new();
        foreach (var col in m_EdgeColliders)
            m_BaseColLength.Add(col.size.x);
        m_BaseEdgeAnchors = new();
        foreach (var joint in m_EdgesToMasses)
            m_BaseEdgeAnchors.Add(joint.anchor);

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
        foreach (var joint in m_MassesToMasses) {
            joint.frequency = m_MassesFrequency;
            joint.dampingRatio = m_MassesDamping;
        }
        foreach (var joint in m_MassesToEdges) {
            joint.frequency = m_EdgesFrequency;
            joint.dampingRatio = m_EdgesDamping;
        }
        foreach (var joint in m_FrameToMasses) {
            joint.frequency = m_FrameFrequency * Scale;
            joint.dampingRatio = m_FrameDamping;
        }

        foreach (var comp in m_Rigidbodies) {
            comp.drag = m_MassesLinearDrag;
            comp.gravityScale = m_MassesGravityScale;
        }
    }

    private void Update() {
        if (m_UpdateSettingsEveryFrame)
            ApplyConfiguration();

        Inflate();
    }

    private void Inflate() {
        m_FrameController.transform.localScale = Vector3.one * Scale;

        for (int i = 0; i < m_MassesToMasses.Count; i++)
            m_MassesToMasses[i].distance = m_BaseJointDist[i] * Scale;

        for (int i = 0; i < m_EdgeColliders.Count; i++)
            m_EdgeColliders[i].size = new(m_BaseColLength[i] * Scale, m_EdgeColliders[i].size.y);

        for (int i = 0; i < m_EdgesToMasses.Count; i++)
            m_EdgesToMasses[i].anchor = m_BaseEdgeAnchors[i] * Scale;

        foreach (var joint in m_FrameToMasses)
            joint.frequency = m_FrameFrequency * Scale;
    }

    private void FixedUpdate() {
        m_FrameController.rotation = PointMassesDeviation;
        ApplyNormalPressure();
    }

    public void ApplyTorque(float torque) {
        m_FrameController.transform.Rotate(Vector3.back * torque * Time.deltaTime);

        //if (Mathf.Abs((PointMassesDeviation - m_PrevPointMassesDeviation) / Time.fixedDeltaTime) > m_MaxRotationByTorque)
        //    return;

        //float distSum = 0f;
        //for (int i = 0; i < m_PointMasses.Count; i++)
        //    distSum += Vector3.Distance(m_PointMasses[i].Position, m_MidMass.Position);

        //float torquePerMass = -torque / distSum;

        //for (int i = 0; i < m_PointMasses.Count; i++) {
        //    Vector2 normal = (m_PointMasses[i].Position - m_MidMass.Position).normalized;
        //    Vector2 force = Vector3.Cross(normal, Vector3.back);
        //    force = (force * torquePerMass) + (normal * Mathf.Abs(torquePerMass));
        //    m_PointMasses[i].Rb.AddForce(force);
        //}
    }

    public void SetCollisionWith(Collider2D collider, bool shouldCollide) {
        foreach (var col in m_EdgeColliders)
            Physics2D.IgnoreCollision(col, collider, !shouldCollide);
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

        m_PrevPointMassesDeviation = PointMassesDeviation;
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
