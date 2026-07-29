using System.Collections.Generic;

namespace DataAccount
{
    public class PlayerPointProcessData
    {
        public int pointPlayer;
        public int currentlevel = 1;
        public int currentlevelShowScreen = 1;
        public int progressConfigId;
        public int configCamera;
        public int lastMechanicProgressPercent;
        public List<int> shownMechanicUnlockIndices = new List<int>();
        public List<int> listLvLoop = new List<int>();

        public bool cheatUnlockAllBoosters;

        public void SetCheatUnlockAllBoosters(bool value)
        {
            cheatUnlockAllBoosters = value;
            DataAccountPlayer.SavePlayerPointProcessData();
        }

        public void MarkMechanicUnlockShown(int listIndex)
        {
            if (listIndex < 0)
                return;

            if (shownMechanicUnlockIndices == null)
                shownMechanicUnlockIndices = new List<int>();

            if (!shownMechanicUnlockIndices.Contains(listIndex))
                shownMechanicUnlockIndices.Add(listIndex);

            DataAccountPlayer.SavePlayerPointProcessData();
        }

        public void SetLastMechanicProgressPercent(int percent)
        {
            lastMechanicProgressPercent = percent;
            DataAccountPlayer.SavePlayerPointProcessData();
        }
        public void SetCurrrentDataShow(int lv)
        {
            currentlevelShowScreen = lv;
            DataAccountPlayer.SavePlayerPointProcessData();
        }
        public void SetConfigCam(int idCOnffig)
        {
            configCamera = idCOnffig;
            DataAccountPlayer.SavePlayerPointProcessData();
        }

        public void SetCurrentLevel(int level)
        {
            currentlevel = level;
            DataAccountPlayer.SavePlayerPointProcessData();
        }

        public void ChangeprogressConfigId()
        {
            progressConfigId += 1;
            DataAccountPlayer.SavePlayerPointProcessData();
        }

        public void WinLevel()
        {
            currentlevel += 1;
            currentlevelShowScreen += 1;
            DataAccountPlayer.SavePlayerPointProcessData();
        }

    }
}