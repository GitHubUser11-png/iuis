using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class AssessmentChargeRule : EntityBase
    {
        private AssessmentChargeRule()
        {
            Code = string.Empty;
            Description = string.Empty;
            Rate = Money.PhilippinePeso(0m);
        }

        public AssessmentChargeRule(
            string id,
            string code,
            string description,
            AssessmentChargeCategory category,
            ChargeCalculationKind calculationKind,
            Money rate,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "ACR", nameof(id)), createdAtUtc, createdByUserId)
        {
            Code = NormalizeCode(code);
            Description = TextNormalizer.Required(description, nameof(description), 200);
            Category = RequireDefined(category, nameof(category));
            CalculationKind = RequireDefined(calculationKind, nameof(calculationKind));
            Rate = RequirePositive(rate, nameof(rate));
            Status = ChargeRuleStatus.Draft;
        }

        public string Code { get; private set; }

        public string Description { get; private set; }

        public AssessmentChargeCategory Category { get; private set; }

        public ChargeCalculationKind CalculationKind { get; private set; }

        public Money Rate { get; private set; }

        public ChargeRuleStatus Status { get; private set; }

        public void UpdateDraftDetails(
            string description,
            AssessmentChargeCategory category,
            ChargeCalculationKind calculationKind,
            Money rate,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            Description = TextNormalizer.Required(description, nameof(description), 200);
            Category = RequireDefined(category, nameof(category));
            CalculationKind = RequireDefined(calculationKind, nameof(calculationKind));
            Rate = RequirePositive(rate, nameof(rate));
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void Activate(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != ChargeRuleStatus.Draft && Status != ChargeRuleStatus.Inactive)
            {
                throw new DomainValidationException(
                    "Only Draft or Inactive charge rules can be activated.");
            }

            Status = ChargeRuleStatus.Active;
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void Deactivate(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != ChargeRuleStatus.Active)
            {
                throw new DomainValidationException("Only Active charge rules can be deactivated.");
            }

            Status = ChargeRuleStatus.Inactive;
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void Retire(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status == ChargeRuleStatus.Retired)
            {
                throw new DomainValidationException("The charge rule is already retired.");
            }

            if (Status == ChargeRuleStatus.Active)
            {
                throw new DomainValidationException(
                    "An Active charge rule must be deactivated before retirement.");
            }

            Status = ChargeRuleStatus.Retired;
            RecordChange(changedAtUtc, changedByUserId);
        }

        public Money Calculate(decimal billableAcademicUnits)
        {
            if (Status != ChargeRuleStatus.Active)
            {
                throw new DomainValidationException("Only Active charge rules can calculate charges.");
            }

            if (CalculationKind == ChargeCalculationKind.FixedAmount)
            {
                return Rate;
            }

            RequireAcademicUnits(billableAcademicUnits);
            return Rate.Multiply(billableAcademicUnits);
        }

        private void RequireDraft()
        {
            if (Status != ChargeRuleStatus.Draft)
            {
                throw new DomainValidationException("Only Draft charge rules can be edited.");
            }
        }

        private static string RequireIdentifier(string value, string prefix, string parameterName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
            {
                throw new DomainValidationException(
                    parameterName + " must use the " + prefix + " identifier prefix.");
            }

            return identifier.Value;
        }

        private static string NormalizeCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 50).ToUpperInvariant();
            if (normalized.Contains(" "))
            {
                throw new DomainValidationException("Charge-rule code cannot contain spaces.");
            }

            return normalized;
        }

        private static TEnum RequireDefined<TEnum>(TEnum value, string parameterName)
            where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), value) || Convert.ToInt32(value) == 0)
            {
                throw new DomainValidationException(parameterName + " must be a defined value.");
            }

            return value;
        }

        private static Money RequirePositive(Money value, string parameterName)
        {
            if (value == null || value.Amount <= 0m)
            {
                throw new DomainValidationException(parameterName + " must be greater than zero.");
            }

            return value;
        }

        private static void RequireAcademicUnits(decimal units)
        {
            if (units <= 0m)
            {
                throw new DomainValidationException(
                    "Billable academic units must be greater than zero.");
            }

            if (decimal.Round(units, 2, MidpointRounding.AwayFromZero) != units)
            {
                throw new DomainValidationException(
                    "Billable academic units cannot contain more than two fractional digits.");
            }
        }
    }
}