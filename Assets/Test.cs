using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField][Range(1f, 3f)] private float m_Scale;

    private void Update() {
        transform.localScale = Vector3.one * m_Scale;
    }
}
