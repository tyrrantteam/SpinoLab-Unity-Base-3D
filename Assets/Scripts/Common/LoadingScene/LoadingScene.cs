using System;
using System.Collections;
using UnityEngine;

namespace UI.LoadingScene
{
    public class LoadingScene : MonoBehaviour
    {
        private static SceneName _sceneName;
        
        private bool CanGoOutOfScene { get; set; }
        
        
        public static void SetSceneName(SceneName sceneName)
        {
            _sceneName = sceneName;
        }
        
        private void Start()
        {
            CanGoOutOfScene = false;

            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
            Resources.UnloadUnusedAssets();
            GC.Collect();
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_sceneName.ToString());
            async.allowSceneActivation = false;

            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    CanGoOutOfScene = true;
                }

                if (CanGoOutOfScene)
                {
                    async.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}