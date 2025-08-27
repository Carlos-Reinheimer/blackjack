using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    SingleValue,
    Faced,
    Ace,
    Joker
}

public enum CardSuit
{
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

[Serializable]
public class Card {
    public int value;
    public int secondValue = -1;
    public CardType type;
    public CardSuit suit;
    public Sprite sprite;

    public Card(Card card) {
        value = card.value;
        type = card.type;
        suit = card.suit;
        sprite = card.sprite;
    }
    
    public Card(int value, CardType type, CardSuit suit, Sprite sprite) {
        this.value = value;
        this.type = type;
        this.suit = suit;
        this.sprite = sprite;
    }
}

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/DeckSO")]
public class DeckSo : ScriptableObject {
    
    public List<Card> cards;
    
}
