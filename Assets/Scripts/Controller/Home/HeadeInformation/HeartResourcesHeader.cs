using DataAccount;
using JinGroup.Common.ResourcesHeader;

namespace JinGroup.Common.HeadeInformation
{
    public class HeartResourcesHeader : BaseResourcesHeader
    {
        protected override void Awake()
        {
            base.Awake();
            this.RegisterListener(EventID.UpdateHeart, (sender, param) => UpdateValue());
        }

        protected override void UpdateValue()
        {
            valueInformation = DataAccountPlayer.PlayerResourceData.heart;
            base.UpdateValue();
        }
    }
}