using System.Collections.Generic;
using UnityEngine;

public class Liquid : MonoBehaviour
{
    [SerializeField] private float m_DragOnEnter;
    [SerializeField] private Vector2 m_BuoyancyForce;

    private LayerMask m_PointMassesLayer => GameManager.Instance.GameConfiguration.PlayerPointsLayer;
    private Dictionary<Rigidbody2D, float> m_EnteredRbs = new();

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        if (m_EnteredRbs.ContainsKey(collision.attachedRigidbody))
            return;

        m_EnteredRbs.Add(collision.attachedRigidbody, collision.attachedRigidbody.drag);

        collision.attachedRigidbody.drag = m_DragOnEnter;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        collision.attachedRigidbody.AddForce(m_BuoyancyForce, ForceMode2D.Force);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.attachedRigidbody == null)
            return;
        if (!m_EnteredRbs.TryGetValue(collision.attachedRigidbody, out float value))
            return;

        collision.attachedRigidbody.drag = value;

        m_EnteredRbs.Remove(collision.attachedRigidbody);
    }
}
