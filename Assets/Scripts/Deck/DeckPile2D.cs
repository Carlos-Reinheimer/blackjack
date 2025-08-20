using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deck {
    public class DeckPile2D : MonoBehaviour {

        [Header("Visuals")] 
        public Transform origin;
        public Sprite cardBack;
        public Material spriteMaterial;
        public int sortingLayerBase = 100; // ensures the pile renders above table
        public float xOffsetPerCard = 0.1f;
        public float yOffsetPerCard = 0.1f;
        public float zOffsetPerCard = 0.001f; // prevents z-fighting in 2D (camera uses z to order)
        public int maxVisible = 52;

        [Header("Animation")]
        public float popDuration = 0.18f;
        public Vector2 popWorldDirection = new Vector2(1.2f, 1.0f);
        public float popRotation = 12f;
        public float popScale = 0.95f;

        [Header("Data")]
        public int fullDeckSize = 52;
        [SerializeField] private int deckCount = 52;

        private readonly List<SpriteRenderer> activeCards = new();
        private readonly Stack<SpriteRenderer> pool = new();

        private void Awake() {
            var prewarm = Mathf.Max(maxVisible, 12);
            for (var i = 0; i < prewarm; i++) {
                pool.Push(CreateCardRenderer());
            }
            RebuildVisual();
        }

        private SpriteRenderer CreateCardRenderer() {
            var go = new GameObject("DeckCard");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = cardBack;
            if (spriteMaterial) sr.material = spriteMaterial;
            sr.sortingOrder = sortingLayerBase;
            sr.sortingLayerName = "Default";
            sr.color = Color.white;
            go.SetActive(false);
            return sr;
        }

        private int CountToVisible(int count) {
            if (fullDeckSize <= 0) return 0;
            var t = Mathf.Clamp01((float)count / fullDeckSize);
            var visible = Mathf.CeilToInt(t * maxVisible);
            return Mathf.Clamp(visible, count > 0 ? 1 : 0, maxVisible);
        }

        private void RebuildVisual() {
            // Clear all active
            for (var i = activeCards.Count - 1; i >= 0; i--) { 
                ReturnToPool(activeCards[i]);
            }
            activeCards.Clear();

            var needed = CountToVisible(deckCount);
            for (var i = 0; i < needed; i++)
                AddOneVisualCard();

            Layout();
        }

        private void AddOneVisualCard() {
            var sr = pool.Count > 0 ? pool.Pop() : CreateCardRenderer();
            var tf = sr.transform;
            sr.gameObject.SetActive(true);
            tf.localPosition = Vector3.zero;
            tf.localScale = Vector3.one;
            tf.localRotation = Quaternion.identity;
            sr.color = Color.white;
            activeCards.Add(sr);
        }

        private void RemoveOneVisualCardImmediate() {
            var top = activeCards[^1];
            activeCards.RemoveAt(activeCards.Count - 1);
            ReturnToPool(top);
        }

        private void ReturnToPool(SpriteRenderer sr) {
            sr.gameObject.SetActive(false);
            pool.Push(sr);
        }

        private void Layout() {
            // Stack from bottom (index 0) to top (last)
            for (int i = 0; i < activeCards.Count; i++) {
                var sr = activeCards[i];
                var x = xOffsetPerCard * i;
                var y = yOffsetPerCard * i;
                var z = -zOffsetPerCard * i; // more negative = closer to camera in 2D (if camera z sorting), adjust as needed
                sr.transform.localPosition = new Vector3(x, y, z);
                sr.sortingOrder = sortingLayerBase + i;
            }
        }

        private IEnumerator PopAndReturn(SpriteRenderer sr) {
            // Animate out: slight move, rotate, scale + fade
            var tf = sr.transform;
            var startPos = tf.position;
            var endPos = startPos + (Vector3)popWorldDirection;
            var startRot = tf.rotation;
            var endRot = Quaternion.Euler(0, 0, popRotation) * startRot;
            var startScale = sr.transform.localScale;
            var endScale = startScale * popScale;
            var startCol = sr.color;
            var endCol = new Color(startCol.r, startCol.g, startCol.b, 0f);

            var t = 0f;
            while (t < popDuration) {
                t += Time.deltaTime;
                var u = Mathf.SmoothStep(0f, 1f, t / popDuration);
                sr.transform.position = Vector3.Lerp(startPos, endPos, u);
                sr.transform.rotation = Quaternion.Slerp(startRot, endRot, u);
                sr.transform.localScale = Vector3.Lerp(startScale, endScale, u);
                sr.color = Color.Lerp(startCol, endCol, u);
                yield return null;
            }
            ReturnToPool(sr);
        }
        
        public void SetDeckCount(int newCount, bool animateDrawIfDecreased = true) {
            newCount = Mathf.Max(0, newCount);
            var decreased = newCount < deckCount;
            deckCount = newCount;

            var targetVisible = CountToVisible(deckCount);

            // pop anim if decreased AND we actually lose one visible sprite
            if (animateDrawIfDecreased && decreased && activeCards.Count > targetVisible && activeCards.Count > 0) {
                var top = activeCards[^1];
                activeCards.RemoveAt(activeCards.Count - 1);
                StartCoroutine(PopAndReturn(top));
            }

            // Adjust count to target
            while (activeCards.Count < targetVisible) {
                AddOneVisualCard();
            }

            while (activeCards.Count > targetVisible) {
                RemoveOneVisualCardImmediate();
            }

            Layout();
        }

        // --- Convenience API you can call from your game logic ---
        public void Initialize(int deckSize) {
            fullDeckSize = Mathf.Max(1, deckSize);
            deckCount = fullDeckSize;
            RebuildVisual();
        }

        public void DrawOne() {
            if (deckCount <= 0) return;
            SetDeckCount(deckCount - 1, animateDrawIfDecreased: true);
        }

        public void RefillToFull() {
            SetDeckCount(fullDeckSize, animateDrawIfDecreased: false);
        }
    }
}
