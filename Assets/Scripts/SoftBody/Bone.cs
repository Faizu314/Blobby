using UnityEngine;
using Phezu.Util;

public class Bone : MonoBehaviour
{
    [SerializeField] private PointMass m_MassToFollow;
    [SerializeField] private Transform m_MidMass;
    [SerializeField] private float m_FollowLife;
    [SerializeField] private float m_FollowRatio;
    [Tooltip("[Mass to follow] to [mid mass] vector angle.")]
    [SerializeField] private float m_BoneBaseRot;
    [SerializeField] private bool m_RandomForce;
    [SerializeField] private int m_Seed;
    [SerializeField] private float m_RandomForceStrength;
    [SerializeField] private float m_RandomForceFrequency;

    private Transform m_Transform;
    private float m_LerpK;

    private void Start() {
        m_Transform = transform;
        m_BoneBaseRot = Util.AngleFromVector(m_MidMass.position - m_MassToFollow.transform.position);
        m_LerpK = FMath.GetFreeLerpK(m_FollowLife, m_FollowRatio);
    }

    private void Update() {
        Vector3 displacement = Vector2.zero;
        if (m_RandomForce && !m_MassToFollow.IsGrounded)
            displacement = Util.RandomContinuousVector(Time.time * m_RandomForceFrequency, m_Seed) * m_RandomForceStrength;

        float lerpT = FMath.GetFreeLerpT(m_LerpK, Time.deltaTime);
        
        m_Transform.position = Vector3.Lerp(m_Transform.position, m_MassToFollow.transform.position, lerpT) + displacement;

        var angle = Util.AngleFromVector(m_MidMass.position - m_MassToFollow.transform.position);
        SetZAngle(Mathf.DeltaAngle(m_BoneBaseRot, angle));
    }



    private void SetZAngle(float angle) {
        var rot = m_Transform.localEulerAngles;
        rot.z = angle;
        m_Transform.localEulerAngles = rot;
    }
}
