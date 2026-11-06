using System;
using Base.Core.Debug;

namespace DataAccount
{
    public class PlayerShopData
    {
        public int starterPackPurchase;

        /// <summary>Unix ms lúc lần đầu đạt levelUnlock starter pack. 0 = chưa unlock.</summary>
        public long starterPackUnlockTimestamp;

        #region Special Bundle

        /// <summary>Ghi timestamp lần đầu đạt levelUnlock (chỉ set 1 lần).</summary>
        public void EnsureStarterPackUnlockTimestamp(int levelUnlock, int currentLevel)
        {
            if (starterPackUnlockTimestamp > 0)
                return;
            if (currentLevel < levelUnlock)
                return;

            starterPackUnlockTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            DataAccountPlayer.SaveShopData();
        }

        /// <summary>
        /// Starter pack còn hiện: đủ level, chưa mua hết limit, và còn trong hourActive kể từ lúc unlock.
        /// hourActive ≤ 0 = không giới hạn thời gian.
        /// </summary>
        public bool IsStarterPackActive(int levelUnlock, int hourActive, int limitPurchase, int currentLevel)
        {
            if (limitPurchase > 0 && starterPackPurchase >= limitPurchase)
                return false;
            if (currentLevel < levelUnlock)
                return false;

            EnsureStarterPackUnlockTimestamp(levelUnlock, currentLevel);

            if (hourActive <= 0)
                return true;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long activeMs = (long)hourActive * 60L * 60L * 1000L;
            return (currentTime - starterPackUnlockTimestamp) < activeMs;
        }

        public TimeSpan GetStarterPackRemaining(int hourActive)
        {
            if (starterPackUnlockTimestamp <= 0 || hourActive <= 0)
                return TimeSpan.Zero;

            long endMs = starterPackUnlockTimestamp + (long)hourActive * 60L * 60L * 1000L;
            long remainMs = endMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return remainMs <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(remainMs);
        }
        

        #endregion
    }
}