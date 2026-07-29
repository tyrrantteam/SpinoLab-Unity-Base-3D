
using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections.Generic;

namespace JinGroup.Common.UIBaseController
{
    public class BundlePurchasePack1Controller : BaseBundlePurchaseController
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitData()
        {
            listBundlePackData = LoadResourceController.Instance.DataBundleController().ContentContent.listBundlePack1;
            base.InitData();
            CreateReward();
        }

        protected override void OnClickPurchase()
        {
            base.OnClickPurchase();
        }
    }
}