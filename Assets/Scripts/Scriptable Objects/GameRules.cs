using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptable_Objects {

    [Serializable]
    public class RoundSettings
    {
        public int dealersLifeChips;
        public int minBet = 1;
        public int maxBet = -1; // -1 = player can All Win | Any other value = limit on the bet
    }
    
    [CreateAssetMenu(fileName = "GameRules", menuName = "Scriptable Objects/GameRules")]
    public class GameRules : ScriptableObject {

        public int targetValue;
        public int initialPlayersLifeChipValue;
        public List<RoundSettings> roundSettings;

    }
}
