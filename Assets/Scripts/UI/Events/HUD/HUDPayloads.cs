using Controllers.Sides;
using JetBrains.Annotations;

namespace UI.Events.HUD
{
    public enum HudAction
    {
        Hit,
        Stand,
        Bet
    }

    public enum ScoreComboAction
    {
        StartWriter,
        StopWriter
    }
    
    public struct BetChannelContract
    {
        public SideController sideController;
        public int bet;
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
