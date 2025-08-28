using TMPro;
using UnityEngine;

namespace Deck {
    public class DefaultCardVisual : GeneralCardVisual {
        
        [Header("UI")]
        public TMP_Text valueText;
        
        [Header("Pointer Enter/Exit Parameters")] 
        [SerializeField] private float scaleMultiplier = 0.2f;

        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private AnimationCurve scaleEaseCurve;
        
        [Header("Camera Shake Parameters")] 
        [SerializeField] private float duration = 0.35f;

        [SerializeField] private float strength = 20f;
        [SerializeField] private int vibrato = 18;
        [SerializeField] private bool ignoreTimeScale = true;
        
        protected override void OnPointerEnter(CardController baseCard) {
            ShakeCard();
        }

        protected override void OnPointerExit(CardController baseCard) { }

        protected override void OnBeginDrag(CardController baseCard) {
            var tf = transform;
            var baseScale = baseCardTf.localScale;
            var targetScale = new Vector3(baseScale.x * scaleMultiplier, baseScale.y * scaleMultiplier, baseScale.z * scaleMultiplier);

            originalScale = tf.localScale;
            defaultSiblingIndex = tf.GetSiblingIndex();
            tf.SetAsLastSibling();
            
            LeanTween.scale(gameObject, targetScale, scaleDuration).setEase(scaleEaseCurve);
            EffectShadow(true);
        }

        protected override void OnEndDrag(CardController baseCard) {
            transform.SetSiblingIndex(defaultSiblingIndex);
            LeanTween.scale(gameObject, originalScale, scaleDuration).setEase(scaleEaseCurve);
            EffectShadow(false);
        }

        protected override void OnPointerDown(CardController baseCard) { }

        private void ShakeCard() {
            LeanTween.cancel(gameObject);

            var origin = rt.anchoredPosition;
            var steps = Mathf.Max(2, vibrato);
            var stepTime = duration / steps;

            var seq = LeanTween.sequence();
            if (ignoreTimeScale) seq.setScale(duration);

            for (var i = 0; i < steps; i++) {
                var t = i / (float)steps;
                var damper = 1f - t;

                var offset = Random.insideUnitCircle * (strength * damper);
                var to = origin + offset;

                seq.append(
                    LeanTween.value(gameObject, rt.anchoredPosition, to, stepTime * 0.5f)
                        .setEase(LeanTweenType.easeInOutSine)
                        .setOnUpdate((Vector2 v) => rt.anchoredPosition = v)
                );
                seq.append(
                    LeanTween.value(gameObject, to, origin, stepTime * 0.5f)
                        .setEase(LeanTweenType.easeInOutSine)
                        .setOnUpdate((Vector2 v) => rt.anchoredPosition = v)
                );
            }

            seq.append(() => rt.anchoredPosition = origin);
        }
        
        public void StartVisual(CardController parentCard, int cardValue, bool autoFlip = true) {
            valueText.text = cardValue.ToString();
            InitiateCardVisual(parentCard, autoFlip);
        }
    }
}
