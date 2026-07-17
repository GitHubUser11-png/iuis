using System;
using System.Linq;

using IUIS.Domain.Common;

namespace IUIS.Domain.People
{
    public sealed class ContactInformation : IEquatable<ContactInformation>
    {
        public ContactInformation(
            string emailAddress,
            string mobileNumber,
            string alternatePhoneNumber)
        {
            EmailAddress = NormalizeEmail(emailAddress);
            MobileNumber = NormalizePhone(mobileNumber, nameof(mobileNumber));
            AlternatePhoneNumber = NormalizePhone(
                alternatePhoneNumber,
                nameof(alternatePhoneNumber));
        }

        public string EmailAddress { get; }

        public string MobileNumber { get; }

        public string AlternatePhoneNumber { get; }

        public bool IsEmpty
        {
            get
            {
                return EmailAddress == null
                    && MobileNumber == null
                    && AlternatePhoneNumber == null;
            }
        }

        public bool Equals(ContactInformation other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(EmailAddress, other.EmailAddress)
                && StringComparer.Ordinal.Equals(MobileNumber, other.MobileNumber)
                && StringComparer.Ordinal.Equals(AlternatePhoneNumber, other.AlternatePhoneNumber);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContactInformation);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EmailAddress == null
                    ? 0
                    : StringComparer.OrdinalIgnoreCase.GetHashCode(EmailAddress);
                hashCode = (hashCode * 397) ^ GetOrdinalHashCode(MobileNumber);
                hashCode = (hashCode * 397) ^ GetOrdinalHashCode(AlternatePhoneNumber);
                return hashCode;
            }
        }

        private static string NormalizeEmail(string value)
        {
            var normalized = TextNormalizer.Optional(value, nameof(value), 254);
            if (normalized == null)
            {
                return null;
            }

            if (normalized.Any(char.IsWhiteSpace))
            {
                throw new DomainValidationException("EmailAddress cannot contain whitespace.");
            }

            var atIndex = normalized.IndexOf('@');
            if (atIndex <= 0
                || atIndex != normalized.LastIndexOf('@')
                || atIndex == normalized.Length - 1)
            {
                throw new DomainValidationException("EmailAddress is not structurally valid.");
            }

            var domain = normalized.Substring(atIndex + 1);
            if (domain.IndexOf('.') <= 0 || domain.EndsWith(".", StringComparison.Ordinal))
            {
                throw new DomainValidationException("EmailAddress must contain a valid domain.");
            }

            return normalized.ToLowerInvariant();
        }

        private static string NormalizePhone(string value, string parameterName)
        {
            var normalized = TextNormalizer.Optional(value, parameterName, 40);
            if (normalized == null)
            {
                return null;
            }

            var output = new char[normalized.Length];
            var outputLength = 0;

            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                if (char.IsDigit(character))
                {
                    output[outputLength++] = character;
                    continue;
                }

                if (character == '+' && outputLength == 0)
                {
                    output[outputLength++] = character;
                    continue;
                }

                if (character == ' '
                    || character == '-'
                    || character == '('
                    || character == ')'
                    || character == '.')
                {
                    continue;
                }

                throw new DomainValidationException(
                    parameterName + " contains unsupported characters.");
            }

            var result = new string(output, 0, outputLength);
            var digits = result[0] == '+' ? result.Substring(1) : result;

            if (digits.Length < 7 || digits.Length > 15 || digits.Any(character => !char.IsDigit(character)))
            {
                throw new DomainValidationException(
                    parameterName + " must contain between 7 and 15 digits.");
            }

            return result;
        }

        private static int GetOrdinalHashCode(string value)
        {
            return value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
        }
    }
}
