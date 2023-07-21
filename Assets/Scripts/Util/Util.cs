using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering.UI;

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

    public static bool IsInLayerMask(LayerMask layermask, int layer) {
        return layermask == (layermask | (1 << layer));
    }

    public static bool HorizontalTest(Vector2 p, Func<int, Vector2> shape, int vertexCount) {
        int intersectionCount = 0;

        for (int i = 0; i < vertexCount; i++) {
            int j = (i + 1) % vertexCount;

            float maxY = Mathf.Max(shape(i).y, shape(j).y);
            float minY = Mathf.Min(shape(i).y, shape(j).y);

            if (p.y > maxY || p.y < minY)
                continue;
            if (shape(i).x < p.x && shape(j).x < p.x)
                continue;
            if (shape(i).x > p.x && shape(j).x > p.x) {
                intersectionCount++;
                continue;
            }

            if (shape(i).x - p.x == 0f || shape(i).x - shape(j).x == 0f) {
                intersectionCount++;
                continue;
            }

            float m1 = (shape(i).y - p.y) / (shape(i).x - p.x);
            float m2 = (shape(i).y - shape(j).y) / (shape(i).x - shape(j).x);

            if (m1 > m2)
                continue;

            intersectionCount++;
        }

        return intersectionCount % 2 == 1;
    }

}
