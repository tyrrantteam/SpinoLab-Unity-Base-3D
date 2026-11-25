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
                _                    => 0
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
        public long firstOpenTime;

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

        #region Gold

        public void ChangeGoldValue(int value)
        {
            if (value > 0)
            {
                gold += (value * DataAccountPlayer.PlayerResourceData.GetMultiplyIncome());
            }
            else
            {
                gold += value;
            }

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

        public bool CheckNoAdsAvailable()
        {
            return IsNoAds24StillActive() || isNoAdsPurchase;
        }

        #endregion

        #region 1stOpen

        public void Change1stStatus(bool value)
        {
            isFirstTimeOpen = value;
            firstOpenTime   = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            DataAccountPlayer.SavePlayerResourceData();
        }

        #endregion

        #region Heart

        public long lastHeartRecoveryTime;
        public bool isHeartInitialized = false;

        #region Infinite Heart

        public long infiniteHeartExpireTime; // Unix timestamp (giây) thời điểm hết hạn vô hạn tim

        /// <summary>
        /// Kiểm tra hiện tại có đang trong thời gian Vô Hạn Tim hay không.
        /// </summary>
        public bool IsInfiniteHeartActive()
        {
            if (infiniteHeartExpireTime <= 0) return false;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now < infiniteHeartExpireTime;
        }

        public bool isInfiniteHeartActive => IsInfiniteHeartActive();

        /// <summary>
        /// Logic 1: Kích hoạt hoặc cộng dồn thời gian Vô Hạn Tim theo số ngày (1 ngày = 86400 giây).
        /// Trong quãng thời gian này, ChangeHeartValue(-1) sẽ không trừ vào tim thực.
        /// Sau khi hết số ngày active sẽ tự động trả về luồng tim bình thường.
        /// </summary>
        /// <param name="days">Số ngày vô hạn tim (ví dụ: 1, 3, 7...)</param>
        public void AddInfiniteHeartDays(int days)
        {
            if (days <= 0) return;
            AddInfiniteHeartSeconds((long)days * 86400);
        }

        public void AddInfiniteHeartDays(float days)
        {
            if (days <= 0f) return;
            AddInfiniteHeartSeconds((long)(days * 86400f));
        }

        /// <summary>
        /// Logic 2: Kích hoạt hoặc cộng dồn thời gian Vô Hạn Tim theo số giờ (1 giờ = 3600 giây).
        /// Trong X tiếng đó, ChangeHeartValue(-1) sẽ không có tác dụng. Khi hết giờ sẽ hoạt động lại bình thường.
        /// </summary>
        /// <param name="hours">Số giờ vô hạn tim (ví dụ: 1 tiếng, 2 tiếng, 0.5 tiếng...)</param>
        public void AddInfiniteHeartHours(int hours)
        {
            if (hours <= 0) return;
            AddInfiniteHeartSeconds((long)hours * 3600);
        }

        public void AddInfiniteHeartHours(float hours)
        {
            if (hours <= 0f) return;
            AddInfiniteHeartSeconds((long)(hours * 3600f));
        }

        /// <summary>
        /// Kích hoạt hoặc cộng dồn thời gian Vô Hạn Tim theo số giây.
        /// </summary>
        public void AddInfiniteHeartSeconds(long seconds)
        {
            if (seconds <= 0) return;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long baseTime = Math.Max(now, infiniteHeartExpireTime);
            infiniteHeartExpireTime = baseTime + seconds;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateHeart);
            }

            DataAccountPlayer.SavePlayerResourceData();
        }

        /// <summary>
        /// Thiết lập trạng thái Vô Hạn Tim (hỗ trợ bật/tắt hoặc đặt thời lượng cụ thể theo giây).
        /// </summary>
        public void SetInfiniteHeart(bool active, long durationSeconds = 0)
        {
            if (active)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                infiniteHeartExpireTime = now + Math.Max(0, durationSeconds);
            }
            else
            {
                infiniteHeartExpireTime = 0;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateHeart);
            }

            DataAccountPlayer.SavePlayerResourceData();
        }

        /// <summary>
        /// Lấy số giây còn lại của Vô Hạn Tim. Trả về 0 nếu đã hết hạn hoặc không kích hoạt.
        /// </summary>
        public long GetRemainingInfiniteHeartSeconds()
        {
            if (infiniteHeartExpireTime <= 0) return 0;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Math.Max(0, infiniteHeartExpireTime - now);
        }

        /// <summary>
        /// Lấy chuỗi định dạng thời gian còn lại của Vô Hạn Tim (vd: "01:23:45", "2d 05h").
        /// </summary>
        public string GetRemainingInfiniteHeartTimeString()
        {
            long remaining = GetRemainingInfiniteHeartSeconds();
            if (remaining <= 0) return "00m : 00s";

            long days = remaining / 86400;
            long hours = (remaining % 86400) / 3600;
            long minutes = (remaining % 3600) / 60;
            long seconds = remaining % 60;

            if (days > 0)
            {
                return $"{days:D2}days : {hours:D2}h : {minutes:D2}m";
            }

            if (hours > 0)
            {
                return $"{hours:D2}h : {minutes:D2}m : {seconds:D2}s";
            }

            return $"{minutes:D2}m : {seconds:D2}s";
        }

        #endregion

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
                heart                 = GetHeartLimit();
                isHeartInitialized    = true;
                lastHeartRecoveryTime = 0;
                DataAccountPlayer.SavePlayerResourceData();
            }
        }

        public void AddHeart(int value)
        {
            UpdateHeartRecovery();
            heart += value;
            int limit = GetHeartLimit();
            if (IsInfiniteHeartActive() || heart >= limit)
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
            if (IsInfiniteHeartActive())
            {
                // Khi đang trong thời gian Vô Hạn Tim: không trừ tim thực
                return;
            }

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
                if (IsInfiniteHeartActive())
                {
                    // Khi đang trong thời gian Vô Hạn Tim: không trừ tim thực
                    return;
                }

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
                    heart                 = limit;
                    lastHeartRecoveryTime = 0;
                }
                else
                {
                    heart                 += (int)heartsToAdd;
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

        /// <summary>
        /// Lấy tổng số giây còn lại cho tới khi hồi đầy tim (heart == limit).
        /// Trả về 0 nếu tim đã đầy hoặc vượt giới hạn.
        /// </summary>
        public long GetRemainingTimeToFullHeartSeconds()
        {
            UpdateHeartRecovery();

            int limit = GetHeartLimit();
            if (heart >= limit)
            {
                return 0;
            }

            int missingHearts = limit - heart;
            long nextHeartRemaining = GetRemainingRecoverySeconds();
            long recoveryInterval = (long)Mathf.Max(1, GetHeartRecoveryTime());

            return nextHeartRemaining + (long)(missingHearts - 1) * recoveryInterval;
        }

        public string GetHeartCooldownString()
        {
            float maxHeart = GetHeartLimit();
            if (IsInfiniteHeartActive() || heart >= maxHeart)
                return "Full";

            long secs = GetRemainingRecoverySeconds();
            long mm = secs / 60;
            long ss = secs % 60;
            return $"{mm:D2}:{ss:D2}";
        }

        /// <summary>
        /// Lấy chuỗi đếm ngược thời gian hồi tim tiếp theo.
        /// - Nếu đang Vô Hạn Tim: trả về chuỗi đếm ngược vô hạn tim (vd: 00h : 00m : 00s).
        /// - Nếu tim đã đầy (heart >= limit): trả về "Full".
        /// - Nếu đang hồi tim: trả về định dạng mm:ss (hoặc hh:mm:ss nếu >= 1 giờ).
        /// </summary>
        public string GetNextHeartRecoveryTimeString()
        {
            if (IsInfiniteHeartActive())
            {
                return GetRemainingInfiniteHeartTimeString();
            }

            int limit = GetHeartLimit();
            if (heart >= limit)
            {
                return "Full";
            }

            long remaining = GetRemainingRecoverySeconds();
            if (remaining >= 3600)
            {
                long hours = remaining / 3600;
                long minutes = (remaining % 3600) / 60;
                long seconds = remaining % 60;
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
            else
            {
                long minutes = remaining / 60;
                long seconds = remaining % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        #endregion

        #region Income

        public int GetMultiplyIncome()
        {
            if (!IsVipActive() || vipIncomeMultiplier < 2)
            {
                return 1;
            }

            return vipIncomeMultiplier;
        }

        #endregion

        #region Vip

        public bool isVipPurchase;
        public int  dayActive;
        public long vipTimestamp;
        public int  vipIncomeMultiplier;

        private const long VipDayMs = 24L * 60L * 60L * 1000L;

        public bool IsVipActive()
        {
            if (dayActive <= 0 || vipTimestamp <= 0)
            {
                return false;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now < vipTimestamp)
            {
                return true;
            }

            return now < vipTimestamp + dayActive * VipDayMs;
        }

        public void SaveTimeLineUnlock(int dayActiveData, int incomeMultiplier)
        {
            if (dayActiveData <= 0)
            {
                return;
            }

            incomeMultiplier = Mathf.Clamp(incomeMultiplier, 1, 4);

            if (IsVipActive())
            {
                dayActive += dayActiveData;
                if (incomeMultiplier > vipIncomeMultiplier)
                {
                    vipIncomeMultiplier = incomeMultiplier;
                }
            }
            else
            {
                dayActive           = dayActiveData;
                vipTimestamp        = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                vipIncomeMultiplier = incomeMultiplier;
            }

            isVipPurchase = true;
            DataAccountPlayer.SavePlayerResourceData();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostEvent(EventID.UpdateVipStatus);
            }
        }

        public long GetRemainingVipSeconds()
        {
            if (!IsVipActive())
            {
                return 0;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remainMs = vipTimestamp + dayActive * VipDayMs - now;
            return Math.Max(0, remainMs / 1000L);
        }

        public string GetRemainingVipTimeString()
        {
            long remaining = GetRemainingVipSeconds();
            if (remaining <= 0)
            {
                return "00m : 00s";
            }

            long days = remaining / 86400;
            long hours = (remaining % 86400) / 3600;
            long minutes = (remaining % 3600) / 60;
            long seconds = remaining % 60;

            if (days > 0)
            {
                return $"{days:D2}days : {hours:D2}h : {minutes:D2}m";
            }

            if (hours > 0)
            {
                return $"{hours:D2}h : {minutes:D2}m : {seconds:D2}s";
            }

            return $"{minutes:D2}m : {seconds:D2}s";
        }

        #endregion
    }
}