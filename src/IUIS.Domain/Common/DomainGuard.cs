using System;

namespace IUIS.Domain.Common
{
    internal static class DomainGuard
    {
        public static string RequiredIdentifier(string value, string parameterName)
        {
            return TextNormalizer.Required(value, parameterName, 64);
        }

        public static string RequiredActorIdentifier(string value, string parameterName)
        {
            return TextNormalizer.Required(value, parameterName, 64);
        }

        public static DateTime RequireUtc(DateTime value, string parameterName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new DomainValidationException(parameterName + " must use DateTimeKind.Utc.");
            }

            return value;
        }

        public static void RequireChronological(
            DateTime earlierUtc,
            DateTime laterUtc,
            string laterParameterName)
        {
            RequireUtc(earlierUtc, nameof(earlierUtc));
            RequireUtc(laterUtc, laterParameterName);

            if (laterUtc < earlierUtc)
            {
                throw new DomainValidationException(
                    laterParameterName + " cannot be earlier than the preceding timestamp.");
            }
        }

        public static long RequireNonNegativeVersion(long version, string parameterName)
        {
            if (version < 0)
            {
                throw new DomainValidationException(parameterName + " cannot be negative.");
            }

            return version;
        }

        public static long IncrementVersion(long currentVersion)
        {
            RequireNonNegativeVersion(currentVersion, nameof(currentVersion));

            try
            {
                return checked(currentVersion + 1L);
            }
            catch (OverflowException exception)
            {
                throw new DomainValidationException("The entity version cannot be advanced.", exception);
            }
        }
    }
}
