using System;
using System.Collections.Generic;
using UI.Events.HUD;
using UnityEngine;
using Utils;

namespace Controllers.Sides {
    public class PlayersScoreHandler : ScoreHandler {
        
        private int _currentOperationIndex;
        private int _currentMatchScore;
        private int _currentMatchMultiplier;
        
        protected override void AssembleScoreOperationSteps() {
            operations = new Dictionary<int, Action> {
                { 0, HandleCurrentCardSum},
                { 1, HandleDistanceToTargetMultiplier},
                { 2, HandleRunScore}
            };
        }

        private void RaiseNewScoreChannelUpdate(int previousValue, int nextValue, OperationValueType valueType) {
            scoreChannel.Raise(new ScoreChannelContract {
                sideController = sideController,
                previousValue = previousValue,
                nextValue = nextValue,
                valueType = valueType
            });
        }
        
        private void HandleCurrentCardSum() {
            Operate(new OperationData {
                operationValue = sideController.currentCardSum,
                operation = Operation.Add,
                valueType = OperationValueType.MatchScore
            });
        }

        private void HandleDistanceToTargetMultiplier() {
            Operate(new OperationData {
                operationValue = GetOptimalMultiplier(),
                operation = Operation.Add,
                valueType = OperationValueType.Multiplier
            });
        }

        private void HandleRunScore() {
            Operate(new OperationData {
                operationValue = currentRoundMultiplier,
                operation = Operation.Multiply,
                valueType = OperationValueType.MatchScore
            });
        }
        
        private void UpdateCurrentScore(int newValue) {
            // NOTE: this is not something I would usually want to do, but it's possible to alter the base score
            var previousScore = RunStats.CurrentScore;
            RunStats.CurrentScore += newValue;

            var finalScore = RunStats.CurrentScore <= 0 ? 0 : RunStats.CurrentScore;
            RaiseNewScoreChannelUpdate(previousScore, finalScore, OperationValueType.Score);
        }
        
        private void UpdateCurrentMultiplier(int newValue) {
            var previousMultiplier = currentRoundMultiplier;
            currentRoundMultiplier += newValue;

            var finalMultiplier = currentRoundMultiplier <= 0 ? 0 : currentRoundMultiplier;
            RaiseNewScoreChannelUpdate(previousMultiplier, finalMultiplier, OperationValueType.Multiplier);
        }
        
        private void UpdateCurrentMatchScore(int newValue) {
            var previousMatchScore = currentMatchScore;
            currentMatchScore = newValue;

            var finalMultiplier = currentMatchScore <= 0 ? 0 : currentMatchScore;
            RaiseNewScoreChannelUpdate(previousMatchScore, finalMultiplier, OperationValueType.MatchScore);
        }
        
        private void Operate(OperationData operationData) {
            _operationsToString.TryGetValue(operationData.operation, out var operatorString);
            var composedOperation = $"{operatorString}{operationData.operationValue}";

            scoreComboChannel.Raise(new ScoreComboChannelContract {
                sideController = sideController,
                action = ScoreComboAction.StartWriter,
                comboText = $"<color={ScoreSettings.GetColorByOperationValueType(operationData.valueType)}>" +
                            $"<+grow>" +
                            $"<grow amplitude=3>" +
                            $"{composedOperation}" +
                            $"</grow>" +
                            $"</+grow><!wait=1.2>" +
                            $"</color>"
            });

            var valueType = operationData.valueType;
            var operationValue = valueType switch {
                OperationValueType.Score => RunStats.CurrentScore,
                OperationValueType.MatchScore => currentMatchScore,
                _ => currentRoundMultiplier
            };
            var updatedValue = operationData.operation switch {
                Operation.Add => operationValue + operationData.operationValue,
                Operation.Subtract => operationValue - operationData.operationValue,
                Operation.Multiply => operationValue * operationData.operationValue,
                Operation.Divide => operationValue / operationData.operationValue,
                _ => throw new ArgumentOutOfRangeException(nameof(operationData.operation), operationData.operation, null)
            };
            
            switch (valueType) {
                case OperationValueType.Score:
                    UpdateCurrentScore((int)updatedValue);
                    break;
                case OperationValueType.Multiplier:
                    UpdateCurrentMultiplier((int)updatedValue);
                    break;
                case OperationValueType.MatchScore:
                default:
                    UpdateCurrentMatchScore((int)updatedValue);
                    break;
            }
        }

        private int GetOptimalMultiplier() {
            var targetValue = MainController.Instance.GetCurrentTargetValue();
            var difference = Math.Abs(targetValue - sideController.currentCardSum); // guarantee I'm working with Natural numbers

            return difference switch {
                0 => 10,
                1 => 8,
                2 => 7,
                3 => 6,
                4 => 5,
                5 => 4,
                6 => 3,
                7 => 2,
                _ => 1
            };
        }
        
        public void TryOperate() {
            if (operations == null) return;
            if (operations.TryGetValue(_currentOperationIndex, out var action)) {
                action?.Invoke();
            }
            else {
                Debug.Log("Finished operations dict");
                // sum current match score with the RunScore;
                var previousScore = RunStats.CurrentScore;
                RunStats.CurrentScore += currentMatchScore;
                
                RaiseNewScoreChannelUpdate(previousScore, RunStats.CurrentScore, OperationValueType.Score);
                RaiseNewScoreChannelUpdate(currentMatchScore, 0, OperationValueType.MatchScore);
                
                currentMatchScore = 0;
                _currentOperationIndex = 0;
                ((PlayersSideController)sideController).FinishLoop();
            }
        }
        
        public void HandleNextOperation() {
            _currentOperationIndex++;
            scoreComboChannel.Raise(new ScoreComboChannelContract {
                sideController = sideController,
                action = ScoreComboAction.StopWriter
            });
            
            TryOperate();
        }
    }
}
