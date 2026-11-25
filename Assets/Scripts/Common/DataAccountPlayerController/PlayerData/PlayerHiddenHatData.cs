using System;
using System.Collections.Generic;

namespace DataAccount
{
    [Serializable]
    public class PlayerHiddenHatData
    {
        public int currentGroupId;
        public bool hasActiveGroup;
        public bool isBoardRevealed;
        public List<int> usedGroupIds = new List<int>();
        public List<int> shuffledRewardIndices = new List<int>();
        public List<int> claimedElementIndices = new List<int>();
        public long startTimeLocalTicks;
        public string startDateLocal;
        public float resetDurationHours = 24f;

        /// <summary>
        /// Kiểm tra đã quá 24 tiếng (hoặc resetDurationHours) tính theo thời gian địa phương hay chưa.
        /// Nếu đã hết hạn thì reset chu kỳ hiện tại và trả về true.
        /// </summary>
        public bool CheckAndResetIfExpired()
        {
            if (!hasActiveGroup)
            {
                return false;
            }

            if (startTimeLocalTicks <= 0)
            {
                ResetCurrentCycle();
                return true;
            }

            DateTime nowLocal = DateTime.Now;
            DateTime startLocal = new DateTime(startTimeLocalTicks, DateTimeKind.Local);

            // Bảo vệ nếu đồng hồ máy bị chỉnh lùi về trước mốc lưu
            if (nowLocal < startLocal)
            {
                startTimeLocalTicks = nowLocal.Ticks;
                startDateLocal = nowLocal.ToString("yyyy-MM-dd HH:mm:ss");
                DataAccountPlayer.SavePlayerHiddenHatData();
                return false;
            }

            float hours = resetDurationHours > 0f ? resetDurationHours : 24f;
            if ((nowLocal - startLocal) >= TimeSpan.FromHours(hours))
            {
                ResetCurrentCycle();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lấy thời gian đếm ngược còn lại trước khi reset theo giờ địa phương.
        /// </summary>
        public TimeSpan GetRemainingTime()
        {
            float hours = resetDurationHours > 0f ? resetDurationHours : 24f;
            if (!hasActiveGroup || startTimeLocalTicks <= 0)
            {
                return TimeSpan.FromHours(hours);
            }

            DateTime nowLocal = DateTime.Now;
            DateTime startLocal = new DateTime(startTimeLocalTicks, DateTimeKind.Local);

            if (nowLocal < startLocal)
            {
                return TimeSpan.FromHours(hours);
            }

            DateTime expireLocal = startLocal.AddHours(hours);
            TimeSpan remaining = expireLocal - nowLocal;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Gán group ID mới cho chu kỳ hiện tại, lưu vào usedGroupIds để không bị random trùng
        /// và ghi nhận mốc thời gian hiện tại theo ngày giờ địa phương.
        /// </summary>
        public void SetCurrentGroup(int groupId, float durationHours = 24f)
        {
            currentGroupId = groupId;
            hasActiveGroup = true;
            isBoardRevealed = false;

            if (usedGroupIds == null)
                usedGroupIds = new List<int>();
            if (!usedGroupIds.Contains(groupId))
                usedGroupIds.Add(groupId);

            if (shuffledRewardIndices == null)
                shuffledRewardIndices = new List<int>();
            else
                shuffledRewardIndices.Clear();

            if (claimedElementIndices == null)
                claimedElementIndices = new List<int>();
            else
                claimedElementIndices.Clear();

            DateTime nowLocal = DateTime.Now;
            startTimeLocalTicks = nowLocal.Ticks;
            startDateLocal = nowLocal.ToString("yyyy-MM-dd HH:mm:ss");
            resetDurationHours = durationHours > 0f ? durationHours : 24f;

            DataAccountPlayer.SavePlayerHiddenHatData();
        }

        public void SetShuffledIndices(List<int> indices)
        {
            shuffledRewardIndices = indices != null ? new List<int>(indices) : new List<int>();
            DataAccountPlayer.SavePlayerHiddenHatData();
        }

        public void SetBoardRevealed(bool revealed)
        {
            if (isBoardRevealed == revealed)
                return;

            isBoardRevealed = revealed;
            DataAccountPlayer.SavePlayerHiddenHatData();
        }

        public bool IsElementClaimed(int elementIndex)
        {
            return claimedElementIndices != null && claimedElementIndices.Contains(elementIndex);
        }

        public void MarkElementClaimed(int elementIndex)
        {
            if (claimedElementIndices == null)
                claimedElementIndices = new List<int>();

            if (!claimedElementIndices.Contains(elementIndex))
            {
                claimedElementIndices.Add(elementIndex);
                DataAccountPlayer.SavePlayerHiddenHatData();
            }
        }

        public void ResetUsedGroupHistory()
        {
            if (usedGroupIds == null)
                usedGroupIds = new List<int>();
            else
                usedGroupIds.Clear();

            DataAccountPlayer.SavePlayerHiddenHatData();
        }

        public void ResetCurrentCycle()
        {
            hasActiveGroup = false;
            isBoardRevealed = false;
            shuffledRewardIndices?.Clear();
            claimedElementIndices?.Clear();
            startTimeLocalTicks = 0;
            startDateLocal = string.Empty;
            DataAccountPlayer.SavePlayerHiddenHatData();
        }
    }
}
