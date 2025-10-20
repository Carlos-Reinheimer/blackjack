using System;
using Controllers.Sides;

namespace Utils {
    public static class ScoreSettings {

        public static string GetColorByOperationValueType(OperationValueType valueType) {
            return valueType switch {
                OperationValueType.Score => "white",
                OperationValueType.MatchScore => "blue",
                OperationValueType.Multiplier => "red",
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
    }
}
