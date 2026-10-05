using System.Reflection;
using NorskaLib.Spreadsheets;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataBoosterController))]
public class DataBoosterControllerEditor : SpreadsheetContainerEditor
{
    private FieldInfo _importerField;
    private SpreadsheetImporter _hookedImporter;

    private void OnEnable()
    {
        _importerField = typeof(SpreadsheetContainerEditor).GetField("importer", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private void OnDisable()
    {
        if (_hookedImporter != null)
        {
            _hookedImporter.onComplete -= OnImportCompleted;
            _hookedImporter = null;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Kiểm tra xem importer có vừa được tạo khi người dùng bấm nút "Import" không
        if (_importerField != null)
        {
            var currentImporter = _importerField.GetValue(this) as SpreadsheetImporter;
            if (currentImporter != null && currentImporter != _hookedImporter)
            {
                if (_hookedImporter != null)
                {
                    _hookedImporter.onComplete -= OnImportCompleted;
                }

                _hookedImporter = currentImporter;
                _hookedImporter.onComplete += OnImportCompleted;
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Booster sprites (imgMechanic: Booster_pre_X.png & imgIconBooster: Icon_Booster X.Png) are automatically linked when importing data.", MessageType.Info);

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
        if (GUILayout.Button("Auto Link Booster Sprites", GUILayout.Height(30)))
        {
            if (target is DataBoosterController controller)
            {
                controller.AutoLinkSprites(true);
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnImportCompleted()
    {
        EditorApplication.delayCall += () =>
        {
            if (target is DataBoosterController controller)
            {
                controller.AutoLinkSprites(true);
            }
        };
    }
}
