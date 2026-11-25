using JinGroup.Module.Reward;

public class SpecialBundle : BundleWithRewards
{
    public ModuleRewardController goldNumber;

    protected override ModuleRewardController GetGoldDisplay() => goldNumber;
    protected override void OnGrantProducts() => GrantAllRewards();
}
