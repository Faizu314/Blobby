using System.Collections.Generic;
using UnityEngine;

public class PointEdge : MonoBehaviour {

    [SerializeField] private PointMass m_MassA;
    [SerializeField] private PointMass m_MassB;

    private LayerMask m_GroundedLayerMask => GameManager.Instance.GameConfiguration.GroundLayer;
    private List<ContactPoint2D> m_Contacts = new();
    public bool IsGrounded { get; private set; }

    private void OnCollisionStay2D(Collision2D collision) {
        if (!Util.IsInLayerMask(m_GroundedLayerMask, collision.gameObject.layer))
            return;
        if (!m_MassA.IsGrounded && !m_MassB.IsGrounded) {
            collision.GetContacts(m_Contacts);
            foreach (var contact in m_Contacts) {
                float distToA = Vector2.SqrMagnitude(contact.point - m_MassA.Position);
                float distToB = Vector2.SqrMagnitude(contact.point - m_MassB.Position);

                if (distToA < distToB)
                    m_MassA.IsGrounded = true;
                else
                    m_MassB.IsGrounded = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (!Util.IsInLayerMask(m_GroundedLayerMask, collision.gameObject.layer))
            return;
        m_MassA.IsGrounded = false;
        m_MassB.IsGrounded = false;
    }
}
