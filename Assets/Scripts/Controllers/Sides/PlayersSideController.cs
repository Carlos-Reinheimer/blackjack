using System;
using System.Collections.Generic;
using Deck;
using UI.Events.HUD;
using UnityEngine;
using Utils;

namespace Controllers.Sides {
    public class PlayersSideController : SideController {
        
        public event Action LoopCompleted;
        
        [SerializeField] private ScoreChannelSO scoreChannel;
        [SerializeField] private ScoreComboChannelSO scoreComboChannel;
        
        private List<OperationData> _operations;
        private int _currentOperationIndex;
        
        private readonly Dictionary<Operation, string> _operationsToString = new() {
            { Operation.Add, "+" },
            { Operation.Subtract, "-" },
            { Operation.Multiply, "x" },
            { Operation.Divide, "/" },
        };
        
        protected override void OnCardInstantiated(GeneralCardVisual cardVisual, DeckCard deckCard) {
            HandleInstantiatedCard(deckCard);
        }

        private void UpdateCurrentScore(int newValue) {
            var previousScore = currentScore;
            currentScore += newValue;

            var finalScore = currentScore <= 0 ? 0 : currentScore;
            scoreChannel.Raise(new ScoreChannelContract {
                sideController = this,
                previousScore = previousScore,
                nextScore = finalScore
            });
        }
        
        private void Operate(OperationData operationData) {
            _operationsToString.TryGetValue(operationData.operation, out var operatorString);
            var composedOperation = $"{operatorString}{operationData.operationValue}";

            scoreComboChannel.Raise(new ScoreComboChannelContract {
                sideController = this,
                action = ScoreComboAction.StartWriter,
                comboText = $"<+grow><grow amplitude=3>{composedOperation}</grow></+grow><!wait=1.2>"
            });

            var newScore = operationData.operation switch {
                Operation.Add => currentScore + operationData.operationValue,
                Operation.Subtract => currentScore - operationData.operationValue,
                Operation.Multiply => currentScore * operationData.operationValue,
                Operation.Divide => currentScore / operationData.operationValue,
                _ => throw new ArgumentOutOfRangeException(nameof(operationData.operation), operationData.operation, null)
            };
            
            UpdateCurrentScore((int)newScore);
        }
        
        public void StartOperations() {
            _operations ??= new List<OperationData>();
            
            _operations.Add(new OperationData {
                operationValue = activeCards.Count,
                operation = Operation.Add
            });
            _operations.Add(new OperationData {
                operationValue = currentCardSum,
                operation = Operation.Multiply
            });
            _operations.Add(new OperationData {
                operationValue = currentCardSum,
                operation = Operation.Add
            });

            Operate(_operations[_currentOperationIndex]);
        }

        public void HandleNextOperation() {
            _currentOperationIndex++;
            scoreComboChannel.Raise(new ScoreComboChannelContract {
                sideController = this,
                action = ScoreComboAction.StopWriter
            });
            
            if (_currentOperationIndex == _operations.Count) {
                RunStats.CurrentScore += currentScore;
                
                _currentOperationIndex = 0;
                _operations.Clear();
                LoopCompleted?.Invoke();
                return;
            }

            Operate(_operations[_currentOperationIndex]);
        }
    }
}
