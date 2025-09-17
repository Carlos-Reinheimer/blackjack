using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Scriptable_Objects {
    
    [Serializable]
    public class JokerCard: BaseCard {
        public JokerType jokerType;
        public bool isUnlocked = true;
        public Sprite unlockedSprite;
        public Sprite lockedSprite;
        [SerializeReference, SubclassSelector] public IJoker iJoker;
    }
    
    [CreateAssetMenu(fileName = "GameJokers", menuName = "Scriptable Objects/GameJokers")]
    public class GameJokers : ScriptableObject {
        
        public List<JokerCard> availableJokers;

    }
}
