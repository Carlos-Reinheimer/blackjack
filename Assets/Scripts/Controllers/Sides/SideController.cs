using System;
using System.Collections.Generic;
using System.Linq;
using Deck;
using UI.Events.HUD;
using UnityEngine;
using UnityEngine.Events;

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
        public MainController mainController;
        
        [Header("Helpers")]
        [SerializeField] private int livesChips;
        [SerializeField] private int currentBet = 1;
        public int currentCardSum;
        public bool isBusted;
        
        [Header("Prefabs")]
        public GameObject cardPrefab;
        public GameObject cardVisualPrefab;
        
        [Header("UI")]
        public Transform cardsContentTf;
        
        [Header("Channels (SO assets)")]
        [SerializeField] private ChipsChannelSO chipsChannel;
        [SerializeField] private CardsSumChannelSO cardsSumChannel;
        [SerializeField] private HUDActionsChannelSO hudActionsChannel;
        [SerializeField] private BetChannelSO betChannel;

        protected List<DeckCard> activeCards;
        protected int currentScore;
        protected InitialParams initialParams;
        
        private List<GameObject> _activeCardsGo;
        private List<GameObject> _activeCardsVisuals;

        private void OnEnable() {
            hudActionsChannel.OnEventRaised += TryBet;
        }

        private void OnDisable() {
            hudActionsChannel.OnEventRaised -= TryBet;
        }

        protected abstract void OnCardInstantiated(GeneralCardVisual cardController, DeckCard deckCard);

        protected void HandleInstantiatedCard(DeckCard deckCard, UnityAction callback = null) {
            UpdateTotalSum(deckCard.value, callback);
        }

        private void Start() {
            activeCards = new List<DeckCard>();
            _activeCardsGo = new List<GameObject>();
            _activeCardsVisuals = new List<GameObject>();
            
            betChannel.Raise(new BetChannelContract {
                sideController = this,
                betAmount = currentBet
            });
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
            cardsSumChannel.Raise(new CardsSumContract {
                sideController = this,
                previousCardSum = previousCardSum,
                newCurrentCardSum = currentCardSum
            });

            var targetValue = MainController.Instance.GetCurrentTargetValue();
            if (currentCardSum > targetValue) HandleCrossTargetValue();
            else if (currentCardSum == targetValue) HandleBlackjack();
            else callback?.Invoke();
        }

        private void HandleBlackjack() {
            // handle visuals and stuff here
            Stand();
        }
        
        private void HandleCrossTargetValue() {
            isBusted = true;
            initialParams.bustedCallback?.Invoke();
        }
        
        private void TryBet(HudAction action) {
            if (action is HudAction.Hit or HudAction.Stand) return;
            if (side == SideType.Dealer) return;

            var newBet = 0;
            switch (action) {
                case HudAction.BetPlusOne:
                    newBet = BetPlusOne();
                    break;
                case HudAction.BetMinusOne:
                    newBet = BetMinusOne();
                    break;
                case HudAction.BetAllWin:
                    newBet = BetAllWin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
            
            betChannel.Raise(new BetChannelContract {
                sideController = this,
                betAmount = newBet
            });
        }

        private int BetMinusOne() {
            var newBet = currentBet - 1;
            var minBet = mainController.currentRoundSettings.minBet;
            if (newBet == minBet) {
                Debug.Log($"New bet is {newBet}, but chips left are {livesChips}. Setting newBet as previous value");
                newBet = currentBet;
            }
            else currentBet = newBet;

            return newBet;
        }
        
        private int BetPlusOne() {
            var newBet = currentBet + 1;
            var max = mainController.currentRoundSettings.maxBet;
            if (newBet == max || newBet == livesChips) {
                Debug.Log($"New bet is {newBet}, but chips left are {livesChips}. Setting newBet as previous value");
                newBet = currentBet;
            }
            else currentBet = newBet;

            return newBet;
        }
        
        private int BetAllWin() {
            var newBet = currentBet + 1;
            if (newBet == livesChips) {
                Debug.Log($"New bet is {newBet}, but chips left are {livesChips}. Setting newBet as previous value");
                newBet = currentBet;
            }
            else currentBet = newBet;

            return newBet;
        }
        
        public void InstantiateNewCard(DeckCard deckCard, Transform visualHandlerTf) {
            var newCard = Instantiate(cardPrefab, cardsContentTf);
            var newCardVisual = Instantiate(cardVisualPrefab, visualHandlerTf);
            var cardController = newCard.GetComponent<CardController>();
            var cardVisual = (DefaultCardVisual)newCardVisual.GetComponent<GeneralCardVisual>();
            
            activeCards.Add(deckCard);
            _activeCardsGo.Add(newCard);
            _activeCardsVisuals.Add(newCardVisual);
            
            cardController.OnInstantiated(side);
            
            var shouldFlip = side == SideType.Player || side == SideType.Dealer && activeCards.Count > 1;
            cardVisual.StartVisual(cardController, deckCard.value, shouldFlip);
            
            OnCardInstantiated(cardVisual, deckCard);
        }
        
        public void Stand() {
            initialParams.standCallback?.Invoke();
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

        public void TakeChip(int amount, UnityAction nextMatchCallback = null, UnityAction noChipsLeftCallback = null) {
            UpdateLivesChipsAmount(livesChips -= amount);
            if (livesChips > 0) {
                nextMatchCallback?.Invoke();
                return;
            }

            UpdateLivesChipsAmount(0);
            noChipsLeftCallback?.Invoke();
        }

        public void ReceiveChip(int amount, UnityAction callback = null) {
            UpdateLivesChipsAmount(livesChips += amount);
            
            callback?.Invoke();
        }
        
        public void UpdateLivesChipsAmount(int amount) {
            livesChips = amount;
            chipsChannel.Raise(new ChipsChannelContract {
                sideController = this,
                amount = livesChips
            });
        }
    }
}
