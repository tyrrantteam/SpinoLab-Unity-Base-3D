using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JinGroup.Common.UIBaseController
{
    public class GroupBundlePackGoldController : MonoBehaviour
    {
        [SerializeField] private BaseGroupPackBundle[] bundleElement;
        [HideInInspector] public List<BundleReward> listBundlePackData;

        private void Awake()
        {
            InitData();
        }

        private void InitData()
        {
            listBundlePackData = LoadResourceController.Instance.DataBundleController().ContentContent.bundle_packGold;
            for (int i = 0; i < listBundlePackData.Count; i++)
            {
                bundleElement[i].idPack = i;
                bundleElement[i].listBundlePackData = listBundlePackData;
                bundleElement[i].InitData();
            }
        }
    }
}