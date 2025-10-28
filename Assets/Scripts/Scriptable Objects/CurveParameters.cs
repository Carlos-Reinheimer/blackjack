using UnityEngine;

namespace Scriptable_Objects {
    [CreateAssetMenu(fileName = "CurveParameters", menuName = "Scriptable Objects/Hand Curve Parameters")]
    public class CurveParameters : ScriptableObject {
        public AnimationCurve positioning;
        public float positioningInfluence = .1f;
        public AnimationCurve rotation;
        public float rotationInfluence = 10f;
    }
}