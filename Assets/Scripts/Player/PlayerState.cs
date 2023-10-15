using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool IsGrounded;
    public bool IsCharging;
    public bool CanEat;

    // Profile Related
    public bool CanBreak;
    public bool CanFloat;

    public Vector2 Velocity;
    public Vector2 Aim;
}
