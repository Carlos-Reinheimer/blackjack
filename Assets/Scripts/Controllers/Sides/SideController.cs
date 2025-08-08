using System.Collections.Generic;
using Deck;
using EasyTextEffects;
using UnityEngine;
using UnityEngine.Events;
using Utils.UI_Animations;

namespace Controllers.Sides {
    
    public enum SideType
    {
        Player,
        Dealer
    }
    
    public abstract class SideController : MonoBehaviour {
        
        private const int TargetValue = 21;

        [Header("Settings")]
        public SideType side;
        
        [Header("Prefabs")]
        public GameObject cardPrefab;

        [Header("UI")]
        public Transform cardsContentTf;
        public UpdateValueOverTimeTween currentSumText;
        public UpdateValueOverTimeTween scoreText;
        public TextEffect comboTextEffect;
        
        private int _currentScore;
        protected int CurrentCardSum;
        protected List<GameObject> ActiveCards;

        protected abstract void OnCardInstantiated(CardController cardController, Card card);
        protected abstract void OnStand();

        protected void HandleInstantiatedCard(CardController cardController, Card card, UnityAction callback = null) {
            cardController.PopulateData(card);
            UpdateTotalSum(card.value, callback);
        }
        
        protected void HandleEndOfRound() {
            MainController.Instance.RestartRound();
        }

        private void Start() {
            ActiveCards = new List<GameObject>();
        }
        
        private void UpdateTotalSum(int value , UnityAction callback = null) {
            var previousCardSum = CurrentCardSum;
            CurrentCardSum += value;

            currentSumText.UpdateTargetValues(previousCardSum, CurrentCardSum);
            currentSumText.StartTween();

            if (CurrentCardSum > TargetValue) HandleCrossTargetValue();
            else callback?.Invoke();
        }
        
        private void UpdateCurrentScore(bool decreaseValue) {
            var newScore = CurrentCardSum + CurrentCardSum * ActiveCards.Count;
            var previousScore = _currentScore;
            if (decreaseValue) {
                _currentScore -= newScore;
                if (_currentScore <= 0) _currentScore = 0;
            }
            else _currentScore += newScore;

            var finalScore = _currentScore <= 0 ? 0 : _currentScore;
            UpdateScoreUI(previousScore, finalScore);
        }
        
        private void UpdateScoreUI(float previousScore, float currentScore) {
            
            scoreText.UpdateTargetValues(previousScore, currentScore);
            scoreText.StartTween();
        }
        
        private void HandleCrossTargetValue() {
            UpdateCurrentScore(true);
            OnStand();
            
            if (side == SideType.Player) MainController.Instance.HandlePlayersStand();
        }
        
        public void InstantiateNewCard(Card card) {
            var newCard = Instantiate(cardPrefab, cardsContentTf);
            var component = newCard.GetComponent<CardController>();
            
            ActiveCards.Add(newCard);
            OnCardInstantiated(component, card);
        }
        
        public void Stand() {
            UpdateCurrentScore(false);
            OnStand();
        }

        public void ResetHand() {
            CurrentCardSum = 0;
            UpdateTotalSum(CurrentCardSum);
            foreach (var activeCard in ActiveCards) {
                Destroy(activeCard);
            }
            
            ActiveCards.Clear();
        }
    }
}
