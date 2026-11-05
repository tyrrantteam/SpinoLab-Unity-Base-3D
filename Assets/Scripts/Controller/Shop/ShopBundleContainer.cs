using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Base.Core.Debug;

public class ShopBundleContainer : MonoBehaviour
{
    public BundleType bundleType;
    public BundleBase prefab;

    [Button]
    public virtual void InitData(DataBundleController dataBundle)
    {
        var data = dataBundle.GetBundleRewardsByType(bundleType);
        if (data == null)
        {
            GameDebug.LogError($"[ShopBundle] data is NULL for bundleType={bundleType} on {gameObject.name}");
            return;
        }

        for (int i = 0; i < data.Count; i++)
        {
            SpawnBundle(data[i], i);
        }

        ShopLayout.RebuildSection(transform as RectTransform);
    }

    private void SpawnBundle(List<BundleReward> rewards, int index)
    {
        if (rewards == null || rewards.Count == 0)
        {
            return;
        }

        var selectedPrefab = SelectPrefab(rewards);
        if (selectedPrefab == null)
        {
            return;
        }

        var bundle = Instantiate(selectedPrefab, transform);
        ShopLayout.PrepareSpawnedItem(bundle.transform as RectTransform);
        bundle.LoadBundle(rewards, index);
    }

    protected virtual BundleBase SelectPrefab(List<BundleReward> rewards)
    {
        return prefab;
    }
}