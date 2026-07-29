using System;
using System.Collections.Generic;

namespace DataAccount
{
    [Serializable]
    public class BoosterOfferPointEntry
    {
        public BoosterType boosterType;
        public int points;
    }

    [Serializable]
    public class BoosterUnlockOfferEntry
    {
        public BoosterType boosterType;
        public int unlockedAtShowLevel;
        public int winsSinceUnlock;
        public bool initialOfferShown;
    }

    public class PlayerSpecialOfferAdsData
    {
        public List<BoosterOfferPointEntry> offerPoints = new List<BoosterOfferPointEntry>();
        public List<BoosterUnlockOfferEntry> unlockEntries = new List<BoosterUnlockOfferEntry>();

        public BoosterType priorityBoosterType = BoosterType.None;
        public bool waitingRetryAfterDismiss;
        public int triggerLevelAfterDismiss;

        public int GetPoints(BoosterType boosterType)
        {
            for (int i = 0; i < offerPoints.Count; i++)
            {
                if (offerPoints[i].boosterType == boosterType)
                    return offerPoints[i].points;
            }

            return 0;
        }

        public void AddPoint(BoosterType boosterType)
        {
            if (boosterType == BoosterType.None)
                return;

            for (int i = 0; i < offerPoints.Count; i++)
            {
                if (offerPoints[i].boosterType == boosterType)
                {
                    offerPoints[i].points++;
                    DataAccountPlayer.SavePlayerSpecialOfferAdsData();
                    return;
                }
            }

            offerPoints.Add(new BoosterOfferPointEntry
            {
                boosterType = boosterType,
                points = 1
            });
            DataAccountPlayer.SavePlayerSpecialOfferAdsData();
        }

        public BoosterUnlockOfferEntry GetUnlockEntry(BoosterType boosterType)
        {
            for (int i = 0; i < unlockEntries.Count; i++)
            {
                if (unlockEntries[i].boosterType == boosterType)
                    return unlockEntries[i];
            }

            return null;
        }

        public void OnBoosterUnlocked(BoosterType boosterType, int showLevel)
        {
            if (boosterType == BoosterType.None)
                return;

            if (GetUnlockEntry(boosterType) != null)
                return;

            unlockEntries.Add(new BoosterUnlockOfferEntry
            {
                boosterType = boosterType,
                unlockedAtShowLevel = showLevel,
                winsSinceUnlock = 0,
                initialOfferShown = false
            });

            priorityBoosterType = boosterType;
            waitingRetryAfterDismiss = false;
            DataAccountPlayer.SavePlayerSpecialOfferAdsData();
        }

        public void OnLevelWon()
        {
            bool changed = false;

            for (int i = 0; i < unlockEntries.Count; i++)
            {
                var entry = unlockEntries[i];
                if (!entry.initialOfferShown)
                {
                    entry.winsSinceUnlock++;
                    changed = true;
                }
            }

            if (changed)
                DataAccountPlayer.SavePlayerSpecialOfferAdsData();
        }

        public void OnOfferClaimed(BoosterType boosterType)
        {
            var unlockEntry = GetUnlockEntry(boosterType);
            if (unlockEntry != null)
                unlockEntry.initialOfferShown = true;

            if (priorityBoosterType == boosterType)
                priorityBoosterType = BoosterType.None;

            waitingRetryAfterDismiss = false;
            DataAccountPlayer.SavePlayerSpecialOfferAdsData();
        }

        public void OnOfferDismissed(int currentShowLevel, int activeAfterLevel)
        {
            var unlockEntry = GetUnlockEntry(priorityBoosterType);
            if (unlockEntry != null)
                unlockEntry.initialOfferShown = true;

            priorityBoosterType = BoosterType.None;
            waitingRetryAfterDismiss = true;
            triggerLevelAfterDismiss = currentShowLevel + activeAfterLevel;
            DataAccountPlayer.SavePlayerSpecialOfferAdsData();
        }

        public BoosterType GetHighestPointBoosterType(SpecialOfferAds config)
        {
            if (config == null || config.entries == null || config.entries.Count == 0)
                return BoosterType.None;

            BoosterType bestType = BoosterType.None;
            int bestPoints = int.MinValue;

            for (int i = 0; i < config.entries.Count; i++)
            {
                var boosterType = config.entries[i].boosterType;
                int points = GetPoints(boosterType);

                if (points > bestPoints)
                {
                    bestPoints = points;
                    bestType = boosterType;
                }
            }

            return bestType;
        }
    }
}
