using Base.Core;
using Base.Core.Sound;
using DataAccount;
using JinGroup.Common.ResourcesHeader;
using JinGroup.Controller.Feature;
using LitMotion;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;


public class GoldResourcesHeader : BaseResourcesHeader
{
    private                  int          _lastValue;
    private                  MotionHandle _handle;
    private                  float        _lastSoundTime;
    [SerializeField] private Button       addMoreGold;

    [SerializeField] private ScaleEffectSquashAndStretch scaleEffectSquashAndStretch;

    protected override void Awake()
    {
        base.Awake();
        _lastValue       = DataAccountPlayer.PlayerResourceData.gold;
        valueInformation = _lastValue;
        base.UpdateValue();
        this.RegisterListener(EventID.UpdateGold, (sender, param) => UpdateValueTween());

        addMoreGold.onClick.AddListener(OnCLickMoreGold);

        if (scaleEffectSquashAndStretch == null)
            scaleEffectSquashAndStretch = GetComponent<ScaleEffectSquashAndStretch>();
    }

    private void UpdateValueTween()
    {
        int newValue = DataAccountPlayer.PlayerResourceData.gold;
        if (newValue == _lastValue) return;


        _handle.TryCancel();
        _handle = LMotion.Create(_lastValue, newValue, 1.5f)
                         .WithEase(Ease.OutCubic)
                         .Bind(x =>
                         {
                             if (x > _lastValue && Time.time - _lastSoundTime > 0.075f)
                             {
                                 if (SoundManager.Instance != null)
                                 {
                                     //SoundManager.Instance.PlayOneShot(SoundType.CoinPlus);
                                     _lastSoundTime = Time.time;
                                 }
                             }

                             _lastValue       = x;
                             valueInformation = x;
                             base.UpdateValue();
                         });
    }


    private void OnCLickMoreGold()
    {
        var scene = GameManager.Instance.currentScene;
        if (scene == SceneName.HomeScene)
        {
            BottomTabController.Instance.SetPanelActive(TabType.Shop);
        }
        else
        {
            // PopupManager.Instance.ShowPopup<PopupShopController>();
        }
    }

    private void OnDestroy()
    {
        _handle.TryCancel();
    }
}