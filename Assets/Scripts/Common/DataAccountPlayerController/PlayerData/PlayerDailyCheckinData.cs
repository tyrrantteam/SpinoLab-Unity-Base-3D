using System;
using System.Collections.Generic;
namespace DataAccount
{
    public class PlayerDailyCheckinData
    {
        public List<int> listDayCheckin = new List<int>();
        public DateTime lastTimeCheckin = DateTime.UnixEpoch;

        public void SaveTheDay(int idDay)
        {
            listDayCheckin.Add(idDay);
            lastTimeCheckin = DateTime.UtcNow;
            DataAccountPlayer.SavePlayerDailyCheckinData();
        }

        public bool CheckDay(int idDay)
        {
            var numberDay = listDayCheckin.Count;
            if ((numberDay > 0))
            {
                var lastDay = listDayCheckin[numberDay - 1];
                if(idDay - lastDay  == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(idDay == 0)
            {
                return true;
            }
            return false;
        }

        public bool IsrightTime()
        {
            return (DateTime.UtcNow - lastTimeCheckin) >= TimeSpan.FromHours(24);
        }
    }
}