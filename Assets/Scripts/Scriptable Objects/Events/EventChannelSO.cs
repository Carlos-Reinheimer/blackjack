using System;
using UnityEngine;

namespace Scriptable_Objects.Events {
    [CreateAssetMenu(fileName = "EventChannelSO", menuName = "Scriptable Objects/EventChannelSO")]
    public class EventChannelSO<T> : ScriptableObject {
        public event Action<T> OnEventRaised;
        public void Raise(T value) => OnEventRaised?.Invoke(value);
    }
}
