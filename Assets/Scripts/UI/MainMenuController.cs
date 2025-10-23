using System;
using UI.Events.Main_Menu;
using UnityEngine;
using Utils;

namespace UI {
    public class MainMenuController : MonoBehaviour {

        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private MainMenuGameInfoChannelSO gameInfoChannel;

        private void OnEnable() {
            actionChannel.OnEventRaised += HandleAction;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleAction;
        }

        private void Start() {
            SaveGameData.Load(SaveGameData.MAIN_SAVE_FILENAME, () => {
                var initialGameDataInfo = new GameInfoSchema {
                    gameVersion = Application.version,
                    globalScore = SaveGameData.coreData.globalScore
                };
                
                gameInfoChannel.Raise(initialGameDataInfo);
            });
        }

        private void HandleAction(MainMenuAction action) {
            switch (action) {
                case MainMenuAction.StartRun:
                    StartRun();
                    break;
                case MainMenuAction.JokerShop:
                    break;
                case MainMenuAction.Settings:
                    break;
                case MainMenuAction.Quit:
                    QuitGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void StartRun() {
            SceneLoader.Instance.LoadScene("Main");
        }
        
        private void QuitGame() {
            #if UNITY_STANDALONE
                Application.Quit();
            #endif
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
