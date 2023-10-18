using System;
using System.Collections.Generic;
using UnityEngine;

public class PointMassesController : MonoBehaviour {

    [Serializable]
    public struct SoftBodyConfig {
        [Tooltip("PA = nRT, PressureConstant = nRT.")]
        public float m_PressureConstant;
        [Tooltip("P = AnimCurve(Area/BaseArea) * PressureConstant")]
        public AnimationCurve m_PressureByArea;

        public float m_MassesDamping;
        public float m_MassesFrequency;

        public float m_EdgesDamping;
        public float m_EdgesFrequency;

        public float m_MassesLinearDrag;
        public float m_MassesGravityScale;
    }

    [SerializeField] private List<PointMass> m_PointMasses;
    [SerializeField] private List<BoxCollider2D> m_EdgeColliders;
    [SerializeField] private PointMass m_MidMass;

    [Tooltip("Rate of change of point masses angle deviation calculation.")]
    [SerializeField] private float m_DevRateOfChange;
    [SerializeField] private float m_MaxRotationByTorque;

    [Header("Spring Joints")][Space(5)]
    [SerializeField] public List<SpringJoint2D> m_MassesToMasses;
    [SerializeField] public List<SpringJoint2D> m_EdgesToMasses;

    [SerializeField] private SoftBodyConfig m_Config;

    [Space(10)]
    public float Scale = 1f;

    [Header("Debug")][Space(5)]
    [SerializeField] private bool m_UpdateSettingsEveryFrame;

    public Vector2 AvgPosition { 
        get {
            var pos = Vector2.zero;
            foreach (var point in m_PointMasses)
                pos += point.Position;

            return pos / m_PointMasses.Count;
        } 
    }

    public Vector2 AvgVelocity {
        get {
            var vel = Vector2.zero;
            foreach (var point in m_PointMasses)
                vel += point.Rb.velocity;

            return vel / m_PointMasses.Count;
        }
    }

    public Vector2 Position => m_MidMass.Position;

    public Vector3 GetPointPosition(int i) {
        return m_PointMasses[i].Position;
    }
    public Vector3 GetPointLocalPosition(int i) {
        return m_PointMasses[i].transform.localPosition;
    }
    public Vector3 MidMassPosition => m_MidMass.Position;
    public Vector3 MidMassLocalposition => m_MidMass.transform.localPosition;
    public int PointMassesCount => m_PointMasses.Count;
    public float MassesFrequency => m_Config.m_MassesFrequency;

    public int GroundedPointsCount { get; private set; }

    public Vector2 GroundNormal { get; private set; }

    public float PointMassesDeviation = 0f;
    private float m_PrevPointMassesDeviation = 0f;
    private float m_PointMassesDeviation = 0f;

    private Rigidbody2D[] m_Rigidbodies;
    private List<float> m_ReferenceBaseAngles;
    private List<float> m_BaseJointDist;
    private List<float> m_BaseColLength;
    private List<Vector2> m_BaseEdgeAnchors;
    private List<float> m_ReferenceRadii;
    private float m_ReferenceArea;

