using Base.Core.Debug;
using DataAccount;

public class AnlyticManager : SingletonMono<AnlyticManager>
{
    #region In game event
    public void LogEventLevelStart()
    {
       var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
       GameDebug.Log("Level started: " + levelInfor);
    }

    public void LogEventLevelComplete()
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Level completed: " + levelInfor);
    }

    public void LogEventLevelFailed()
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Level failed: " + levelInfor);
    }


    public void LogEventLevelTryAgain()
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Try again Level: " + levelInfor);
    }


    public void BoosterUsing(BoosterType boosterType)
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log($"Booster used: {boosterType} at level {levelInfor}");
    }

    public void BoosterClaim(BoosterType boosterType)
    {
        GameDebug.Log($"Booster Claim: {boosterType}");
    }


    #endregion

    #region Tutorial event
    public void LogEventTutorialStart()
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Tutorial started: " + levelInfor);
    }

    public void LogEventTutorialStep(int step)
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Tutorial step " + levelInfor + "Step : " + step);
    }

    public void LogEventTutorialComplete()
    {
        var levelInfor = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        GameDebug.Log("Tutorial completed: " + levelInfor);
    }

    
    #endregion
}
