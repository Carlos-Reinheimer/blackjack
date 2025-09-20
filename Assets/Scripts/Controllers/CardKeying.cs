using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers {
    
    public abstract class CardKeying {
        public struct ParsedCardKey {
            public int DeckIndex;
            public CardType Type;
            public CardSuit Suit;
            public string Name;
            public int Value;

            public ParsedCardKey(int deckIndex, CardType type, CardSuit suit, string name, int value) {
                DeckIndex = deckIndex;
                Type = type;
                Suit = suit;
                Name = name;
                Value = value;
            }
        }
        
        public struct ParsedJokerKey {
            public string Name;
            public CardType Type;

            public ParsedJokerKey(string name, CardType type) {
                Name = name;
                Type = type;
            }
        }
        
        public class CardFilter {
            public int? deckIndex { get; private set; }
            public CardType? type { get; private set; }
            public CardSuit? suit { get; private set; } // null = any
            public string name { get; private set; } // null = any
            public int? value { get; private set; } // null = any
            
            public CardFilter (int? deckIndex = null, CardType? type = null, CardSuit? suit = null, string name = null, int? value = null) {
                this.deckIndex = deckIndex;
                this.type = type;
                this.suit = suit;
                this.name = name;
                this.value = value;
            }
        }
        
        private static bool StringEquals(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        
        public static bool IsCardKeyValid(string key, out ParsedCardKey parsedCard) {
            parsedCard = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('_');
            if (parts.Length != 5) return false;

            if (!int.TryParse(parts[0], out var deck)) return false;
            if (!Enum.TryParse(parts[1], ignoreCase: true, out CardType type)) return false;
            if (!Enum.TryParse(parts[2], ignoreCase: true, out CardSuit suit)) return false;
            var name = parts[3];
            if (string.IsNullOrEmpty(name)) return false;
            if (!int.TryParse(parts[4], out var value)) return false;

            parsedCard = new ParsedCardKey(deck, type, suit, name, value);
            return true;
        }
        
        public static bool IsJokerKeyValid(string key, out ParsedJokerKey parsedCard) {
            parsedCard = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('_');
            if (parts.Length != 2) return false;

            var name = parts[0];
            if (string.IsNullOrEmpty(name)) return false;
            if (!Enum.TryParse(parts[1], ignoreCase: true, out CardType type)) return false;

            parsedCard = new ParsedJokerKey(name, type);
            return true;
        }
        
        public static bool Matches(string key, CardFilter f) {
            if (!IsCardKeyValid(key, out var k)) return false;

            return
                (!f.deckIndex.HasValue || k.DeckIndex == f.deckIndex.Value) &&
                (!f.type.HasValue      || k.Type == f.type.Value) &&
                (f.suit == null        || StringEquals(k.Suit.ToString(), f.suit.ToString())) &&
                (f.name == null        || StringEquals(k.Name, f.name)) &&
                (!f.value.HasValue     || k.Value == f.value.Value);
        }
        
        public static List<TKey> RemoveAllMatching<TKey, TValue>(IDictionary<TKey, TValue> dict, CardFilter f) where TKey : notnull {
            var toRemove = new List<TKey>();
            foreach (var kvp in dict) {
                if (kvp.Key is string keyStr && Matches(keyStr, f)) toRemove.Add(kvp.Key);
            }

            // I don't think I want to remove them here, but it's possible
            // foreach (var k in toRemove) dict.Remove(k);
            if (toRemove.Count == 0) Debug.Log("No cards to remove!");
            return toRemove;
        }
        
        public static List<TKey> FindAnyMatching<TKey, TValue>(IDictionary<TKey, TValue> dict, CardFilter f) where TKey : notnull {
            var found = new List<TKey>();
            foreach (var kvp in dict) {
                if (kvp.Key is string keyStr && Matches(keyStr, f)) found.Add(kvp.Key);
            }

            if (found.Count == 0) Debug.Log("No cards found :(");
            return found;
        }
        
        public static string BuildKey(int deckIndex, CardType type, CardSuit suit, string name, int value) {
            return $"{deckIndex}_{type}_{suit}_{name}_{value}";
        }
    }
}