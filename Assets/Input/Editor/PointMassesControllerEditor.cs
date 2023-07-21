using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PointMassesController))]
public class PointMassesControllerEditor : Editor
{
    private PointMassesController Target {
        get {
            if (m_Target == null)
                m_Target = target as PointMassesController;

            return m_Target;
        }
    }

    private PointMassesController m_Target;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Find Joints")) {
            var allJoints = Target.GetComponentsInChildren<SpringJoint2D>();

            //Target.m_BetweenPointMasses = new();
            //Target.m_MassesToEdges = new();

            foreach (var joint in allJoints) {
                if (!joint.enableCollision)
                    Target.m_MassesToMasses.Add(joint);
                else
                    Target.m_MassesToEdges.Add(joint);
            }
        }
    }
}
