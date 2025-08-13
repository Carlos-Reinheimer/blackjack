using Deck;
using UnityEngine.Events;

namespace Controllers.Sides {
    public class DealersSideController : SideController {
        
        private const int StandValue = 17;
        
        private CardController _holdCardController;
        private Card _holdCard;
        
        protected override void OnCardInstantiated(CardController cardController, Card card) {
            var activeCardCount = ActiveCards.Count;
            if (activeCardCount == 1) {
                _holdCardController = cardController;
                _holdCard = card;
                return;
            }
            
            HandleInstantiatedCard(cardController, card, activeCardCount == 2 ? null : HandleNewSum);
        }

        protected override void OnFinishStand(UnityAction callback) {
            callback?.Invoke();
        }
        
        private void HandleNewSum() {
            if (CurrentCardSum < StandValue) MainController.Instance.InstantiateNewCard();
            else Stand();
        }

        public void ReleaseCurrentHoldCard() {
            // handling the "hold" card that is the first card of the dealer
            HandleInstantiatedCard(_holdCardController, _holdCard, HandleNewSum);
            _holdCardController = null;
            _holdCard = null;
        }
    }
}
