using UI.Events.Game_Over;
using UnityEngine;

namespace UI.Controllers {
    public class GameOverUIController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private ShowGameOverChannelSO showChannel;
        [SerializeField] private GameOverActionChannelSO actionChannel;

        [Header("Canvas")]
        [SerializeField] private GameObject canvas;

        private void OnEnable() {
            showChannel.OnEventRaised += OnShowRequested;
        }

        private void OnDisable() {
            showChannel.OnEventRaised -= OnShowRequested;
        }
        
        private void OnShowRequested(bool showPanel) {
            Debug.Log("showPanel: " + showPanel);
            if (canvas != null) canvas.SetActive(showPanel);
        }

        public void PlayAgain() {
            canvas.SetActive(false);
            actionChannel.Raise(GameOverAction.PlayAgain);
        }

        public void ReturnHome() {
            canvas.SetActive(false);
            actionChannel.Raise(GameOverAction.ReturnHome);
        }
    }
}
