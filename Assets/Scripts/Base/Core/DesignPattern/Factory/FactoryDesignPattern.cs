using Base.Core.Debug;
using Core.Pool;
using UnityEngine;

namespace Core.DesignPattern.Factory
{
    public class FactoryDesignPattern : SingletonMono<FactoryDesignPattern>
    {
        [SerializeField] private FactoryPoolData poolData;

        #region SpawnAPI

        public GameObject CreateObjectUI(string id, RectTransform parent, Vector3? anchoredPosition = null)
        {
            var dataItem = poolData.GetElementByNameIDUI(id);
            if (dataItem == null)
            {
                Debug.LogError("FactoryDesignPattern: Can't find object with id " + id);
                return null;
            }
            var prefab = dataItem.typeObject;

            if (anchoredPosition.HasValue)
            {
                var pos = anchoredPosition.Value;
                anchoredPosition = new Vector3(pos.x, pos.y, 0f);
            }

            var go = SmartPool.Instance.SpawnUI(prefab, parent, anchoredPosition);

            // Ép z = 0 sau khi spawn (fix trường hợp SpawnUI tự gán lại z)
            if (go != null)
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    var ap = rt.anchoredPosition3D;
                    rt.anchoredPosition3D = new Vector3(ap.x, ap.y, 0f);
                }
            }

            return go;
        }

        public GameObject CreateGameObject(string id, Vector3 position, Quaternion rotation)
        {
            var dataItem = poolData.GetElementByNameID(id);
            if (dataItem == null)
            {
                Debug.LogError("FactoryDesignPattern: Can't find object with id " + id);
                return null;
            }

            var prefab = dataItem.typeObject;
            return SmartPool.Instance.Spawn(prefab, position, rotation);
        }

        #endregion
    }
}