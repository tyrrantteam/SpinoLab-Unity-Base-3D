using Base.Core.Sound;
using JinGroup.Module.Resources;
using System.Collections.Generic;
using System.IO;
using Unity.Burst.Intrinsics;
using UnityEditor.UI;
using UnityEngine;

namespace JinGroup.Base.LoadData
{
    public class LoadResourceController : SingletonMono<LoadResourceController>
    {
        private Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();

        #region LoadMethod
        public MapManager LevelGame(int level)
        {
            var path = string.Format(ResourcesFolderPath.Level);
            return Load<MapManager>(path, "Level" + level);
        }

        private T Load<T>(string path, string fileName) where T : Object
        {
            var fullPath = Path.Combine(path, fileName);
            if (_resourceCache.ContainsKey(fullPath) is false)
            {
                _resourceCache.Add(fullPath, TryToLoad<T>(path, fileName));
            }

            return _resourceCache[fullPath] as T;
        }

        private static T TryToLoad<T>(string path, string fileName) where T : Object
        {
            var fullPath = Path.Combine(path, fileName);
            var result = Resources.Load<T>(fullPath);
            return result;
        }

        #endregion

        #region Public Load Method

        public Sprite LoadItemIcon( TypeResources moneyType)
        {
            var path = string.Format(ResourcesFolderPath.SpriteFolder, ResourcesFolderPath.MONEYTYPE);
            return Load<Sprite>(path, moneyType.ToString());
        }

        public TypeRarity loadRarityStaticByType( TypeResources rarityType)
        {
            switch (rarityType)
            {
                case TypeResources.gold:
                    return TypeRarity.good;
               case TypeResources.diamond:
                   return TypeRarity.excellent;
                default:
                    return TypeRarity.normal;
            }
        }
        public DataProcessMechanic LoadDataProcessMechanic()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataProcessMechanic>(path, "DataProcess");
        }

        public DataTutorialController LoadDataTutorial()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataTutorialController>(path, "DataTutorial");
        }

        public Sprite LoadRanking(int id)
        {
            var path = string.Format(ResourcesFolderPath.SpriteFolder, ResourcesFolderPath.RANKING);
            return Load<Sprite>(path, id.ToString());
        }

        public Sprite LoadAvatar(int id)
        {
            var path = string.Format(ResourcesFolderPath.SpriteFolder, ResourcesFolderPath.AVATAR);
            return Load<Sprite>(path, id.ToString());
        }

        public Sprite LoadBackGroundIcon(TypeRarity rarityType)
        {
            var path = string.Format(ResourcesFolderPath.SpriteFolder, ResourcesFolderPath.BACKGROUND);
            return Load<Sprite>(path, rarityType.ToString());
        }

        public AudioClip LoadAudioClip(SoundType soundType)
        {
            return Load<AudioClip>(ResourcesFolderPath.SoundFolder, soundType.ToString());
        }

        //public string loadpathdatacharacter(skintype skin)
        //{
        //    var path = path.combine(resourcesfolderpath.herofolder, "character/");
        //    return (path + skin.tostring());
        //}


        #endregion

        #region LoadDataAsset


        //public StatUpgradeDataAsset LoadStatUpgradeData()
        //{
        //    var path = string.Format(ResourcesFolderPath.DataFolder, ResourcesFolderPath.DataFolderStatUpgrade);
        //    return Load<StatUpgradeDataAsset>(path, "StatUpgradeData");
        //}

        public DataItemController DataItemController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataItemController>(path, "DataItem");
        }
        public DataImageHand DataImageHand()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataImageHand>(path, "DataImageHand");
        }

        public DataBundleController DataBundleController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataBundleController>(path, "DataBundle");
        }

        public DataRewardController DataRewardController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataRewardController>(path, "DataRewardPopup");
        }

        public DataPiggyBankController DataPiggyBankController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataPiggyBankController>(path, "DataPiggyBank");
        }

        public DataRankingController DataRankingController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataRankingController>(path, "DataRanking");
        }
        public DataBoosterController DataBoosterController()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataBoosterController>(path, "DataBooster");
        }

        public DataProcessMechanic DataProcessMechanic()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataProcessMechanic>(path, "DataProcess");
        }

        public GameConfig GameConfig()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<GameConfig>(path, "GameConfig");
        }

        public SpecialOfferAds SpecialOfferAds()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<SpecialOfferAds>(path, "SpecialOfferAds");
        }

        public DataDailyStreak DataDailyStreak()
        {
            var path = string.Format(ResourcesFolderPath.DataFolder);
            return Load<DataDailyStreak>(path, "DataDailyStreak");
        }
        #endregion
    }
}
