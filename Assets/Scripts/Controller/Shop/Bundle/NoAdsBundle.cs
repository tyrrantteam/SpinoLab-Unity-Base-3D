using DataAccount;
using JinGroup.Module.Reward;

public class NoAdsBundle : BundleWithRewards
{
    public ModuleRewardController goldNumber;

    protected override ModuleRewardController GetGoldDisplay() => goldNumber;
    protected override void OnGrantProducts() => GrantAllRewards();

    protected override void GrantAllRewards()
    {
        base.GrantAllRewards();

        long twentyFourHoursInMillis = 24 * 60 * 60 * 1000;
        var  bundleName              = _bundleRewards[0].idBundle;

        if (bundleName == "com.noads24h.pack")
        {
            DataAccountPlayer.PlayerResourceData.ChangeNoAds24hStatus(twentyFourHoursInMillis);
        }
        else
        {
            DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);
        }

        DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);
    }
}
