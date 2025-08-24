using UnityEngine;

namespace Deck {
    public class JokerCardVisual : GeneralCardVisual {
        
        [Header("Pointer Enter/Exit Parameters")] 
        [SerializeField] private float yUpAmount = 0.5f;
        [SerializeField] private float yUpAmountDuration = 0.2f;

        protected override void OnPointerEnter(CardController baseCard) {
            var pos = baseCard.transform.localPosition;
            defaultSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
            LeanTween.moveLocal(baseCard.gameObject, new Vector3(pos.x, pos.y + yUpAmount, pos.z), yUpAmountDuration);
            EffectShadow(true);
        }

        protected override void OnPointerExit(CardController baseCard) {
            transform.SetSiblingIndex(defaultSiblingIndex);
            var pos = baseCard.transform.localPosition;
            LeanTween.moveLocal(baseCard.gameObject, new Vector3(pos.x, pos.y - yUpAmount, pos.z), yUpAmountDuration);
            EffectShadow(false);
        }

        protected override void OnBeginDrag(CardController baseCard) { }
        protected override void OnEndDrag(CardController baseCard) { }
        protected override void OnPointerDown(CardController baseCard) {
            Debug.Log("click");
        }
    }
}
