using System.Collections;
using System.Collections.Generic;
using Controllers.Sides;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
    public class MainController : MonoBehaviour {

        public SideController dealersSide;
        public SideController playersSide;

        [Header("Data")]
        public DeckSo deck;

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
        private SideType _currentPlayingSide;
        
        #region Singleton
            public static MainController Instance {
                get {
                    if (_instance == null)
                        _instance = FindFirstObjectByType(typeof(MainController)) as MainController;

                    return _instance;
                }
                set => _instance = value;
            }
            private static MainController _instance;
        #endregion

        private void Start() {
            _availableCards = new List<Card>(deck.cards);
            UpdateCurrentPlayersSide(SideType.Player);
            UpdateCardsLeftCount();
            UpdateActionButtonsState(false);
            
            StartCoroutine(DealCards());
        }

        private IEnumerator DealCards() {
            HandleNewRound();

            for (var i = 0; i < initialCardsCount; i++) {
                yield return new WaitForSeconds(delayBetweenDealing);
                InstantiateNewCard();
                UpdateCurrentPlayersSide();
            }

            UpdateCurrentPlayersSide(SideType.Player);
            UpdateActionButtonsState(true);
        }

        private void HandleNewRound() {
            _currentRound += 1;
            UpdateRoundUI(_currentRound);
        }

        private void UpdateRoundUI(int value) {
            roundText.text = $"Round {value}";
        }

        private void UpdateCardsLeftCount() {
            cardsLeftText.text = _availableCards.Count.ToString();
        }

        private IEnumerator DealCardsAgain() {
            yield return new WaitForSeconds(restartTimer);
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

        public void HandlePlayersStand() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            
            var dealersSideController = (DealersSideController)GetCurrentSideController();
            dealersSideController.ReleaseCurrentHoldCard();
        }
        
        public void InstantiateNewCard() {
            var randomCard = Random.Range(0, _availableCards.Count);
            var deckCard = _availableCards[randomCard];
                
            GetCurrentSideController().InstantiateNewCard(deckCard);
            _availableCards.Remove(deckCard);
            UpdateCardsLeftCount();
        }
        
        public void RestartRound() {
            GetCurrentSideController().ResetHand();
            UpdateCurrentPlayersSide(SideType.Player);
            GetCurrentSideController().ResetHand();
            
            StartCoroutine(DealCardsAgain());
        }
        
        public void HitMe() {
            InstantiateNewCard();
        }

        public void Stand() {
            UpdateActionButtonsState(false);
            GetCurrentSideController().Stand();
            // HandlePlayersStand();
        }
    }
}
