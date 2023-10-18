using UnityEngine;

public class Shaft : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private Transform m_StartPoint;
    [SerializeField] private Transform m_EndPoint;
    [SerializeField] private bool m_IsHorizontal;

    private void Start() {

    }

    private void FixedUpdate() {
        if (m_IsHorizontal) {
            if (m_Rigidbody.position.x < m_StartPoint.position.x) {
                m_Rigidbody.velocity = Vector2.zero;
                m_Rigidbody.position = m_StartPoint.position;
            }
            else if (m_Rigidbody.position.x > m_EndPoint.position.x) {
                m_Rigidbody.velocity = Vector2.zero;
                m_Rigidbody.position = m_EndPoint.position;
            }
        }
        else {
            if (m_Rigidbody.position.y < m_StartPoint.position.y) {
                m_Rigidbody.velocity = Vector2.zero;
                m_Rigidbody.position = m_StartPoint.position;
            }
            else if (m_Rigidbody.position.y > m_EndPoint.position.y) {
                m_Rigidbody.velocity = Vector2.zero;
                m_Rigidbody.position = m_EndPoint.position;
            }
        }
    }
}
