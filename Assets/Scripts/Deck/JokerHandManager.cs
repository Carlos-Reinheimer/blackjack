using System.Collections.Generic;
using Controllers.Sides;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Splines;

namespace Deck {
    public class JokerHandManager : MonoBehaviour {

        [SerializeField] private int maxHandSize;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private GameObject cardVisualPrefab;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private Transform spawnPoint;

        private readonly List<CardController> _handCards = new();

        private void UpdateCardPositions() {
            if (_handCards.Count == 0) return;

            var cardSpacing = 1f / maxHandSize;
            var firstCardPosition = 0.5f - (_handCards.Count - 1) * cardSpacing / 2;
            var spline = splineContainer.Splines[0];

            for (var i = 0; i < _handCards.Count; i++) {
                var p = firstCardPosition + i * cardSpacing;
                var splinePosition = spline.EvaluatePosition(p);
                LeanTween.moveLocal(_handCards[i].gameObject, splinePosition, 0.25f);

                // var forward = spline.EvaluateTangent(p);
                // var up = spline.EvaluateUpVector(p);
                // var rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
                // LeanTween.rotateLocal(_handCards[i].gameObject, rotation.eulerAngles, 0.25f);
            }
        }

        public void DrawCard(JokerCard jokerCard) {
            // TODO: this is something we need to look up to:
            // If the player is already on the limit of jokers and Hit another one, it will be ignored.
            // This can be an issue, because the index of the deck will continue to go down and that joker card will never be Hit until the next Random shuffle
            // There are two options for this: 1) when this happens, we can reorder the joker until the player Hit it | OR | 2) we leave this at it is, the player will not Hit that joker and better luck next time 

            if (_handCards.Count >= maxHandSize) {
                Debug.Log("Trying to draw a new joker, but already on the limit");
                return;
            }
            
            var newCard = Instantiate(cardPrefab, spawnPoint);
            var newCardVisual = Instantiate(cardVisualPrefab, spawnPoint);
            var cardController = newCard.GetComponent<CardController>();
            var cardVisual = (JokerCardVisual)newCardVisual.GetComponent<GeneralCardVisual>();
            
            cardController.OnInstantiated(SideType.Player, CardType.Joker);
            cardVisual.StartVisual(cardController, jokerCard);
            _handCards.Add(cardController);
            UpdateCardPositions();
        }
    }
}
