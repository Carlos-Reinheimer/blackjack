using General;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils.UI_Animations {
    public class ButtonInputFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler {

        // [Header("Haptic Feedback")]
        // [SerializeField] private float hoverHapticAmplitude = 0.1f;
        // [SerializeField] private float clickHapticAmplitude = 0.4f;
        // [SerializeField] private float hapticDuration = 0.05f;

        [Header("Scale Feedback")]
        [SerializeField] private bool animateHoverScale = true;
        [SerializeField] private bool animateClickScale = true;
        [SerializeField] private float hoverScale = 1.01f;
        [SerializeField] private float clickScale = 0.9f;
        [SerializeField] private float tweenDuration = 0.08f;
        [SerializeField] private float returnDuration = 0.05f;

        private Vector3 _originalScale;
        private bool _runningClickAnim;

        private void Awake() {
            _originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (_runningClickAnim) return;
            UIAudioManager.Instance.PlayHover();
            TriggerHaptics(false);

            if (!animateHoverScale) return;
            AnimateScale(_originalScale * hoverScale, tweenDuration);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (!animateHoverScale) return;
            AnimateScale(_originalScale, returnDuration);
        }

        public void OnPointerDown(PointerEventData eventData) {
            UIAudioManager.Instance.PlayClick();
            TriggerHaptics(true);

            if (!animateClickScale) return;
            _runningClickAnim = true;
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, _originalScale * clickScale, tweenDuration)
                .setEaseInOutQuad()
                .setOnComplete(() => {
                    AnimateScale(_originalScale * hoverScale, returnDuration);
                    _runningClickAnim = false;
                });
        }

        private void TriggerHaptics(bool isClick) { }

        private void AnimateScale(Vector3 targetScale, float duration) {
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, targetScale, duration).setEaseOutQuad();
        }
    }
}
