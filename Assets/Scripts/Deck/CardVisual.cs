using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deck {
    public class CardVisual : MonoBehaviour {
        
        [NonSerialized] private CardController card;
        
        [Header("UI")]
        public TMP_Text valueText;

        [Header("Follow Parameters")] 
        [SerializeField] private float followSpeed = 30;

        [Header("Pointer Enter/Exit Parameters")] 
        [SerializeField] private float scaleMultiplier = 0.2f;

        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private AnimationCurve scaleEaseCurve;
        
        [Header("Camera Shake Parameters")] 
        [SerializeField] private float duration = 0.35f;

        [SerializeField] private float strength = 20f;
        [SerializeField] private int vibrato = 18;
        [SerializeField] private bool ignoreTimeScale = true;

        private int savedIndex;
        private Vector3 rotationDelta;
        private Vector3 movementDelta;
        private Transform _baseCardTf;
        private Vector3 _originalScale;
        private Vector3 _originalPos;

        private RectTransform _rt;
        private float _defaultZ;

        private void OnDisable() {
            card.onPointerEnterEvent.RemoveListener(OnPointerEnter);
            card.onPointerExitEvent.RemoveListener(OnPointerExit);
            card.OnBeginDragEvent.RemoveListener(OnBeginDrag);
            card.OnEndDragEvent.RemoveListener(OnEndDrag);
        }

        private void Update() {
            if (!card) return;

            SmoothFollow();
        }

        private void SmoothFollow() {
            transform.position = Vector3.Lerp(transform.position,  new Vector3(_baseCardTf.position.x, _baseCardTf.position.y, transform.position.z), followSpeed * Time.deltaTime);
        }

        private void ShakeCard() {
            LeanTween.cancel(gameObject);

            var origin = _rt.anchoredPosition;
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
                    LeanTween.value(gameObject, _rt.anchoredPosition, to, stepTime * 0.5f)
                        .setEase(LeanTweenType.easeInOutSine)
                        .setOnUpdate((Vector2 v) => _rt.anchoredPosition = v)
                );
                seq.append(
                    LeanTween.value(gameObject, to, origin, stepTime * 0.5f)
                        .setEase(LeanTweenType.easeInOutSine)
                        .setOnUpdate((Vector2 v) => _rt.anchoredPosition = v)
                );
            }

            seq.append(() => _rt.anchoredPosition = origin);
        }

        private void OnPointerEnter(CardController baseCard) {
            ShakeCard();
        }

        private void OnPointerExit(CardController baseCard) { }

        private void OnBeginDrag(CardController baseCard) {
            var tf = transform;
            var pos = tf.position;
            var baseScale = _baseCardTf.localScale;
            var targetScale = new Vector3(baseScale.x * scaleMultiplier, baseScale.y * scaleMultiplier, baseScale.z * scaleMultiplier);
            
            _defaultZ = pos.z;
            tf.position = new Vector3(pos.x, pos.y, -20);
            
            LeanTween.scale(gameObject, targetScale, scaleDuration).setEase(scaleEaseCurve);
        }

        private void OnEndDrag(CardController baseCard) {
            var tf = transform;
            var pos = tf.position;
            
            _defaultZ = pos.z;
            tf.position = new Vector3(pos.x, pos.y, _defaultZ);
            LeanTween.scale(gameObject, _originalScale, scaleDuration).setEase(scaleEaseCurve);
        }
        
        public void StartVisual(CardController parentCard, int cardValue) {
            var tf = transform;
            card = parentCard;
            _baseCardTf = card.transform;
            _originalScale = tf.localScale;
            _rt = (RectTransform)tf;
            
            valueText.text = cardValue.ToString();

            card.onPointerEnterEvent.AddListener(OnPointerEnter);
            card.onPointerExitEvent.AddListener(OnPointerExit);
            card.OnBeginDragEvent.AddListener(OnBeginDrag);
            card.OnEndDragEvent.AddListener(OnEndDrag);
        }
    }
}
