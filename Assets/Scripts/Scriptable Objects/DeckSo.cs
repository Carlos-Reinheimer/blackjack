using System;
using System.Collections.Generic;
using Interfaces;
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

public enum JokerType
{
    Active,
    Passive
}

[Serializable]
public class BaseCard {
    public CardType type;
    public string name;
}

[Serializable]
public class DeckCard : BaseCard {
    public int value;
    public int secondValue;
    public CardSuit suit;
    public Sprite sprite;
    
    public DeckCard(DeckCard baseCard) {
        value = baseCard.value;
        secondValue = baseCard.secondValue;
        type = baseCard.type;
        suit = baseCard.suit;
        sprite = baseCard.sprite;
        name = baseCard.name;
    }
    
    public DeckCard(int value, int secondValue, CardType type, CardSuit suit, Sprite sprite, string name) {
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
    
    public List<DeckCard> deckCards;
}
