using System.Collections.Generic;
using TMPro;
using UI.Events.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Controllers {
    public class HudUIController : MonoBehaviour {

        [Header("Channels (SO assets)")]
        [SerializeField] private BetChannelSO betChannel;
        [SerializeField] private CardsLeftChannelSO cardsLeftChannel;
        [SerializeField] private ChipsChannelSO chipsChannel;
        [SerializeField] private RoundChannelSO roundChannel;
        [SerializeField] private ScoreChannelSO scoreChannel;
        [SerializeField] private HUDActionsChannelSO hudActionsChannel;
        [SerializeField] private HUDActionsStateChannelSo hudActionsStateChannel;
        
        [Header("UI - Bet")]
        [SerializeField] private TMP_Text playersBetText;
        [SerializeField, HideInInspector] private TMP_Text dealersBetText; // maybe in the future
        
        [Header("UI - Cards Left")]
        [SerializeField] private TMP_Text cardsLeftText;
        
        [Header("UI - Chips")]
        [SerializeField] private TMP_Text playersChipsText;
        [SerializeField] private TMP_Text dealersChipsText;
        
        [Header("UI - Round")]
        [SerializeField] private TMP_Text roundText;
        
        [Header("UI - Score")]
        [SerializeField] private TMP_Text playersScoreText;
        [SerializeField, HideInInspector] private TMP_Text dealersScoreText; // maybe in the future
        
        [Header("UI - Action Buttons")]
        [SerializeField] private List<Button> actionButtons;

        private void OnEnable() {
            hudActionsStateChannel.OnEventRaised += UpdateActionButtonsState;
            roundChannel.OnEventRaised += UpdateRoundUI;
        }

        private void OnDisable() {
            hudActionsStateChannel.OnEventRaised -= UpdateActionButtonsState;
            roundChannel.OnEventRaised -= UpdateRoundUI;
        }

        private void UpdateRoundUI(int value) {
            roundText.text = $"{value}/21";
        }
        
        private void UpdateActionButtonsState(bool newState) {
            foreach (var actionButton in actionButtons) {
                actionButton.interactable = newState;
            }
        }
        
        public void HitMe() {
            hudActionsChannel.Raise(HudAction.Hit);
        }

        public void Stand() {
            UpdateActionButtonsState(false);
            hudActionsChannel.Raise(HudAction.Stand);
        }

        public void Bet() {
            hudActionsChannel.Raise(HudAction.Bet);
        }
    }
}
