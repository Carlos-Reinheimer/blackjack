using System;
using System.Collections.Generic;
using System.Linq;
using Deck;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils.UI_Animations;

namespace Controllers.Sides {
    
    public enum SideType
    {
        Player,
        Dealer
    }

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public class OperationData {
        public float operationValue;
        public Operation operation;
    }
    
    public class InitialParams {
        public UnityAction standCallback;
        
        public InitialParams() { }

        public InitialParams(InitialParams initialParams) {
            standCallback = initialParams.standCallback;
        }
    }
    
    public abstract class SideController : MonoBehaviour {
        
        [Header("Settings")]
        public int targetValue = 21;
        public SideType side;
        
        [Header("Prefabs")]
        public GameObject cardPrefab;
        public GameObject cardVisualPrefab;

        [Header("UI")]
        public Transform cardsContentTf;
        public UpdateValueOverTimeTween currentSumText;
        public UpdateValueOverTimeTween scoreText;
        public TMP_Text comboText;
        public TMPWriter comboTmpWriter;
        public TMPAnimator comboTmpAnimator;

        private List<OperationData> _operations;
        private int _currentOperationIndex;
        private bool _isDecreasingScore;
        private int _currentScore;
        private InitialParams _initialParams;
        protected int CurrentCardSum;
        protected List<Card> ActiveCards;
        protected List<GameObject> ActiveCardsGo;

        private readonly Dictionary<Operation, string> _operationsToString = new() {
            { Operation.Add, "+" },
            { Operation.Subtract, "-" },
            { Operation.Multiply, "x" },
            { Operation.Divide, "/" },
        };

        protected abstract void OnCardInstantiated(CardController cardController, Card card);
        protected abstract void OnFinishStand(UnityAction callback);

        protected void HandleInstantiatedCard(Card card, UnityAction callback = null) {
            UpdateTotalSum(card.value, callback);
        }

        private void Start() {
            ActiveCards = new List<Card>();
            ActiveCardsGo = new List<GameObject>();
        }

        private int GetBestPossibleSum(int newTempTotal) {
            var aces = ActiveCards.Where(card => card.type == CardType.Ace).ToList();
            var acesCount = aces.Count;
            if (acesCount == 0) return newTempTotal;
            
            var totalWithoutAces = ActiveCards.Where(card => card.type != CardType.Ace && card.type != CardType.Joker).Sum(card => card.value);
            var totalAllAcesSecondValue = ActiveCards.Where(card => card.type == CardType.Ace).Sum(card => card.secondValue);
            var virtualTotal = totalWithoutAces + totalAllAcesSecondValue;
                
            while (virtualTotal > targetValue && acesCount > 0) {
                acesCount--;
                virtualTotal -= 10;
            }
            
            return virtualTotal;
        }
        
        private void UpdateTotalSum(int value , UnityAction callback = null) {
            var previousCardSum = CurrentCardSum;
            
            // verify Aces here
            var tempTotal = CurrentCardSum + value;
            CurrentCardSum = GetBestPossibleSum(tempTotal);

            currentSumText.UpdateTargetValues(previousCardSum, CurrentCardSum);
            currentSumText.StartTween();

            if (CurrentCardSum > targetValue) HandleCrossTargetValue();
            else callback?.Invoke();
        }

        private void StartOperations() {
            _operations ??= new List<OperationData>();
            
            _operations.Add(new OperationData {
                operationValue = ActiveCards.Count,
                operation = Operation.Add
            });
            _operations.Add(new OperationData {
                operationValue = CurrentCardSum,
                operation = Operation.Multiply
            });
            _operations.Add(new OperationData {
                operationValue = CurrentCardSum,
                operation = Operation.Add
            });

            Operate(_operations[_currentOperationIndex]);
        }
        
        private void UpdateCurrentScore(int newValue) {
            var previousScore = _currentScore;
            if (_isDecreasingScore) {
                _currentScore -= newValue;
                if (_currentScore <= 0) _currentScore = 0;
            }
            else _currentScore += newValue;

            var finalScore = _currentScore <= 0 ? 0 : _currentScore;
            UpdateScoreUI(previousScore, finalScore);
        }
        
        private void UpdateScoreUI(float previousScore, float currentScore) {
            scoreText.UpdateTargetValues(previousScore, currentScore);
            scoreText.StartTween();
        }
        
        private void HandleCrossTargetValue() {
            _isDecreasingScore = true;
            StartOperations();
        }
        
        private void Operate(OperationData operationData) {
            _operationsToString.TryGetValue(operationData.operation, out var operatorString);
            var composedOperation = $"{operatorString}{operationData.operationValue}";

            comboText.text = $"<+grow><grow amplitude=3>{composedOperation}</grow></+grow><!wait=1.2>";
            comboTmpWriter.StartWriter();
            comboTmpAnimator.StartAnimating();
            var newScore = operationData.operation switch {
                Operation.Add => _currentScore + operationData.operationValue,
                Operation.Subtract => _currentScore - operationData.operationValue,
                Operation.Multiply => _currentScore * operationData.operationValue,
                Operation.Divide => _currentScore / operationData.operationValue,
                _ => throw new ArgumentOutOfRangeException(nameof(operationData.operation), operationData.operation, null)
            };
            
            UpdateCurrentScore((int)newScore);
        }

        public void HandleNextOperation() {
            _currentOperationIndex++;
            comboTmpWriter.StopWriter();
            comboTmpAnimator.StopAnimating();
            comboText.text = "";
            
            if (_currentOperationIndex == _operations.Count) {
                _currentOperationIndex = 0;
                _operations.Clear();
                _isDecreasingScore = false;
                OnFinishStand(_initialParams.standCallback);
                return;
            }

            Operate(_operations[_currentOperationIndex]);
        }
        
        public void InstantiateNewCard(Card card, Transform visualHandlerTf) {
            var newCard = Instantiate(cardPrefab, cardsContentTf);
            var newCardVisual = Instantiate(cardVisualPrefab, visualHandlerTf);
            var cardController = newCard.GetComponent<CardController>();
            var cardVisual = (DefaultCardVisual)newCardVisual.GetComponent<GeneralCardVisual>();
            
            cardController.OnInstantiated(side);
            cardVisual.StartVisual(cardController, card.value);
            ActiveCards.Add(card);
            ActiveCardsGo.Add(newCard);
            OnCardInstantiated(cardController, card);
        }
        
        public void Stand() {
            StartOperations();
        }

        public void ResetHand() {
            CurrentCardSum = 0;
            UpdateTotalSum(CurrentCardSum);
            foreach (var activeCard in ActiveCardsGo) {
                Destroy(activeCard);
            }
            
            ActiveCards.Clear();
            ActiveCardsGo.Clear();
        }

        public void SetInitialParams(InitialParams initialParams) {
            _initialParams = new InitialParams(initialParams);
        }
    }
}
