using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private BinarySource m_Output;
    [SerializeField] private Color m_OffColor;
    [SerializeField] private Color m_OnColor;

    private SpriteRenderer m_Renderer;
    private LayerMask m_PointMassesLayer => GameManager.Instance.GameConfiguration.PlayerPointsLayer;
    private int m_TouchingPointsCount = 0;

    private void Start() {
        m_Renderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        m_TouchingPointsCount++;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        m_TouchingPointsCount--;
    }

    private void FixedUpdate() {
        if (m_TouchingPointsCount > 0) {
            m_Output.SetState(true);
            m_Renderer.color = m_OnColor;
        }
        else {
            m_Output.SetState(false);
            m_Renderer.color = m_OffColor;
        }
    }

}
