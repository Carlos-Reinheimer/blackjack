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
    public CardType type;
    public CardSuit suit;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/DeckSO")]
public class DeckSo : ScriptableObject {
    
    public List<Card> cards;
    
}
