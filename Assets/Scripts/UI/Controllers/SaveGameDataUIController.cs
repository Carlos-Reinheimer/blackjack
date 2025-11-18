using UI.Events.Save_Game_Data;
using UnityEngine;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class SaveGameDataUIController : MonoBehaviour {

        [Header("Channels (SO assets)")]
        [SerializeField] private SaveGameDataEventChannel saveGameDataEventChannel;
        
        [Header("UI")]
        [SerializeField] private FadeCanvasGroupTween fadeCanvas;

        private void OnEnable() {
            saveGameDataEventChannel.OnEventRaised += SetLoadingState;
        }

        private void OnDisable() {
            saveGameDataEventChannel.OnEventRaised -= SetLoadingState;
        }
        
        private void SetLoadingState(bool state) {
            fadeCanvas.Fade(state);
        }
    }
}
