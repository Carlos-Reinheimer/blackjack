using System.Collections;
using System.Collections.Generic;
using Controllers.Sides;
using TMPro;
using UI_Controllers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
    public class MainController : MonoBehaviour {

        public SideController dealersSide;
        public SideController playersSide;
        public NextRoundTransitionCanvasUIController nextRoundCanvas;

        [Header("Data")]
        public DeckSo deck;
        public Transform visualHandlerTransform;
        // public DeckPile2D deckPile2D;

        [Header("Settings")]
        public int initialCardsCount = 4;
        public float restartTimer = 2;
        public float delayBetweenDealing = 1;

        [Header("UI")]
        public TMP_Text roundText;
        public TMP_Text cardsLeftText;
        public List<Button> actionButtons;
        
        private int _currentRound;
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
            // deckPile2D.Initialize(_availableCards.Count);
            
            // set initial params for Dealer
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = RestartRound
            });
            
            // update to player
            UpdateCurrentPlayersSide(SideType.Player);
            
            // set initial params for Player
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = HandlePlayersStand
            });
            UpdateCardsLeftCount();
            UpdateActionButtonsState(false);
            HandleNewRound();
            
            StartCoroutine(DealCards());
        }

        private IEnumerator DealCards() {
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
        
        private void RestartRound() {
            GetCurrentSideController().ResetHand();
            UpdateCurrentPlayersSide(SideType.Player);
            GetCurrentSideController().ResetHand();
            HandleNewRound();

            nextRoundCanvas.UpdateRoundValue(_currentRound, DealCardsAgain);
        }

        private void HandleNewRound() {
            _currentRound += 1;
            UpdateRoundUI(_currentRound);
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
    }
}
