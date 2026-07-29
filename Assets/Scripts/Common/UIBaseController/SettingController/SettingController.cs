using Base.Core;
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

        protected override void Awake()
        {
            base.Awake();
            soundSetting.SettingPopupController = this;
            musicSetting.SettingPopupController = this;
            vibrationSetting.SettingPopupController = this;
        }

        protected override void ListenerButton()
        {
            base.ListenerButton();
            HomeBtn.onClick.AddListener(Home);
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
            GameManager.Instance.LoadScene(SceneName.HomeScene);
        }
    }
}