using DataAccount;
using System;
using UnityEngine;

[Serializable]
public class PlayerLuckySpinData
{
    public int currentLevelProcess;

    public void IncreasecurrentLevelProcess()
    {
        currentLevelProcess += 1;
        DataAccountPlayer.SavePlayerLuckySpinData();
    }

    public void ResetProcess()
    {
        currentLevelProcess = 0;
        DataAccountPlayer.SavePlayerLuckySpinData();
    }
}
