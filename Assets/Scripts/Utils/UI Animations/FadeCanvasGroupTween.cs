using System;
using UnityEngine;

namespace Utils.UI_Animations {
    public class FadeCanvasGroupTween : MonoBehaviour {

        public enum StartMethod
        {
            FadeIn,
            FadeOut
        }

        [Header("UI")]
        public CanvasGroup canvasGroup;
        
        [Header("Settings")]
        public float fadeTime;
        public float delay;
        public StartMethod startMethod;
        public bool autoStart;

        private void OnEnable() {
            if (!autoStart) return;
            
            switch (startMethod) {
                case StartMethod.FadeIn:
                    Fade(true, delay);
                    break;
                case StartMethod.FadeOut:
                default:
                    Fade(false, delay);
                    break;
            }
        }

        public void Fade(bool fadeIn, float alphaDelay = 0, Action callback = null) {
            canvasGroup.alpha = fadeIn ? 0 : 1;
            LeanTween.alphaCanvas(canvasGroup, fadeIn ? 1f : 0f, fadeTime)
                .setDelay(alphaDelay)
                .setEase(LeanTweenType.linear)
                .setOnComplete(callback);
        }

        public void ForceSetAlpha(bool fadeIn) {
            canvasGroup.alpha = fadeIn ? 0 : 1;
        }
    }
}
