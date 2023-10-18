using UnityEngine;
using Phezu.Util;

public class Station : MonoBehaviour, IGrabber {

    [SerializeField] private LayerMask m_MetalBallsLayer;
    [SerializeField] private Transform m_StationedPosition;
    [SerializeField] private float m_ChargeRate;
    [SerializeField] private float m_MovementLerpLife;
    [SerializeField] private float m_MovementLerpRatio;

    private float m_ChargeLevel = 0f;

    private IEatable m_Target;
    private IEatable m_Grabbed;
    private IEatable m_ToIgnore;

    private float m_LerpK;

    private void Start() {
        m_LerpK = FMath.GetFreeLerpK(m_MovementLerpLife, m_MovementLerpRatio);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (m_Grabbed != null)
            return;

        var ball = collision.GetComponent<IEatable>();

        if (ball == null)
            return;
        if (!ball.IsEaten)
            return;

        m_Target = ball;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (m_Grabbed != null)
            return;

        var ball = collision.GetComponent<IEatable>();

        if (ball != m_Target)
            return;

        if (m_Target != null && m_Target != m_ToIgnore)
            Charge();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (m_Grabbed != null)
            return;

        var ball = collision.GetComponent<IEatable>();

        if (ball == m_Target) {
            m_Target = null;
            m_ToIgnore = null;
            m_ChargeLevel = 0f;
        }
    }

    public void OnEatableReleased(IEatable eatable) {
        m_ToIgnore = eatable;
        m_Grabbed = null;
    }

    private void Charge() {
        m_ChargeLevel += Time.deltaTime * m_ChargeRate;

        if (m_ChargeLevel >= 1f && m_Target.TryRelease()) {
            m_Grabbed = m_Target;
            m_Grabbed.OnGrab(this);
            m_ChargeLevel = 0f;
        }
    }

    private void Update() {
        if (m_Grabbed == null)
            return;
        //move this to the eatable
        m_Grabbed.Position = Vector2.Lerp(m_Grabbed.Position, m_StationedPosition.position, FMath.GetFreeLerpT(m_LerpK, Time.deltaTime));
    }
}
