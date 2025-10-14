using System.Collections.Generic;
using System.Linq;
using Controllers.Sides;
using Scriptable_Objects;
using UI.Events.HUD;
using UnityEngine;
using UnityEngine.Events;

namespace Controllers {
    public class DeckController : MonoBehaviour {

        [Header("Game Definitions")]
        public List<DeckSo> decks;
        public GameJokers gameJokers;

        [Header("Settings")] 
        public int cardsLeftBeforeAutoShuffle = 3;
        
        [Header("Channels (SO assets)")]
        [SerializeField] private CardsLeftChannelSO cardsLeftChannel;
        
        private Dictionary<string, BaseCard> _cardLookupDict;
        private Dictionary<string, int> _keyToIndexReference;
        private List<int> _drawOrder; // list of indexes | shuffled
        private System.Random _rng;
        private int _deckCount;
        private int _cardsCount;

        #region Singleton
        public static DeckController Instance {
            get {
                if (_instance == null)
                    _instance = FindFirstObjectByType(typeof(DeckController)) as DeckController;

                return _instance;
            }
        }
        private static DeckController _instance;
        #endregion

        private List<int> FisherYatesShuffle(List<int> targetList, int totalCount) {
            var n = totalCount;
            while (n > 1) {  
                n--;  
                var k = _rng.Next(n + 1);  
                (targetList[k], targetList[n]) = (targetList[n], targetList[k]);
            }

            return targetList;
        }

        private void DebugDrawOrder(string title) {
            Debug.Log($"-------- {title} --------");
            Debug.Log(string.Join(", ", _drawOrder));
            Debug.Log($"-------------------------");
        }

        private void SetKeyToIndexReference() {
            _keyToIndexReference = new Dictionary<string, int>(_drawOrder.Count);
            foreach (var index in _drawOrder) {
                _keyToIndexReference[_cardLookupDict.ElementAt(index).Key] = index;
                // Debug.Log($"_keyToIndexReference ({index}): {_cardLookupDict.ElementAt(index).Key}");
            }
        }

        private void RemoveIndexFromReference(string key) {
            _keyToIndexReference.Remove(key);
        }

        private void ResetDrawOrder(int cardsCount) {
            // TODO: I probably have to check this out again, but I'm unsure at the moment, so we ball fn
            _drawOrder?.Clear();
            _drawOrder = Enumerable.Range(0, cardsCount).ToList();
            _drawOrder = FisherYatesShuffle(_drawOrder, cardsCount);
            SetKeyToIndexReference();
        }

        private void ShuffleCurrentDeck() {
            Debug.Log("Shuffling entire deck...");

            var lastItems = new List<int>();
            if (_drawOrder?.Count > 0) {
                // here, I need to save {cardsLeftBeforeAutoShuffle} and add it at the end of the new shuffled list
                Debug.Log($"Trying to shuffle while there are {_drawOrder.Count} cards left");
                lastItems.AddRange(_drawOrder);
            }
            
            var totalCardsCount = _cardsCount - lastItems.Count; // lastItems is empty most of the time
            ResetDrawOrder(totalCardsCount);

            if (lastItems.Count <= 0) return;
            _drawOrder?.AddRange(lastItems);
            lastItems.Clear();
        }

        private void BuildCardsDictionary() {
            // for playing cards, composed key is: deckCount_CardType_CardSuit_name_value
            // for joker cards, composed key is:  name_type
            _cardLookupDict = new Dictionary<string, BaseCard>(_cardsCount);
            
            for (var i = 0; i < decks.Count; i++) {
                foreach (var card in decks[i].deckCards) {
                    var key = $"{i}_{card.type}_{card.suit}_{card.name}_{card.value}";
                    _cardLookupDict[key] = card;
                }
            }

            foreach (var jokerCard in gameJokers.availableJokers) {
                if (!jokerCard.isUnlocked) return;
                var key = $"{jokerCard.name}_{jokerCard.type}";
                _cardLookupDict[key] = jokerCard;
            }
        }

        private void GetLastCardFromDrawOrder(SideType currentSide, int lookupIndex, out string key) {
            var index = _drawOrder[lookupIndex]; // last one
            key = _cardLookupDict.ElementAt(index).Key;
            
            var isJokerKeyValid = CardKeying.IsJokerKeyValid(key, out var isJokerKey);
            if (!isJokerKeyValid || currentSide == SideType.Player) return;
            
            // TODO: THIS IS REALLY NOT WORKING, PLEASE FIX IT
            Debug.Log("Card was a Joker but for the Dealers Side, so searching for the prior key");
            GetLastCardFromDrawOrder(currentSide, lookupIndex - 1, out var anotherKey);
        }
        
        public void StartGame(UnityAction callback) {
            _rng = new System.Random(); // set seed here
            _cardsCount = decks.Sum(deck => deck.deckCards.Count);
            _cardsCount += gameJokers.availableJokers.Count(j => j.isUnlocked);

            BuildCardsDictionary();
            ShuffleCurrentDeck();
            cardsLeftChannel.Raise(GetCardsLeft());
            callback?.Invoke();
        }

        public BaseCard DrawTopCard(SideType currentSide) {
            if (_drawOrder == null || _drawOrder.Count == 0) return null;

            var cardIndex = _drawOrder.Count - 1;
            GetLastCardFromDrawOrder(currentSide, cardIndex, out var key);
            _drawOrder.RemoveAt(cardIndex);
            _cardLookupDict.TryGetValue(key, out var card);
            RemoveIndexFromReference(key);
            
            if (_drawOrder.Count <= cardsLeftBeforeAutoShuffle) ShuffleCurrentDeck();
            cardsLeftChannel.Raise(GetCardsLeft());
            return card;
        }
        
        public void CleanDeckShuffle() {
            // var removedKeys = CardKeying.RemoveAllMatching(
            //     _keyToIndexReference, 
            //     new CardKeying.CardFilter(
            //         value: 7
            //     ));
            //
            // foreach (var key in removedKeys) {
            //     _keyToIndexReference.TryGetValue(key, out var drawOrderIndex);
            //     _drawOrder.Remove(drawOrderIndex);
            //     _keyToIndexReference.Remove(key);
            // }
            // UpdateCardsLeftCount();
            
            // Debug.Log("Performing a clean deck shuffle");
            // ResetDrawOrder(_cardsCount);
        }

        public int GetCardsLeft() => _drawOrder.Count;
    }
}
