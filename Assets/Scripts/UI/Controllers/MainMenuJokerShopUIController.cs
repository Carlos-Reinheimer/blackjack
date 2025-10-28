using Deck;
using UI.Events.Main_Menu;
using UnityEngine;
using Utils.UI_Animations;

namespace UI.Controllers {
    public class MainMenuJokerShopUIController : MonoBehaviour {
        
        [Header("Channels (SO assets)")]
        [SerializeField] private MainMenuActionChannelSO actionChannel;
        [SerializeField] private CreateJokerShopCardChannelSO createJokerShopCardChannel;

        [Header("UI")]
        [SerializeField] private FadeCanvasGroupTween jokerShopCanvas;
        
        [Header("UI - Shop Content")]
        [SerializeField] private Transform contentTf;
        [SerializeField] private JokerShopCard jokerShopCardPrefab;
        
        private void OnEnable() {
            actionChannel.OnEventRaised += HandleAction;
            createJokerShopCardChannel.OnEventRaised += HandleNewShopJokerCard;
        }

        private void OnDisable() {
            actionChannel.OnEventRaised -= HandleAction;
            createJokerShopCardChannel.OnEventRaised -= HandleNewShopJokerCard;
        }
        
        private void HandleAction(MainMenuAction action) {
            if (action != MainMenuAction.JokerShop) return;
            jokerShopCanvas.gameObject.SetActive(true);
        }

        // TODO: need to replace this with a pooling system
        private void HandleNewShopJokerCard(CreateJokerShopCardSchema jokerShopCardSchema) {
            var newCard = Instantiate(jokerShopCardPrefab, contentTf);
            newCard.GetComponent<JokerShopCard>().Initialize(jokerShopCardSchema);
        }

        public void CloseJokerShop() {
            jokerShopCanvas.Fade(false, 0, () => jokerShopCanvas.gameObject.SetActive(false));
        }
    }
}
