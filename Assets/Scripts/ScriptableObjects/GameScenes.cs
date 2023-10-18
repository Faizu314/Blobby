using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scenes", menuName = "Blobby/GameScenes")]
public class GameScenes : ScriptableObject {

    [Serializable]
    public struct SceneData {
        [SceneField] public string SceneName;
    }

    public List<SceneData> Levels;
}
