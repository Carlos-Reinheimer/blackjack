using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Utils.UI_Animations;

namespace UI_Controllers {
    public class NextRoundTransitionCanvasUIController : MonoBehaviour {

        public float duration = 2;
        
        [SerializeField] private TMP_Text roundValueText;
        [SerializeField] private FadeCanvasGroupTween fadeCanvasGroupTween;

        private IEnumerator FadeAndClose(Action completeCallback) {
            yield return new WaitForSeconds(duration);
            fadeCanvasGroupTween.Fade(false, 0, completeCallback);
        }

        public void UpdateRoundValue(int nextRound, Action completeCallback) {
            fadeCanvasGroupTween.Fade(true, 0);
            roundValueText.text = $"Round {nextRound}/21";
            StartCoroutine(FadeAndClose(completeCallback));
        }
    }
}
