using Action = System.Action;

namespace UI.Events.Next_Round {

    public struct AdvanceRoundModel {
        public int nextRound;
        public Action completeCallback;
    }
    
}
