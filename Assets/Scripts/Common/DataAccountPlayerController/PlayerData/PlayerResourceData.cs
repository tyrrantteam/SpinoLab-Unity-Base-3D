using Base.Core;
using System;
using UnityEngine;

namespace DataAccount
{
    public class PlayerResourceData 
    {
        //resource
        public int gold;
        public int diamond;
        public int skipAds;
        //Booster
        public int booster1;
        public int booster2;
        public int booster3;
        public int booster4;

        public int GetBoosterCount(BoosterType type)
        {
            return type switch
            {
                BoosterType.booster1 => booster1,
                BoosterType.booster2 => booster2,
                BoosterType.booster3 => booster3,
                BoosterType.booster4 => booster4,
                _ => 0
            };
        }

        public void SetBoosterCount(BoosterType type, int value)
        {
            value = Mathf.Max(0, value);
            switch (type)
            {
                case BoosterType.booster1:
                    booster1 = value;
                    break;
                case BoosterType.booster2:
                    booster2 = value;
                    break;
                case BoosterType.booster3:
                    booster3 = value;
                    break;
                case BoosterType.booster4:
                    booster4 = value;
                    break;
            }

            DataAccountPlayer.SavePlayerResourceData();
        }

        public void ChangeBoosterCount(BoosterType type, int delta)
        {
            SetBoosterCount(type, GetBoosterCount(type) + delta);
        }

        //NoAds
        public bool isNoAdsPurchase = false;
        public long isNoAdsPurchase24h;

        //1st time data
        public bool isFirstTimeOpen = true;

        #region skipAds
        public void SetSkipAdsValue(int value)
        {
            skipAds = value;
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void ChangeSkipAdsValue(int value)
        {
            skipAds += value;
            DataAccountPlayer.SavePlayerResourceData();
        }

        #endregion

        #region diamond
        public void SetDiamondValue(int value)
        {
            diamond = value;
            GameManager.Instance.PostEvent(EventID.UpdateGem);
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void ChangeDiamondValue(int value)
        {
            diamond += value;
            GameManager.Instance.PostEvent(EventID.UpdateGem);
            DataAccountPlayer.SavePlayerResourceData();
        }

        #endregion

        # region Gold

        public void ChangeGoldValue(int value)
        {
            gold += value;
            GameManager.Instance.PostEvent(EventID.UpdateGold);
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void SetGoldValue(int value)
        {
            gold = value;
            GameManager.Instance.PostEvent(EventID.UpdateGold);
            DataAccountPlayer.SavePlayerResourceData();
        }
        #endregion

        #region no Ads
        public void ChangeNoAdsStatus(bool value)
        {
            isNoAdsPurchase = value;
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void ChangeNoAds24hStatus(long value)
        {
            isNoAdsPurchase24h = value;
            DataAccountPlayer.SavePlayerResourceData();
        }

        public bool IsNoAds24StillActive()
        {
            // Thời gian hiện tại (Unix time tính bằng milliseconds)
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // 24 giờ = 24 * 60 * 60 * 1000 milliseconds
            long twentyFourHoursInMillis = 24 * 60 * 60 * 1000;

            // So sánh thời gian hiện tại với thời điểm lưu
            return (currentTime - isNoAdsPurchase24h) < twentyFourHoursInMillis;
        }

        #endregion

        #region 1stOpen
        public void Change1stStatus(bool value)
        {
            isFirstTimeOpen = value;
            DataAccountPlayer.SavePlayerResourceData();
        }
        #endregion
    }
}