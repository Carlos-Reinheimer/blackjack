using System;
using Interfaces;
using UnityEngine;

namespace Utils.UI_Animations {
    public class ScaleTween : LeanTweenAnimation {

        public Vector3 targetVector;
        public bool startAtZeroScale;
        
        public override void StartTween(Action onCompleteCallback = null) {
            if (startAtZeroScale) transform.localScale = Vector3.zero;
            
            LeanTween.scale(gameObject, targetVector, duration)
                .setDelay(delay)
                .setEase(easeType)
                .setOnComplete(onCompleteCallback);
        }

        public override void StopTween(Action onCompleteCallback = null) {
            LeanTween.cancel(gameObject);
        }
    }
}
