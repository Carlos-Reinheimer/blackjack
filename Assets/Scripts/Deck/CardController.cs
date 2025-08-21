using Controllers.Sides;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deck {
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

        public Selectable selectable;
        public bool isDragging;

        private Vector3 _offset;
        private Vector3 _initialPosBeforeDrag;
        private bool _isPlayersSide;

        [HideInInspector] public UnityEvent<CardController> onPointerEnterEvent;
        [HideInInspector] public UnityEvent<CardController> onPointerExitEvent;
        [HideInInspector] public UnityEvent<CardController> OnBeginDragEvent;
        [HideInInspector] public UnityEvent<CardController> OnEndDragEvent;
        
        public void OnInstantiated(SideType side) {
            _isPlayersSide = side == SideType.Player;
            selectable.enabled = _isPlayersSide;
            selectable.interactable = _isPlayersSide;
        }

        public void OnDrag(PointerEventData eventData) {
            if (!_isPlayersSide) return;

            transform.position = Input.mousePosition;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!_isPlayersSide) return;
            
            OnBeginDragEvent?.Invoke(this);
            isDragging = true;
            _initialPosBeforeDrag = transform.position;
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!_isPlayersSide) return;
            
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
    }
}
