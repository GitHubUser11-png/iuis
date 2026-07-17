using System;
using System.Text;

namespace IUIS.Domain.Common
{
    internal static class TextNormalizer
    {
        public static string Required(string value, string parameterName, int maximumLength)
        {
            var normalized = Normalize(value);
            if (normalized.Length == 0)
            {
                throw new DomainValidationException(parameterName + " is required.");
            }

            EnsureMaximumLength(normalized, parameterName, maximumLength);
            return normalized;
        }

        public static string Optional(string value, string parameterName, int maximumLength)
        {
            var normalized = Normalize(value);
            if (normalized.Length == 0)
            {
                return null;
            }

            EnsureMaximumLength(normalized, parameterName, maximumLength);
            return normalized;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var pendingSpace = false;

            foreach (var character in value.Trim())
            {
                if (char.IsWhiteSpace(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }

        private static void EnsureMaximumLength(string value, string parameterName, int maximumLength)
        {
            if (maximumLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLength));
            }

            if (value.Length > maximumLength)
            {
                throw new DomainValidationException(
                    parameterName + " cannot exceed " + maximumLength + " characters.");
            }
        }
    }
}
