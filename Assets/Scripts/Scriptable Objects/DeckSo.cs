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
    public int secondValue;
    public CardType type;
    public CardSuit suit;
    public Sprite sprite;
    public string name;

    public Card(Card card) {
        value = card.value;
        secondValue = card.secondValue;
        type = card.type;
        suit = card.suit;
        sprite = card.sprite;
        name = card.name;
    }
    
    public Card(int value, int secondValue, CardType type, CardSuit suit, Sprite sprite, string name) {
        this.value = value;
        this.secondValue = secondValue;
        this.type = type;
        this.suit = suit;
        this.sprite = sprite;
        this.name = name;
    }
}

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/DeckSO")]
public class DeckSo : ScriptableObject {
    
    public List<Card> cards;
    
}
