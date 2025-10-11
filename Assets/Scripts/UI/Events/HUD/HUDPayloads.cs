using Controllers.Sides;
using JetBrains.Annotations;

namespace UI.Events.HUD
{
    public enum HudAction
    {
        Hit,
        Stand,
        BetPlusOne,
        BetMinusOne,
        BetAllWin
    }

    public enum ScoreComboAction
    {
        StartWriter,
        StopWriter
    }

    public enum BetAction
    {
        UpdateBetValue,
        UpdateMinBet,
        UpdateMaxBet,
        UpdateAllWinState,
        UpdateMinusOneState,
        UpdatePlusOneState,
    }
    
    public struct BetChannelContract
    {
        public BetAction betAction;
        [CanBeNull] public SideController sideController;
        public int? betAmount;
        public int? minBet;
        public int? maxBet;
        public bool? newState;
    }

    public struct ChipsChannelContract
    {
        public SideController sideController;
        public int amount;
    }
    
    public struct CardsSumContract
    {
        public SideController sideController;
        public int previousCardSum;
        public int newCurrentCardSum;
    }

    public struct ScoreChannelContract
    {
        public SideController sideController;
        public int previousScore;
        public int nextScore;
    }
    
    public struct ScoreComboChannelContract
    {
        public SideController sideController;
        public ScoreComboAction action;
        [CanBeNull] public string comboText;
    }
}
