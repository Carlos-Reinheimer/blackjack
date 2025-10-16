using System.Collections;
using Controllers.Sides;
using Deck;
using Scriptable_Objects;
using UI.Events.HUD;
using UI.Events.Next_Round;
using UnityEngine;
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
        
        [Header("Channels (SO assets)")]
        [SerializeField] private AdvanceRoundChannelSO advanceRoundChannel;
        [SerializeField] private HUDActionsChannelSO hudActionsChannel;
        [SerializeField] private HUDActionsStateChannelSo hudActionsStateChannel;
        [SerializeField] private RoundChannelSO roundChannel;

        [Header("Settings")]
        public int initialCardsCount = 4;
        public float delayBetweenDealing = 1;

        [Header("Helpers")] 
        public RoundSettings currentRoundSettings;

        [Header("UI")]
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

            hudActionsChannel.OnEventRaised += HandleHudActionChannel;
        }
        
        private void OnDisable() {
            // SceneLoader.OnSceneLoadCompleteCallback -= StartGame;
            playersSide.LoopCompleted -= HandlePlayersChips;
            
            hudActionsChannel.OnEventRaised -= HandleHudActionChannel;
        }

        private void Start() {
            RunStats.ClearRunStats();
            
            deckController.StartGame(StartFirstRound);
        }
        
        private void StartFirstRound() {
            HandleNewRound();
            UpdateCurrentRoundSettings();
            SetInitialParams();
            ClearCurrentBets();
            StartCoroutine(DealCards());
        }
        
        private void UpdateCurrentRoundSettings() {
            currentRoundSettings = gameRules.roundSettings[_currentRound];
            playersSide.UpdateBetOptions();
        }

        private void ClearCurrentBets() {
            playersSide.ClearSideBet();
            dealersSide.ClearSideBet();
            playersSide.SetBetStates();
        }

        private void SetInitialParams() {
            // set initial params for Dealer
            GetCurrentSideController().SetInitialParams(new InitialParams {
                standCallback = CheckWhoWon,
                bustedCallback = HandlePlayersWon,
                initialLives = currentRoundSettings.dealersLifeChips
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
            hudActionsStateChannel.Raise(false);
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
            hudActionsStateChannel.Raise(true);
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

            if (dealersTotal > targetValue || playersTotal > dealersTotal) {
                HandlePlayersWon();
                return;
            }

            HandleDealersWon();
        }

        private void HandlePlayersWon() {
            Debug.Log("Player has won the match");
            playersSide.StartOperations();
        }

        private void HandleDealersWon() {
            Debug.Log("Dealer has won the match");
            var currentBet = playersSide.GetCurrentBet();
            dealersSide.ReceiveChip(currentBet);
            playersSide.TakeChip(currentBet, NextMatch, gameOverController.GameOver);
        }

        private void HandlePlayersChips() {
            var currentBet = playersSide.GetCurrentBet();
            playersSide.ReceiveChip(currentBet);
            dealersSide.TakeChip(currentBet, NextMatch, AdvanceRound);
        }

        private void NextMatch() {
            ResetHands();
            ClearCurrentBets();
            DealCardsAgain();
        }
        
        private void AdvanceRound() {
            ResetHands();
            HandleNewRound();
            UpdateCurrentRoundSettings();
            ClearCurrentBets();
            UpdateDealerLivesChips();
            advanceRoundChannel.Raise(new AdvanceRoundModel {
                nextRound = _currentRound, 
                completeCallback = DealCardsAgain
            });

            RunStats.CurrentRound = _currentRound;
        }

        private void UpdateDealerLivesChips() {
            dealersSide.UpdateLivesChipsAmount(currentRoundSettings.dealersLifeChips);
        }

        private void ResetHands() {
            UpdateCurrentPlayersSide(SideType.Dealer);
            GetCurrentSideController().ResetHand();
            UpdateCurrentPlayersSide(SideType.Player);
            GetCurrentSideController().ResetHand();
        }

        private void HandleNewRound() {
            _currentRound += 1;
            roundChannel.Raise(_currentRound + 1);
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

        private void HandleHudActionChannel(HudAction action) {
            switch (action) {
                case HudAction.Hit:
                    HitMe();
                    break;
                case HudAction.Stand:
                    Stand();
                    break;
                case HudAction.BetPlusOne:
                case HudAction.BetMinusOne:
                case HudAction.BetAllWin:
                default:
                    break;
            }
        }
        
        private void HitMe() {
            InstantiateNewCard();
        }

        private void Stand() {
            GetCurrentSideController().Stand();
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
        
        public int GetCurrentTargetValue() => gameRules.targetValue;
    }
}
