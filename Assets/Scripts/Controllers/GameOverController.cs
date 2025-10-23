using UI.Events.Game_Over;
using UI.Events.Save_Game_Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Controllers {
    public class GameOverController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private ShowGameOverChannelSO showChannel;
        [SerializeField] private GameOverActionChannelSO actionChannel;
        [SerializeField] private SaveGameDataEventChannel saveGameDataEventChannel;

        private void OnEnable() {
            actionChannel.OnEventRaised += HandleGameOverActionChannel;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleGameOverActionChannel;
        }
        
        private void HandleGameOverActionChannel(GameOverAction action) {
            SceneLoader.Instance.LoadScene(action == GameOverAction.PlayAgain ? "Main" : "Menu");
        }

        public void GameOver() {
            saveGameDataEventChannel.Raise(true);
            SaveGameData.Save(SaveGameData.MAIN_SAVE_FILENAME, () => saveGameDataEventChannel.Raise(false));
            
            showChannel.Raise(true);
        }
    }
}
