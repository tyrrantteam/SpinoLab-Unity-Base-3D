using Base.Core;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Controller.PiggyBank;
using JinGroup.UI.Common.Setting;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;


namespace JinGroup.Common
{
    public class HomeUIManager : SingletonMono<HomeUIManager>
    {
        [SerializeField] private Button playBtn;
        [SerializeField] private RectTransform footerArea;
        [SerializeField] private float offsetY = 500f;

        [Header("Player Settings")]
        [SerializeField] private Button settings;
        [Header("Right Feature ")]
        [SerializeField] private Button dailyCheckinBtn;
        [Header("Left Feature ")]
        [SerializeField] private Button rankingBtn;
        [SerializeField] private Button piggyGemBtn;
        [SerializeField] private Button piggyGoldBtn;
        [Header("Header")]
        private GameConfig _gameConfig;
        [SerializeField] private GoldResourcesHeader GoldResourcesHeader;
        [SerializeField] private GemResourcesHeader GemResourcesHeader;
        [SerializeField] private HeartResourcesHeader HeartResourcesHeader;

        protected override void Awake()
        {
            RegisterButton();
            InitData();
        }

        private void RegisterButton()
        {
            playBtn.onClick.AddListener(OnClickPlay);
            dailyCheckinBtn.onClick.AddListener(OnClickDailyCheckin);
            rankingBtn.onClick.AddListener(OnClickranking);
            piggyGemBtn.onClick.AddListener(OnClickPiggyGem);
            piggyGoldBtn.onClick.AddListener(OnClickPiggyGold);
            settings.onClick.AddListener(OnClickSettings);
        }

        private void OnClickDailyCheckin()
        {
            PopupManager.Instance.ShowPopup<PopupDailyCheckinController>();
        }

        private void OnClickranking()
        {
            PopupManager.Instance.ShowPopup<RankingController>();
        }

        private void OnClickPiggyGem()
        {
            PopupManager.Instance.ShowPopup<PiggiBankGemController>();
        }

        private void OnClickPiggyGold()
        {
            PopupManager.Instance.ShowPopup<PiggiBankGoldController>();
        }

        private void OnClickPlay()
        {
            GameManager.instance.LoadScene(SceneName.GamePlayScreen);
        }

        private void OnClickSettings()
        {
            PopupManager.Instance.ShowPopup<SettingController>();
        }

        private void InitData()
        {
            if (DataAccountPlayer.PlayerResourceData.isNoAdsPurchase)
            {
                GameDebug.Log("is remove no ads");
                footerArea.anchoredPosition = new Vector2(footerArea.anchoredPosition.x, 0);
            }
            else
            {
                GameDebug.Log("is no ads");
                footerArea.anchoredPosition = new Vector2(footerArea.anchoredPosition.x, offsetY);
            }

            _gameConfig = LoadResourceController.instance.GameConfig();
            var usingGem = _gameConfig.usingGem;
            var usingHeart = _gameConfig.usingHeart;

            GemResourcesHeader.gameObject.SetActive(usingGem);
            HeartResourcesHeader.gameObject.SetActive(usingHeart);
        }

    }
}