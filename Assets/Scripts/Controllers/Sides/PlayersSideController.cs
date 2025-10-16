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
        private int _currentMatchScore;
        private int _currentMatchMultiplier;
        
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
            // NOTE: this is not something I would usually want to do, but it's possible to alter the base score
            var previousScore = RunStats.CurrentScore;
            RunStats.CurrentScore += newValue;

            var finalScore = RunStats.CurrentScore <= 0 ? 0 : RunStats.CurrentScore;
            scoreChannel.Raise(new ScoreChannelContract {
                sideController = this,
                previousValue = previousScore,
                nextValue = finalScore,
                valueType = OperationValueType.Score
            });
        }
        
        private void UpdateCurrentMultiplier(int newValue) {
            var previousMultiplier = currentRoundMultiplier;
            currentRoundMultiplier += newValue;

            var finalMultiplier = currentRoundMultiplier <= 0 ? 0 : currentRoundMultiplier;
            scoreChannel.Raise(new ScoreChannelContract {
                sideController = this,
                previousValue = previousMultiplier,
                nextValue = finalMultiplier,
                valueType = OperationValueType.Multiplier
            });
        }
        
        private void UpdateCurrentMatchScore(int newValue) {
            var previousMatchScore = currentMatchScore;
            currentMatchScore = newValue;

            var finalMultiplier = currentMatchScore <= 0 ? 0 : currentMatchScore;
            scoreChannel.Raise(new ScoreChannelContract {
                sideController = this,
                previousValue = previousMatchScore,
                nextValue = finalMultiplier,
                valueType = OperationValueType.MatchScore
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
            var difference = Math.Abs(targetValue - currentCardSum); // guarantee I'm working with Natural numbers

            return difference switch
            {
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
        
        public void StartOperations() {
            _operations ??= new List<OperationData>();
            
            _operations.Add(new OperationData {
                operationValue = currentCardSum,
                operation = Operation.Add,
                valueType = OperationValueType.MatchScore
            });
            _operations.Add(new OperationData {
                operationValue = GetOptimalMultiplier(),
                operation = Operation.Add,
                valueType = OperationValueType.Multiplier
            });
            _operations.Add(new OperationData {
                operationValue = GetOptimalMultiplier(), // TODO: this is not right. This current system of defining the values of the operations beforehand is not working
                operation = Operation.Multiply,
                valueType = OperationValueType.MatchScore
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
                // sum current match score with the RunScore;
                var previousScore = RunStats.CurrentScore;
                RunStats.CurrentScore += currentMatchScore;
                
                scoreChannel.Raise(new ScoreChannelContract {
                    sideController = this,
                    previousValue = previousScore,
                    nextValue = RunStats.CurrentScore,
                    valueType = OperationValueType.Score
                });
                scoreChannel.Raise(new ScoreChannelContract {
                    sideController = this,
                    previousValue = currentMatchScore,
                    nextValue = 0,
                    valueType = OperationValueType.MatchScore
                });
                
                currentMatchScore = 0;
                _currentOperationIndex = 0;
                _operations.Clear();
                LoopCompleted?.Invoke();
                
                return;
            }

            Operate(_operations[_currentOperationIndex]);
        }
        
        public void UpdateBetOptions() {
            var currentRoundSettings = MainController.Instance.currentRoundSettings;
            // handle min and max bet
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateMinBet,
                minBet = currentRoundSettings.minBet
            });
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateMaxBet,
                minBet = currentRoundSettings.maxBet
            });
            
            // reset current multipliers
            currentRoundMultiplier = 0;
        }
        
        public void SetBetStates() {
            var doesRoundAllowAllWin = MainController.Instance.currentRoundSettings.maxBet == -1;
            var canBetMore = GetCurrentBet() != livesChips;
            
            // TODO: this is considering the minBet = 1 (always)
            // if we're really gonna update that, we need to think how this is going to impact players if they don't have that min value available
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateMinusOneState,
                newState = false
            });
            
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateAllWinState,
                newState = doesRoundAllowAllWin && canBetMore
            });
            
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdatePlusOneState,
                newState = canBetMore
            });
        }
    }
}
