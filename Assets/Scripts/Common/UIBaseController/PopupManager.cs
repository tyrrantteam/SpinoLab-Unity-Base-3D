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

    protected override void Awake()
    {
        base.Awake();
        GetOrSpawnPopupShowReward();
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
        EnqueuePopup(instance.gameObject, onShown);
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

    public void CloseCurrentPopup()
    {
        isPopupActive = false;

        if (popupQueue.Count > 0)
        {
            var next = popupQueue.Dequeue();
            next.Invoke();
        }
    }

    private T GetOrSpawnInstance<T>() where T : PopupBaseController
    {
        var type = typeof(T);

        if (instanceCache.TryGetValue(type, out var cached))
            return cached as T;

        for (int i = 0; i < popupPrefabs.Count; i++)
        {
            if (popupPrefabs[i] is T prefab)
            {
                var instance = Instantiate(prefab, transform);
                instance.gameObject.SetActive(false);
                instanceCache[type] = instance;
                return instance;
            }
        }

        Debug.LogWarning($"[PopupManager] Prefab of type {typeof(T).Name} not found in popupPrefabs list.");
        return null;
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
