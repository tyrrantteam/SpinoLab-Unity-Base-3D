using UnityEngine;

namespace Controller.Shop
{
    public class ShopManagerWinLose : ShopManager
    {
        protected override void Awake()
        {
        }

        public void InitBundles(BundleType type)
        {
            Debug.Log("init bundles");
            foreach (var bundle in shopBundles)
            {
                if (bundle.bundleType != type)
                {
                    bundle.gameObject.SetActive(false);
                    continue;
                }

                // check bundle co available
                var isBundleAvailable = dataBundle.IsBundleAvailable(bundle.bundleType);
                if (!isBundleAvailable)
                {
                    bundle.gameObject.SetActive(false);
                    continue;
                }

                bundle.InitData(dataBundle, settings);
            }
        }
    }
}