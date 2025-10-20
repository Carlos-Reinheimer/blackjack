using System;
using Deck;
using UI.Events.HUD;

namespace Controllers.Sides {
    public class PlayersSideController : SideController {
        
        public event Action LoopCompleted;
        
        protected override void OnCardInstantiated(GeneralCardVisual cardVisual, DeckCard deckCard) {
            HandleInstantiatedCard(deckCard);
        }
        
        public void StartOperations() {
            ((PlayersScoreHandler)scoreHandler).TryOperate();
        }

        public void FinishLoop() {
            LoopCompleted?.Invoke();
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
                maxBet = currentRoundSettings.maxBet
            });
            
            // reset current multipliers
            scoreHandler.SetRoundMultiplier(0);
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
