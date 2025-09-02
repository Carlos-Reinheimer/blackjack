using System.Collections.Generic;
using System.Linq;
using Deck;
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
        public UnityAction bustedCallback;
        public int initialLives;
        
        public InitialParams() { }

        public InitialParams(InitialParams initialParams) {
            standCallback = initialParams.standCallback;
            bustedCallback = initialParams.bustedCallback;
            initialLives = initialParams.initialLives;
        }
    }
    
    public abstract class SideController : MonoBehaviour {
        
        [Header("Settings")]
        public SideType side;
        
        [Header("Helpers")]
        public int currentCardSum;
        public int livesChips;
        public bool isBusted;
        
        [Header("Prefabs")]
        public GameObject cardPrefab;
        public GameObject cardVisualPrefab;

        [Header("UI")]
        public Transform cardsContentTf;
        public UpdateValueOverTimeTween currentSumText;
        public TMP_Text livesChipsText;

        protected List<Card> activeCards;
        protected int currentScore;
        protected InitialParams initialParams;
        
        private List<GameObject> _activeCardsGo;
        private List<GameObject> _activeCardsVisuals;

        protected abstract void OnCardInstantiated(GeneralCardVisual cardController, Card card);
        protected abstract void OnStand();

        protected void HandleInstantiatedCard(Card card, UnityAction callback = null) {
            UpdateTotalSum(card.value, callback);
        }

        private void Start() {
            activeCards = new List<Card>();
            _activeCardsGo = new List<GameObject>();
            _activeCardsVisuals = new List<GameObject>();
        }

        private int GetBestPossibleSum(int newTempTotal) {
            var aces = activeCards.Where(card => card.type == CardType.Ace).ToList();
            var acesCount = aces.Count;
            if (acesCount == 0) return newTempTotal;
            
            var totalWithoutAces = activeCards.Where(card => card.type != CardType.Ace && card.type != CardType.Joker).Sum(card => card.value);
            var totalAllAcesSecondValue = activeCards.Where(card => card.type == CardType.Ace).Sum(card => card.secondValue);
            var virtualTotal = totalWithoutAces + totalAllAcesSecondValue;
                
            while (virtualTotal > MainController.Instance.GetCurrentTargetValue() && acesCount > 0) {
                acesCount--;
                virtualTotal -= 10;
            }
            
            return virtualTotal;
        }
        
        private void UpdateTotalSum(int value, UnityAction callback = null) {
            var previousCardSum = currentCardSum;
            
            // verify Aces here
            var tempTotal = currentCardSum + value;
            currentCardSum = GetBestPossibleSum(tempTotal);

            currentSumText.UpdateTargetValues(previousCardSum, currentCardSum);
            currentSumText.StartTween();

            if (currentCardSum > MainController.Instance.GetCurrentTargetValue()) HandleCrossTargetValue();
            else callback?.Invoke();
        }
        
        private void HandleCrossTargetValue() {
            isBusted = true;
            initialParams.bustedCallback?.Invoke();
        }
        
        public void InstantiateNewCard(Card card, Transform visualHandlerTf) {
            var newCard = Instantiate(cardPrefab, cardsContentTf);
            var newCardVisual = Instantiate(cardVisualPrefab, visualHandlerTf);
            var cardController = newCard.GetComponent<CardController>();
            var cardVisual = (DefaultCardVisual)newCardVisual.GetComponent<GeneralCardVisual>();
            
            activeCards.Add(card);
            _activeCardsGo.Add(newCard);
            _activeCardsVisuals.Add(newCardVisual);
            
            cardController.OnInstantiated(side);
            
            var shouldFlip = side == SideType.Player || side == SideType.Dealer && activeCards.Count > 1;
            cardVisual.StartVisual(cardController, card.value, shouldFlip);
            
            OnCardInstantiated(cardVisual, card);
        }
        
        public void Stand() {
            OnStand();
        }

        public void ResetHand() {
            currentCardSum = 0;
            isBusted = false;
            
            for (var i = 0; i < _activeCardsGo.Count; i++) {
                    Destroy(_activeCardsGo[i]);
                    Destroy(_activeCardsVisuals[i]);
            }
            
            activeCards.Clear();
            _activeCardsGo.Clear();
            _activeCardsVisuals.Clear();
            UpdateTotalSum(currentCardSum);
        }

        public void SetInitialParams(InitialParams @params) {
            initialParams = new InitialParams(@params);
            UpdateLivesChipsAmount(initialParams.initialLives);
        }

        public void TakeChip(int amount, UnityAction nextMatchCallback = null, UnityAction chipEndCallback = null) {
            UpdateLivesChipsAmount(livesChips -= amount);
            if (livesChips > 0) {
                nextMatchCallback?.Invoke();
                return;
            }

            UpdateLivesChipsAmount(0);
            chipEndCallback?.Invoke();
        }

        public void ReceiveChip(int amount, UnityAction nextMatchCallback = null) {
            UpdateLivesChipsAmount(livesChips += amount);
            
            nextMatchCallback?.Invoke();
        }
        
        public void UpdateLivesChipsAmount(int amount) {
            livesChips = amount;
            livesChipsText.text = livesChips.ToString();
        }
    }
}
