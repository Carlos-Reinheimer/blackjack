using System;
using System.Collections.Generic;
using Deck;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using Utils.UI_Animations;

namespace Controllers.Sides {
    public class PlayersSideController : SideController {
        
        public event Action LoopCompleted;
        
        [Header("UI")]
        public UpdateValueOverTimeTween scoreText;
        public TMP_Text comboText;
        public TMPWriter comboTmpWriter;
        public TMPAnimator comboTmpAnimator;
        
        private List<OperationData> _operations;
        private int _currentOperationIndex;
        
        private readonly Dictionary<Operation, string> _operationsToString = new() {
            { Operation.Add, "+" },
            { Operation.Subtract, "-" },
            { Operation.Multiply, "x" },
            { Operation.Divide, "/" },
        };
        
        protected override void OnCardInstantiated(GeneralCardVisual cardVisual, Card card) {
            HandleInstantiatedCard(card);
        }

        private void UpdateCurrentScore(int newValue) {
            var previousScore = currentScore;
            currentScore += newValue;

            var finalScore = currentScore <= 0 ? 0 : currentScore;
            UpdateScoreUI(previousScore, finalScore);
        }
        
        private void UpdateScoreUI(float previousScore, float nextScore) {
            scoreText.UpdateTargetValues(previousScore, nextScore);
            scoreText.StartTween();
        }
        
        private void Operate(OperationData operationData) {
            _operationsToString.TryGetValue(operationData.operation, out var operatorString);
            var composedOperation = $"{operatorString}{operationData.operationValue}";

            comboText.text = $"<+grow><grow amplitude=3>{composedOperation}</grow></+grow><!wait=1.2>";
            comboTmpWriter.StartWriter();
            comboTmpAnimator.StartAnimating();
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
            comboTmpWriter.StopWriter();
            comboTmpAnimator.StopAnimating();
            comboText.text = "";
            
            if (_currentOperationIndex == _operations.Count) {
                _currentOperationIndex = 0;
                _operations.Clear();
                LoopCompleted?.Invoke();
                return;
            }

            Operate(_operations[_currentOperationIndex]);
        }
    }
}
