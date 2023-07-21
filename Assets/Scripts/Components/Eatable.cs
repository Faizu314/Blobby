using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eatable : MonoBehaviour
{
    [SerializeField] private LayerMask m_PointMassesLayer;
    [SerializeField] private float m_MassesDragOnEnter;
    [SerializeField] private List<Transform> m_ColTestPoints;
    [SerializeField] private Collider2D m_NonTriggerCollider;

    public List<bool> Debug;

    private Dictionary<Rigidbody2D, float> m_EnteredRbs = new();
    private Rigidbody2D m_Rb;
    private bool m_IsInside;
    private bool m_WaitingForRelease = false;

    private void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();

        Debug = new();
        for (int i = 0; i < m_ColTestPoints.Count; i++)
            Debug.Add(false);
    }

    private void Start() {
        InputManager.Instance.OnThrowUpButtonPressed += Release;

        GameManager.Instance.PlayerController.SetCollisionWith(m_NonTriggerCollider, false);
    }

    private void OnDestroy() {
        InputManager.Instance.OnThrowUpButtonPressed -= Release;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (m_IsInside)
            return;

        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        if (m_EnteredRbs.ContainsKey(collision.attachedRigidbody))
            return;

        m_EnteredRbs.Add(collision.attachedRigidbody, collision.attachedRigidbody.drag);

        collision.attachedRigidbody.drag = m_MassesDragOnEnter;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (m_IsInside)
            return;

        if (!Util.IsInLayerMask(m_PointMassesLayer, collision.gameObject.layer))
            return;

        if (!m_EnteredRbs.TryGetValue(collision.attachedRigidbody, out float value))
            return;

        collision.attachedRigidbody.drag = value;

        m_EnteredRbs.Remove(collision.attachedRigidbody);
    }

    private void FixedUpdate() {
        if (!m_IsInside && !m_WaitingForRelease)
            CheckIfInsidePlayer();
    }

    private void CheckIfInsidePlayer() {
        m_IsInside = true;

        for (int i = 0; i < m_ColTestPoints.Count; i++) {
            Debug[i] = Util.HorizontalTest(m_ColTestPoints[i].position, (i) => GameManager.Instance.PlayerController.GetPointPos(i), 12);
            m_IsInside &= Debug[i];
        }

        if (m_IsInside)
            OnEaten();
    }

    private void OnEaten() {
        foreach (var pair in m_EnteredRbs)
            pair.Key.drag = pair.Value;

        m_EnteredRbs.Clear();

        m_Rb.gravityScale = 0f;

        GameManager.Instance.PlayerController.SetCollisionWith(m_NonTriggerCollider, true);
    }

    public void Release() {
        m_Rb.gravityScale = 1f;

        GameManager.Instance.PlayerController.SetCollisionWith(m_NonTriggerCollider, false);

        m_IsInside = false;

        StartCoroutine(nameof(WaitAfterRelease), 2f);
    }

    private IEnumerator WaitAfterRelease(float seconds) {
        m_WaitingForRelease = true;

        yield return new WaitForSeconds(seconds);

        m_WaitingForRelease = false;
    }
}
