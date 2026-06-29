using System;
using System.Collections.Generic;

namespace LiquidShader.RuntimeData
{
    public static class GameStateValidator
    {
        public static GameStateValidationResult Validate(GameState state)
        {
            GameStateValidationResult result = new GameStateValidationResult();

            if (state == null)
            {
                result.errors.Add("State is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(state.version))
                result.warnings.Add("State version is empty.");

            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < state.values.Count; i++)
            {
                StateValue value = state.values[i];
                if (value == null)
                {
                    result.errors.Add($"Value at index {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value.key))
                {
                    result.errors.Add($"Value at index {i} has an empty key.");
                    continue;
                }

                if (!keys.Add(value.key))
                    result.errors.Add($"Duplicate key: {value.key}");
            }

            return result;
        }
    }
}
