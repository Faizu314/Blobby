using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour
{
    [SerializeField] private PointMassesController m_MassesController;

    private CustomCollider2D m_Collider;
    private PhysicsShapeGroup2D m_ShapeGroup;
    private Vector2[] m_Vertices;
    private Transform m_Transform;

    private void Start() {
        m_Transform = transform;
        m_Collider = GetComponent<CustomCollider2D>();
        m_ShapeGroup = new(m_MassesController.PointMassesCount);

        m_Vertices = new Vector2[m_MassesController.PointMassesCount + 1];

        SetMeshVertices();
        AddTriangles();
        
        m_Collider.SetCustomShapes(m_ShapeGroup);
    }

    private void Update() {
        SetMeshVertices();
        SetTrianglesVertices();

        m_Collider.SetCustomShapes(m_ShapeGroup);
    }

    private void SetMeshVertices() {
        m_Vertices[0] = m_MassesController.MidMassPosition;

        for (int i = 0; i < m_MassesController.PointMassesCount; i++)
            m_Vertices[i + 1] = m_MassesController.GetPointPosition(i);
    }
    private void AddTriangles() {
        for (int t = 0; t < m_MassesController.PointMassesCount; t++) {
            List<Vector2> vertices = new() {
                m_Vertices[0],
                m_Vertices[t + 1]
            };

            if (t == m_MassesController.PointMassesCount - 1)
                vertices.Add(m_Vertices[1]);
            else
                vertices.Add(m_Vertices[t + 2]);

            m_ShapeGroup.AddPolygon(vertices);
        }
    }
    private void SetTrianglesVertices() {
        for (int t = 0; t < m_MassesController.PointMassesCount; t++) {
            m_ShapeGroup.SetShapeVertex(t, 0, m_Vertices[0]);
            m_ShapeGroup.SetShapeVertex(t, 1, m_Vertices[t + 1]);

            if (t == m_MassesController.PointMassesCount - 1)
                m_ShapeGroup.SetShapeVertex(t, 2, m_Vertices[1]);
            else
                m_ShapeGroup.SetShapeVertex(t, 2, m_Vertices[t + 2]);
        }
    }
}
