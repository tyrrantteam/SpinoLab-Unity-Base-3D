using Base.Core;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using System.Collections;
using System.Collections.Generic;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.UI.Common.Setting
{
    public class SettingController : PopupBaseController
    {

        [SerializeField] private SettingElement soundSetting;

        [SerializeField] private SettingElement musicSetting;

        [SerializeField] private SettingElement vibrationSetting;

        [SerializeField] private Button HomeBtn;
        [SerializeField] private Button ReplayBtn;

        private GameConfig _gameConfig;

        protected override void Awake()
        {
            base.Awake();

            _gameConfig = LoadResourceController.Instance.GameConfig();

            soundSetting.SettingPopupController = this;
            musicSetting.SettingPopupController = this;
            vibrationSetting.SettingPopupController = this;

            var isActiveHome = _gameConfig.usingMetaSys;
            var currentScene = GameManager.Instance.currentScene;
            var lvUnlockMeta = _gameConfig.levelUnlockMetaSystem;
            var currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
            bool isHomeScene = (currentScene == SceneName.HomeScene);
            bool levelUnlockMeta = (currentLv >= lvUnlockMeta);

            HomeBtn.gameObject.SetActive(isActiveHome && !isHomeScene && levelUnlockMeta);
            ReplayBtn.gameObject.SetActive(!isHomeScene);
        }

        protected override void ListenerButton()
        {
            base.ListenerButton();
            HomeBtn.onClick.AddListener(Home);
            ReplayBtn.onClick.AddListener(Replay);
        }

        public void CallLockSettings()
        {
            if (soundSetting.isActive)
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetSound(true);
            }
            else
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetSound(false);
            }

            if (musicSetting.isActive)
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetMusic(true);
            }
            else
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetMusic(false);
            }

            if (vibrationSetting.isActive)
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetVibration(true);
            }
            else
            {
                DataAccount.DataAccountPlayer.PlayerSettings.SetVibration(false);
            }
        }

        private void Home()
        {
            var isUsingHeart = _gameConfig.usingHeart;
            if (isUsingHeart)
            {
                OnClosePopup();
                PopupManager.Instance.ShowPopup<PopupAreYourSureController>();
            }
            else
            {
                GameManager.Instance.LoadScene(SceneName.HomeScene);
            }
        }

        private void Replay()
        {
            GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        }
    }
}