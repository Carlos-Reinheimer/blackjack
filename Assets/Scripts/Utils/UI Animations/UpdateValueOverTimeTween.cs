using System;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Utils.UI_Animations {
    public class UpdateValueOverTimeTween : LeanTweenAnimation {

        [Header("UI")]
        public TMP_Text valueText;

        [Header("Value Settings")]
        public float initialValue;
        public float targetValue;
        
        [Header("Special Characters")]
        public string valuePrefix;
        public string valuePostfix;
        
        [Header("Other General Settings")]
        public float speedFactor = 7.5f;
        public bool autoCastToInteger = true;

        [Header("Callbacks")]
        public UnityEvent<int> onUpdateCallback;

        public override void StartTween(Action onCompleteCallback = null) {
            var adjustedTimeToComplete = duration <= -1 ? GetValueBasedTimeToComplete() : duration;
            if (adjustedTimeToComplete <= 0) return;

            var castedInitialValue = autoCastToInteger ? (int)initialValue : initialValue;
            var castedTargetValue = autoCastToInteger ? (int)targetValue : targetValue;
            
            LeanTween.value(castedInitialValue, castedTargetValue, adjustedTimeToComplete).setOnUpdate(val => {
                var castedValue = autoCastToInteger ? (int)val : val;
                valueText.text = valuePrefix + castedValue + valuePostfix;

                if ((int)castedValue % 1 == 0) onUpdateCallback?.Invoke((int)castedValue); // only whole numbers
            }).setDelay(delay).setOnComplete(onCompleteCallback);
        }

        public override void StopTween(Action onCompleteCallback = null) { }
        
        private float GetValueBasedTimeToComplete() {
            var changeAmount = Mathf.Abs(initialValue - targetValue);
            if (changeAmount == 0) return 0; // or else it will return infinite
            
            var time = speedFactor * Mathf.Pow(changeAmount, -1.0f / speedFactor);
            return time;
        }

        public void UpdateTargetValues(float newInitialValue, float newTargetValue) {
            initialValue = newInitialValue;
            targetValue = newTargetValue;
        }

        public void UpdateInitialValue(float newInitialValue) {
            initialValue = newInitialValue;
        }
        
        public void UpdateTargetValue(float newTargetValue) {
            targetValue = newTargetValue;
        }
    }
}
