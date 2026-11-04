using System;
using System.Collections.Generic;
using Base.Core.Debug;
using DataAccount;
using UnityEngine;
using UnityEngine.Serialization;

public enum TabType
{
    Shop    = 0,
    Home    = 1,
    Setting = 2,
}

/// <summary>
/// Persistent bottom navigation for Home: tab selection, panel switching, and notifications.
/// Add tabs via the Inspector array or <see cref="RegisterTab"/> at runtime without changing this class.
/// </summary>
public class BottomNavigationController : SingletonMono<BottomNavigationController>
{
    [FormerlySerializedAs("groupTabBtn")]
    [SerializeField, Tooltip("Tabs left-to-right; index matches SelectTab(int).")]
    private BottomNavigationItem[] tabs = Array.Empty<BottomNavigationItem>();

    [SerializeField, Tooltip("Tab index selected on start (1 = Home).")]
    private int initialTabIndex = 1;

    [SerializeField, Tooltip("When false, selecting the current tab does not refire OnTabChanged or panel logic.")]
    private bool allowReselectCurrentTab;

    [SerializeField] private GameObject bannerAds;
    [SerializeField] private DataAdsController dataAdsController;

    private readonly List<BottomNavigationItem> _registeredTabs = new();
    private int _selectedIndex = -1;
    private bool _initialized;

    /// <summary>Fired with tab id after a successful selection.</summary>
    public event Action<string> OnTabChanged;

    /// <summary>Fired with TabType after a successful selection.</summary>
    public event Action<TabType> OnTabTypeChanged;

    public int SelectedIndex => _selectedIndex;

    public string SelectedTabId =>
        _selectedIndex >= 0 && _selectedIndex < _registeredTabs.Count
            ? _registeredTabs[_selectedIndex].TabId
            : string.Empty;

    protected override void Awake()
    {
        base.Awake();
        CheckNoAds();
        BuildTabRegistryFromSerialized();
        InitializeTabItems();
        _initialized = true;
    }

    private void Start()
    {
        SelectTab(initialTabIndex);
    }

    /// <summary>Selects a tab by index in the registered tab list.</summary>
    public void SelectTab(int index)
    {
        if (!_initialized)
            BuildTabRegistryFromSerialized();

        if (_registeredTabs.Count == 0)
        {
            GameDebug.LogWarning("[BottomNavigation] No tabs registered.");
            return;
        }

        if (index < 0 || index >= _registeredTabs.Count)
        {
            GameDebug.LogWarning($"[BottomNavigation] Invalid tab index: {index}.");
            return;
        }

        if (index == _selectedIndex && !allowReselectCurrentTab)
            return;

        var reselect = index == _selectedIndex && allowReselectCurrentTab;
        ApplySelection(index, fireEvents: true, reselectOnly: reselect);
    }

    /// <summary>Selects a tab by unique id.</summary>
    public void SelectTab(string tabId)
    {
        if (string.IsNullOrWhiteSpace(tabId))
        {
            GameDebug.LogWarning("[BottomNavigation] Tab id is null or empty.");
            return;
        }

        for (var i = 0; i < _registeredTabs.Count; i++)
        {
            if (_registeredTabs[i] != null &&
                string.Equals(_registeredTabs[i].TabId, tabId, StringComparison.Ordinal))
            {
                SelectTab(i);
                return;
            }
        }

        GameDebug.LogWarning($"[BottomNavigation] Unknown tab id: {tabId}.");
    }

    /// <summary>Registers an extra tab at runtime (e.g. new feature panel).</summary>
    public void RegisterTab(BottomNavigationItem item)
    {
        if (item == null)
            return;

        if (!_registeredTabs.Contains(item))
            _registeredTabs.Add(item);

        item.Initialize(this);
        ValidateTabConfiguration();
    }

    /// <summary>Shows or hides the notification badge for a tab id.</summary>
    public void SetNotification(string tabId, bool visible)
    {
        var item = FindTabById(tabId);
        if (item == null)
        {
            GameDebug.LogWarning($"[BottomNavigation] SetNotification: unknown tab id '{tabId}'.");
            return;
        }

        item.SetNotificationVisible(visible);
    }

