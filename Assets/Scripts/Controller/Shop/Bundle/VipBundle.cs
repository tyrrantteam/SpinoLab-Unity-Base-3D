using System.Collections.Generic;
using JinGroup.Module.Reward;
using TMPro;

public class VipBundle : BundleWithRewards
{
    public ModuleRewardController incomes;
    public ModuleRewardController goldNumber;
    public ModuleRewardController slotDocker;
    public TMP_Text               vipTextHeader;

    protected override ModuleRewardController GetGoldDisplay() => goldNumber;
    protected override void OnGrantProducts() => GrantAllRewards();

    protected override void OnBeforeParseRewards(List<BundleReward> reward)
    {
        if (vipTextHeader != null)
        {
            vipTextHeader.text = reward[0].name;
        }
    }
}
