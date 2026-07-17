using System;

namespace IUIS.Domain.Finance
{
    public static class MoneyRules
    {
        public const int Scale = 2;

        public const string PhilippinePesoCurrencyCode = "PHP";

        public const MidpointRounding RoundingMode = MidpointRounding.AwayFromZero;

        public static decimal Round(decimal amount)
        {
            return decimal.Round(amount, Scale, RoundingMode);
        }
    }
}
