using UnityEngine;

public class MagneticBall : EatableBase {

    public override void ApplyAttractionForce(Vector2 force) {
        if (IsEaten && Vector2.Dot(m_Player.State.Velocity, force.normalized) < 3f)
            m_Player.AddForce(force);

        if (Vector2.Dot(m_Rb.velocity, force.normalized) < 3f)
            m_Rb.AddForce(force);
    }

}
