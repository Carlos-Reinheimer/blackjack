using TMPro;
using UI.Events.Main_Menu;
using UnityEngine;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class MainMenuUIController : MonoBehaviour {

        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private MainMenuGameInfoChannelSO gameInfoChannel;
        
        [Header("UI")]
        [SerializeField] private UpdateValueOverTimeTween globalScoreText;
        [SerializeField] private TMP_Text gameVersionText;

        private void OnEnable() {
            gameInfoChannel.OnEventRaised += HandleGameInfo;
        }

        private void OnDisable() {
            gameInfoChannel.OnEventRaised -= HandleGameInfo;
        }

        private void HandleGameInfo(GameInfoSchema gameInfoSchema) {
            HandleGlobalScore(gameInfoSchema.globalScore);
            SetGameVersion(gameInfoSchema.gameVersion);
        }

        private void HandleGlobalScore(int globalScore) {
            globalScoreText.UpdateTargetValues(0, globalScore);
            globalScoreText.StartTween();
        }

        private void SetGameVersion(string version) {
            gameVersionText.text = $"v{version}";
        }
        
        public void StartRun() {
            actionChannel.Raise(MainMenuAction.StartRun);
        }

        public void OpenJokerShop() {
            actionChannel.Raise(MainMenuAction.JokerShop);
        }

        public void OpenSettings() {
            actionChannel.Raise(MainMenuAction.Settings);
        }

        public void QuitGame() {
            actionChannel.Raise(MainMenuAction.Quit);
        }
    }
}
