using Cinemachine;
using Phezu.SceneManagingSystem;
using Phezu.Util;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public CinemachineTargetGroup TargetGroup;
    [HideInInspector] public PlayerController PlayerController;
    public GameConfiguration GameConfiguration;

    [SerializeField] private SceneLoadManager SceneManager;
    [SerializeField] private GameScenes Scenes;
    [SerializeField] private AppConfiguration AppConfiguration;

    private void Awake() {
        Application.targetFrameRate = AppConfiguration.TargetFrameRate;
    }

    private void Start() {
        if (Scenes.Levels.Count < 1)
            return;
        SceneManager.LoadNewScene(Scenes.Levels[0].SceneName, false);
    }

    public void LoadLevel(int levelIndex) {
        if (levelIndex >= Scenes.Levels.Count)
            return;
        SceneManager.LoadNewScene(Scenes.Levels[levelIndex].SceneName, true);
    }
}
