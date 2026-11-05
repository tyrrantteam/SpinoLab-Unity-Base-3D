using System.Collections;
using System.Collections.Generic;
using DataAccount;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Base.Core.Debug;

public class ShopManager : MonoBehaviour
{
    public DataBundleController dataBundle;
    public List<ShopBundleContainer>     shopBundles;

    [SerializeField] private float spawnDelay;

    private bool spawnComplete;

    // private bool hasNoAds;
    private bool specialAdsAvailable;
    private bool hasVip;

    private void Awake()
    {
        InitBundles();
    }

    private void OnEnable()
    {
        var playerData = DataAccountPlayer.PlayerResourceData;
        if (playerData != null)
        {
            // playerData.CheckUpdateHeartRecover();
        }

        if (spawnComplete)
        {
            StartCoroutine(DelayLayout());
        }

        this.RegisterListener(EventID.ActiveVip, (component, o) => DisableBundleType(BundleType.VIP));
        this.RegisterListener(EventID.ActiveNoAds, (component, o) => DisableBundleType(BundleType.NoAds));
    }

    private IEnumerator DelayLayout()
    {
        yield return null;
        RebuildAllLayouts();
    }

    private void CheckBundleStatus()
    {
        var playerData = DataAccountPlayer.PlayerResourceData;
        // hasVip              = playerData.IsVipActive();
        // specialAdsAvailable = playerData.IsSpecialBundleStillAvailable();
    }

    private void InitBundles()
    {
        CheckBundleStatus();
        dataBundle.BuildCacheData();
        foreach (var bundle in shopBundles)
        {
            if (bundle == null)
            {
                continue;
            }

            var isBundleAvailable = dataBundle.IsBundleAvailable(bundle.bundleType);
            if (!isBundleAvailable)
            {
                bundle.gameObject.SetActive(false);
                continue;
            }
            // if (bundle.bundleType == BundleType.NoAds && hasNoAds)
            // {
            //     bundle.gameObject.SetActive(false);
            //     continue;
            // }

            // if (bundle.bundleType == BundleType.SpecialOffer && !specialAdsAvailable)
            // {
            //     bundle.gameObject.SetActive(false);
            //     continue;
            // }

            if (bundle.bundleType == BundleType.VIP && hasVip)
            {
                bundle.gameObject.SetActive(false);
            }

            bundle.InitData(dataBundle);
        }

        spawnComplete = true;
        RebuildAllLayouts();
    }

    public void DisableBundleType(BundleType type)
    {
        foreach (var bundle in shopBundles)
        {
            if (bundle.bundleType == type)
            {
                bundle.gameObject.SetActive(false);
            }
        }

        RebuildAllLayouts();
    }


    [Button]
    private void RebuildAllLayouts()
    {
        GameDebug.Log("rebuild");
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        ShopLayout.RebuildSections(GetActiveSections());
    }

    private IEnumerable<RectTransform> GetActiveSections()
    {
        foreach (var bundle in shopBundles)
        {
            if (bundle != null)
            {
                yield return bundle.transform as RectTransform;
            }
        }
    }
}