using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.UI_Animations;

namespace Utils {
    public class SceneLoader : MonoBehaviour {
        
        public static event Action OnSceneLoadCompleteCallback;

        public FadeCanvasGroupTween fade;
        
        #region Singleton
            public static SceneLoader Instance {
                get {
                    if (_instance == null)
                        _instance = FindFirstObjectByType(typeof(SceneLoader)) as SceneLoader;

                    return _instance;
                }
            }
            private static SceneLoader _instance;
        #endregion
        
        private void Awake() {
            DontDestroyOnLoad(this);
        }
        
        public void LoadScene(string sceneName) {
            StartCoroutine(LoadSceneAsyncWithLoadingScreen(sceneName));
        }

        private IEnumerator LoadSceneAsyncWithLoadingScreen(string sceneName) {
            var fadeInComplete = false;
            fade.Fade(true, 0, () => fadeInComplete = true);
            yield return new WaitUntil(() => fadeInComplete); // Wait for fade-in to finish
            
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            while (async.progress < 0.9f) {
                yield return null;
            }

            async.allowSceneActivation = true;
            while (!async.isDone) {
                yield return null;
            }

            fade.Fade(false);
            OnSceneLoadCompleteCallback?.Invoke();
        }
    }
}
