using Scriptable_Objects.Events;
using UnityEngine;

namespace UI.Events.HUD
{
    [CreateAssetMenu(menuName = "Events/HUD/ActionsChannel")]
    public class HUDActionsChannelSO : EventChannelSO<HudAction> {}
}
