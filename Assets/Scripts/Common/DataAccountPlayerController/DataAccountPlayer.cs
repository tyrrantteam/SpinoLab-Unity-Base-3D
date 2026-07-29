namespace DataAccount
{
    public static class DataAccountPlayer
    {
        private static PlayerSettings _playerSettings;
        private static PlayerResourceData _playerResourceData;
        private static PlayerDailyCheckinData _playerDailyCheckinData;
        private static PlayerPiggyBank _playerPiggyBank;
        private static PlayerPointProcessData _playerPointProcessData;
        private static PlayerTutorialData _playerTutorialData;
        private static PlayerSpecialOfferAdsData _playerSpecialOfferAdsData;
        private static PlayerStreakData _playerStreakData;


        #region Getters

        public static PlayerTutorialData PlayerTutorialData
        {
            get
            {
                if (_playerTutorialData != null)
                {
                    return _playerTutorialData;
                }
                var playerTutorialData = new PlayerTutorialData();
                _playerTutorialData = ES3.Load(DataAccountPlayerConstants.PlayerTutorialData, playerTutorialData);
                return _playerTutorialData;
            }
        }

        public static PlayerSpecialOfferAdsData PlayerSpecialOfferAdsData
        {
            get
            {
                if (_playerSpecialOfferAdsData != null)
                    return _playerSpecialOfferAdsData;

                var playerSpecialOfferAdsData = new PlayerSpecialOfferAdsData();
                _playerSpecialOfferAdsData = ES3.Load(DataAccountPlayerConstants.PlayerSpecialOfferAdsData, playerSpecialOfferAdsData);
                return _playerSpecialOfferAdsData;
            }
        }

        public static PlayerPointProcessData PlayerPointProcessData
        {
            get
            {
                if (_playerPointProcessData != null)
                {
                    return _playerPointProcessData;
                }
                var playerPointProcessData = new PlayerPointProcessData(); 
                _playerPointProcessData = ES3.Load(DataAccountPlayerConstants.PlayerPointProcessData, playerPointProcessData);
                return _playerPointProcessData;
            }
        }
        public static PlayerResourceData PlayerResourceData
        {
            get
            {
                if (_playerResourceData != null)
                {
                    return _playerResourceData;
                }

                var playerResourceData = new PlayerResourceData();
                _playerResourceData = ES3.Load(DataAccountPlayerConstants.PlayerResourceData, playerResourceData);
                return _playerResourceData;
            }
        }

        public static PlayerSettings PlayerSettings
        {
            get
            {
                if (_playerSettings != null)
                {
                    return _playerSettings;
                }

                var playerSettings = new PlayerSettings();
                _playerSettings = ES3.Load(DataAccountPlayerConstants.PlayerSettings, playerSettings);
                return _playerSettings;
            }
        }

        public static PlayerDailyCheckinData PlayerDailyCheckinData
        {
            get
            {
                if (_playerDailyCheckinData != null)
                    return _playerDailyCheckinData;

                var playerDailyCheckinData = new PlayerDailyCheckinData();
                _playerDailyCheckinData = ES3.Load(DataAccountPlayerConstants.PlayerDailyCheckinData, playerDailyCheckinData);
                return _playerDailyCheckinData;
            }
        }

        public static PlayerPiggyBank PlayerPiggyBank
        {
            get
            {
                if (_playerPiggyBank != null)
                    return _playerPiggyBank;

                var playerDailyCheckinData = new PlayerPiggyBank();
                _playerPiggyBank = ES3.Load(DataAccountPlayerConstants.PlayerPiggyBank, playerDailyCheckinData);
                return _playerPiggyBank;
            }
        }

        public static PlayerStreakData PlayerStreakData
        {
            get
            {
                if (_playerStreakData != null)
                    return _playerStreakData;

                var playerStreakData = new PlayerStreakData();
                _playerStreakData = ES3.Load(DataAccountPlayerConstants.PlayerStreakData, playerStreakData);
                return _playerStreakData;
            }
        }

        #endregion

        #region Save

        public static void SavePlayerSettings()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerSettings, _playerSettings);
        }

        public static void SavePlayerResourceData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerResourceData, _playerResourceData);
        }

        public static void SavePlayerDailyCheckinData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerDailyCheckinData, _playerDailyCheckinData);
        }

        public static void SavePlayerPiggyBankData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerPiggyBank, _playerPiggyBank);
        }
        public static void SavePlayerPointProcessData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerPointProcessData, _playerPointProcessData);
        }
        public static void SavePlayerTutorialData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerTutorialData, _playerTutorialData);
        }

        public static void SavePlayerSpecialOfferAdsData()
        {
            ES3.Save(DataAccountPlayerConstants.PlayerSpecialOfferAdsData, _playerSpecialOfferAdsData);
        }

        public static void SavePlayerStreakData()
        {
            if (_playerStreakData == null)
                _ = PlayerStreakData;

            ES3.Save(DataAccountPlayerConstants.PlayerStreakData, _playerStreakData);
        }

        #endregion
    }
}