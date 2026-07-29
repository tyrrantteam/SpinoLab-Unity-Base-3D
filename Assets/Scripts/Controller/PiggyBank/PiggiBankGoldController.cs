using DataAccount;
using JinGroup.Controller.PiggyBank;

public class PiggiBankGoldController : PiggyBankController
{
    
    protected override void InitData()
    {
        currentValue = DataAccountPlayer.PlayerPiggyBank.piggyBankGold;
        base.InitData();
    }

    protected override void OnClickPurchase()
    {
        base.OnClickPurchase();
        DataAccountPlayer.PlayerResourceData.ChangeGoldValue(currentValue);
    }
}
