using Deck;
using UnityEngine.Events;

namespace Controllers.Sides {
    public class PlayersSideController : SideController {
        protected override void OnCardInstantiated(CardController cardController, Card card) {
            HandleInstantiatedCard(cardController, card);
        }

        protected override void OnFinishStand(UnityAction callback) {
            callback?.Invoke();
        }
    }
}
