using DataAccount;
using JinGroup.Common.ResourcesHeader;


public class GoldResourcesHeader : BaseResourcesHeader
{
    protected override void Awake()
    {
        base.Awake();
        this.RegisterListener(EventID.UpdateGold, (sender, param) => UpdateValue());
    }

    protected override void UpdateValue()
    {
        valueInformation = DataAccountPlayer.PlayerResourceData.gold;
        ChangeValueWithAnimation();
    }


}
