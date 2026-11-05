using JinGroup.Module.Reward;

public class NormalBundle : BundleWithRewards
{
    public ModuleRewardController goldNumber;
    public ModuleRewardController heartNumber;

    protected override ModuleRewardController GetGoldDisplay()  => goldNumber;
    protected override ModuleRewardController GetHeartDisplay() => heartNumber;
    protected override void OnGrantProducts() => GrantAllRewards();
}
