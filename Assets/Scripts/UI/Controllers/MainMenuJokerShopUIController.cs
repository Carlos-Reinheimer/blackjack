using System;
using System.Collections.Generic;
using Deck;
using UI.Events.Main_Menu;
using UI.Events.Save_Game_Data;
using UnityEngine;
using Utils;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class MainMenuJokerShopUIController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private CreateJokerShopCardChannelSO createJokerShopCardChannel;
        [SerializeField] private SaveGameDataEventChannel saveGameDataEventChannel;

        [Header("UI")]
        [SerializeField] private FadeCanvasGroupTween jokerShopCanvas;
        
        [Header("UI - Shop Content")]
        [SerializeField] private Transform contentTf;
        [SerializeField] private JokerShopCard jokerShopCardPrefab;

        private List<JokerShopCard> _jokerCards;
        
        private void OnEnable() {
            actionChannel.OnEventRaised += HandleAction;
            createJokerShopCardChannel.OnEventRaised += HandleNewShopJokerCard;
            saveGameDataEventChannel.OnEventRaised += HandleJokerPurchase;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleAction;
            createJokerShopCardChannel.OnEventRaised -= HandleNewShopJokerCard;
            saveGameDataEventChannel.OnEventRaised -= HandleJokerPurchase;
        }

        private void Start() {
            _jokerCards = new List<JokerShopCard>();
        }

        private void HandleAction(MainMenuAction action) {
            if (action != MainMenuAction.JokerShop) return;
            jokerShopCanvas.gameObject.SetActive(true);
        }

        // TODO: need to replace this with a pooling system
        private void HandleNewShopJokerCard(CreateJokerShopCardSchema jokerShopCardSchema) {
            var newCard = Instantiate(jokerShopCardPrefab, contentTf);
            var jokerShopCard = newCard.GetComponent<JokerShopCard>();
            _jokerCards.Add(jokerShopCard);
            jokerShopCard.Initialize(jokerShopCardSchema);
        }

        private void HandleJokerPurchase(bool loadingState) {
            foreach (var jokerCard in _jokerCards) {
                jokerCard.CheckPurchaseAvailability(SaveGameData.coreData.globalScore);
            }
        }

        public void CloseJokerShop() {
            jokerShopCanvas.Fade(false, 0, () => jokerShopCanvas.gameObject.SetActive(false));
        }
    }
}
