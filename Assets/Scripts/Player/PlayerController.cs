using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;

    [SerializeField] private GameObject m_HookPrefab;
    [SerializeField] private LayerMask m_EdgesLayer;

    [SerializeField] private float Speed;
    [SerializeField] private float InflationSpeed;
    [SerializeField] private float HookForce;

    private GameObject m_HookObj;
    private InputManager.PlayerInputs m_Inputs;
    private Coroutine m_Coroutine = null;

    public Vector2 GetPointPos(int i) => m_BodyController.PointPosition(i);

    private void Start() {
        m_Inputs = InputManager.Instance.playerInputs;
        InputManager.Instance.OnHookButtonPressed += OnHookDeployed;
    }

    private void OnDestroy() {
        InputManager.Instance.OnHookButtonPressed -= OnHookDeployed;
    }

    private void FixedUpdate() {
        if (m_Inputs.MovingForward)
            Move(Speed);
        else if (m_Inputs.MovingBackward)
            Move(-Speed);
    }

    private void Move(float amount) {
        m_BodyController.ApplyTorque(amount);
    }

    public void InflateTo(float target) {
        if (m_Coroutine != null)
            return;

        m_Coroutine = StartCoroutine(nameof(InflateToCoroutine), target);
    }

    public void SetCollisionWith(Collider2D collider, bool shouldCollide) {
        m_BodyController.SetCollisionWith(collider, shouldCollide);
    }

    private IEnumerator InflateToCoroutine(float target) {
        while (Mathf.Abs(m_BodyController.Scale - target) > 0.01f) {
            m_BodyController.Scale = Mathf.Lerp(m_BodyController.Scale, target, InflationSpeed * Time.deltaTime);

            yield return null;
        }

        m_Coroutine = null;
    }

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
            hook.Shoot(dir * HookForce);
        }
        else {
            GameManager.Instance.TargetGroup.RemoveMember(m_HookObj.transform);
            Destroy(m_HookObj);
        }
    }
}
