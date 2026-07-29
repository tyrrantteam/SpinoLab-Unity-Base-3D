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
        EnqueuePopup(popupShowReward.gameObject, onShown);
        return popupShowReward;
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