    /// <summary>Selects a tab by <see cref="TabType"/>.</summary>
    public void SelectTab(TabType tabType)
    {
        for (var i = 0; i < _registeredTabs.Count; i++)
        {
            if (_registeredTabs[i] != null && _registeredTabs[i].TabType == tabType)
            {
                SelectTab(i);
                return;
            }
        }

        SelectTab((int)tabType);
    }

    /// <summary>Legacy API: opens a tab by enum (Shop / Home / Setting).</summary>
    public void SetPanelActive(TabType tabType)
    {
        SelectTab(tabType);
    }

    /// <summary>Called by <see cref="BottomNavigationItem"/> when its toggle turns on.</summary>
    public void NotifyTabSelected(BottomNavigationItem item)
    {
        if (item == null)
            return;

        var index = _registeredTabs.IndexOf(item);
        if (index < 0)
            return;

        item.PlayButtonSound();
        SelectTab(index);
    }

    /// <summary>Editor and tests: validates duplicate ids and missing references.</summary>
    public void ValidateTabConfiguration()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < _registeredTabs.Count; i++)
        {
            var tab = _registeredTabs[i];
            if (tab == null)
            {
                GameDebug.LogWarning($"[BottomNavigation] Tab slot {i} is null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(tab.TabId))
                GameDebug.LogWarning($"[BottomNavigation] Tab '{tab.name}' has empty tabId.", tab);

            if (!ids.Add(tab.TabId))
                GameDebug.LogWarning($"[BottomNavigation] Duplicate tabId '{tab.TabId}'.", tab);

            if (tab.ContentPanel == null)
                GameDebug.LogWarning($"[BottomNavigation] Tab '{tab.TabId}' missing contentPanel.", tab);
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying || tabs == null)
            return;

        BuildTabRegistryFromSerialized();
        ValidateTabConfiguration();
#endif
    }

    /// <summary>Rebuilds runtime tab registry after the <see cref="tabs"/> array changes at runtime or in tests.</summary>
    public void RebuildTabsForTests()
    {
        foreach (var tab in _registeredTabs)
            tab?.Uninitialize();

        _registeredTabs.Clear();
        BuildTabRegistryFromSerialized();
        InitializeTabItems();
        _initialized = true;
    }

    private void CheckNoAds()
    {
        if (bannerAds == null || dataAdsController == null)
            return;

        var isNoAdsPurchased = DataAccountPlayer.PlayerResourceData.isNoAdsPurchase;
        bannerAds.SetActive(!isNoAdsPurchased && !dataAdsController.UsingBannerAds);
    }

    private void BuildTabRegistryFromSerialized()
    {
        _registeredTabs.Clear();
        if (tabs == null)
            return;

        foreach (var tab in tabs)
        {
            if (tab != null && !_registeredTabs.Contains(tab))
                _registeredTabs.Add(tab);
        }
    }

    private void InitializeTabItems()
    {
        foreach (var tab in _registeredTabs)
            tab.Initialize(this);

        ValidateTabConfiguration();
    }

    private void ApplySelection(int index, bool fireEvents, bool reselectOnly)
    {
        _selectedIndex = index;

        for (var i = 0; i < _registeredTabs.Count; i++)
        {
            var tab = _registeredTabs[i];
            if (tab == null)
                continue;

            var selected = i == index;
            tab.SetToggleOnSilently(selected);
            tab.ApplyVisualState(selected);

            if (!reselectOnly && tab.ContentPanel != null)
                tab.ContentPanel.SetActive(selected);
        }

        if (!fireEvents)
            return;

        var selectedTab = _registeredTabs[index];
        selectedTab.InvokeClickEvent();
        OnTabChanged?.Invoke(selectedTab.TabId);
        OnTabTypeChanged?.Invoke(selectedTab.TabType);
    }

    private BottomNavigationItem FindTabById(string tabId)
    {
        foreach (var tab in _registeredTabs)
        {
            if (tab != null && string.Equals(tab.TabId, tabId, StringComparison.Ordinal))
                return tab;
        }

        return null;
    }
}
