using System.Collections;
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
        [SerializeField] private Button hiddenHatBtn;
        [SerializeField] private Button vipBundleBtn;
        [SerializeField] private Button noAdsBtn;
        [SerializeField] private Button specialBundBtn;
        [Header("Left Feature ")]
        [SerializeField] private Button rankingBtn;
        [SerializeField] private Button piggyGemBtn;
        [SerializeField] private Button piggyGoldBtn;
        [SerializeField] private Button luckSpinBtn;
        [SerializeField] private Button dailyTask;

   
        [Header("Header")]
        private GameConfig _gameConfig;
        [SerializeField] private GoldResourcesHeader GoldResourcesHeader;
        [SerializeField] private GemResourcesHeader GemResourcesHeader;
        [SerializeField] private HeartResourcesHeader HeartResourcesHeader;


        private Coroutine   _starterPackWatchCo;

        protected override void Awake()
        {
            base.Awake(); // registers the singleton instance so HomeUIManager.IsNull is accurate
            RegisterButton();
            InitData();
        }

        private void OnEnable()
        {
            RefreshBundleButtons();
        }

        private void OnDisable()
        {
            // Unity stops coroutines on disable; clear the handle so the watch can restart on re-enable.
            _starterPackWatchCo = null;
        }

        private void RegisterButton()
        {
            playBtn.onClick.AddListener(OnClickPlay);
            dailyCheckinBtn.onClick.AddListener(OnClickDailyCheckin);
            rankingBtn.onClick.AddListener(OnClickranking);
            piggyGemBtn.onClick.AddListener(OnClickPiggyGem);
            piggyGoldBtn.onClick.AddListener(OnClickPiggyGold);
            settings.onClick.AddListener(OnClickSettings);
            luckSpinBtn.onClick.AddListener(OnClickLuckySpin);
            dailyTask.onClick.AddListener(OnClickDailyTask);

            hiddenHatBtn.onClick.AddListener(OnClickHiddenHat);
            vipBundleBtn.onClick.AddListener(OnClickVipBundle);
            noAdsBtn.onClick.AddListener(OnNoAdsClick);
            specialBundBtn.onClick.AddListener(OnClickSpecialBundle);
        }

        private void OnClickVipBundle()
        {
            PopupManager.Instance.ShowPopup<PopupBundleVipPass>();
        }
         private void OnNoAdsClick()
        {
            PopupManager.Instance.ShowPopup<PopupBundleRemoveAds>();
        }
         private void OnClickSpecialBundle()
        {
            RefreshStarterPackButton();
            if (!specialBundBtn.gameObject.activeSelf)
                return;

            PopupManager.Instance.ShowPopup<PopupBundleStartePack>();
        }
        private void OnClickDailyCheckin()
        {
            PopupManager.Instance.ShowPopup<PopupDailyCheckinController>();
        }

        private void OnClickHiddenHat()
        {
             PopupManager.Instance.ShowPopup<PopupHiddenHatController>();
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
            DataAccountPlayer.PlayerDailyTashkData.UpdateTask(TypeTaskDaily.PlayGame);
        }

        private void OnClickSettings()
        {
            PopupManager.Instance.ShowPopup<SettingController>();
        }

        private void OnClickLuckySpin()
        {
            PopupManager.Instance.ShowPopup<PopupLuckySpinController>();
        }

        private void OnClickDailyTask()
        {
            PopupManager.Instance.ShowPopup<PopupDailyTaskController>();
        }


        private void InitData()
        {
            // if (DataAccountPlayer.PlayerResourceData.isNoAdsPurchase)
            // {
            //     GameDebug.Log("is remove no ads");
            //     footerArea.anchoredPosition = new Vector2(footerArea.anchoredPosition.x, 0);
            // }
            // else
            // {
            //     GameDebug.Log("is no ads");
            //     footerArea.anchoredPosition = new Vector2(footerArea.anchoredPosition.x, offsetY);
            // }

            // Ghi nhận task login mỗi ngày nếu chưa đạt
            if (DataAccountPlayer.PlayerDailyTashkData.GetCurrentValue(TypeTaskDaily.Login) < 1)
            {
                DataAccountPlayer.PlayerDailyTashkData.UpdateTask(TypeTaskDaily.Login);
            }

            _gameConfig = LoadResourceController.instance.GameConfig();
            var usingGem = _gameConfig.usingGem;
            var usingHeart = _gameConfig.usingHeart;

            GemResourcesHeader.gameObject.SetActive(usingGem);
            HeartResourcesHeader.gameObject.SetActive(usingHeart);
        }

        public void RefreshBundleButtons()
        {
            if (noAdsBtn != null)
                noAdsBtn.gameObject.SetActive(!DataAccountPlayer.PlayerResourceData.isNoAdsPurchase);

            RefreshStarterPackButton();
        }

        /// <summary>
        /// starterPackBtn active khi: currentLevel ≥ levelUnlock và còn trong hourActive
        /// kể từ lúc đạt unlock (và chưa mua hết limit).
        /// </summary>
        public void RefreshStarterPackButton()
        {
            if (specialBundBtn == null)
            {
                GameDebug.LogWarning("[StarterPack] specialBundBtn is not assigned on HomeUIManager");
                return;
            }

            bool hasConfig = TryGetStarterPackConfig(out var cfg);
            bool show = hasConfig && IsStarterPackButtonAvailable(cfg);
            LogStarterPackState(hasConfig, cfg, show); // TODO: remove after debugging

            specialBundBtn.gameObject.SetActive(show);

            var featureBtn = specialBundBtn.GetComponent<TimerFeatureHomeBtn>();
            if (show)
            {
                StartStarterPackWatch();
                StartStarterPackCountdown(featureBtn, cfg.hourActive);
            }
            else
            {
                StopStarterPackWatch();
                featureBtn?.StopCountdown();
            }
        }

        /// <summary>Debug: prints every condition that decides whether the starter pack button shows.</summary>
        private void LogStarterPackState(bool hasConfig, BundleReward cfg, bool show)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[StarterPack] show = {show}");

            if (!hasConfig)
            {
                var loader = LoadResourceController.Instance;
                var data = loader != null ? loader.DataBundleController() : null;
                var special = data != null ? data.ContentContent?.specialBundle : null;
                sb.Append($"\n  x no config: loader={(loader != null)}, DataBundleController={(data != null)}, " +
                          $"ContentContent={(data != null && data.ContentContent != null)}, " +
                          $"specialBundle count={(special == null ? "null" : special.Count.ToString())}, " +
                          $"specialBundle[0]={(special != null && special.Count > 0 && special[0] != null)}");
                GameDebug.Log(sb.ToString(), specialBundBtn);
                return;
            }

            var shop = DataAccountPlayer.PlayerShopData;
            int currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;

            bool limitOk = !(cfg.limitPurchase > 0 && shop.starterPackPurchase >= cfg.limitPurchase);
            bool levelOk = currentLv >= cfg.levelUnlock;
            bool timeOk  = true;
            string timeInfo = "no time limit (hourActive <= 0)";
            if (cfg.hourActive > 0)
            {
                if (shop.starterPackUnlockTimestamp <= 0)
                {
                    timeInfo = "unlock timestamp not set yet";
                }
                else
                {
                    long nowMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    double elapsedH = (nowMs - shop.starterPackUnlockTimestamp) / 3600000d;
                    timeOk = elapsedH < cfg.hourActive;
                    var unlockUtc = System.DateTimeOffset.FromUnixTimeMilliseconds(shop.starterPackUnlockTimestamp).UtcDateTime;
                    timeInfo = $"unlocked at {unlockUtc:yyyy-MM-dd HH:mm:ss} UTC, elapsed {elapsedH:F2}h / {cfg.hourActive}h";
                }
            }

            sb.Append($"\n  {(limitOk ? "ok" : "x ")} purchase limit: bought {shop.starterPackPurchase} / limit {cfg.limitPurchase}");
            sb.Append($"\n  {(levelOk ? "ok" : "x ")} level: current {currentLv} / unlock {cfg.levelUnlock}");
            sb.Append($"\n  {(timeOk ? "ok" : "x ")} time: {timeInfo}");
            sb.Append($"\n  button parent active in hierarchy: " +
                      $"{(specialBundBtn.transform.parent == null || specialBundBtn.transform.parent.gameObject.activeInHierarchy)}");
            GameDebug.Log(sb.ToString(), specialBundBtn);
        }

        private void StartStarterPackCountdown(TimerFeatureHomeBtn featureBtn, int hourActive)
        {
            if (featureBtn == null)
                return;

            if (hourActive <= 0)
            {
                featureBtn.StopCountdown();
                return;
            }

            var remaining = DataAccountPlayer.PlayerShopData.GetStarterPackRemaining(hourActive);
            if (remaining.TotalSeconds <= 0)
            {
                featureBtn.StopCountdown();
                return;
            }

            featureBtn.StartCountdown(remaining);
        }

        private static bool TryGetStarterPackConfig(out BundleReward cfg)
        {
            cfg = null;
            var loader = LoadResourceController.Instance;
            if (loader == null)
                return false;

            var data = loader.DataBundleController();
            var special = data != null ? data.ContentContent?.specialBundle : null;
            if (special == null || special.Count == 0)
                return false;

            cfg = special[0];
            return cfg != null;
        }

        private static bool IsStarterPackButtonAvailable()
        {
            return TryGetStarterPackConfig(out var cfg) && IsStarterPackButtonAvailable(cfg);
        }

        private static bool IsStarterPackButtonAvailable(BundleReward cfg)
        {
            int currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
            return DataAccountPlayer.PlayerShopData.IsStarterPackActive(
                cfg.levelUnlock, cfg.hourActive, cfg.limitPurchase, currentLv);
        }

        private void StartStarterPackWatch()
        {
            if (_starterPackWatchCo != null)
                return;
            _starterPackWatchCo = StartCoroutine(CoWatchStarterPackExpiry());
        }

        private void StopStarterPackWatch()
        {
            if (_starterPackWatchCo == null)
                return;
            StopCoroutine(_starterPackWatchCo);
            _starterPackWatchCo = null;
        }

        private IEnumerator CoWatchStarterPackExpiry()
        {
            while (specialBundBtn != null && specialBundBtn.gameObject.activeSelf)
            {
                if (!IsStarterPackButtonAvailable())
                {
                    specialBundBtn.gameObject.SetActive(false);
                    _starterPackWatchCo = null;
                    yield break;
                }

                yield return new WaitForSecondsRealtime(1f);
            }

            _starterPackWatchCo = null;
        }

    }
}
