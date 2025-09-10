using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Controllers {
    public class DeckController : MonoBehaviour {

        [Header("Game Definitions")]
        public List<DeckSo> decks;

        [Header("Settings")] 
        public int cardsLeftBeforeAutoShuffle = 3;
        
        [Header("UI")]
        public TMP_Text cardsLeftText;
        
        private Dictionary<string, Card> _cardLookupDict;
        private Dictionary<string, int> _keyToIndexReference;
        private List<int> _drawOrder; // list of indexes | shuffled
        private System.Random _rng;
        private int _deckCount;
        private int _cardsCount;

        private List<string> _shoe; // list of card keys (from the lookup dict) | NOT shuffled
        
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

        private void RelateKeysToIndex(int cardsCount) {
            // to remove specific cards, I need to have a reference between the key and their index in the _drawOrder
            // this will have to be updated each time a card is removed || the deck is shuffled
            
            // TODO: continue from here!
            
            _keyToIndexReference = new Dictionary<string, int>(cardsCount);
            for (var i = 0; i < decks.Count; i++) {
                foreach (var card in decks[i].cards) {
                    _keyToIndexReference[$"{i}_{card.type}_{card.suit}_{card.name}_{card.value}"] = 0;
                }
            }
        }
        
        private void AssembleShoe(int totalAmountOfCards) {
            Debug.Log("Assembling shoe...");
            // building the Shoe for multiple decks
            _shoe = new List<string>(totalAmountOfCards);
            foreach (var card in _cardLookupDict) {
                _shoe.Add(card.Key);
            }
        }

        private List<int> FisherYatesShuffle(List<int> targetList, int totalCount) {
            var n = totalCount;
            while (n > 1) {  
                n--;  
                var k = _rng.Next(n + 1);  
                (targetList[k], targetList[n]) = (targetList[n], targetList[k]);
            }

            return targetList;
        }

        // private void DebugDrawOrder(string title) {
        //     Debug.Log($"-------- {title} --------");
        //     Debug.Log(string.Join(", ", _drawOrder));
        //     Debug.Log($"-------------------------");
        // }

        private void ResetDrawOrder(int cardsCount) {
            _drawOrder?.Clear();
            _drawOrder = Enumerable.Range(0, cardsCount).ToList(); // to avoid messing up the original list
            _drawOrder = FisherYatesShuffle(_drawOrder, cardsCount);
        }

        private void ShuffleCurrentDeck() {
            Debug.Log("Shuffling entire deck...");

            var lastItems = new List<int>();
            if (_drawOrder?.Count > 0) {
                Debug.Log($"Trying to shuffle while there are {_drawOrder.Count} cards left");
                // here, I need to save {cardsLeftBeforeAutoShuffle} and add it at the end of the new shuffled list
                lastItems.AddRange(_drawOrder);
            }
            
            var totalCardsCount = _cardsCount - lastItems.Count; // lastItems is empty most of the time
            ResetDrawOrder(totalCardsCount);

            if (lastItems.Count <= 0) return;
            _drawOrder?.AddRange(lastItems);
            lastItems.Clear();
        }

        public void CleanDeckShuffle() {
            Debug.Log("Performing a clean deck shuffle");
            ResetDrawOrder(_cardsCount);
        }

        private void BuildCardsDictionary(int totalAmountOfCards) {
            // composed key is: _deckCount_CardType_CardSuit_name_value
            _cardLookupDict = new Dictionary<string, Card>(totalAmountOfCards);
            
            for (var i = 0; i < decks.Count; i++) {
                foreach (var card in decks[i].cards) {
                    _cardLookupDict[$"{i}_{card.type}_{card.suit}_{card.name}_{card.value}"] = card;
                }
            }
        }
        
        private void UpdateCardsLeftCount() {
            cardsLeftText.text = GetCardsLeft().ToString();
        }
        
        public void StartGame(UnityAction callback) {
            _rng = new System.Random(); // set seed here
            _cardsCount = decks.Sum(deck => deck.cards.Count);

            BuildCardsDictionary(_cardsCount);
            AssembleShoe(_cardsCount);
            ShuffleCurrentDeck();
            UpdateCardsLeftCount();
            callback?.Invoke();
        }

        public Card DrawTopCard() {
            if (_drawOrder == null || _drawOrder.Count == 0) return null;
            var index = _drawOrder[^1]; // last one
            _drawOrder.RemoveAt(_drawOrder.Count - 1);
            _cardLookupDict.TryGetValue(_shoe[index], out var card);
            if (_drawOrder.Count <= cardsLeftBeforeAutoShuffle) ShuffleCurrentDeck();
            UpdateCardsLeftCount();
            return card;
        }

        public int GetCardsLeft() => _drawOrder.Count;
    }
}
