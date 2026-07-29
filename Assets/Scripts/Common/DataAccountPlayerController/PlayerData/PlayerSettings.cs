using Base.Core;

namespace DataAccount
{
    public class PlayerSettings
    {
        public string namePlayer;
        public bool MusicOff;
        public bool SoundOff;
        public bool VibrationOff;

        public void SetName(string nameUser)
        {
            namePlayer = nameUser;
            DataAccountPlayer.SavePlayerSettings();
        }

        public void SetMusic(bool isOff)
        {
            if (MusicOff != isOff)
            {
                MusicOff = isOff;   
                DataAccountPlayer.SavePlayerSettings();
                GameManager.Instance.PostEvent(EventID.OnMusicChange);
            }
        }

        public void SetSound(bool isOff)
        {
            if (SoundOff != isOff)
            {
                SoundOff = isOff;
                DataAccountPlayer.SavePlayerSettings();
                GameManager.Instance.PostEvent(EventID.OnSoundChange);
            }
        }

        public void SetVibration(bool isOff)
        {
            if (VibrationOff != isOff)
            {
                VibrationOff = isOff;
                DataAccountPlayer.SavePlayerSettings();
                GameManager.Instance.PostEvent(EventID.OnVibrationChange);
            }
        }
    }
}