using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.UI.Common.Setting
{
    public class SettingElement : MonoBehaviour
    {
        [SerializeField] private GameObject disableImage;
        [SerializeField] private GameObject on;
        [SerializeField] private GameObject off;

        private Button activeButton;

        [HideInInspector]
        public SettingController SettingPopupController;
        public bool isActive;

        private void Awake()
        {
            activeButton = GetComponent<Button>();
            activeButton.onClick.AddListener(OnClickBtn);
        }

        private void Start()
        {
            OnClickBtn();
        }

        private void OnClickBtn()
        {
            if (isActive)
            {
                on.SetActive(false);
                off.SetActive(true);
                disableImage.SetActive(true);
                isActive = false;
            }
            else
            {
                on.SetActive(true);
                off.SetActive(false);
                disableImage.SetActive(false);
                isActive = true;
            }
            SettingPopupController.CallLockSettings();
        }
    }
}