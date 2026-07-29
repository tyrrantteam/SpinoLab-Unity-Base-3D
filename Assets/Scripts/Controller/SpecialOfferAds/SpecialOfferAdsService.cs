using DataAccount;
using JinGroup.Base.LoadData;

public static class SpecialOfferAdsService
{
    public static void RecordEmptyBoosterAttempt(BoosterType boosterType)
    {
        if (boosterType == BoosterType.None)
            return;

        if (DataAccountPlayer.PlayerResourceData.GetBoosterCount(boosterType) > 0)
            return;

        DataAccountPlayer.PlayerSpecialOfferAdsData.AddPoint(boosterType);
    }

    public static void OnBoosterUnlocked(BoosterType boosterType)
    {
        int showLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        DataAccountPlayer.PlayerSpecialOfferAdsData.OnBoosterUnlocked(boosterType, showLevel);
    }

    public static void OnLevelWon()
    {
        DataAccountPlayer.PlayerSpecialOfferAdsData.OnLevelWon();
    }

    public static bool TryShowPopup()
    {
        var config = LoadResourceController.Instance.SpecialOfferAds();
        if (config == null || !config.isActive)
            return false;

        if (PopupManager.IsNull)
            return false;

        var playerData = DataAccountPlayer.PlayerSpecialOfferAdsData;
        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;

        if (!TryResolveBoosterToShow(config, playerData, currentLevel, out var boosterType))
            return false;

        if (!config.TryGetEntry(boosterType, out var entry))
            return false;

        var popup = PopupManager.Instance.ShowPopup<PopupSpecialOfferAds>();
        if (popup == null)
            return false;

        popup.Setup(entry);
        return true;
    }

    public static void OnOfferClaimed(BoosterType boosterType)
    {
        DataAccountPlayer.PlayerSpecialOfferAdsData.OnOfferClaimed(boosterType);
    }

    public static void OnOfferDismissed()
    {
        var config = LoadResourceController.Instance.SpecialOfferAds();
        int activeAfterLevel = config != null ? config.activeAfterLevel : 0;
        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        DataAccountPlayer.PlayerSpecialOfferAdsData.OnOfferDismissed(currentLevel, activeAfterLevel);
    }

    private static bool TryResolveBoosterToShow(
        SpecialOfferAds config,
        PlayerSpecialOfferAdsData playerData,
        int currentLevel,
        out BoosterType boosterType)
    {
        boosterType = BoosterType.None;

        if (playerData.priorityBoosterType != BoosterType.None)
        {
            var unlockEntry = playerData.GetUnlockEntry(playerData.priorityBoosterType);
            if (unlockEntry != null
                && !unlockEntry.initialOfferShown
                && unlockEntry.winsSinceUnlock >= config.activeAfterLevel)
            {
                boosterType = playerData.priorityBoosterType;
                return true;
            }
        }

        if (playerData.waitingRetryAfterDismiss && currentLevel >= playerData.triggerLevelAfterDismiss)
        {
            boosterType = playerData.GetHighestPointBoosterType(config);
            return boosterType != BoosterType.None;
        }

        return false;
    }
}
