using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Patroller : MonoBehaviour
{
    [SerializeField] private List<Transform> m_Checkpoints;
    [SerializeField] private float m_MaxVelocity;
    [SerializeField] private float m_LerpTime;
    [SerializeField] private float m_WaitTime;

    private Rigidbody2D m_Rb;
    private Vector2 m_Velocity;
    private int m_CurrCheckpoint = 0;

    private void Start() {
        m_Rb = GetComponent<Rigidbody2D>();
        StartCoroutine(nameof(GoToNextCheckpoint));
    }

    private IEnumerator GoToNextCheckpoint() {
        Vector2 targetPos = m_Checkpoints[m_CurrCheckpoint].position;

        while (Vector2.SqrMagnitude((Vector2)transform.position - targetPos) > 0.001f) {
            yield return new WaitForFixedUpdate();

            Vector2 currPos = m_Rb.position;

            Vector2.SmoothDamp(currPos, targetPos, ref m_Velocity, m_LerpTime, m_MaxVelocity);

            //m_Rb.MovePosition(currPos + (m_Velocity * Time.fixedDeltaTime));
        }

        yield return new WaitForSeconds(m_WaitTime);

        m_CurrCheckpoint = (m_CurrCheckpoint + 1) % m_Checkpoints.Count;

        StartCoroutine(nameof(GoToNextCheckpoint));
    }
}
