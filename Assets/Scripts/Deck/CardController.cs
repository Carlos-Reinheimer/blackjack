using TMPro;
using UnityEngine;

namespace Deck {
    public class CardController : MonoBehaviour {

        [Header("UI")]
        public TMP_Text valueText;

        public void PopulateData(Card cardData) {
            valueText.text = cardData.value.ToString();
        }
    }
}
