using System.Collections.Generic;
using System.Linq;
using Deck;
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

        protected bool isDecreasingScore;
        protected int currentCardSum;
        protected List<Card> activeCards;
        protected int currentScore;
        protected InitialParams initialParams;
        
        private List<GameObject> _activeCardsGo;

        protected abstract void OnCardInstantiated(GeneralCardVisual cardController, Card card);
        protected abstract void OnStand();

        protected void HandleInstantiatedCard(Card card, UnityAction callback = null) {
            UpdateTotalSum(card.value, callback);
        }

        private void Start() {
            activeCards = new List<Card>();
            _activeCardsGo = new List<GameObject>();
        }

        private int GetBestPossibleSum(int newTempTotal) {
            var aces = activeCards.Where(card => card.type == CardType.Ace).ToList();
            var acesCount = aces.Count;
            if (acesCount == 0) return newTempTotal;
            
            var totalWithoutAces = activeCards.Where(card => card.type != CardType.Ace && card.type != CardType.Joker).Sum(card => card.value);
            var totalAllAcesSecondValue = activeCards.Where(card => card.type == CardType.Ace).Sum(card => card.secondValue);
            var virtualTotal = totalWithoutAces + totalAllAcesSecondValue;
                
            while (virtualTotal > targetValue && acesCount > 0) {
                acesCount--;
                virtualTotal -= 10;
            }
            
            return virtualTotal;
        }
        
        private void UpdateTotalSum(int value , UnityAction callback = null) {
            var previousCardSum = currentCardSum;
            
            // verify Aces here
            var tempTotal = currentCardSum + value;
            currentCardSum = GetBestPossibleSum(tempTotal);

            currentSumText.UpdateTargetValues(previousCardSum, currentCardSum);
            currentSumText.StartTween();

            if (side == SideType.Player && currentCardSum > targetValue) HandleCrossTargetValue();
            else callback?.Invoke();
        }
        
        private void HandleCrossTargetValue() {
            isDecreasingScore = true;
            OnStand();
        }
        
        public void InstantiateNewCard(Card card, Transform visualHandlerTf) {
            var newCard = Instantiate(cardPrefab, cardsContentTf);
            var newCardVisual = Instantiate(cardVisualPrefab, visualHandlerTf);
            var cardController = newCard.GetComponent<CardController>();
            var cardVisual = (DefaultCardVisual)newCardVisual.GetComponent<GeneralCardVisual>();
            
            activeCards.Add(card);
            cardController.OnInstantiated(side);
            
            var shouldFlip = side == SideType.Player || side == SideType.Dealer && activeCards.Count > 1;
            cardVisual.StartVisual(cardController, card.value, shouldFlip);
            
            _activeCardsGo.Add(newCard);
            OnCardInstantiated(cardVisual, card);
        }
        
        public void Stand() {
            OnStand();
        }

        public void ResetHand() {
            currentCardSum = 0;
            UpdateTotalSum(currentCardSum);
            foreach (var activeCard in _activeCardsGo) {
                Destroy(activeCard);
            }
            
            activeCards.Clear();
            _activeCardsGo.Clear();
        }

        public void SetInitialParams(InitialParams @params) {
            initialParams = new InitialParams(@params);
        }
    }
}
