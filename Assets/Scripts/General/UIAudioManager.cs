using UnityEngine;

namespace General {
    public class UIAudioManager : MonoBehaviour {
        
        [SerializeField] private AudioSource audioSource;

        [Header("Default Clips")]
        public AudioClip pointerEnterClip;
        public AudioClip pointerClickClip;

        #region Singleton
            public static UIAudioManager Instance {
                get {
                    if (_instance == null)
                        _instance = FindFirstObjectByType(typeof(UIAudioManager)) as UIAudioManager;

                    return _instance;
                }
                set => _instance = value;
            }
            private static UIAudioManager _instance;
        #endregion

        public void PlayHover() {
            PlayClip(pointerEnterClip);
        }

        public void PlayClick() {
            PlayClip(pointerClickClip);
        }

        private void PlayClip(AudioClip clip) {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
        }
    }
}