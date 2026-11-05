using System;
using System.Collections.Generic;
using JinGroup.Module.Resources;

public class AdsShopBundleContainer : ShopBundleContainer
{
    public BundleBase prefab2;

    protected override BundleBase SelectPrefab(List<BundleReward> rewards)
    {
        if (prefab2 == null || rewards == null)
        {
            return prefab;
        }

        return HasItemRewards(rewards) ? prefab2 : prefab;
    }

    private static bool HasItemRewards(List<BundleReward> rewards)
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] == null)
            {
                continue;
            }

            if (Enum.TryParse<TypeResources>(rewards[i].typeResources, true, out var type)
             && (type == TypeResources.booster1
              || type == TypeResources.booster2
              || type == TypeResources.booster3))
            {
                return true;
            }
        }

        return false;
    }
}