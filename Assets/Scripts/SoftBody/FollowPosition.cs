using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    [SerializeField] private Transform m_MassToFollow;
    [SerializeField] private Transform m_MidMass;
    [SerializeField] private float m_FollowSpeed;
    [Tooltip("[Mass to follow] to [mid mass] vector angle.")]
    [SerializeField] private float m_BoneBaseRot;

    private Transform m_Transform;

    private void Start() {
        m_Transform = transform;
        m_BoneBaseRot = Util.AngleFromVector(m_MidMass.position - m_MassToFollow.position);
    }

    private void Update() {
        m_Transform.position = Vector3.Lerp(m_Transform.position, m_MassToFollow.position, Time.deltaTime * m_FollowSpeed);

        var angle = Util.AngleFromVector(m_MidMass.position - m_MassToFollow.position);
        SetZAngle(Mathf.DeltaAngle(m_BoneBaseRot, angle));
    }

    private void SetZAngle(float angle) {
        var rot = m_Transform.localEulerAngles;
        rot.z = angle;
        m_Transform.localEulerAngles = rot;
    }
}
