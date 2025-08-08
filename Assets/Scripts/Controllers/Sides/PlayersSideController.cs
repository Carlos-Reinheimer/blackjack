using Deck;

namespace Controllers.Sides {
    public class PlayersSideController : SideController {
        protected override void OnCardInstantiated(CardController cardController, Card card) {
            HandleInstantiatedCard(cardController, card);
        }

        protected override void OnStand() { }
    }
}
