using System.Collections;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private int m_Health;
    [SerializeField] private float m_ImpactVelocityThreshold;
    [SerializeField] private GameObject m_BreakableRoot;
    [SerializeField] private BreakableVisuals m_Visuals;

    private int m_CurrHealth;
    private WaitForSeconds m_AfterCollisionWait;
    private bool m_IsWaiting;

    private void Start() {
        m_CurrHealth = m_Health;
        m_AfterCollisionWait = new WaitForSeconds(1f);
        m_IsWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (m_IsWaiting)
            return;
        if (!Util.IsInLayerMask(GameManager.Instance.GameConfiguration.PlayerCollisionLayer, collision.gameObject.layer))
            return;
        if (!GameManager.Instance.PlayerController.State.CanBreak)
            return;

        var vel = collision.relativeVelocity;
        if (vel.magnitude >= m_ImpactVelocityThreshold) {
            m_CurrHealth--;
            m_Visuals.Damage();
        }

        if (m_CurrHealth <= 0f)
            StartCoroutine(nameof(Break));

        StartCoroutine(nameof(WaitAfterFirstCollision));
    }

    private IEnumerator WaitAfterFirstCollision() {
        m_IsWaiting = true;

        yield return m_AfterCollisionWait;

        m_IsWaiting = false;
    }

    private IEnumerator Break() {
        yield return m_AfterCollisionWait;
        Destroy(m_BreakableRoot);
    }
}
