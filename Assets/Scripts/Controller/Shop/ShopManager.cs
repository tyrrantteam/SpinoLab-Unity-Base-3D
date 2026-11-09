using System.Collections;
using System.Collections.Generic;
using DataAccount;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Base.Core.Debug;

public class ShopManager : MonoBehaviour
{
    public DataBundleController      dataBundle;
    public GameDebugSettings         settings;
    public List<ShopBundleContainer> shopBundles;
    
    [SerializeField] private float spawnDelay;

    private bool spawnComplete;

    private bool hasNoAds;
    private bool specialAdsAvailable;

    protected virtual void Awake()
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

        this.RegisterListener(EventID.ActiveNoAds, (component, o) => DisableBundleType(BundleType.NoAds));
        this.RegisterListener(EventID.HideSpecialBundle, (component, o) => DisableBundleType(BundleType.SpecialOffer));
    }

    private IEnumerator DelayLayout()
    {
        yield return null;
        RebuildAllLayouts();
    }

    private void CheckBundleStatus()
    {
        var playerData = DataAccountPlayer.PlayerResourceData;
        var playerShopData = DataAccountPlayer.PlayerShopData;
        hasNoAds = playerData.CheckNoAdsAvailable();
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

            // check bundle co available
            var isBundleAvailable = dataBundle.IsBundleAvailable(bundle.bundleType);
            if (!isBundleAvailable)
            {
                bundle.gameObject.SetActive(false);
                continue;
            }

            // check noads de hien bundle nodads
            if (bundle.bundleType == BundleType.NoAds && hasNoAds)
            {
                bundle.gameObject.SetActive(false);
                continue;
            }

            // check special offer de hien bundle special offer
            
            bundle.InitData(dataBundle, settings);
        }

        spawnComplete = true;
        RebuildAllLayouts();
    }

    public void DisableBundleType(BundleType type)
    {
        GameDebug.Log("Disable pack");
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