#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Utils {
    public class DeckPopulator : MonoBehaviour {
        
        [MenuItem("Card Game/Generate Standard Deck")]
        public static void GenerateDeck() {
            const string deckPath = "Assets/Resources/Deck.asset";
            const string spriteFolder = "Cards"; // inside Resources/Cards/

            // Create or load the DeckSO
            var deck = AssetDatabase.LoadAssetAtPath<DeckSo>(deckPath);
            if (deck == null) {
                deck = ScriptableObject.CreateInstance<DeckSo>();
                AssetDatabase.CreateAsset(deck, deckPath);
            }

            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit))) {
                for (var i = 2; i <= 10; i++) {
                    AddCard(deck, i, CardType.SingleValue, suit, spriteFolder, GetCardNameByValue(i));
                }

                AddCard(deck, 10, CardType.Faced, suit, spriteFolder, "Jack"); // Jack
                AddCard(deck, 10, CardType.Faced, suit, spriteFolder, "Queen"); // Queen
                AddCard(deck, 10, CardType.Faced, suit, spriteFolder, "King"); // King
                AddCard(deck, 1, CardType.Ace, suit, spriteFolder, "Ace");    // Ace
            }

            // Add Jokers
            // AddCard(deck, 0, CardType.Joker, CardSuit.Spades, spriteFolder, "BlackJoker"); // Black Joker
            // AddCard(deck, 0, CardType.Joker, CardSuit.Hearts, spriteFolder, "RedJoker"); // Red Joker

            EditorUtility.SetDirty(deck);
            AssetDatabase.SaveAssets();

            Debug.Log("Deck populated successfully.");
        }

        static void AddCard(DeckSo deck, int value, CardType type, CardSuit suit, string spriteFolder, string cardName) {
            // var spriteName = GetCardSpriteName(value, type, suit);
            const string spriteName = "Default Card";
            var sprite = Resources.Load<Sprite>($"{spriteFolder}/{spriteName}");

            if (sprite == null) {
                Debug.LogWarning($"Sprite not found for card: {spriteName}");
            }

            var isAce = type == CardType.Ace;
            var card = new Card(
                value: value, 
                secondValue: isAce ? 11 : -1, 
                type: type, 
                suit: suit, 
                sprite: sprite,
                name: cardName
                );

            deck.cards.Add(card);
        }

        static string GetCardSpriteName(int value, CardType type, CardSuit suit) {
            if (type == CardType.Joker) return suit == CardSuit.Hearts ? "JokerRed" : "JokerBlack";

            var rank = value switch {
                1 => "A",
                11 => "J",
                12 => "Q",
                13 => "K",
                _ => value.ToString()
            };

            return $"{rank}_of_{suit}";
        }
        
        static string GetCardNameByValue(int value) {
            var cardName = value switch {
                2 => "Two",
                3 => "Three",
                4 => "Four",
                5 => "Five",
                6 => "Six",
                7 => "Seven",
                8 => "Eight",
                9 => "Nine",
                _ => "Ten"
            };

            return cardName;
        }
    }
}
#endif
