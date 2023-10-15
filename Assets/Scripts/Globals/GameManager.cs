using Cinemachine;
using Phezu.SceneManagingSystem;
using Phezu.Util;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public CinemachineTargetGroup TargetGroup;
    [HideInInspector] public PlayerController PlayerController;
    public GameConfiguration GameConfiguration;
    public SceneLoadManager SceneManager;

    private void Start() {
        SceneManager.LoadNewScene("Level", false);
    }

}
