using UnityEngine;

namespace Assets.Scripts.Util {
    public class NonPhysicsRotator : MonoBehaviour {

        [SerializeField] private float m_Speed;

        private Transform m_Transform;

        private void Start() {
            m_Transform = transform;
        }

        private void Update() {
            m_Transform.Rotate(Vector3.forward * m_Speed * Time.deltaTime);
        }
    }
}