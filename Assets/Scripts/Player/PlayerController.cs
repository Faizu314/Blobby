using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;
    [SerializeField] private PlayerInputs m_Inputs;
    [SerializeField] private PlayerState m_State;
    [SerializeField] private PlayerProfile m_DefaultPlayerProfile;
    [SerializeField] private int m_MinPointsForGrounded;

    [SerializeField] private LayerMask m_EdgesLayer;

    [SerializeField] private float m_InflationSpeed;
    [SerializeField] private float m_MaxEatenFollowSpeed;

    private PlayerProfile m_CurrProfile;
    private IEatable m_Eatable = null;
    private Coroutine m_InflateCoroutine = null;
    private Vector2 m_ReleaseVector;
    private float m_EatenFollowSpeed = 0f;
    private bool m_IsCharging;

    public Vector2 GetPointPos(int i) => m_BodyController.GetPointPosition(i);
    public PlayerState State => m_State;

    private void Awake() {
        GameManager.Instance.PlayerController = this;
    }

    private void Start() {
        m_BodyController.Init();
        ApplyPlayerProfile(m_DefaultPlayerProfile);
    }

    public void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Force) {
        if (m_BodyController.AvgVelocity.y < 5f)
            m_BodyController.ApplyForceOnAllMasses(force / 12f, forceMode);
    }

    private void Update() {
        m_BodyController.UpdateTick();
    }

    private void FixedUpdate() {
        m_BodyController.FixedUpdateTick();

        MovementTick();
        EatTick();
        SetState();
    }

    private void SetState() {
        m_State.IsCharging = m_IsCharging;
        m_State.IsGrounded = m_BodyController.GroundedPointsCount >= m_MinPointsForGrounded;
        m_State.CanEat = m_Eatable == null;
        m_State.Velocity = m_BodyController.AvgVelocity;
        m_State.Aim = m_ReleaseVector;
    }

    private void ApplyPlayerProfile(PlayerProfile profile) {
        m_State.CanBreak = profile.CanBreak;
        m_State.CanFloat = profile.CanFloat;

        m_BodyController.SetConfiguration(profile.SoftBodyConfig);
        m_BodyController.ApplyConfiguration();

        InflateTo(profile.InflationLevel);
        m_CurrProfile = profile;
    }

    #region Movement

    private void MovementTick() {
        if (m_Inputs.WasPrimaryDownFixed && m_State.IsGrounded && !m_IsCharging) {
            BeginCharge();
        }
        else if (m_Inputs.WasPrimaryDownFixed && m_IsCharging) {
            Charge();
        }
        else if (m_Inputs.WasPrimaryReleasedFixed && m_IsCharging) {
            ReleaseCharge();
        }
    }

    private Vector2 GetChargeVector(out Vector2 dir) {
        var vector = m_Inputs.DragVectorFixed;
        float magnitude = vector.magnitude;
        dir = vector.normalized;
        var chargeVec = dir;

        for (int i = m_CurrProfile.ReleaseLevels.Count - 1; i >= 0; i--) {
            float threshold = m_CurrProfile.ReleaseLevels[i].Threshold;

            if (magnitude >= threshold) {
                chargeVec = dir * m_CurrProfile.ReleaseLevels[i].Strength;
                break;
            }
        }

        return chargeVec;
    }

    private void BeginCharge() {
        GetChargeVector(out var chargeDir);

        var groundedPoints = m_BodyController.GetGroundedPointMasses();
        foreach (var point in groundedPoints)
            point.SetFreeze(true);

        m_BodyController.ApplyForceOnMidMass(chargeDir * m_CurrProfile.ChargeForce);
        m_ReleaseVector = Vector2.zero;
        m_IsCharging = true;
    }

    private void Charge() {
        var chargeForce = GetChargeVector(out var chargeDir);

        var groundedPoints = m_BodyController.GetGroundedPointMasses();
        foreach (var point in groundedPoints)
            point.SetFreeze(true);

        m_BodyController.ApplyForceOnMidMass(chargeDir * m_CurrProfile.ChargeForce);
        m_ReleaseVector = -chargeForce;
    }

    private void ReleaseCharge() {
        m_BodyController.UnfreezePoints();

        m_BodyController.ApplyForceOnAllMasses(m_ReleaseVector, ForceMode2D.Impulse);
        m_IsCharging = false;
    }

    #endregion

    #region Eating

    private void EatTick() {
        if (m_Eatable == null)
            return;

        m_Eatable.Position = Vector2.Lerp(m_Eatable.Position, m_BodyController.AvgPosition, Time.deltaTime * m_EatenFollowSpeed);
        m_Eatable.Rotation = Mathf.Lerp(m_Eatable.Rotation, m_BodyController.PointMassesDeviation, Time.deltaTime * m_EatenFollowSpeed);

        if (m_EatenFollowSpeed < m_MaxEatenFollowSpeed) {
            m_EatenFollowSpeed += Time.deltaTime * 10f;
            m_EatenFollowSpeed = Mathf.Clamp(m_EatenFollowSpeed, 1f, m_MaxEatenFollowSpeed);
        }
    }

    public void OnEatableCaptured(IEatable eatable) {
        if (m_Eatable != null)
            return;

        m_EatenFollowSpeed = 1f;
        m_Eatable = eatable;
        ApplyPlayerProfile(eatable.Profile);
    }

    public void OnEatableReleased(IEatable eatable) {
        if (eatable != m_Eatable)
            return;

        m_Eatable = null;
        ApplyPlayerProfile(m_DefaultPlayerProfile);
    }

    #endregion

    #region Inflation

    public void InflateTo(float target) {
        if (m_InflateCoroutine != null)
            return;

        m_InflateCoroutine = StartCoroutine(nameof(InflateToCoroutine), target);
    }

    private IEnumerator InflateToCoroutine(float target) {
        while (Mathf.Abs(m_BodyController.Scale - target) > 0.01f) {
            m_BodyController.Scale = Mathf.Lerp(m_BodyController.Scale, target, m_InflationSpeed * Time.deltaTime);

            yield return null;
        }

        m_BodyController.Scale = target;
        m_InflateCoroutine = null;
    }

    #endregion

    #region Not Currently Using

    private GameObject m_HookObj;
    private GameObject m_HookPrefab;
    private float m_HookForce;

    private void OnHookDeployed(Vector2 mousePos) {
        if (m_HookObj == null) {
            Vector2 dir = (mousePos - m_BodyController.Position).normalized;
            Vector2 pos = m_BodyController.Position + (dir * m_BodyController.Scale);

            m_HookObj = Instantiate(m_HookPrefab, pos, Quaternion.identity);
            GameManager.Instance.TargetGroup.AddMember(m_HookObj.transform, 0.5f, 4f);
            var hook = m_HookObj.GetComponent<Hook>();

            var hitInfo = Physics2D.Raycast(m_BodyController.Position, dir, 50f, m_EdgesLayer);
            if (hitInfo.collider == null)
                Debug.Log("No edges found via raycast from mid mass.");

            hook.AttachTo(hitInfo.rigidbody);
            hook.Shoot(dir * m_HookForce);
        }
        else {
            GameManager.Instance.TargetGroup.RemoveMember(m_HookObj.transform);
            Destroy(m_HookObj);
        }
    }

    #endregion
}
