using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    [SerializeField] private LayerMask m_MetalBallsLayer;
    [SerializeField] private Vector2 m_AttractionForce;

    private List<IEatable> m_Magnetics = new();

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_MetalBallsLayer, collision.gameObject.layer))
            return;

        var magnetic = collision.GetComponent<IEatable>();

        if (magnetic != null)
            m_Magnetics.Add(magnetic);
    }

    private void OnTriggerStay2D(Collider2D collision) {
        foreach (var magnetic in m_Magnetics)
            magnetic.ApplyAttractionForce(m_AttractionForce);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!Util.IsInLayerMask(m_MetalBallsLayer, collision.gameObject.layer))
            return;

        var magnetic = collision.GetComponent<IEatable>();

        if (magnetic != null)
            m_Magnetics.Remove(magnetic);
    }

}
