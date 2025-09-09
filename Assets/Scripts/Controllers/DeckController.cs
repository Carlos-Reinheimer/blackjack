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
        public int cardsLeftBeforeAutoShuffle = 47;
        
        [Header("UI")]
        public TMP_Text cardsLeftText;
        
        private Dictionary<string, Card> _cardLookupDict;
        private List<int> _drawOrder; // shuffle this
        private System.Random _rng;
        private int _deckCount;
        private int _cardsCount;

        private List<string> _shoe;

        private void AssembleShoe(int totalAmountOfCards) {
            Debug.Log("Assembling shoe...");
            // building the Shoe for multiple decks
            _shoe = new List<string>(totalAmountOfCards);
            foreach (var card in _cardLookupDict) {
                _shoe.Add(card.Key);
            }
            
            _cardsCount = _shoe.Count;
        }

        private void AddCardsAndShuffle() {
            Debug.Log("Shuffling entire deck...");

            var lastItems = new List<int>();
            if (_drawOrder?.Count > 0) {
                Debug.Log($"Trying to shuffle while there are {_drawOrder.Count} cards left");
                // here, I need to save {cardsLeftBeforeAutoShuffle} and add it at the end of the new shuffled list
                lastItems.AddRange(_drawOrder);
            }
            
            _drawOrder?.Clear();
            _drawOrder = Enumerable.Range(0, _cardsCount).ToList(); // to avoid messing up the original list
            // Fisher-Yates shuffle algorithm
            var n = _cardsCount;  
            while (n > 1) {  
                n--;  
                var k = _rng.Next(n + 1);  
                (_drawOrder[k], _drawOrder[n]) = (_drawOrder[n], _drawOrder[k]);
            } 
            
            if (lastItems.Count == 0 ) return;
            _drawOrder.AddRange(lastItems);
            lastItems.Clear();
        }

        private void BuildCardsDictionary(int totalAmountOfCards) {
            // composed key is: _deckCount_CardType_CardSuit_value
            _cardLookupDict = new Dictionary<string, Card>(totalAmountOfCards);
            
            for (var i = 0; i < decks.Count; i++) {
                foreach (var card in decks[i].cards) {
                    Debug.Log("$\"{i}_{card.type}_{card.suit}_{card.value}\": " + $"{i}_{card.type}_{card.suit}_{card.value}");
                    _cardLookupDict[$"{i}_{card.type}_{card.suit}_{card.value}"] = card;
                }
            }
        }
        
        private void UpdateCardsLeftCount() {
            cardsLeftText.text = GetCardsLeft().ToString();
        }
        
        public void StartGame(UnityAction callback) {
            _rng = new System.Random(); // set seed here
            var totalAmountOfCards = decks.Sum(deck => deck.cards.Count);

            BuildCardsDictionary(totalAmountOfCards);
            AssembleShoe(totalAmountOfCards);
            AddCardsAndShuffle();
            UpdateCardsLeftCount();
            callback?.Invoke();
        }

        public Card DrawTopCard() {
            if (_drawOrder == null || _drawOrder.Count == 0) return null;
            var index = _drawOrder[^1]; // last one
            _drawOrder.RemoveAt(_drawOrder.Count - 1);
            _cardLookupDict.TryGetValue(_shoe[index], out var card);
            
            if (_drawOrder.Count <= cardsLeftBeforeAutoShuffle) AddCardsAndShuffle();
            
            UpdateCardsLeftCount();
            return card;
        }

        public int GetCardsLeft() => _drawOrder.Count;
    }
}
