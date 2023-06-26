using System;
using System.Collections.Generic;
using UnityEngine;

public class PointMassesController : MonoBehaviour
{
    [SerializeField] private List<PointMass> m_PointMasses;
    [SerializeField] private PointMass m_MidMass;

    [Tooltip("Rate of change of point masses angle deviation calculation.")]
    [SerializeField] private float m_DevRateOfChange;

    [Tooltip("PA = nRT, PressureConstant = nRT.")]
    [SerializeField] private float m_PressureConstant;

    [Tooltip("Strength of reference frame force as point mass moves away from it.")]
    [SerializeField] private AnimationCurve m_FrameForceByDistSqr;

    public List<SpringJoint2D> m_BetweenPointMasses;
    public List<SpringJoint2D> m_MassesToEdges;
    public List<WeightedPoint> m_WeightedPoints;

    [Header("Point Masses Joint settings")] [Space(5)]
    [SerializeField] private float m_MassesDamping;
    [SerializeField] private float m_MassesFrequency;

    [Header("Point Masses to Edges Joint settings")] [Space(5)]
    [SerializeField] private float m_EdgesDamping;
    [SerializeField] private float m_EdgesFrequency;

    [Header("RigidBody settings")] [Space(5)]
    [SerializeField] private float m_MassesLinearDrag;

    [Header("Debug")] [Space(5)]
    [SerializeField] private bool m_UpdateSettingsEveryFrame;
    [SerializeField] private bool m_ShowRPoints;
    public bool m_ShowWeightedPoints;

    public float PointMassesDeviation = 0f;
    private float m_PointMassesDeviation = 0f;

    public Vector3 Position { 
        get {
            Vector2 position = Vector2.zero;

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

        foreach (var comp in m_Rigidbodies)
            comp.drag = m_MassesLinearDrag;

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


            m_DebugRPoints.Add(Vector3.zero);
        }

        for (int i = 0; i < m_PointMasses.Count; i++)
            m_ReferenceRadii.Add((m_PointMasses[i].Position - m_MidMass.Position).magnitude);

        foreach (var point in m_WeightedPoints)
            point.Init();
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
        MoveWeightedPoints();
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

    private void MoveWeightedPoints() {
        foreach (var weightedPoint in m_WeightedPoints) {
            var weightedPointPos = weightedPoint.GetPosition();
            var toFollowPos = weightedPoint.ToFollow.position;

            weightedPoint.Move(toFollowPos - weightedPointPos);

            var force = toFollowPos - weightedPointPos;
            float dist = (toFollowPos - weightedPointPos).magnitude;
            force /= dist;

            weightedPoint.ToFollow.AddForce(force * weightedPoint.ForcePerDist.Evaluate(dist));
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

    public Vector2 GetReferencePointPos(int index) {
        Vector2 disp = Util.VectorFromAngle(PointMassesDeviation + m_ReferenceBaseAngles[index]);
        return m_MidMass.Position + (disp * m_ReferenceRadii[index]);
    }


    [Serializable]
    public class WeightedPoint {
        public List<WeightedPointMass> Points;
        public Rigidbody2D ToFollow;
        public AnimationCurve ForcePerDist;
        [SerializeField] private float m_WeightSum;

        public void Init() {
            if (Points == null)
                return;

            m_WeightSum = 0f;
            foreach (var point in Points)
                m_WeightSum += point.Weight;
        }


        public Vector2 GetPosition() {
            var pos = Vector2.zero;

            foreach (var point in Points)
                pos += point.Mass.Position * point.Weight;

            return pos / m_WeightSum;
        }

        public void Move(Vector2 displacement) {
            foreach (var point in Points) {
                point.Mass.TranslateRb(displacement * (point.Weight / m_WeightSum));
            }
        }
    }

    [Serializable]
    public class WeightedPointMass {
        public PointMass Mass;
        [Range(0f, 1f)] public float Weight;
    }


    private void OnDrawGizmos() {
        if (m_ShowRPoints) {
            for (int i = 0; i < m_DebugRPoints.Count; i++) {
                if (i == 0)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.grey;
                Gizmos.DrawSphere(m_DebugRPoints[i], 0.1f);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        if (Application.isPlaying)
            return;

        if (m_ShowWeightedPoints) {
            foreach (var point in m_WeightedPoints) {
                point.Init();
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(point.GetPosition(), 0.1f);
            }
        }
    }
}
