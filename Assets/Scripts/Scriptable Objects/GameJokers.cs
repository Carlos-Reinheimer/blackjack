using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Scriptable_Objects {
    
    [Serializable]
    public class JokerCard: BaseCard {
        public string id;
        public JokerType jokerType;
        public bool isUnlocked;
        public Sprite unlockedSprite;
        public Sprite lockedSprite;
        public int unlockPrice;
        [SerializeReference, SubclassSelector] public IJoker iJoker;
    }
    
    [CreateAssetMenu(fileName = "GameJokers", menuName = "Scriptable Objects/GameJokers")]
    public class GameJokers : ScriptableObject {
        
        public List<JokerCard> availableJokers;

    }
}
