using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidShader.RuntimeData
{
    [Serializable]
    public sealed class GameStateValidationResult
    {
        public readonly List<string> warnings = new List<string>();
        public readonly List<string> errors = new List<string>();

        public bool IsValid => errors.Count == 0;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(IsValid ? "Game state valid" : "Game state invalid");

            for (int i = 0; i < errors.Count; i++)
                builder.AppendLine().Append("Error: ").Append(errors[i]);

            for (int i = 0; i < warnings.Count; i++)
                builder.AppendLine().Append("Warning: ").Append(warnings[i]);

            return builder.ToString();
        }
    }
}
