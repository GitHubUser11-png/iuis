using System;
using System.Globalization;

using IUIS.Domain.Common;

namespace IUIS.Domain.Finance
{
    public sealed class Money : IEquatable<Money>
    {
        public Money(decimal amount, string currencyCode)
        {
            Amount = MoneyRules.Round(amount);
            CurrencyCode = NormalizeCurrencyCode(currencyCode);
        }

        public decimal Amount { get; }

        public string CurrencyCode { get; }

        public static Money PhilippinePeso(decimal amount)
        {
            return new Money(amount, MoneyRules.PhilippinePesoCurrencyCode);
        }

        public static Money Zero(string currencyCode)
        {
            return new Money(0m, currencyCode);
        }

        public Money Add(Money other)
        {
            RequireSameCurrency(other);
            return new Money(Amount + other.Amount, CurrencyCode);
        }

        public Money Subtract(Money other)
        {
            RequireSameCurrency(other);
            return new Money(Amount - other.Amount, CurrencyCode);
        }

        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, CurrencyCode);
        }

        public Money Negate()
        {
            return new Money(-Amount, CurrencyCode);
        }

        public Money RequireNonNegative(string parameterName)
        {
            if (Amount < 0m)
            {
                throw new DomainValidationException(parameterName + " cannot be negative.");
            }

            return this;
        }

        public bool Equals(Money other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Amount == other.Amount
                && StringComparer.Ordinal.Equals(CurrencyCode, other.CurrencyCode);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Amount.GetHashCode() * 397)
                    ^ StringComparer.Ordinal.GetHashCode(CurrencyCode);
            }
        }

        public override string ToString()
        {
            return CurrencyCode + " " + Amount.ToString("N2", CultureInfo.InvariantCulture);
        }

        private void RequireSameCurrency(Money other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (!StringComparer.Ordinal.Equals(CurrencyCode, other.CurrencyCode))
            {
                throw new DomainValidationException(
                    "Monetary arithmetic requires matching currency codes.");
            }
        }

        private static string NormalizeCurrencyCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 3).ToUpperInvariant();
            if (normalized.Length != 3)
            {
                throw new DomainValidationException(
                    "CurrencyCode must contain exactly three ASCII letters.");
            }

            for (var index = 0; index < normalized.Length; index++)
            {
                if (normalized[index] < 'A' || normalized[index] > 'Z')
                {
                    throw new DomainValidationException(
                        "CurrencyCode must contain exactly three ASCII letters.");
                }
            }

            return normalized;
        }
    }
}
