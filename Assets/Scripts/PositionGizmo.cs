using UnityEngine;

public class PositionGizmo : MonoBehaviour
{
    [SerializeField] private float m_Radius;
    [SerializeField] private Color m_Color;

    private void OnDrawGizmos() {
        Gizmos.color = m_Color;
        Gizmos.DrawSphere(transform.position, m_Radius);
    }
}
