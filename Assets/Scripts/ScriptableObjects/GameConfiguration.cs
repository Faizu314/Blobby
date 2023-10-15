using UnityEngine;

[CreateAssetMenu(menuName = "Blobby/GameConfiguration", fileName = "GameConfig")]
public class GameConfiguration : ScriptableObject {

    public LayerMask GroundLayer;
    public LayerMask WaterLayer;
    public LayerMask PlayerCollisionLayer;
    public LayerMask PlayerPointsLayer;
    public LayerMask PlayerGraphicsLayer;
}
