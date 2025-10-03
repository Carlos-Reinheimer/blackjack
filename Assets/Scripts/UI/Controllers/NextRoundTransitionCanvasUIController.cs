using System;
using System.Collections;
using TMPro;
using UI.Events.Next_Round;
using UnityEngine;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class NextRoundTransitionCanvasUIController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private AdvanceRoundChannelSO advanceRoundChannel;

        [Header("Settings")]
        public float duration = 2;
        
        [Header("UI")]
        [SerializeField] private GameObject canvas;
        [SerializeField] private TMP_Text roundValueText;
        [SerializeField] private FadeCanvasGroupTween fadeCanvasGroupTween;

        private void OnEnable() {
            advanceRoundChannel.OnEventRaised += UpdateRoundValue;
        }

        private void OnDisable() {
            advanceRoundChannel.OnEventRaised -= UpdateRoundValue;
        }

        private IEnumerator FadeAndClose(Action completeCallback) {
            yield return new WaitForSeconds(duration);
            fadeCanvasGroupTween.Fade(false, 0, () => {
                completeCallback?.Invoke();
                canvas.SetActive(false);
            });
        }

        private void UpdateRoundValue(AdvanceRoundModel advanceRoundModel) {
            canvas.SetActive(true);
            roundValueText.text = $"Round {advanceRoundModel.nextRound + 1}/21";
            StartCoroutine(FadeAndClose(advanceRoundModel.completeCallback));
        }
    }
}
