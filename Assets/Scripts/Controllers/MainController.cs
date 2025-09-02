using System.Collections;
using System.Collections.Generic;
using Controllers.Sides;
using Scriptable_Objects;
using TMPro;
using UI_Controllers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
    public class MainController : MonoBehaviour {

        [Header("Rules")]
        public GameRules gameRules;
        
        [Header("Scripts")]
        public SideController dealersSide;
        public SideController playersSide;
        public NextRoundTransitionCanvasUIController nextRoundCanvas;

        [Header("Data")]
        public DeckSo deck;
        public Transform visualHandlerTransform;

        [Header("Settings")]
        public int initialCardsCount = 4;
        public float restartTimer = 2;
        public float delayBetweenDealing = 1;

        [Header("UI")]
        public TMP_Text roundText;
        public TMP_Text cardsLeftText;
        public List<Button> actionButtons;
        
        private int _currentRound = -1;
        private List<Card> _availableCards;
        private SideType _currentPlayingSide = SideType.Dealer;
        
        #region Singleton
            public static MainController Instance {
                get {
                    if (_instance == null)
                        _instance = FindFirstObjectByType(typeof(MainController)) as MainController;

                    return _instance;
                }
            }
            private static MainController _instance;
        #endregion
        
        // private void OnEnable() {
        //     SceneLoader.OnSceneLoadCompleteCallback += StartGame;
        // }
        //
        // private void OnDisable() {
        //     SceneLoader.OnSceneLoadCompleteCallback -= StartGame;
        // }

        private void Start() {
            StartGame();
        }

        private void StartGame() {
            _availableCards = new List<Card>(deck.cards);
            
            HandleNewRound();
            SetInitialParams();
            UpdateCardsLeftCount();
            UpdateActionButtonsState(false);
            
            StartCoroutine(DealCards());
        }

        private void SetInitialParams() {
            // set initial params for Dealer
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = CheckWhoWon,
                initialLives = gameRules.roundSettings[_currentRound].dealersLifeChips
            });
            
            // update to player
            UpdateCurrentPlayersSide(SideType.Player);
            
            // set initial params for Player
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = HandlePlayersStand,
                initialLives = gameRules.initialPlayersLifeChipValue
            });
        }

        private IEnumerator DealCards() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            for (var i = 0; i < initialCardsCount; i++) {
                yield return new WaitForSeconds(delayBetweenDealing);
                InstantiateNewCard();
                UpdateCurrentPlayersSide();
            }

            UpdateCurrentPlayersSide(SideType.Player);
            UpdateActionButtonsState(true);
        }
        
        private void HandlePlayersStand() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            
            var dealersSideController = (DealersSideController)GetCurrentSideController();
            dealersSideController.ReleaseCurrentHoldCard();
        }

        private void CheckWhoWon() {
            var dealersTotal = GetCurrentSideController().currentCardSum;
            UpdateCurrentPlayersSide(SideType.Player);
            var playersTotal = GetCurrentSideController().currentCardSum;
         
            // TODO: if player is busted, Dealer only turns their card and calls OnStand()
            var targetValue = gameRules.targetValue;

            if (playersTotal == dealersTotal) {
                Debug.Log("Draw");
                NextMatch();
                return;
            }
            
            if (playersTotal > targetValue) {
                HandleDealersWon();
                return;
            }

            if (dealersTotal > targetValue) {
                HandlePlayersWon();
                return;
            }

            if (playersTotal > dealersTotal) {
                HandlePlayersWon();
                return;
            }
            
            HandleDealersWon();
        }

        private void HandlePlayersWon() {
            Debug.Log("Player has won");
            playersSide.ReceiveChip(1);
            dealersSide.TakeChip(1, NextMatch, RestartRound);
        }

        private void HandleDealersWon() {
            Debug.Log("Dealer has won");
            dealersSide.ReceiveChip(1);
            playersSide.TakeChip(1, NextMatch, RestartRound);
        }

        private void NextMatch() {
            ResetHands();
            DealCardsAgain();
        }
        
        private void RestartRound() {
            ResetHands();
            HandleNewRound();
            UpdateDealerLivesChips();
            nextRoundCanvas.UpdateRoundValue(_currentRound, DealCardsAgain);
        }

        private void UpdateDealerLivesChips() {
            Debug.Log("_currentRound: " + _currentRound);
            Debug.Log("gameRules.roundSettings[_currentRound].dealersLifeChips: "+ gameRules.roundSettings[_currentRound].dealersLifeChips);
            dealersSide.UpdateLivesChipsAmount(gameRules.roundSettings[_currentRound].dealersLifeChips);
        }

        private void ResetHands() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            GetCurrentSideController().ResetHand();
            UpdateCurrentPlayersSide(SideType.Player);
            GetCurrentSideController().ResetHand();
        }

        private void HandleNewRound() {
            _currentRound += 1;
            UpdateRoundUI(_currentRound + 1);
        }

        private void UpdateRoundUI(int value) {
            roundText.text = $"Round {value}/21";
        }

        private void UpdateCardsLeftCount() {
            cardsLeftText.text = _availableCards.Count.ToString();
        }

        private void DealCardsAgain() {
            StartCoroutine(DealCards());
        }
        
        private SideController GetCurrentSideController() {
            return _currentPlayingSide == SideType.Dealer ? dealersSide : playersSide;
        }

        private void UpdateCurrentPlayersSide() {
            _currentPlayingSide = _currentPlayingSide == SideType.Player ? SideType.Dealer : SideType.Player;
        }
        
        private void UpdateCurrentPlayersSide(SideType newType) {
            _currentPlayingSide = newType;
        }

        private void UpdateActionButtonsState(bool newState) {
            foreach (var actionButton in actionButtons) {
                actionButton.interactable = newState;
            }
        }
        
        public void InstantiateNewCard() {
            var randomCard = Random.Range(0, _availableCards.Count);
            var deckCard = _availableCards[randomCard];
                
            GetCurrentSideController().InstantiateNewCard(deckCard, visualHandlerTransform);
            _availableCards.Remove(deckCard);
            UpdateCardsLeftCount();
        }
        
        public void HitMe() {
            InstantiateNewCard();
        }

        public void Stand() {
            UpdateActionButtonsState(false);
            GetCurrentSideController().Stand();
        }

        public int GetCurrentTargetValue() => gameRules.targetValue;
    }
}
