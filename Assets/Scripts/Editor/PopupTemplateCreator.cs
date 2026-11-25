using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JinGroup.Editor
{
    public static class PopupTemplateCreator
    {
        private const string PrefabPath = "Assets/Resources/Prefabs/UI/PF_PopupTemplate.prefab";

        [MenuItem("GameObject/Create Popup template", false, 11)]
        [MenuItem("GameObject/UI/Create Popup template", false, 2000)]
        public static void CreatePopupTemplate(MenuCommand menuCommand)
        {
            // 1. Tải prefab từ Assets/Resources/Prefabs/UI/PF_PopupTemplate.prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Prefabs/UI/PF_PopupTemplate");
            }

            if (prefab == null)
            {
                Debug.LogError($"[PopupTemplateCreator] Không tìm thấy prefab tại đường dẫn: {PrefabPath}");
                return;
            }

            // 2. Instantiate giữ nguyên liên kết Prefab (Prefab Instance)
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
            }

            // 3. Xác định Parent thích hợp trong Scene
            Transform parentTransform = null;
            GameObject contextGo = menuCommand.context as GameObject;

            if (contextGo != null)
            {
                parentTransform = contextGo.transform;
            }
            else if (Selection.activeTransform != null)
            {
                parentTransform = Selection.activeTransform;
            }

            // Nếu không chọn GameObject nào, tự động tìm PopupManager hoặc Canvas trong Scene
            if (parentTransform == null)
            {
                PopupManager popupManager = Object.FindObjectOfType<PopupManager>();
                if (popupManager != null)
                {
                    parentTransform = popupManager.transform;
                }
                else
                {
                    Canvas canvas = Object.FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        parentTransform = canvas.transform;
                    }
                }
            }

            if (parentTransform != null)
            {
                // Giữ nguyên local scale, position và anchors của prefab
                instance.transform.SetParent(parentTransform, false);
            }
            else
            {
                SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
            }

            // 4. Đảm bảo tên duy nhất và đăng ký Undo
            GameObjectUtility.EnsureUniqueNameForSibling(instance);
            Undo.RegisterCreatedObjectUndo(instance, "Create Popup template");

            // 5. Focus chọn đối tượng mới tạo trong Hierarchy
            Selection.activeObject = instance;
        }
    }
}
