using UnityEngine;
using DG.Tweening;

public class BreakableVisuals : MonoBehaviour
{
    [SerializeField] private float m_Duration;
    [SerializeField] private Vector3 m_RotStrength;
    [SerializeField] private Vector3 m_PosStrength;
    [SerializeField] private int m_Vibrato;

    public void Damage() {
        transform.DOShakeRotation(m_Duration, m_RotStrength, m_Vibrato);
        transform.DOShakePosition(m_Duration, m_PosStrength, m_Vibrato);
    }

    public void Break() {

    }
}