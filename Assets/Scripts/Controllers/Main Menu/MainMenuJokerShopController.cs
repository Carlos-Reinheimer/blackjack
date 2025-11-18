using System.Collections.Generic;
using Scriptable_Objects;
using UI.Events.Main_Menu;
using UI.Events.Save_Game_Data;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Controllers.Main_Menu {
    public class MainMenuJokerShopController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private CreateJokerShopCardChannelSO createJokerShopCard;
        [SerializeField] private SaveGameDataEventChannel saveGameDataEventChannel;
        [SerializeField] private MainMenuGameInfoChannelSO gameInfoChannel;

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

        private void PurchaseJoker(JokerCard jokerCard, UnityAction unlockCallback) {
            if (jokerCard.isUnlocked) {
                Debug.Log("Card is already unlocked yet");
                return;
            }

            if (jokerCard.unlockPrice > SaveGameData.coreData?.globalScore) {
                Debug.Log("Player do not have enough currency to buy the Joker");
                return;
            }

            SaveGameData.coreData ??= new CoreData();
            SaveGameData.coreData.unlockedJokers ??= new List<string>();
            SaveGameData.coreData.unlockedJokers.Add(jokerCard.id);
            SaveGameData.coreData.globalScore -= jokerCard.unlockPrice;
            SaveGameData.Save(SaveGameData.MAIN_SAVE_FILENAME, () => {
                SyncJokersWithPlayersUnlockedCards();
                unlockCallback?.Invoke();
                saveGameDataEventChannel.Raise(false);
                gameInfoChannel.Raise(new GameInfoSchema {
                    globalScore = SaveGameData.coreData.globalScore
                });
            }); // the callback here is to filter the shop to highlight which jokers are available to buy with the current score
        }

        private void TryLoadJokers() {
            if (_areJokersLoaded) return;
            _areJokersLoaded = true;

            var availableScore = SaveGameData.coreData.globalScore;
            for (var i = 0; i < gameJokers.availableJokers.Count; i++) {
                var joker = gameJokers.availableJokers[i];
                createJokerShopCard.Raise(new CreateJokerShopCardSchema {
                    jokerCard = joker,
                    index = i,
                    purchaseJokerCallback = PurchaseJoker,
                    availableScore = availableScore
                });
            }
        }

        public void SyncJokersWithPlayersUnlockedCards() {
            SaveGameData.coreData?.unlockedJokers?.ForEach(jokerId => {
                var indexOf = gameJokers.availableJokers.FindIndex(x => x.id == jokerId);
                if (indexOf != -1) gameJokers.availableJokers[indexOf].isUnlocked = true;
            });
        }
    }
}
