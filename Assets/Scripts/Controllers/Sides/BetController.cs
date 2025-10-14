using UI.Events.HUD;
using UnityEngine;

namespace Controllers.Sides {
    public class BetController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private BetChannelSO betChannel;

        public int currentBet { get; private set; } = 1;

        public int BetMinusOne() {
            var newBet = currentBet - 1;
            var minBet = MainController.Instance.currentRoundSettings.minBet;
            if (newBet < minBet) {
                Debug.Log($"New bet is {newBet}, but it has reached the min bet value ({minBet})");
                newBet = currentBet;
            }
            else {
                currentBet = newBet;
                betChannel.Raise(new BetChannelContract {
                    betAction = BetAction.UpdateMinusOneState,
                    newState = true
                });
                betChannel.Raise(new BetChannelContract {
                    betAction = BetAction.UpdatePlusOneState,
                    newState = true
                });
            }
            
            if (newBet == minBet) betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateMinusOneState,
                newState = false
            });

            return newBet;
        }
        
        public int BetPlusOne(int livesChips) {
            var newBet = currentBet + 1;
            var max = MainController.Instance.currentRoundSettings.maxBet;
            if ((max != -1 && newBet > max) || newBet > livesChips) {
                Debug.Log($"New bet is {newBet}, but chips left are {livesChips}. Setting newBet as previous value");
                newBet = currentBet;
            }
            else {
                currentBet = newBet;
                betChannel.Raise(new BetChannelContract {
                    betAction = BetAction.UpdatePlusOneState,
                    newState = true
                });
                betChannel.Raise(new BetChannelContract {
                    betAction = BetAction.UpdateMinusOneState,
                    newState = true
                });
            }
            
            if (newBet == livesChips) betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdatePlusOneState,
                newState = false
            });
            
            return newBet;
        }
        
        public int BetAllWin(int livesChips) {
            currentBet = livesChips;
            
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateMinusOneState,
                newState = false
            });
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdatePlusOneState,
                newState = false
            });
            betChannel.Raise(new BetChannelContract {
                betAction = BetAction.UpdateAllWinState,
                newState = false
            });

            return currentBet;
        }
    }
}
