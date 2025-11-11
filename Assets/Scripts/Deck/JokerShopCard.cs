using Scriptable_Objects;
using TMPro;
using UI.Events.Main_Menu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Deck {
    public class JokerShopCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

        [Header("Shadow Parameters")] 
        [SerializeField] private GameObject shadow;
        [SerializeField] private float shadowYDownValue = 15;
        [SerializeField] private float shadowYDownDuration = 0.1f;
        
        [Header("Rotation Parameters")]
        [SerializeField] private float autoTiltAmount = 30;
        [SerializeField] private float manualTiltAmount = 20;
        [SerializeField] private float tiltSpeed = 20;
        
        [Header("Curve")]
        [SerializeField] private CurveParameters curve;
        
        [Header("Curve")]
        [SerializeField] private TMP_Text priceText; 

        private JokerCard _thisCard;
        private bool _isHovering;
        private int _cardIndex;
        private UnityAction<JokerCard, UnityAction> _purchaseJokerCallback;
        
        private float _curveYOffset;
        private float _curveRotationOffset;
        private Coroutine _pressCoroutine;

        private void Update() {
            HandPositioning();
            Tilt();
        }

        private void HandPositioning() {
            _curveRotationOffset = curve.rotation.Evaluate(0);
        }

        private void Tilt() {
            var sine = Mathf.Sin(Time.time + _cardIndex) * (_isHovering ? .2f : 1);
            var cosine = Mathf.Cos(Time.time + _cardIndex) * (_isHovering ? .2f : 1);

            if (Camera.main == null) return;
            var offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tiltX =  0;
            var tiltY =  0;
            // var tiltX = _isHovering ? offset.y * -1 * manualTiltAmount : 0;
            // var tiltY = _isHovering ? offset.x * manualTiltAmount : 0;
            var tiltZ = _curveRotationOffset * curve.rotationInfluence;

            var eulerAngles = transform.eulerAngles;
            var lerpX = Mathf.LerpAngle(eulerAngles.x, tiltX + sine * autoTiltAmount, tiltSpeed * Time.deltaTime);
            var lerpY = Mathf.LerpAngle(eulerAngles.y, tiltY + cosine * autoTiltAmount, tiltSpeed * Time.deltaTime);
            var lerpZ = Mathf.LerpAngle(eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

            transform.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
        }

        private void EffectShadow(bool enable) {
            var shadowPos = shadow.transform.localPosition;
            var yModifiedValue = enable ? -shadowYDownValue : +shadowYDownValue;
            var finalPos = new Vector3(shadowPos.x, shadowPos.y + yModifiedValue, shadowPos.z);
            LeanTween.moveLocal(shadow, finalPos, shadowYDownDuration).setOnComplete(() => shadow.transform.localPosition = finalPos);
        }

        private void UnlockCard() {
            RotateUnlockedCard();
        }
        
        private void RotateUnlockedCard() {
            LeanTween.rotate(gameObject, new Vector3(0, 180, 0), Random.Range(0.5f, 1.5f)).setDelay(Random.Range(0.1f, 0.3f));
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _isHovering = true;
            LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.1f).setEase(LeanTweenType.easeInBounce).setOnComplete(() => transform.localScale = new Vector3(1.1f, 1.1f, 1.1f));
            EffectShadow(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isHovering = false;
            LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), 0.1f).setEase(LeanTweenType.easeInBounce).setOnComplete(() => transform.localScale = new Vector3(1, 1, 1));;
            EffectShadow(false);
        }

        public void OnPointerDown(PointerEventData eventData) {
            _purchaseJokerCallback?.Invoke(_thisCard, UnlockCard);
        }

        public void Initialize(CreateJokerShopCardSchema jokerShopCardSchema) {
            _thisCard = jokerShopCardSchema.jokerCard;
            _cardIndex = jokerShopCardSchema.index;
            _purchaseJokerCallback = jokerShopCardSchema.purchaseJokerCallback;
            
            priceText.text = _thisCard.unlockPrice.ToString();
            if (!_thisCard.isUnlocked) return;
            RotateUnlockedCard();
        }
    }
}
