using Controllers.Sides;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deck {

    public enum CardType
    {
        Default,
        Joker
    }
    
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

        public Selectable selectable;
        public bool isDragging;
        
        private Vector3 _offset;
        private Vector3 _initialPosBeforeDrag;
        private CardType _cardType;
        
        private bool _isPlayersSide;
        private bool _isJoker;
        
        [HideInInspector] public UnityEvent<CardController> onPointerEnterEvent;
        [HideInInspector] public UnityEvent<CardController> onPointerExitEvent;
        [HideInInspector] public UnityEvent<CardController> OnBeginDragEvent;
        [HideInInspector] public UnityEvent<CardController> OnEndDragEvent;
        [HideInInspector] public UnityEvent<CardController> OnPointerDownEvent;
        
        public void OnInstantiated(SideType side, CardType type = CardType.Default) {
            selectable.enabled = _isPlayersSide;
            selectable.interactable = _isPlayersSide;
            _cardType = type;
            
            _isPlayersSide = side == SideType.Player;
            _isJoker = type == CardType.Joker;
        }

        public void OnDrag(PointerEventData eventData) {
            if (!_isPlayersSide || _isJoker) return;

            transform.position = Input.mousePosition;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!_isPlayersSide || _isJoker) return;
            
            OnBeginDragEvent?.Invoke(this);
            isDragging = true;
            _initialPosBeforeDrag = transform.position;
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!_isPlayersSide || _isJoker) return;
            
            OnEndDragEvent?.Invoke(this);
            isDragging = false;
            transform.position = _initialPosBeforeDrag;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (!_isPlayersSide || isDragging) return;
            
            onPointerEnterEvent?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (!_isPlayersSide || isDragging) return;
            
            onPointerExitEvent?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!_isPlayersSide || isDragging) return;
            
            OnPointerDownEvent?.Invoke(this);
        }
    }
}
