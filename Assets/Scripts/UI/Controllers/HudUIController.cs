using System;
using System.Collections.Generic;
using Controllers.Sides;
using TMPEffects.Components;
using TMPro;
using UI.Events.HUD;
using UnityEngine;
using UnityEngine.UI;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class HudUIController : MonoBehaviour {

        [Header("Channels (SO assets)")]
        [SerializeField] private BetChannelSO betChannel;
        [SerializeField] private CardsLeftChannelSO cardsLeftChannel;
        [SerializeField] private ChipsChannelSO chipsChannel;
        [SerializeField] private RoundChannelSO roundChannel;
        [SerializeField] private CardsSumChannelSO cardsSumChannel;
        [SerializeField] private ScoreChannelSO scoreChannel;
        [SerializeField] private ScoreComboChannelSO scoreComboChannel;
        [SerializeField] private HUDActionsChannelSO hudActionsChannel;
        [SerializeField] private HUDActionsStateChannelSo hudActionsStateChannel;
        
        [Header("UI - Bet")]
        [SerializeField] private TMP_Text playersBetText;
        [SerializeField] private TMP_Text minBetText;
        [SerializeField] private TMP_Text maxBetText;
        [SerializeField, HideInInspector] private TMP_Text dealersBetText; // maybe in the future
        
        [Header("UI - Cards Left")]
        [SerializeField] private TMP_Text cardsLeftText;
        
        [Header("UI - Cards Sum")]
        [SerializeField] private UpdateValueOverTimeTween playersCurrentSumText;
        [SerializeField] private UpdateValueOverTimeTween dealersCurrentSumText;
        
        [Header("UI - Chips")]
        [SerializeField] private TMP_Text playersChipsText;
        [SerializeField] private TMP_Text dealersChipsText;
        
        [Header("UI - Round")]
        [SerializeField] private TMP_Text roundText;
        
        [Header("UI - Score")]
        [SerializeField] private UpdateValueOverTimeTween scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMPWriter comboTmpWriter;
        [SerializeField] private TMPAnimator comboTmpAnimator;
        [SerializeField, HideInInspector] private TMP_Text dealersScoreText; // maybe in the future
        
        [Header("UI - Action Buttons")]
        [SerializeField] private List<Button> actionButtons;
        [SerializeField] private Button betMinusOneButton;
        [SerializeField] private Button betPlusOneButton;
        [SerializeField] private Button betAllWinButton;
        [SerializeField] private GameObject betButton;
        [SerializeField] private FadeCanvasGroupTween betButtonFadeCanvasGroup;
        [SerializeField] private GameObject betOptionsGameObject;
        [SerializeField] private FadeCanvasGroupTween betOptionsFadeCanvasGroup;

        private bool _isHoveringBetButton;

        private void OnEnable() {
            hudActionsStateChannel.OnEventRaised += UpdateActionButtonsState;
            roundChannel.OnEventRaised += UpdateRoundUI;
            cardsSumChannel.OnEventRaised += UpdateCurrentSum;
            cardsLeftChannel.OnEventRaised += UpdateCardsLeftCount;
            chipsChannel.OnEventRaised += UpdateChipsLeft;
            scoreChannel.OnEventRaised += UpdateScoreUI;
            scoreComboChannel.OnEventRaised += UpdateScoreComboUI;
            betChannel.OnEventRaised += UpdateCurrentBet;
        }

        private void OnDisable() {
            hudActionsStateChannel.OnEventRaised -= UpdateActionButtonsState;
            roundChannel.OnEventRaised -= UpdateRoundUI;
            cardsSumChannel.OnEventRaised -= UpdateCurrentSum;
            cardsLeftChannel.OnEventRaised -= UpdateCardsLeftCount;
            chipsChannel.OnEventRaised -= UpdateChipsLeft;
            scoreChannel.OnEventRaised -= UpdateScoreUI;
            scoreComboChannel.OnEventRaised -= UpdateScoreComboUI;
            betChannel.OnEventRaised -= UpdateCurrentBet;
        }

        private void UpdateRoundUI(int value) {
            roundText.text = $"{value}/21";
        }
        
        private void UpdateCardsLeftCount(int amount) {
            cardsLeftText.text = amount.ToString();
        }

        private void UpdateChipsLeft(ChipsChannelContract channelContract) {
            var side = channelContract.sideController.side;
            if (side == SideType.Player) playersChipsText.text = channelContract.amount.ToString();
            else dealersChipsText.text = channelContract.amount.ToString();
        }

        private void UpdateCurrentBet(BetChannelContract betContract) {
            switch (betContract.betAction) {
                case BetAction.UpdateBetValue:
                    if (betContract.sideController == null) return;
                    var side = betContract.sideController.side;
                    if (side == SideType.Player) playersBetText.text = betContract.betAmount.ToString();
                    // else dealersBetText.text = betContract.betAmount.ToString();
                    break;
                case BetAction.UpdateMinBet:
                    var newMinBetText = minBetText.text.Replace("{0}", betContract.minBet.ToString());
                    minBetText.text = newMinBetText;
                    break;
                case BetAction.UpdateMaxBet:
                    var maxBet = betContract.maxBet;
                    var betStringValue = maxBet == -1 ? "infinite" : maxBet.ToString();
                    var newMaxBetText = maxBetText.text.Replace("{0}", betStringValue);
                    maxBetText.text = newMaxBetText;
                    break;
                case BetAction.UpdateAllWinState:
                    if (betContract.newState != null) betAllWinButton.interactable = (bool)betContract.newState;
                    break;
                case BetAction.UpdateMinusOneState:
                    if (betContract.newState != null) betMinusOneButton.interactable = (bool)betContract.newState;
                    break;
                case BetAction.UpdatePlusOneState:
                    if (betContract.newState != null) betPlusOneButton.interactable = (bool)betContract.newState;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateCurrentSum(CardsSumContract cardsSumContract) {
            var side = cardsSumContract.sideController.side;
            var previousCardSum = cardsSumContract.previousCardSum;
            var currentCardSum = cardsSumContract.newCurrentCardSum;
            
            if (side == SideType.Player) {
                playersCurrentSumText.UpdateTargetValues(previousCardSum, currentCardSum);
                playersCurrentSumText.StartTween();
            }
            else {
                dealersCurrentSumText.UpdateTargetValues(previousCardSum, currentCardSum);
                dealersCurrentSumText.StartTween();
            }
        }

        #region Score

            private void UpdateScoreComboUI(ScoreComboChannelContract comboContract) {
                if (comboContract.action == ScoreComboAction.StartWriter) {
                    comboText.text = comboContract.comboText;
                    comboTmpWriter.StartWriter();
                    comboTmpAnimator.StartAnimating();
                }
                else {
                    comboTmpWriter.StopWriter();
                    comboTmpAnimator.StopAnimating();
                    comboText.text = "";
                }
            }
            
            private void UpdateScoreUI(ScoreChannelContract scoreChannelContract) {
                scoreText.UpdateTargetValues(scoreChannelContract.previousScore, scoreChannelContract.nextScore);
                scoreText.StartTween();
            }

        #endregion

        #region Actions

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

            public void Bet(int option) {
                hudActionsChannel.Raise((HudAction)option);
            }

            public void OnBetPointerEnter() {
                if (_isHoveringBetButton) return;
                _isHoveringBetButton = true;
                LeanTween.rotate(betButton, new Vector3(0, 180, 0), 0.2f);
                betButtonFadeCanvasGroup.Fade(false, 0);
                LeanTween.rotate(betOptionsGameObject, new Vector3(0, 0, 0), 0.2f);
                betOptionsFadeCanvasGroup.Fade(true, 0);
            }
            
            public void OnBetPointerExit() {
                if (!_isHoveringBetButton) return;
                _isHoveringBetButton = false;
                LeanTween.rotate(betButton, new Vector3(0, 0, 0), 0.2f);
                betButtonFadeCanvasGroup.Fade(true, 0);
                LeanTween.rotate(betOptionsGameObject, new Vector3(0, 180, 0), 0.2f);
                betOptionsFadeCanvasGroup.Fade(false, 0);
            }

        #endregion
    }
}
