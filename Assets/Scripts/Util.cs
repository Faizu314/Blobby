using UnityEngine;

public static class Util {

    /// <returns>Angle in degrees.</returns>
    public static float AngleFromVector(Vector3 pos) {
        return Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
    }

    /// <param name="angle">Angle in degrees.</param>
    public static Vector2 VectorFromAngle(float angle) {
        angle *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

}
