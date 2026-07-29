using System;

namespace DataAccount
{
    public enum StreakCheatMode
    {
        None = 0,
        MoveToDay = 1,
        FinishStreak = 2,
    }

    public class PlayerStreakData
    {
        public int currentStreakDay;
        public DateTime lastCheckinDate = DateTime.UnixEpoch;
        public bool pendingShowOnNextLevel;
        public bool hasClaimedFinalReward;

        public StreakCheatMode cheatMode;
        public int cheatTargetDay;

        public void MarkPendingShow()
        {
            pendingShowOnNextLevel = true;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void ClearPendingShow()
        {
            pendingShowOnNextLevel = false;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void CompleteDayCheckin(int day)
        {
            currentStreakDay = day;
            lastCheckinDate = DateTime.UtcNow;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void MarkFinalRewardClaimed()
        {
            hasClaimedFinalReward = true;
            currentStreakDay = 0;
            lastCheckinDate = DateTime.UnixEpoch;
            pendingShowOnNextLevel = false;
            cheatMode = StreakCheatMode.None;
            cheatTargetDay = 0;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void ResetStreak()
        {
            currentStreakDay = 0;
            lastCheckinDate = DateTime.UnixEpoch;
            pendingShowOnNextLevel = false;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void SetCheatMoveToDay(int targetDay)
        {
            cheatMode = StreakCheatMode.MoveToDay;
            cheatTargetDay = targetDay;
            hasClaimedFinalReward = false;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void SetCheatFinishStreak()
        {
            cheatMode = StreakCheatMode.FinishStreak;
            cheatTargetDay = 0;
            hasClaimedFinalReward = false;
            DataAccountPlayer.SavePlayerStreakData();
        }

        public void ClearCheatMode()
        {
            cheatMode = StreakCheatMode.None;
            cheatTargetDay = 0;
            DataAccountPlayer.SavePlayerStreakData();
        }
    }
}
