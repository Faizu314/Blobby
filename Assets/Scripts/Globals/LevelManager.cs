using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int m_LevelIndex;

    public void OnLevelCompleted() {
        GameManager.Instance.LoadLevel(m_LevelIndex + 1);
    }
}
