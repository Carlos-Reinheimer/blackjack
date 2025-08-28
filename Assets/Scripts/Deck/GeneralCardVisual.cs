using System;
using UnityEngine;

namespace Deck {
    public abstract class GeneralCardVisual : MonoBehaviour {
        
        [NonSerialized] private CardController card;

        [Header("Follow Parameters")] 
        [SerializeField] private GameObject shadow;
        [SerializeField] private float shadowYDownValue = 15;
        [SerializeField] private float shadowYDownDuration = 0.1f;

        [Header("Follow Parameters")] 
        [SerializeField] private float followSpeed = 30;

        protected Transform baseCardTf;
        protected Vector3 originalScale;
        protected Vector3 originalPos;
        protected RectTransform rt;
        protected int defaultSiblingIndex;
        
        private int savedIndex;
        private Vector3 rotationDelta;
        private Vector3 movementDelta;
        
        protected abstract void OnPointerEnter(CardController baseCard);
        protected abstract void OnPointerExit(CardController baseCard);
        protected abstract void OnBeginDrag(CardController baseCard);
        protected abstract void OnEndDrag(CardController baseCard);
        protected abstract void OnPointerDown(CardController baseCard);
        
        protected void InitiateCardVisual(CardController parentCard, bool autoFlip) {
            var tf = transform;
            card = parentCard;
            baseCardTf = card.transform;
            rt = (RectTransform)tf;

            if (autoFlip) FlipCard();
            
            card.onPointerEnterEvent.AddListener(OnPointerEnter);
            card.onPointerExitEvent.AddListener(OnPointerExit);
            card.OnBeginDragEvent.AddListener(OnBeginDrag);
            card.OnEndDragEvent.AddListener(OnEndDrag);
            card.OnPointerDownEvent.AddListener(OnPointerDown);
        }

        protected void EffectShadow(bool enable) {
            var shadowPos = shadow.transform.localPosition;
            var yModifiedValue = enable ? -shadowYDownValue : +shadowYDownValue;
            LeanTween.moveLocal(shadow, new Vector3(shadowPos.x, shadowPos.y + yModifiedValue, shadowPos.z), shadowYDownDuration);
        }

        private void OnDisable() {
            card.onPointerEnterEvent.RemoveListener(OnPointerEnter);
            card.onPointerExitEvent.RemoveListener(OnPointerExit);
            card.OnBeginDragEvent.RemoveListener(OnBeginDrag);
            card.OnEndDragEvent.RemoveListener(OnEndDrag);
            card.OnPointerDownEvent.RemoveListener(OnPointerDown);
        }

        private void Update() {
            if (!card) return;

            SmoothFollow();
        }

        private void SmoothFollow() {
            transform.position = Vector3.Lerp(transform.position,  baseCardTf.position, followSpeed * Time.deltaTime);
        }
        
        public void StartVisual(CardController parentCard) {
            InitiateCardVisual(parentCard, true); // for jokers
        }

        public void FlipCard() {
            LeanTween.rotate(gameObject, new Vector3(0, 180, 0), 0.5f);
        }
    }
}