    public void Init() {
        m_Rigidbodies = GetComponentsInChildren<Rigidbody2D>();

        m_BaseJointDist = new();
        foreach (var joint in m_MassesToMasses) {
            joint.enableCollision = false;
            joint.autoConfigureDistance = false;
            m_BaseJointDist.Add(joint.distance);
        }

        m_BaseColLength = new();
        foreach (var col in m_EdgeColliders)
            m_BaseColLength.Add(col.size.x);
        m_BaseEdgeAnchors = new();
        foreach (var joint in m_EdgesToMasses)
            m_BaseEdgeAnchors.Add(joint.anchor);

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

    public void SetConfiguration(SoftBodyConfig config) {
        m_Config = config;
    }
    public void ApplyConfiguration() {
        foreach (var joint in m_MassesToMasses) {
            joint.frequency = m_Config.m_MassesFrequency;
            joint.dampingRatio = m_Config.m_MassesDamping;
        }
        foreach (var joint in m_EdgesToMasses) {
            joint.frequency = m_Config.m_EdgesFrequency;
            joint.dampingRatio = m_Config.m_EdgesDamping;
        }

        foreach (var comp in m_Rigidbodies) {
            comp.drag = m_Config.m_MassesLinearDrag;
            comp.gravityScale = m_Config.m_MassesGravityScale;
        }
    }

    public void UpdateTick() {
        if (m_UpdateSettingsEveryFrame)
            ApplyConfiguration();
        Inflate();
    }

    public void FixedUpdateTick() {
        ApplyNormalPressure();
        CheckIsGrounded();
        UpdatePointMassesDeviation();
    }

    public void ApplyTorque(float torque) {
        if (Mathf.Abs((m_PointMassesDeviation - m_PrevPointMassesDeviation) / Time.fixedDeltaTime) > m_MaxRotationByTorque)
            return;

        float distSum = 0f;
        for (int i = 0; i < m_PointMasses.Count; i++)
            distSum += Vector3.Distance(m_PointMasses[i].Position, m_MidMass.Position);
        //if (Mathf.Abs((PointMassesDeviation - m_PrevPointMassesDeviation) / Time.fixedDeltaTime) > m_MaxRotationByTorque)
        //    return;

        float torquePerMass = -torque / distSum;
        //float distSum = 0f;
        //for (int i = 0; i < m_PointMasses.Count; i++)
        //    distSum += Vector3.Distance(m_PointMasses[i].Position, m_MidMass.Position);

        for (int i = 0; i < m_PointMasses.Count; i++) {
            Vector2 normal = (m_PointMasses[i].Position - m_MidMass.Position).normalized;
            Vector2 force = Vector3.Cross(normal, Vector3.back);
            force = (force * torquePerMass) + (normal * Mathf.Abs(torquePerMass));
            m_PointMasses[i].Rb.AddForce(force);
        }
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
    public void ApplyForceOnMidMass(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
        m_MidMass.Rb.AddForce(force, mode);
    }
    public void ApplyForceOnAllMasses(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
        for (int i = 0; i < m_PointMasses.Count; i++)
            m_PointMasses[i].Rb.AddForce(force, mode);
        m_MidMass.Rb.AddForce(force, mode);
    }
    public List<PointMass> GetGroundedPointMasses() {
        List<PointMass> groundedMasses = new();

        for (int i = 0; i < m_PointMasses.Count; i++) {
            if (m_PointMasses[i].IsGrounded) {
                groundedMasses.Add(m_PointMasses[i]);
            }
        }

        return groundedMasses;
    }

    public void UnfreezePoints() {
        foreach (var point in m_PointMasses)
            point.SetFreeze(false);
    }

    private void CheckIsGrounded() {
        int groundedCount = 0;
        Vector2 groundedPointsAvgPos = Vector2.zero;

        for (int i = 0; i < m_PointMasses.Count; i++) {
            if (m_PointMasses[i].IsGrounded) {
                groundedCount++;
                groundedPointsAvgPos += m_PointMasses[i].Position;
            }
        }
        groundedPointsAvgPos /= groundedCount;

        GroundedPointsCount = groundedCount;
        GroundNormal = (m_MidMass.Position - groundedPointsAvgPos).normalized;
    }

    private void Inflate() {
        for (int i = 0; i < m_MassesToMasses.Count; i++)
            m_MassesToMasses[i].distance = m_BaseJointDist[i] * Scale;

        for (int i = 0; i < m_EdgeColliders.Count; i++)
            m_EdgeColliders[i].size = new(m_BaseColLength[i] * Scale, m_EdgeColliders[i].size.y);

        for (int i = 0; i < m_EdgesToMasses.Count; i++)
            m_EdgesToMasses[i].anchor = m_BaseEdgeAnchors[i] * Scale;
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
        float area = GetCurrArea() / (m_ReferenceArea * Scale * Scale);
        if (Mathf.Abs(area) < 0.01f)
            area = 0.01f * Mathf.Sign(area);
        float pressure = m_Config.m_PressureConstant * m_Config.m_PressureByArea.Evaluate(area);

        Vector2 normalForce;
        for (int i = 0; i < m_PointMasses.Count; i++) {
            float angle = m_ReferenceBaseAngles[i] + PointMassesDeviation;
            normalForce = Util.VectorFromAngle(angle);

            m_PointMasses[i].ApplyNormalForce(normalForce * pressure);
        }
    }

    private void UpdatePointMassesDeviation() {
        m_PrevPointMassesDeviation = m_PointMassesDeviation;
        m_PointMassesDeviation = GetPointMassesDeviation();
        PointMassesDeviation = Mathf.LerpAngle(PointMassesDeviation, m_PointMassesDeviation, Time.deltaTime * m_DevRateOfChange);
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
