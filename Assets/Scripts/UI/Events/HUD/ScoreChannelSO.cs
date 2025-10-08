using Scriptable_Objects.Events;
using UnityEngine;

namespace UI.Events.HUD
{
    [CreateAssetMenu(menuName = "Events/HUD/ScoreChannel")]
    public class ScoreChannelSO : EventChannelSO<ScoreChannelContract> {}
}
