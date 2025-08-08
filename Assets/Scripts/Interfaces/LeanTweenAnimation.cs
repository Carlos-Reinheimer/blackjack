using System;
using UnityEngine;

namespace Interfaces {
    
    public abstract class LeanTweenAnimation : MonoBehaviour {
        
        [Header("Animation Settings")]
        public float duration;
        public float delay;
        public LeanTweenType easeType;
        public LeanTweenType loopType = LeanTweenType.clamp;
        
        [Header("Play Settings")]
        public bool startOnEnabled;

        public abstract void StartTween(Action onCompleteCallback = null);
        public abstract void StopTween(Action onCompleteCallback = null);
        
        private void OnEnable() { 
            if (startOnEnabled) StartTween();   
        }
    }
}
