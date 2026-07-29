using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(PopupManager))]
public class PopupManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Init Popup", GUILayout.Height(30)))
        {
            InitPopup();
        }
    }

    private void InitPopup()
    {
        PopupManager manager = (PopupManager)target;
        Transform root = manager.transform;

        Type managerType = typeof(PopupManager);

        FieldInfo[] fields = managerType.GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Undo.RecordObject(manager, "Init Popup");

        foreach (FieldInfo field in fields)
        {
            // Chỉ xử lý SerializeField
            if (!Attribute.IsDefined(field, typeof(SerializeField)))
                continue;

            Type fieldType = field.FieldType;

            // Bỏ qua nếu không phải Component
            if (!typeof(Component).IsAssignableFrom(fieldType))
                continue;

            Component found = root.GetComponentInChildren(fieldType, true);

            if (found != null)
            {
                field.SetValue(manager, found);
                Debug.Log($"[PopupManager] Bind {field.Name} <- {found.name}");
            }
        }

        EditorUtility.SetDirty(manager);
    }
}