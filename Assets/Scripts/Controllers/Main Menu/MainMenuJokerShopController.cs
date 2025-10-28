using Scriptable_Objects;
using UI.Events.Main_Menu;
using UnityEngine;

namespace Controllers.Main_Menu {
    public class MainMenuJokerShopController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private CreateJokerShopCardChannelSO createJokerShopCard;
        
        [Header("Scriptable Objects")]
        [SerializeField] private GameJokers gameJokers;

        private bool _areJokersLoaded;
        
        private void OnEnable() {
            actionChannel.OnEventRaised += HandleAction;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleAction;
        }
        
        private void HandleAction(MainMenuAction action) {
            if (action != MainMenuAction.JokerShop) return;
            TryLoadJokers();
        }

        private void TryLoadJokers() {
            if (_areJokersLoaded) return;
            _areJokersLoaded = true;

            for (var i = 0; i < gameJokers.availableJokers.Count; i++) {
                var joker = gameJokers.availableJokers[i];
                createJokerShopCard.Raise(new CreateJokerShopCardSchema {
                    jokerCard = joker,
                    index = i
                });
            }

        }

        public void SyncJokersWithPlayersUnlockedCards()
        {
            // TODO: call this from the MainMenuController.cs after loading the file
        }
    }
}
