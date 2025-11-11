using Scriptable_Objects;
using UnityEngine.Events;

namespace UI.Events.Main_Menu {
    public enum MainMenuAction
    {
        StartRun,
        JokerShop,
        Settings,
        Quit
    }

    public struct GameInfoSchema {
        public string gameVersion;
        public int globalScore;
    }
    
    public struct CreateJokerShopCardSchema {
        public JokerCard jokerCard;
        public int index;
        public UnityAction<JokerCard, UnityAction> purchaseJokerCallback;
    }
}