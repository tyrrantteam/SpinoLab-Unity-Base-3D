using Base.Core;
using JinGroup.Base.LoadData;
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
        public int heart;
        
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

        #region Heart
        public long lastHeartRecoveryTime;
        public bool isHeartInitialized = false;

        public int GetHeartLimit()
        {
            var dataHeart = LoadResourceController.Instance != null ? LoadResourceController.Instance.DataHeartController() : null;
            return dataHeart != null ? dataHeart.Limit : 5;
        }

        public float GetHeartRecoveryTime()
        {
            var dataHeart = LoadResourceController.Instance != null ? LoadResourceController.Instance.DataHeartController() : null;
            return dataHeart != null ? dataHeart.RecoveryTime : 600f;
        }

        public void CheckInitHeartFirstTime()
        {
            if (isFirstTimeOpen || !isHeartInitialized)
            {
                heart = GetHeartLimit();
                isHeartInitialized = true;
                lastHeartRecoveryTime = 0;
                DataAccountPlayer.SavePlayerResourceData();
            }
        }

        public void AddHeart(int value)
        {
            UpdateHeartRecovery();
            heart += value;
            int limit = GetHeartLimit();
            if (heart >= limit)
            {
                lastHeartRecoveryTime = 0;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateHeart);
            }
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void SubtractHeart(int value)
        {
            UpdateHeartRecovery();
            int limit = GetHeartLimit();
            heart = Mathf.Max(0, heart - value);
            if (heart < limit && lastHeartRecoveryTime <= 0)
            {
                lastHeartRecoveryTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateHeart);
            }
            DataAccountPlayer.SavePlayerResourceData();
        }

        public void ChangeHeartValue(int value)
        {
            if (value >= 0)
            {
                AddHeart(value);
            }
            else
            {
                SubtractHeart(-value);
            }
        }

        public void SetHeartValue(int value)
        {
            int limit = GetHeartLimit();
            heart = Mathf.Max(0, value);
            if (heart >= limit)
            {
                lastHeartRecoveryTime = 0;
            }
            else if (lastHeartRecoveryTime <= 0)
            {
                lastHeartRecoveryTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateHeart);
            }
            DataAccountPlayer.SavePlayerResourceData();
        }

        public bool UpdateHeartRecovery()
        {
            int limit = GetHeartLimit();
            if (heart >= limit)
            {
                if (lastHeartRecoveryTime != 0)
                {
                    lastHeartRecoveryTime = 0;
                    DataAccountPlayer.SavePlayerResourceData();
                }
                return false;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (lastHeartRecoveryTime <= 0)
            {
                lastHeartRecoveryTime = now;
                DataAccountPlayer.SavePlayerResourceData();
                return false;
            }

            // System clock moved backward safeguard
            if (lastHeartRecoveryTime > now)
            {
                lastHeartRecoveryTime = now;
                DataAccountPlayer.SavePlayerResourceData();
                return false;
            }

            long recoveryInterval = (long)Mathf.Max(1, GetHeartRecoveryTime());
            long elapsed = now - lastHeartRecoveryTime;
            long heartsToAdd = elapsed / recoveryInterval;

            if (heartsToAdd > 0)
            {
                int missingHearts = limit - heart;
                if (heartsToAdd >= missingHearts)
                {
                    heart = limit;
                    lastHeartRecoveryTime = 0;
                }
                else
                {
                    heart += (int)heartsToAdd;
                    lastHeartRecoveryTime += heartsToAdd * recoveryInterval;
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PostEvent(EventID.UpdateHeart);
                }
                DataAccountPlayer.SavePlayerResourceData();
                return true;
            }

            return false;
        }

        public long GetRemainingRecoverySeconds()
        {
            UpdateHeartRecovery();

            int limit = GetHeartLimit();
            if (heart >= limit)
            {
                return 0;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (lastHeartRecoveryTime <= 0)
            {
                lastHeartRecoveryTime = now;
                DataAccountPlayer.SavePlayerResourceData();
            }

            long recoveryInterval = (long)Mathf.Max(1, GetHeartRecoveryTime());
            long elapsed = now - lastHeartRecoveryTime;
            long remaining = recoveryInterval - (elapsed % recoveryInterval);
            return Mathf.Max(0, (int)remaining);
        }
        #endregion
    }
}