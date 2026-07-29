using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HierarchicalChildLayoutGroup))]
public class HierarchicalChildLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var layout = (HierarchicalChildLayoutGroup)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply Layout", GUILayout.Height(30)))
            {
                RecordDescendantsForUndo(layout.transform, "HierarchicalChildLayoutGroup Apply");
                layout.ApplyLayout();
            }

            if (GUILayout.Button("Reset Positions", GUILayout.Height(30)))
            {
                RecordDescendantsForUndo(layout.transform, "HierarchicalChildLayoutGroup Reset");
                layout.ResetPositions();
            }
        }
    }

    static void RecordDescendantsForUndo(Transform root, string operationName)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        Undo.RecordObjects(all, operationName);
    }
}
