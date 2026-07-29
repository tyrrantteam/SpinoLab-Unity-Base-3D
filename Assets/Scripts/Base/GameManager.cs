
using UnityEngine;
using UI.LoadingScene;
using UnityEngine.Events;
using DataAccount;

namespace Base.Core
{
    public class GameManager : SingletonMono<GameManager>
    {
        private const int TargetFrameRate = 60;
        public static bool isTest = false;
        public SceneName currentScene;
        private float timeCount;
        private float timeCountInGameScene;
        public bool canCount;
        public bool canCountInGameScene;
        public bool canShowAds;
        public bool usingMetaSys;
        private float timeLimit;
        public DataAdsController DataAds;
        private bool hasInternet = true;

        private void Start()
        {
            ReSetValueAdsIntern();
            canCount = true;
            timeLimit = DataAds.TimeShow();
            var dataLocal = DataAccountPlayer.PlayerResourceData;
            if (dataLocal.isFirstTimeOpen)
            {
                dataLocal.Change1stStatus(false);
                LoadScene(SceneName.GamePlayScreen);
            }
            else
            {
                if (usingMetaSys)
                {
                    LoadScene(SceneName.HomeScene);
                }
                else
                {
                    LoadScene(SceneName.GamePlayScreen);
                }
            }
        }
        protected override void Awake()
        {
            base.Awake();
            SetForcedFrameRate();
        }

        public void SetForcedFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
        }

        public void SetCurrentScene(SceneName sceneName)
        {
            currentScene = sceneName;
          
        }

        private void Update()
        {
            if (canCount && DataAds.UsingInterAds)
            {
                timeCount += Time.deltaTime;
                if (timeCount >= timeLimit && DataAds.CanShowInterAds())
                {
                    canShowAds = true;
                    canCount = false;
                }
            }
        }

        public void ReSetValueAdsIntern()
        {
            canShowAds = false;
            canCount = true;
            timeCount = 0;
            // timeLimit = DataAds.TimeShow();
        }

        public void LoadScene(SceneName sceneName, bool asyncLoad = true)
        {
            //AdsManager.Instance.HideBannerAds();

            GameManager.Instance.StopAllCoroutines();
            if (asyncLoad)
            {
                LoadingScene.SetSceneName(sceneName);
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName.LoadingScene.ToString());
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName.ToString());
            }

            SetCurrentScene(sceneName);
        }

        public void ReLoadScene(bool asyncLoad = true)
        {
            GameManager.Instance.StopAllCoroutines();
            if (asyncLoad)
            {

                LoadingScene.SetSceneName(currentScene);

                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName.LoadingScene.ToString());
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.ToString());
            }
        }
    }
}