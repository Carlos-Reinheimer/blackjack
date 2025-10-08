using Controllers.Sides;

namespace UI.Events.HUD
{
    public enum HudAction
    {
        Hit,
        Stand,
        Bet
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

    public struct ScoreChannelContract
    {
        public SideController sideController;
        public int score;
    }
}
