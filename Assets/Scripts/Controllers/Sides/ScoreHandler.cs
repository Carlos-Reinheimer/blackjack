using System;
using System.Collections.Generic;
using UI.Events.HUD;
using UnityEngine;

namespace Controllers.Sides {
    public abstract class ScoreHandler : MonoBehaviour {

        [Header("Scripts")]
        public SideController sideController;
        
        [Header("Channels (SO assets)")]
        [SerializeField] protected ScoreChannelSO scoreChannel;
        [SerializeField] protected ScoreComboChannelSO scoreComboChannel;
        
        protected Dictionary<int, Action> operations;
        protected int currentMatchScore; // this is valid throughout the Match
        protected int currentRoundMultiplier; // this is valid throughout the Round
        
        protected readonly Dictionary<Operation, string> _operationsToString = new() {
            { Operation.Add, "+" },
            { Operation.Subtract, "-" },
            { Operation.Multiply, "x" },
            { Operation.Divide, "/" },
        };

        protected abstract void AssembleScoreOperationSteps();
        
        private void Start() {
            AssembleScoreOperationSteps();
        }

        public void SetRoundMultiplier(int newValue) {
            currentRoundMultiplier = newValue;
        }
    }
}
