using UnityEngine;

public class Pump : MonoBehaviour
{
    [SerializeField] private LayerMask m_EdgesLayer;
    [SerializeField] private float m_TargetInflation;

    private PlayerController m_Player => GameManager.Instance.PlayerController;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_EdgesLayer, collision.gameObject.layer))
            return;

        m_Player.InflateTo(m_TargetInflation);
    }
}
