using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => m_Instance;
    private static GameManager m_Instance;

    public CinemachineTargetGroup TargetGroup;

    private void Awake() {
        if (m_Instance != null)
            Destroy(this);
        m_Instance = this;
    }

}
