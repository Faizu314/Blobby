using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_BodyController;

    [SerializeField] private GameObject m_HookPrefab;
    [SerializeField] private LayerMask m_EdgesLayer;

    [SerializeField] private float Speed;
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float HookForce;

    private GameObject m_HookObj;
    private InputManager.PlayerInputs m_Inputs;

    private void Start() {
        m_Inputs = InputManager.Instance.playerInputs;
        InputManager.Instance.OnJump += OnHookDeployed;
    }

    private void OnDestroy() {
        InputManager.Instance.OnJump -= OnHookDeployed;
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
