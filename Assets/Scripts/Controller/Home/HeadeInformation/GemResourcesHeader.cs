using DataAccount;
using JinGroup.Common.ResourcesHeader;

public class GemResourcesHeader : BaseResourcesHeader
{
    protected override void Awake()
    {
        base.Awake();
        this.RegisterListener(EventID.UpdateGem, (sender, param) => UpdateValue());
    }

    protected override void UpdateValue()
    {
        valueInformation = DataAccountPlayer.PlayerResourceData.diamond;
        base.UpdateValue();
    }
}
