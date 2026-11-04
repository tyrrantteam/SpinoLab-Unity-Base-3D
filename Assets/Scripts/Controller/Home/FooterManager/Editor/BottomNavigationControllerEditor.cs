#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BottomNavigationController))]
public class BottomNavigationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Validate Tab Configuration"))
        {
            foreach (var target in targets)
            {
                if (target is BottomNavigationController controller)
                    controller.ValidateTabConfiguration();
            }
        }
    }
}
#endif
