using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptable_Objects {

    [Serializable]
    public class RoundSettings
    {
        public int dealersLifeChips;
    }
    
    [CreateAssetMenu(fileName = "GameRules", menuName = "Scriptable Objects/GameRules")]
    public class GameRules : ScriptableObject {

        public int targetValue;
        public int initialPlayersLifeChipValue;
        public List<RoundSettings> roundSettings;

    }
}
