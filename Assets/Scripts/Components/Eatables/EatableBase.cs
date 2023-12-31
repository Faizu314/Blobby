using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableBase : MonoBehaviour, IEatable
{
    [SerializeField] private LayerMask m_PointMassesLayer;
    [SerializeField] private List<Transform> m_ColTestPoints;
    [SerializeField] private PlayerProfile m_PlayerProfile;
    [SerializeField] protected Collider2D m_Collider;

    private List<bool> m_IsColPointInside;

    private bool m_IsInsidePlayer;
    private bool m_WaitingAfterExiting = false;
    private bool m_WaitingAfterEntering = false;
    protected Rigidbody2D m_Rb;
    protected PlayerController m_Player;
    protected IGrabber m_Grabber;

    #region IEatable

    public bool IsEaten => m_IsInsidePlayer;

    Vector2 IEatable.Position { get => m_Rb.position; set => transform.position = value; }// m_Rb.MovePosition(value); }
    float IEatable.Rotation { get => m_Rb.rotation; set => m_Rb.MoveRotation(value); }
    PlayerProfile IEatable.Profile => m_PlayerProfile;

    public virtual void OnGrab(IGrabber grabber) {
        m_Grabber = grabber;
        m_Collider.enabled = false;
        m_Rb.gravityScale = 0f;

        StartCoroutine(nameof(WaitAfterCapture), 2f);
    }

    public virtual bool TryRelease() {
        if (m_WaitingAfterEntering)
            return false;

        TryForcePlayerRelease();
        TryForceGrabberRelease();

        return true;
    }

    protected void TryForcePlayerRelease() {
        if (!m_IsInsidePlayer)
            return;
        m_IsInsidePlayer = false;
        ExitPlayer();
    }
    protected void TryForceGrabberRelease() {
        if (m_Grabber == null)
            return;
        m_Grabber.OnEatableReleased(this);
        m_Grabber = null;
    }

    public virtual void ApplyAttractionForce(Vector2 force) { }

    #endregion

    protected virtual void Awake() {
        m_Rb = GetComponent<Rigidbody2D>();

        m_IsColPointInside = new();
        for (int i = 0; i < m_ColTestPoints.Count; i++)
            m_IsColPointInside.Add(false);
    }

    protected virtual void Start() {
        m_Player = GameManager.Instance.PlayerController;
    }

    protected virtual void FixedUpdate() {
        if (!m_IsInsidePlayer && !m_WaitingAfterExiting)
            OutsideTick();
        else if (m_IsInsidePlayer && !m_WaitingAfterExiting)
            InsideTick();
    }

    private void OutsideTick() {
        m_IsInsidePlayer = IsInsidePlayer();

        if (CanBeGrabbedByPlayer())
            OnEatenByPlayer();
    }

    private void InsideTick() {
        if (CanBeGrabbedByPlayer())
            OnEatenByPlayer();
    }

    private bool CanBeGrabbedByPlayer() {
        if (!m_IsInsidePlayer)
            return false;
        return m_Player.State.CanEat;
    }

    private bool IsInsidePlayer() {
        var isInside = true;

        for (int i = 0; i < m_ColTestPoints.Count; i++) {
            m_IsColPointInside[i] = Util.HorizontalTest(m_ColTestPoints[i].position, (i) => m_Player.GetPointPos(i), 12);
            isInside &= m_IsColPointInside[i];
        }

        return isInside;
    }

    private void OnEatenByPlayer() {
        TryForceGrabberRelease();

        m_Collider.enabled = false;
        m_Rb.gravityScale = 0f;

        m_Player.OnEatableCaptured(this);

        StartCoroutine(nameof(WaitAfterCapture), 2f);
    }

    private void ExitPlayer() {
        m_Collider.enabled = true;
        m_Rb.gravityScale = 1f;

        m_Player.OnEatableReleased(this);

        StartCoroutine(nameof(WaitAfterRelease), 2f);
    }

    private IEnumerator WaitAfterCapture(float seconds) {
        m_WaitingAfterEntering = true;

        yield return new WaitForSeconds(seconds);

        m_WaitingAfterEntering = false;
    }

    private IEnumerator WaitAfterRelease(float seconds) {
        m_WaitingAfterExiting = true;

        yield return new WaitForSeconds(seconds);

        m_WaitingAfterExiting = false;
    }
}
