using System;
using Interfaces;
using UnityEngine;

namespace Deck.Jokers {
    [Serializable]
    public class BrokenHeartJoker : IJoker {

        public void PlayJoker() {
            Debug.Log("Playing BrokenHeartJoker");
        }
    }
}
