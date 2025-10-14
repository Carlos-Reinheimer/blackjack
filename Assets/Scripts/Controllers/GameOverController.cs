using UI.Events.Game_Over;
using UnityEngine;
using Utils;

namespace Controllers {
    public class GameOverController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private ShowGameOverChannelSO showChannel;
        [SerializeField] private GameOverActionChannelSO actionChannel;

        private void OnEnable() {
            actionChannel.OnEventRaised += HandleGameOverActionChannel;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleGameOverActionChannel;
        }

        private void HandleGameOverActionChannel(GameOverAction action) {
            if (action == GameOverAction.PlayAgain) SceneLoader.Instance.LoadScene("Main");
            else SceneLoader.Instance.LoadScene("Main");
        }

        public void GameOver() {
            // TODO: the buttons on the UI are not working
            Debug.Log("Game is over");
            showChannel.Raise(true);
        }
    }
}
