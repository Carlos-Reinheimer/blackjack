using System.Collections;
using System.Collections.Generic;
using Controllers.Sides;
using Deck;
using Scriptable_Objects;
using TMPro;
using UI_Controllers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Controllers {
    public class MainController : MonoBehaviour {

        [Header("Game Definitions")]
        public GameRules gameRules;

        [Header("Scripts")] 
        public DeckController deckController;
        public JokerHandManager jokerHandManager;
        public DealersSideController dealersSide;
        public PlayersSideController playersSide;
        public GameOverController gameOverController;
        public NextRoundTransitionCanvasUIController nextRoundCanvas;

        [Header("Settings")]
        public int initialCardsCount = 4;
        public float delayBetweenDealing = 1;

        [Header("UI")]
        public TMP_Text roundText;
        public List<Button> actionButtons;
        public Transform visualHandlerTransform;
        
        private int _currentRound = -1;
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
        
        private void OnEnable() {
            // SceneLoader.OnSceneLoadCompleteCallback += StartGame;
            playersSide.LoopCompleted += HandlePlayersChips;
        }
        
        private void OnDisable() {
            // SceneLoader.OnSceneLoadCompleteCallback -= StartGame;
            playersSide.LoopCompleted -= HandlePlayersChips;
        }

        private void Start() {
            RunStats.ClearRunStats();
            
            deckController.StartGame(StartFirstRound);
        }

        private void StartFirstRound() {
            HandleNewRound();
            SetInitialParams();
            UpdateActionButtonsState(false);
            
            StartCoroutine(DealCards());
        }

        private void SetInitialParams() {
            // set initial params for Dealer
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = CheckWhoWon,
                bustedCallback = HandlePlayersWon,
                initialLives = gameRules.roundSettings[_currentRound].dealersLifeChips
            });
            
            // update to player
            UpdateCurrentPlayersSide(SideType.Player);
            
            // set initial params for Player
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = HandlePlayersStand,
                bustedCallback = HandlePlayersStand,
                initialLives = gameRules.initialPlayersLifeChipValue
            });
        }

        private IEnumerator DealCards() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            var count = 0;
            while (true){
                yield return new WaitForSeconds(delayBetweenDealing);
                var shouldUpdateSides = InstantiateNewCard();
                if (shouldUpdateSides) {
                    count++;
                    UpdateCurrentPlayersSide();
                }
                
                if (count >= initialCardsCount) break;
            }

            UpdateCurrentPlayersSide(SideType.Player);
            UpdateActionButtonsState(true);
        }
        
        private void HandlePlayersStand() {
            UpdateCurrentPlayersSide(SideType.Dealer);

            var isPlayerBusted = playersSide.isBusted;
            dealersSide.ReleaseCurrentHoldCard(isPlayerBusted ? HandleDealersWon : null);
        }

        private void CheckWhoWon() {
            var dealersTotal = GetCurrentSideController().currentCardSum;
            UpdateCurrentPlayersSide(SideType.Player);
            var playersTotal = GetCurrentSideController().currentCardSum;
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

            if ((dealersTotal > targetValue) || (playersTotal > dealersTotal)) {
                HandlePlayersWon();
                return;
            }

            HandleDealersWon();
        }

        private void HandlePlayersWon() {
            Debug.Log("Player has won");
            playersSide.StartOperations();
        }

        private void HandleDealersWon() {
            Debug.Log("Dealer has won");
            dealersSide.ReceiveChip(1);
            playersSide.TakeChip(1, NextMatch, gameOverController.GameOver);
        }

        private void HandlePlayersChips() {
            playersSide.ReceiveChip(1);
            dealersSide.TakeChip(1, NextMatch, RestartRound);
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

            RunStats.CurrentLevel = _currentRound;
        }

        private void UpdateDealerLivesChips() {
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
        
        public bool InstantiateNewCard() {
            var randomCard = deckController.DrawTopCard(GetCurrentSideController().side);
            if (randomCard.type != CardType.Joker) {
                GetCurrentSideController().InstantiateNewCard((DeckCard)randomCard, visualHandlerTransform);
                return true;
            }

            jokerHandManager.DrawCard((JokerCard)randomCard);
            return false;
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
