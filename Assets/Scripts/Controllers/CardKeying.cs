using System;
using System.Collections.Generic;

namespace Controllers {
    
    public abstract class CardKeying {
        
        private struct ParsedKey {
            public int DeckIndex;
            public CardType Type;
            public CardSuit Suit;
            public string Name;
            public int Value;

            public ParsedKey(int deckIndex, CardType type, CardSuit suit, string name, int value) {
                DeckIndex = deckIndex;
                Type = type;
                Suit = suit;
                Name = name;
                Value = value;
            }
        }
        
        public class CardFilter {
            public int? deckIndex { get; private set; }
            public CardType? type { get; private set; }
            public CardSuit? suit { get; private set; }          // null = any; set => case-insensitive compare
            public string name { get; private set; }          // null = any
            public int? value { get; private set; }           // null = any
            
            public CardFilter (int? deckIndex = null, CardType? type = null, CardSuit? suit = null, string name = null, int? value = null) {
                this.deckIndex = deckIndex;
                this.type = type;
                this.suit = suit;
                this.name = name;
                this.value = value;
            }
        }
        
        private static bool IsKeyValid(string key, out ParsedKey parsed) {
            parsed = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('_');
            if (parts.Length != 5) return false;

            if (!int.TryParse(parts[0], out var deck)) return false;
            if (!Enum.TryParse(parts[1], ignoreCase: true, out CardType type)) return false;
            if (!Enum.TryParse(parts[2], ignoreCase: true, out CardSuit suit)) return false;
            var name = parts[3];
            if (string.IsNullOrEmpty(name)) return false;
            if (!int.TryParse(parts[4], out var value)) return false;

            parsed = new ParsedKey(deck, type, suit, name, value);
            return true;
        }
        
        private static bool StringEquals(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        
        public static bool Matches(string key, CardFilter f) {
            if (!IsKeyValid(key, out var k)) return false;

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

            // foreach (var k in toRemove) dict.Remove(k);
            return toRemove;
        }
        
        public static string BuildKey(int deckIndex, CardType type, CardSuit suit, string name, int value) {
            return $"{deckIndex}_{type}_{suit}_{name}_{value}";
        }
    }
}