using UnityEngine;

public class AimVisualHandler : MonoBehaviour {

    [SerializeField] private float m_LengthScaling;
    [SerializeField] private float m_LerpSpeed;

    private MeshRenderer m_Renderer;
    private PlayerState m_PlayerState;
    private Vector2 m_PosTarget;
    private Vector2 m_Pos;
    private float m_AngleTarget;
    private float m_Angle;
    private bool m_HasSnappedToNewAngle = false;
    private bool m_Enabled;

    private void Start() {
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.enabled = m_Enabled = false;
        m_PosTarget = Vector2.zero;
        m_PlayerState = GameManager.Instance.PlayerController.State;
    }

    private void Update() {
        if (m_PlayerState.IsCharging && !m_Enabled) {
            m_Enabled = true;
            Show();
        }
        else if (!m_PlayerState.IsCharging && m_Enabled) {
            m_Enabled = false;
            Hide();
        }
        if (m_Enabled) {
            SetAim(m_PlayerState.Aim);
            SetTransform();
        }
    }

    private void SetTransform() {
        m_Pos = Vector2.Lerp(m_Pos, m_PosTarget, Time.deltaTime * m_LerpSpeed);
        transform.localPosition = m_Pos / 2f;
        m_Angle = Mathf.LerpAngle(m_Angle, m_AngleTarget, Time.deltaTime * m_LerpSpeed);
        transform.eulerAngles = new(0f, 0f, m_Angle);
        var scale = transform.localScale;
        scale.x = m_PosTarget.magnitude;
        transform.localScale = scale;
    }

    private void Show() {
        m_HasSnappedToNewAngle = false;
        m_Renderer.enabled = true;
        m_PosTarget = Vector2.zero;
        m_Pos = Vector2.zero;
    }

    private void Hide() {
        m_Renderer.enabled = false;
        m_AngleTarget = 0f;
        m_Angle = 0f;
    }

    private void SetAim(Vector2 aim) {
        m_PosTarget = aim * m_LengthScaling;
        m_AngleTarget = Util.AngleFromVector(m_PosTarget);
        if (!m_HasSnappedToNewAngle && aim.sqrMagnitude > 0.001f) {
            m_Pos = m_PosTarget;
            m_Angle = m_AngleTarget;
            m_HasSnappedToNewAngle = true;
        }
    }
}
