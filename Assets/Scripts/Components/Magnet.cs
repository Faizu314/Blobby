using UnityEngine;

public class Magnet : MonoBehaviour
{
    [SerializeField] private LayerMask m_MetalBallsLayer;
    [SerializeField] private Vector2 m_AttractionForce;

    private Rigidbody2D m_Rb;

    private void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_MetalBallsLayer, collision.gameObject.layer))
            return;

        collision.attachedRigidbody.AddForce(m_AttractionForce);
    }

}
