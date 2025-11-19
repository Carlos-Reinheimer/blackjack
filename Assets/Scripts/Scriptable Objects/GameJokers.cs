using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Scriptable_Objects {
    
    [Serializable]
    public class JokerCard: BaseCard {
        // identification parameters
        public string id;
        public JokerType jokerType;
        
        // Joker shop parameters
        public bool isUnlocked;
        public Sprite unlockedSprite;
        public Sprite lockedSprite;
        public int unlockPrice;
        
        // in-game parameters
        [TextArea(10, 10)] public string actionDescription; // I think of this as something like Hades does, replacing parts of the text with icons or highlights that triggers other tooltips
        public int purchasePrice;
        public int availabilityAmount = 1;
        [SerializeReference, SubclassSelector] public IJoker iJoker;
    }
    
    [CreateAssetMenu(fileName = "GameJokers", menuName = "Scriptable Objects/GameJokers")]
    public class GameJokers : ScriptableObject {
        
        public List<JokerCard> availableJokers;

    }
}
