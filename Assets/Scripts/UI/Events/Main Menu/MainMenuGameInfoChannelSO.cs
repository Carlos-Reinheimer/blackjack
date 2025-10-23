using Scriptable_Objects.Events;
using UnityEngine;

namespace UI.Events.Main_Menu
{
    [CreateAssetMenu(menuName = "Events/MainMenu/GameInfo")]
    public class MainMenuGameInfoChannelSO : EventChannelSO<GameInfoSchema> {}
}
