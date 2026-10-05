using Base.Core;
using JinGroup.Base.LoadData;
using JinGroup.UI.Common.Setting;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class HeaderUIController : MonoBehaviour
{
    [SerializeField] private GoldResourcesHeader goldHeader;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private GameObject levelHodler;
    private GameConfig _gameConfig;
    private bool _usingResourceInGame;

    private void Awake()
    {
        _gameConfig = LoadResourceController.Instance.GameConfig();
        _usingResourceInGame = _gameConfig.usingResourceInGame;
        SetupIAAproduction();
    }

    public void ListenerButton()
    {
        retryBtn.onClick.AddListener(RetryGame);
        settingBtn.onClick.AddListener(OpenSetting);
    }

    public void OpenSetting()
    {
        PopupManager.Instance.ShowPopup<SettingController>();
    }

    public void RetryGame()
    {
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
    }

    public void SetupIAAproduction()
    {
        goldHeader.gameObject.SetActive(_usingResourceInGame);
        retryBtn.gameObject.SetActive(!_usingResourceInGame);
    }

    public void HighLightResource()
    {
        levelHodler.gameObject.SetActive(false);
        settingBtn.gameObject.SetActive(false);
        retryBtn.gameObject.SetActive(false);

        if (_usingResourceInGame)
        {
            goldHeader.gameObject.SetActive(true);
        }
        else
        {
            goldHeader.gameObject.SetActive(false);
        }
    }

    public void CloseResource()
    {
        levelHodler.gameObject.SetActive(true);
        settingBtn.gameObject.SetActive(true);
        if (_usingResourceInGame)
        {
            goldHeader.gameObject.SetActive(true);
        }
        else
        {
            goldHeader.gameObject.SetActive(false);
            retryBtn.gameObject.SetActive(true);
        }
    }
}
