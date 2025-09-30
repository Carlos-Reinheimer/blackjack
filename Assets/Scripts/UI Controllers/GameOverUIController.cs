using UnityEngine;
using Utils;

namespace UI_Controllers {
    public class GameOverUIController : MonoBehaviour {
        
        public void PlayAgain() {
            SceneLoader.Instance.LoadScene("Main");
        }

        public void ReturnHome() {
            
        }
        
    }
}
