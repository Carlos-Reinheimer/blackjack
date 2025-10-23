using UnityEngine;
using UnityEngine.EventSystems;

namespace Deck {
    public class JokerShopCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

        [Header("Shadow Parameters")] 
        [SerializeField] private GameObject shadow;
        [SerializeField] private float shadowYDownValue = 15;
        [SerializeField] private float shadowYDownDuration = 0.1f;

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        private void EffectShadow(bool enable) {
            var shadowPos = shadow.transform.localPosition;
            var yModifiedValue = enable ? -shadowYDownValue : +shadowYDownValue;
            LeanTween.moveLocal(shadow, new Vector3(shadowPos.x, shadowPos.y + yModifiedValue, shadowPos.z), shadowYDownDuration);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }
    }
}
