using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Services
{
    internal static class ServiceDomainGuard
    {
        public static string RequireIdentifier(
            string value,
            string expectedPrefix,
            string parameterName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, expectedPrefix))
            {
                throw new DomainValidationException(
                    parameterName + " must use the " + expectedPrefix + " identifier prefix.");
            }

            return identifier.Value;
        }

        public static string RequiredText(
            string value,
            string parameterName,
            int maximumLength)
        {
            return TextNormalizer.Required(value, parameterName, maximumLength);
        }

        public static string OptionalText(
            string value,
            string parameterName,
            int maximumLength)
        {
            return TextNormalizer.Optional(value, parameterName, maximumLength);
        }

        public static string RequiredCode(
            string value,
            string parameterName,
            int maximumLength)
        {
            return TextNormalizer.Required(value, parameterName, maximumLength)
                .ToUpperInvariant();
        }

        public static T RequireDefined<T>(T value, string parameterName)
            where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new DomainValidationException(
                    parameterName + " contains an unsupported value.");
            }

            return value;
        }

        public static int RequirePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new DomainValidationException(parameterName + " must be greater than zero.");
            }

            return value;
        }

        public static DateTime RequireUtc(DateTime value, string parameterName)
        {
            return DomainGuard.RequireUtc(value, parameterName);
        }
    }
}
