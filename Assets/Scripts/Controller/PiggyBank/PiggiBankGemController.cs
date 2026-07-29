using DataAccount;
using JinGroup.Controller.PiggyBank;

public class PiggiBankGemController : PiggyBankController
{
    protected override void InitData()
    {
        currentValue = DataAccountPlayer.PlayerPiggyBank.piggyBankGem;
        base.InitData();
    }

    protected override void OnClickPurchase()
    {
        base.OnClickPurchase();
        DataAccountPlayer.PlayerResourceData.ChangeDiamondValue(currentValue);
    }
}
