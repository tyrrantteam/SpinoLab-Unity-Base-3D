using JinGroup.Common.UIBaseController;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : SingletonMono<PopupManager>
{
    private bool isPopupActive = false;
    private readonly Queue<Action> popupQueue = new Queue<Action>();
    private readonly Dictionary<Type, PopupBaseController> instanceCache = new Dictionary<Type, PopupBaseController>();

    [Title("Popup Prefabs")]
    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    private List<PopupBaseController> popupPrefabs;

    [SerializeField] private PopupShowReward popupShowReward;
    [SerializeField] private PopupShowRewardSingleUnitController popupShowRewardSingleUnit;

    protected override void Awake()
    {
        base.Awake();
        GetOrSpawnPopupShowReward();
        GetOrSpawnPopupShowRewardSingleUnit();
    }

    [Button]
    public T ShowPopup<T>(Action onShown = null) where T : PopupBaseController
    {
        return ShowPopup<T>(null, onShown);
    }

    public T ShowPopup<T>(Action<T> configure, Action onShown = null) where T : PopupBaseController
    {
        var instance = GetOrSpawnInstance<T>();
        if (instance == null) return null;
        configure?.Invoke(instance);
        ShowWithoutHidingOthers(instance.gameObject, onShown);
        return instance;
    }

    public PopupShowReward ModuleShowReward(Action onShown = null)
    {
        var instance = GetOrSpawnPopupShowReward();
        if (instance != null)
        {
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);
            isPopupActive = true;
            onShown?.Invoke();
        }
        return instance;
    }

    public PopupShowRewardSingleUnitController ModuleShowRewardSingleUnit(Action onShown = null)
    {
        var instance = GetOrSpawnPopupShowRewardSingleUnit();
        if (instance != null)
        {
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);
            isPopupActive = true;
            onShown?.Invoke();
        }
        return instance;
    }

    private PopupShowReward GetOrSpawnPopupShowReward()
    {
        // 1. Đã là GameObject instance hợp lệ trong scene
        if (popupShowReward != null && popupShowReward.gameObject.scene.IsValid())
        {
            return popupShowReward;
        }

        // 2. popupShowReward là Prefab Asset (chưa instantiate vào scene)
        if (popupShowReward != null)
        {
            var instance = Instantiate(popupShowReward, transform);
            instance.gameObject.SetActive(false);
            popupShowReward = instance;
            return popupShowReward;
        }

        // 3. Thử tìm trong children
        popupShowReward = GetComponentInChildren<PopupShowReward>(true);
        if (popupShowReward != null)
        {
            return popupShowReward;
        }

        // 4. Fallback: Load prefab từ Resources
        var prefab = Resources.Load<PopupShowReward>("Prefabs/UI/Popup/PopupClaimReward");
        if (prefab != null)
        {
            var instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            popupShowReward = instance;
            return popupShowReward;
        }

        Debug.LogError("[PopupManager] PopupShowReward prefab not found!");
        return null;
    }

    public PopupShowRewardSingleUnitController GetOrSpawnPopupShowRewardSingleUnit()
    {
        // 1. Đã là GameObject instance hợp lệ trong scene
        if (popupShowRewardSingleUnit != null && popupShowRewardSingleUnit.gameObject.scene.IsValid())
        {
            return popupShowRewardSingleUnit;
        }

        // 2. popupShowRewardSingleUnit là Prefab Asset (chưa instantiate vào scene)
        if (popupShowRewardSingleUnit != null)
        {
            var instance = Instantiate(popupShowRewardSingleUnit, transform);
            instance.gameObject.SetActive(false);
            popupShowRewardSingleUnit = instance;
            return popupShowRewardSingleUnit;
        }

        // 3. Thử tìm trong children
        popupShowRewardSingleUnit = GetComponentInChildren<PopupShowRewardSingleUnitController>(true);
        if (popupShowRewardSingleUnit != null)
        {
            return popupShowRewardSingleUnit;
        }

        // 4. Tìm trong popupPrefabs
        if (popupPrefabs != null)
        {
            for (int i = 0; i < popupPrefabs.Count; i++)
            {
                if (popupPrefabs[i] is PopupShowRewardSingleUnitController prefab && prefab != null)
                {
                    var instance = Instantiate(prefab, transform);
                    instance.gameObject.SetActive(false);
                    popupShowRewardSingleUnit = instance;
                    return popupShowRewardSingleUnit;
                }
            }
        }

        // 5. Fallback: Load prefab từ Resources
        string[] resourcePaths = new string[]
        {
            "Prefabs/UI/Popup/Public/PopupClaimRewardSingleUnit",
            "Prefabs/UI/Popup/PopupClaimRewardSingleUnit",
            "Prefabs/UI/Popup/Public/PopupShowRewardSingleUnit",
            "Prefabs/UI/Popup/PopupShowRewardSingleUnit",
            "Prefabs/UI/PopupShowRewardSingleUnit",
            "Prefabs/UI/Popup/Meta/PopupShowRewardSingleUnit"
        };
        foreach (var path in resourcePaths)
        {
            var prefab = Resources.Load<PopupShowRewardSingleUnitController>(path);
            if (prefab != null)
            {
                var instance = Instantiate(prefab, transform);
                instance.gameObject.SetActive(false);
                popupShowRewardSingleUnit = instance;
                return popupShowRewardSingleUnit;
            }
        }

        return null;
    }

    public void CloseCurrentPopup()
    {
        isPopupActive = false;

        if (popupQueue.Count > 0)
        {
            var next = popupQueue.Dequeue();
            next.Invoke();
        }
    }

    /// <summary>Đóng popup lớp phủ — giữ popup đang mở phía dưới và không lấy hàng đợi.</summary>
    public void ClosePopupKeepOthers()
    {
        if (AnyPopupVisible())
        {
            isPopupActive = true;
            return;
        }

        CloseCurrentPopup();
    }

    private T GetOrSpawnInstance<T>() where T : PopupBaseController
    {
        var type = typeof(T);

        if (instanceCache.TryGetValue(type, out var cached) && cached != null)
            return cached as T;

        if (type == typeof(PopupShowRewardSingleUnitController))
        {
            var single = GetOrSpawnPopupShowRewardSingleUnit();
            if (single != null)
            {
                instanceCache[type] = single;
                return single as T;
            }
        }

        if (popupPrefabs != null)
        {
            for (int i = 0; i < popupPrefabs.Count; i++)
            {
                if (popupPrefabs[i] is T prefab && prefab != null)
                {
                    var instance = Instantiate(prefab, transform);
                    instance.gameObject.SetActive(false);
                    instanceCache[type] = instance;
                    return instance;
                }
            }
        }

        var inChild = GetComponentInChildren<T>(true);
        if (inChild != null)
        {
            instanceCache[type] = inChild;
            return inChild;
        }

        string shortName = type.Name.Replace("Controller", "");
        string[] resourcePaths = new string[]
        {
            $"Prefabs/UI/Popup/Meta/DailyTask/{type.Name}",
            $"Prefabs/UI/Popup/Meta/DailyTask/{shortName}",
            $"Prefabs/UI/Popup/Meta/{type.Name}",
            $"Prefabs/UI/Popup/Meta/{shortName}",
            $"Prefabs/UI/Popup/InGame/{type.Name}",
            $"Prefabs/UI/Popup/InGame/{shortName}",
            $"Prefabs/UI/Popup/Public/{type.Name}",
            $"Prefabs/UI/Popup/Public/{shortName}",
            $"Prefabs/UI/Popup/{type.Name}",
            $"Prefabs/UI/Popup/{shortName}",
            $"Prefabs/UI/{type.Name}",
            $"Prefabs/UI/{shortName}"
        };

        foreach (var path in resourcePaths)
        {
            var resPrefab = Resources.Load<T>(path);
            if (resPrefab != null)
            {
                var instance = Instantiate(resPrefab, transform);
                instance.gameObject.SetActive(false);
                instanceCache[type] = instance;
                return instance;
            }
        }

        Debug.LogWarning($"[PopupManager] Prefab of type {typeof(T).Name} not found in popupPrefabs list.");
        return null;
    }
    
    private bool AnyPopupVisible()
    {
        foreach (var popup in instanceCache.Values)
        {
            if (popup != null && popup.gameObject.activeSelf)
                return true;
        }

        bool showRewardVisible = popupShowReward != null && popupShowReward.gameObject.activeSelf;
        bool singleUnitVisible = popupShowRewardSingleUnit != null && popupShowRewardSingleUnit.gameObject.activeSelf;

        return showRewardVisible || singleUnitVisible;
    }

    private void ShowWithoutHidingOthers(GameObject popup, Action onShown = null)
    {
        isPopupActive = true;
        popup.transform.SetAsLastSibling();
        popup.SetActive(true);
        onShown?.Invoke();
    }

    private void EnqueuePopup(GameObject popup, Action onShown = null)
    {
        Action showAction = () =>
        {
            isPopupActive = true;
            popup.SetActive(true);
            onShown?.Invoke();
        };

        if (!isPopupActive)
        {
            showAction.Invoke();
        }
        else
        {
            popupQueue.Enqueue(showAction);
        }
    }
}
