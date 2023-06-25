using System;
using System.Collections.Generic;
using UnityEngine;

public class SoftBody : MonoBehaviour
{
    public List<SoftBodyPart> m_Parts;
    public List<Joint> m_Joints;
    public SoftBodyConfig m_Config;

    private void Start() {
        foreach (var part in m_Parts)
            part.Init(m_Config);

        foreach (var joint in m_Joints) {
            joint.A.AddJoint(joint.B);
            joint.B.AddJoint(joint.A);
        }
    }

    private void FixedUpdate() {
        List<Vector2> forces = new();
        int N = m_Parts.Count;

        for (int i = 0; i < N; i++) {
            forces.Add(m_Parts[i].CalculateJointsForce(m_Config));
        }
        for (int i = 0; i < N; i++) {
            m_Parts[i].ApplyForce(forces[i]);
        }
    }
}

[Serializable]
public struct SoftBodyConfig {
    public float ForceStrength;
    public float MaxForce;
    public float PointMassRadius;
    public float GravityScale;
    public float LinearDrag;
}

[Serializable]
public struct Joint {
    public SoftBodyPart A;
    public SoftBodyPart B;
}