using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils {
    public class SceneLoader : MonoBehaviour {
        
        public static event Action OnSceneLoadCompleteCallback;
        
        public void LoadScene(string sceneName) {
            StartCoroutine(LoadSceneAsyncWithLoadingScreen(sceneName));
        }

        private IEnumerator LoadSceneAsyncWithLoadingScreen(string sceneName) {
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            while (async.progress < 0.9f) {
                yield return null;
            }

            async.allowSceneActivation = true;
            while (!async.isDone) {
                yield return null;
            }

            OnSceneLoadCompleteCallback?.Invoke();
        }
    }
}
