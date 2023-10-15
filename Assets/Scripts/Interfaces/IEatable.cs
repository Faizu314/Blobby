using UnityEngine;

public interface IEatable {
    Vector2 Position { get; set; }
    float Rotation { get; set; }
    bool IsEaten { get; }
    PlayerProfile Profile { get; }

    void ApplyAttractionForce(Vector2 force);
    void OnGrab(IGrabber grabber);
    bool TryRelease();
}
