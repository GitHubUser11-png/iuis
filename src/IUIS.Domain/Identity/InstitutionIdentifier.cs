using System;
using System.Globalization;

using IUIS.Domain.Common;

namespace IUIS.Domain.Identity
{
    public sealed class InstitutionIdentifier :
        IComparable<InstitutionIdentifier>,
        IEquatable<InstitutionIdentifier>
    {
        private const int MinimumYear = 2000;
        private const int MaximumYear = 9999;
        private const int MaximumSequence = 999999;

        public InstitutionIdentifier(string prefix, int year, int sequence)
        {
            Prefix = NormalizePrefix(prefix, nameof(prefix));

            if (year < MinimumYear || year > MaximumYear)
            {
                throw new DomainValidationException(
                    nameof(year) + " must be between " + MinimumYear + " and " + MaximumYear + ".");
            }

            if (sequence < 1 || sequence > MaximumSequence)
            {
                throw new DomainValidationException(
                    nameof(sequence) + " must be between 1 and " + MaximumSequence + ".");
            }

            Year = year;
            Sequence = sequence;
        }

        public string Prefix { get; }

        public int Year { get; }

        public int Sequence { get; }

        public static InstitutionIdentifier Parse(string value)
        {
            InstitutionIdentifier result;
            if (!TryParse(value, out result))
            {
                throw new DomainValidationException(
                    "Institution identifiers must use the canonical PREFIX-YYYY-NNNNNN format.");
            }

            return result;
        }

        public static bool TryParse(string value, out InstitutionIdentifier result)
        {
            result = null;

            if (string.IsNullOrEmpty(value)
                || !StringComparer.Ordinal.Equals(value, value.Trim()))
            {
                return false;
            }

            var parts = value.Split('-');
            if (parts.Length != 3
                || parts[1].Length != 4
                || parts[2].Length != 6
                || !IsCanonicalPrefix(parts[0]))
            {
                return false;
            }

            int year;
            int sequence;
            if (!int.TryParse(
                    parts[1],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out year)
                || !int.TryParse(
                    parts[2],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out sequence))
            {
                return false;
            }

            if (year < MinimumYear
                || year > MaximumYear
                || sequence < 1
                || sequence > MaximumSequence)
            {
                return false;
            }

            var candidate = new InstitutionIdentifier(parts[0], year, sequence);
            if (!StringComparer.Ordinal.Equals(candidate.ToString(), value))
            {
                return false;
            }

            result = candidate;
            return true;
        }

        public InstitutionIdentifier RequirePrefix(string expectedPrefix, string parameterName)
        {
            var normalizedExpectedPrefix = NormalizePrefix(expectedPrefix, nameof(expectedPrefix));
            if (!StringComparer.Ordinal.Equals(Prefix, normalizedExpectedPrefix))
            {
                throw new DomainValidationException(
                    parameterName + " must use the " + normalizedExpectedPrefix + " identifier prefix.");
            }

            return this;
        }

        public int CompareTo(InstitutionIdentifier other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            var prefixComparison = StringComparer.Ordinal.Compare(Prefix, other.Prefix);
            if (prefixComparison != 0)
            {
                return prefixComparison;
            }

            var yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0)
            {
                return yearComparison;
            }

            return Sequence.CompareTo(other.Sequence);
        }

        public bool Equals(InstitutionIdentifier other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StringComparer.Ordinal.Equals(Prefix, other.Prefix)
                && Year == other.Year
                && Sequence == other.Sequence;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstitutionIdentifier);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StringComparer.Ordinal.GetHashCode(Prefix);
                hashCode = (hashCode * 397) ^ Year;
                hashCode = (hashCode * 397) ^ Sequence;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1:0000}-{2:000000}",
                Prefix,
                Year,
                Sequence);
        }

        private static string NormalizePrefix(string value, string parameterName)
        {
            var normalized = TextNormalizer.Required(value, parameterName, 12).ToUpperInvariant();
            if (normalized.Length < 2 || !IsCanonicalPrefix(normalized))
            {
                throw new DomainValidationException(
                    parameterName + " must contain 2 to 12 uppercase ASCII letters or digits.");
            }

            return normalized;
        }

        private static bool IsCanonicalPrefix(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2 || value.Length > 12)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var isUpperLetter = character >= 'A' && character <= 'Z';
                var isDigit = character >= '0' && character <= '9';
                if (!isUpperLetter && !isDigit)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static class IdentifierPrefixes
    {
        public const string Student = "STU";
        public const string Employee = "EMP";
        public const string User = "USR";
        public const string Session = "SES";
        public const string Course = "CRS";
        public const string Subject = "SUB";
        public const string Enrollment = "ENR";
        public const string Payment = "PAY";
        public const string Borrowing = "BRW";
    }

    public static class IdentifierPolicy
    {
        public static string Require(string value, string expectedPrefix, string parameterName)
        {
            return InstitutionIdentifier.Parse(value)
                .RequirePrefix(expectedPrefix, parameterName)
                .ToString();
        }

        public static bool HasPrefix(string value, string expectedPrefix)
        {
            InstitutionIdentifier identifier;
            if (!InstitutionIdentifier.TryParse(value, out identifier))
            {
                return false;
            }

            try
            {
                identifier.RequirePrefix(expectedPrefix, nameof(value));
                return true;
            }
            catch (DomainValidationException)
            {
                return false;
            }
        }
    }
}
