using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SoftBodyPart : MonoBehaviour
{
    public Vector3 Position { get => m_Rb.position; }

    private Transform m_Transform;
    private Rigidbody2D m_Rb;
    private Dictionary<SoftBodyPart, Vector2> m_ConnectedParts = new();

    private Vector2 m_DebugForce;
    private Vector2 m_DebugTarget;

    public void Init(SoftBodyConfig config) {        
        m_Rb = GetComponent<Rigidbody2D>();
        m_Transform = transform;
        m_Rb.gravityScale = config.GravityScale;
        m_Rb.drag = config.LinearDrag;
    }
    public void AddJoint(SoftBodyPart other) {
        m_ConnectedParts.Add(other, GetJointData(other));
    }

    private Vector2 GetJointData(SoftBodyPart other) {
        var offset = other.m_Transform.InverseTransformPoint(m_Transform.position);
        return offset;
    }

    public Vector2 CalculateJointsForce(SoftBodyConfig config) {
        Vector2 cumulativeForce = Vector2.zero;

        foreach (var keyValue in m_ConnectedParts)
            cumulativeForce += GetForceByJoint(keyValue.Key, keyValue.Value, config);

        return cumulativeForce;
    }
    public void ApplyForce(Vector2 force) {
        m_Rb.AddForce(force, ForceMode2D.Force);
        m_DebugForce = force;
    }

    private Vector2 GetForceByJoint(SoftBodyPart other, Vector2 jointOffset, SoftBodyConfig config) {
        Vector3 targetPos = other.m_Transform.TransformPoint(jointOffset);

        Vector2 force = targetPos - Position;
        float dist = (Position - other.Position).magnitude;
        float restDist = (targetPos - other.Position).magnitude;
        float forceMag;

        if (dist > restDist) {
            forceMag = config.ForceStrength * dist;
        }
        else {
            float t = 1f - Mathf.InverseLerp(config.PointMassRadius * 2f, restDist, dist);
            forceMag = Mathf.Lerp(0f, config.MaxForce, t);
        }

        return forceMag * force.normalized;
    }

}
