using System;
using System.Collections.Generic;

namespace Controllers {
    
    public static class CardKeying {

        public static string BuildKey(int deckIndex, CardType type, string suit, string name, int value) {
            return $"{deckIndex}_{type}_{ToTitle(suit)}_{ToTitle(name)}_{value}";
        }
        
        public sealed class CardFilter {
            public int? deckIndex { get; private set; }
            public CardType? type { get; private set; }
            public CardSuit? suit { get; private set; }          // null = any; set => case-insensitive compare
            public string name { get; private set; }          // null = any
            public int? value { get; private set; }           // null = any

            public static CardFilter Of(
                int? deckIndex = null,
                CardType? type = null,
                CardSuit? suit = null,
                string name = null,
                int? value = null
            ) => new() { deckIndex = deckIndex, type = type, suit = suit, name = name, value = value };
        }
        
        public static bool Matches(string key, CardFilter f) {
            if (!TryParse(key, out var k)) return false;

            return
                (!f.deckIndex.HasValue || k.DeckIndex == f.deckIndex.Value) &&
                (!f.type.HasValue      || k.Type == f.type.Value) &&
                (f.suit == null        || StringEquals(k.Suit, f.suit.ToString())) &&
                (f.name == null        || StringEquals(k.Name, f.name)) &&
                (!f.value.HasValue     || k.Value == f.value.Value);
        }
        
        public static int RemoveAllMatching<TKey, TValue>(IDictionary<TKey, TValue> dict, CardFilter f) where TKey : notnull {
            var toRemove = new List<TKey>();
            foreach (var kvp in dict) {
                if (kvp.Key is string keyStr && Matches(keyStr, f)) toRemove.Add(kvp.Key);
            }

            foreach (var k in toRemove) dict.Remove(k);
            return toRemove.Count;
        }
        
        private static bool TryParse(string key, out ParsedKey parsed) {
            parsed = default;
            if (string.IsNullOrEmpty(key)) return false;

            var parts = key.Split('_');
            if (parts.Length != 5) return false;

            if (!int.TryParse(parts[0], out var deck)) return false;
            if (!Enum.TryParse(parts[1], ignoreCase: true, out CardType type)) return false;
            var suit = parts[2];
            var name = parts[3];
            if (!int.TryParse(parts[4], out var value)) return false;

            parsed = new ParsedKey(deck, type, suit, name, value);
            return true;
        }

        private struct ParsedKey {
            public int DeckIndex;
            public CardType Type;
            public string Suit;
            public string Name;
            public int Value;

            public ParsedKey(int deckIndex, CardType type, string suit, string name, int value) {
                DeckIndex = deckIndex;
                Type = type;
                Suit = suit;
                Name = name;
                Value = value;
            }
        }
        
        private static bool StringEquals(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static string ToTitle(string s) {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..].ToLowerInvariant() : "");
        }
    }
}