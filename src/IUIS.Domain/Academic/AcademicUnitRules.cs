using System;

using IUIS.Domain.Common;

namespace IUIS.Domain.Academic
{
    public static class AcademicUnitRules
    {
        public const decimal MaximumUnitsPerSubject = 99.00m;

        public static decimal RequireValid(decimal units, string parameterName)
        {
            if (units <= 0m || units > MaximumUnitsPerSubject)
            {
                throw new DomainValidationException(
                    parameterName + " must be greater than zero and cannot exceed "
                    + MaximumUnitsPerSubject.ToString("0.00") + ".");
            }

            if (decimal.Round(units, 2, MidpointRounding.AwayFromZero) != units)
            {
                throw new DomainValidationException(
                    parameterName + " cannot contain more than two fractional digits.");
            }

            return units;
        }

        public static decimal Sum(decimal currentTotal, decimal unitsToAdd)
        {
            RequireValid(unitsToAdd, nameof(unitsToAdd));

            try
            {
                return checked(currentTotal + unitsToAdd);
            }
            catch (OverflowException exception)
            {
                throw new DomainValidationException(
                    "The academic-unit total cannot be calculated.",
                    exception);
            }
        }
    }
}
