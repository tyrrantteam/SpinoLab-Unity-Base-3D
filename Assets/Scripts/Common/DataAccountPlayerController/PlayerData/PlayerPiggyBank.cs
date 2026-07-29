using System;

namespace DataAccount
{
    public class PlayerPiggyBank
    {
        public int piggyBankGem = 500;
        public int piggyBankGold = 100;

        public void ChangeValuePiggyBankGem(int value)
        {
            piggyBankGem += value;
            DataAccountPlayer.SavePlayerPiggyBankData();
        }

        public void ChangeValuePiggyBankGold(int value)
        {
            piggyBankGold += value;
            DataAccountPlayer.SavePlayerPiggyBankData();
        }

    }
}