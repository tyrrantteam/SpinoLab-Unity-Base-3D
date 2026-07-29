using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChildLayoutGroup))]
public class ChildLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChildLayoutGroup layout = (ChildLayoutGroup)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply Layout", GUILayout.Height(30)))
            {
                RecordChildrenForUndo(layout, "ChildLayoutGroup Apply");
                layout.ApplyLayout();
            }

            if (GUILayout.Button("Reset Positions", GUILayout.Height(30)))
            {
                RecordChildrenForUndo(layout, "ChildLayoutGroup Reset");
                layout.ResetPositions();
            }
        }
    }

    private static void RecordChildrenForUndo(ChildLayoutGroup layout, string operationName)
    {
        var children = new Transform[layout.transform.childCount];
        for (int i = 0; i < layout.transform.childCount; i++)
            children[i] = layout.transform.GetChild(i);

        Undo.RecordObjects(children, operationName);
    }
}
