using System;
using System.Globalization;
using System.Text.RegularExpressions;

using IUIS.Domain.Common;

namespace IUIS.Domain.Identity
{
    public sealed class InstitutionIdentifier : IEquatable<InstitutionIdentifier>
    {
        private static readonly Regex Pattern = new Regex(
            "^(?<prefix>[A-Z]{2,8})-(?<year>[0-9]{4})-(?<sequence>[0-9]{6})$",
            RegexOptions.CultureInvariant);

        private InstitutionIdentifier(string value, string prefix, int year, int sequence)
        {
            Value = value;
            Prefix = prefix;
            Year = year;
            Sequence = sequence;
        }

        public string Value { get; }
        public string Prefix { get; }
        public int Year { get; }
        public int Sequence { get; }

        public static InstitutionIdentifier Create(string prefix, int year, int sequence)
        {
            var normalizedPrefix = (prefix ?? string.Empty).Trim().ToUpperInvariant();
            if (!Regex.IsMatch(normalizedPrefix, "^[A-Z]{2,8}$"))
            {
                throw new DomainValidationException("Identifier prefix must contain 2 to 8 uppercase letters.");
            }

            if (year < 2000 || year > 9999)
            {
                throw new DomainValidationException("Identifier year must be between 2000 and 9999.");
            }

            if (sequence < 1 || sequence > 999999)
            {
                throw new DomainValidationException("Identifier sequence must be between 1 and 999999.");
            }

            var value = string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1:0000}-{2:000000}",
                normalizedPrefix,
                year,
                sequence);

            return new InstitutionIdentifier(value, normalizedPrefix, year, sequence);
        }

        public static InstitutionIdentifier Parse(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
            var match = Pattern.Match(normalized);
            if (!match.Success)
            {
                throw new DomainValidationException("Identifier must use PREFIX-YYYY-NNNNNN format.");
            }

            return Create(
                match.Groups["prefix"].Value,
                int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups["sequence"].Value, CultureInfo.InvariantCulture));
        }

        public bool Equals(InstitutionIdentifier other)
        {
            return !ReferenceEquals(other, null)
                && StringComparer.Ordinal.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstitutionIdentifier);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
