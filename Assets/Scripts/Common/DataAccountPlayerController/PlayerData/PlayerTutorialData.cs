using System.Collections.Generic;

namespace DataAccount
{
    public class PlayerTutorialData
    {
        public int tutorialStep = 1;

        public void SetTutorialStep(int step)
        {
                tutorialStep = step;
                DataAccountPlayer.SavePlayerTutorialData();
        }

        public void NextTutorialStep()
        {
                tutorialStep += 1;
                DataAccountPlayer.SavePlayerTutorialData();
        }
    }
}