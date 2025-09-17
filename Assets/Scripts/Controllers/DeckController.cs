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
            // composed key is: _deckCount_CardType_CardSuit_name_value
            _cardLookupDict = new Dictionary<string, BaseCard>(_cardsCount);
            
            for (var i = 0; i < decks.Count; i++) {
                foreach (var card in decks[i].deckCards) {
                    var key = $"{i}_{card.type}_{card.suit}_{card.name}_{card.value}";
                    _cardLookupDict[key] = card;
                }
            }
        }
        
        private void UpdateCardsLeftCount() {
            cardsLeftText.text = GetCardsLeft().ToString();
        }
        
        public void StartGame(UnityAction callback) {
            _rng = new System.Random(); // set seed here
            _cardsCount = decks.Sum(deck => deck.deckCards.Count);

            BuildCardsDictionary();
            ShuffleCurrentDeck();
            UpdateCardsLeftCount();
            callback?.Invoke();
        }

        public BaseCard DrawTopCard() {
            if (_drawOrder == null || _drawOrder.Count == 0) return null;
            var index = _drawOrder[^1]; // last one
            var key = _cardLookupDict.ElementAt(index).Key;
            
            _drawOrder.RemoveAt(_drawOrder.Count - 1);
            _cardLookupDict.TryGetValue(key, out var card);
            RemoveIndexFromReference(key);
            
            if (_drawOrder.Count <= cardsLeftBeforeAutoShuffle) ShuffleCurrentDeck();
            UpdateCardsLeftCount();
            return card;
        }
        
        public void CleanDeckShuffle() {
            var removedKeys = CardKeying.RemoveAllMatching(
                _keyToIndexReference, 
                new CardKeying.CardFilter(
                    value: 7
                ));

            foreach (var key in removedKeys) {
                _keyToIndexReference.TryGetValue(key, out var drawOrderIndex);
                _drawOrder.Remove(drawOrderIndex);
                _keyToIndexReference.Remove(key);
            }
            UpdateCardsLeftCount();
            
            // Debug.Log("Performing a clean deck shuffle");
            // ResetDrawOrder(_cardsCount);
        }

        public int GetCardsLeft() => _drawOrder.Count;
    }
}
