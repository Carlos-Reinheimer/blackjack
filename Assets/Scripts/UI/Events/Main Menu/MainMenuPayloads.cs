
namespace UI.Events.Main_Menu
{
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
}