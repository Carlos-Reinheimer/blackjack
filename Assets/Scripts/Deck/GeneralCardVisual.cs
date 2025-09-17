using System;
using UnityEngine;
using UnityEngine.UI;
using Utils.UI_Animations;

namespace Deck {
    public abstract class GeneralCardVisual : MonoBehaviour {
        
        [NonSerialized] private CardController card;

        [Header("UI")] 
        public Image frontSideImage;
        public Image backSideImage;

        [Header("Shadow Parameters")] 
        [SerializeField] private GameObject shadow;
        [SerializeField] private float shadowYDownValue = 15;
        [SerializeField] private float shadowYDownDuration = 0.1f;

        [Header("Follow Parameters")] 
        [SerializeField] private float followSpeed = 30;

        [Header("Animation Scripts")] 
        [SerializeField] private ScaleTween scaleTween;

        protected Transform baseCardTf;
        protected Vector3 originalScale;
        protected Vector3 originalPos;
        protected RectTransform rt;
        protected int defaultSiblingIndex;
        
        private int savedIndex;
        private Vector3 rotationDelta;
        private Vector3 movementDelta;

        private bool _isScaling;
        private bool _isFlipping;
        
        protected abstract void OnPointerEnter(CardController baseCard);
        protected abstract void OnPointerExit(CardController baseCard);
        protected abstract void OnBeginDrag(CardController baseCard);
        protected abstract void OnEndDrag(CardController baseCard);
        protected abstract void OnPointerDown(CardController baseCard);
        
        protected void InitiateCardVisual(CardController parentCard, bool autoFlip) {
            _isScaling = true;
            scaleTween.StartTween(() => _isScaling = false);
            
            var tf = transform;
            card = parentCard;
            baseCardTf = card.transform;
            rt = (RectTransform)tf;

            if (autoFlip) FlipCard();
            
            card.onPointerEnterEvent.AddListener(HandleOnPointerEnter);
            card.onPointerExitEvent.AddListener(HandleOnPointerExit);
            card.OnBeginDragEvent.AddListener(HandleOnBeginDrag);
            card.OnEndDragEvent.AddListener(HandleOnEndDrag);
            card.OnPointerDownEvent.AddListener(HandleOnPointerDown);
        }

        protected void EffectShadow(bool enable) {
            var shadowPos = shadow.transform.localPosition;
            var yModifiedValue = enable ? -shadowYDownValue : +shadowYDownValue;
            LeanTween.moveLocal(shadow, new Vector3(shadowPos.x, shadowPos.y + yModifiedValue, shadowPos.z), shadowYDownDuration);
        }

        private void OnDisable() {
            card.onPointerEnterEvent.RemoveListener(HandleOnPointerEnter);
            card.onPointerExitEvent.RemoveListener(HandleOnPointerExit);
            card.OnBeginDragEvent.RemoveListener(HandleOnBeginDrag);
            card.OnEndDragEvent.RemoveListener(HandleOnEndDrag);
            card.OnPointerDownEvent.RemoveListener(HandleOnPointerDown);
        }

        private void Update() {
            if (!card) return;

            SmoothFollow();
        }

        private void HandleOnPointerEnter(CardController _)
        {
            if (IsAnimating()) return;
            OnPointerEnter(card);
        }

        private void HandleOnPointerExit(CardController _)
        {
            if (IsAnimating()) return;
            OnPointerExit(card);
        }

        private void HandleOnBeginDrag(CardController _)
        {
            if (IsAnimating()) return;
            OnBeginDrag(card);
        }

        private void HandleOnEndDrag(CardController _)
        {
            if (IsAnimating()) return;
            OnEndDrag(card);
        }

        private void HandleOnPointerDown(CardController _)
        {
            if (IsAnimating()) return;
            OnPointerDown(card);
        }

        private void SmoothFollow() {
            transform.position = Vector3.Lerp(transform.position,  baseCardTf.position, followSpeed * Time.deltaTime);
        }

        private bool IsAnimating() => _isFlipping || _isScaling;

        public void FlipCard() {
            _isFlipping = true;
            LeanTween.rotate(gameObject, new Vector3(0, 180, 0), 0.5f).setOnComplete(() => _isFlipping = false);
        }
    }
}
