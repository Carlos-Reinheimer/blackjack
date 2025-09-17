using System.Collections;
using Deck;
using UnityEngine;
using UnityEngine.Events;

namespace Controllers.Sides {
    public class DealersSideController : SideController {
        
        private const int StandValue = 17;
        
        private GeneralCardVisual _cardInHold;
        private DeckCard _holdDeckCard;
        
        protected override void OnCardInstantiated(GeneralCardVisual cardVisual, DeckCard deckCard) {
            var activeCardCount = activeCards.Count;
            if (activeCardCount == 1) {
                _cardInHold = cardVisual;
                _holdDeckCard = deckCard;
                return;
            }
            
            HandleInstantiatedCard(deckCard, activeCardCount == 2 ? null : HandleNewSum);
        }
        
        private IEnumerator CalculateSum() {
            yield return new WaitForSeconds(1);
            
            if (currentCardSum < StandValue) MainController.Instance.InstantiateNewCard();
            else initialParams.standCallback?.Invoke();
        }

        private void HandleNewSum() {
            StartCoroutine(CalculateSum());
        }

        public void ReleaseCurrentHoldCard(UnityAction handleInstantiatedCardCallback = null) {
            // handling the "hold" card that is the first card of the dealer
            _cardInHold.FlipCard();
            HandleInstantiatedCard(_holdDeckCard, handleInstantiatedCardCallback ?? HandleNewSum);
            _cardInHold = null;
            _holdDeckCard = null;
        }
    }
}
