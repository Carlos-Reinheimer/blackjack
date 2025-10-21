using Scriptable_Objects.Events;
using UnityEngine;

namespace UI.Events.Save_Game_Data
{
    [CreateAssetMenu(menuName = "Events/Save/GameDataChannel")]
    public class SaveGameDataEventChannel : EventChannelSO<bool> {}
}
