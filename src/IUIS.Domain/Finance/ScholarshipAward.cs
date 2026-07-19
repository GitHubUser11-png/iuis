using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class ScholarshipAwardEffect
    {
        public ScholarshipAwardEffect(
            string scholarshipAwardId,
            string studentId,
            string assessmentId,
            Money creditAmount,
            string description)
        {
            ScholarshipAwardId = RequireIdentifier(
                scholarshipAwardId,
                "SAW",
                nameof(scholarshipAwardId));
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            AssessmentId = RequireIdentifier(assessmentId, "ASM", nameof(assessmentId));
            if (creditAmount == null || creditAmount.Amount <= 0m)
            {
                throw new DomainValidationException(
                    "Scholarship credit amount must be greater than zero.");
            }

            CreditAmount = creditAmount;
            Description = TextNormalizer.Required(description, nameof(description), 300);
        }

        public string ScholarshipAwardId { get; private set; }

        public string StudentId { get; private set; }

        public string AssessmentId { get; private set; }

        public Money CreditAmount { get; private set; }

        public string Description { get; private set; }

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
    }

    public sealed class ScholarshipAward : EntityBase
    {
        private ScholarshipAward()
        {
            StudentId = string.Empty;
            ScholarshipProgramId = string.Empty;
            AcademicPeriodId = string.Empty;
            CurrencyCode = MoneyRules.PhilippinePesoCurrencyCode;
        }

        public ScholarshipAward(
            string id,
            string studentId,
            string scholarshipProgramId,
            string academicPeriodId,
            ScholarshipEffectKind effectKind,
            string currencyCode,
            Money fixedAmount,
            decimal percentage,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "SAW", nameof(id)), createdAtUtc, createdByUserId)
        {
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            ScholarshipProgramId = RequireIdentifier(
                scholarshipProgramId,
                "SCP",
                nameof(scholarshipProgramId));
            AcademicPeriodId = RequireIdentifier(
                academicPeriodId,
                "APD",
                nameof(academicPeriodId));
            EffectKind = RequireEffectKind(effectKind);
            CurrencyCode = Money.Zero(currencyCode).CurrencyCode;
            ConfigureEffect(fixedAmount, percentage);
            Status = ScholarshipAwardStatus.Prepared;
        }

        public string StudentId { get; private set; }

        public string ScholarshipProgramId { get; private set; }

        public string AcademicPeriodId { get; private set; }

        public ScholarshipEffectKind EffectKind { get; private set; }

        public string CurrencyCode { get; private set; }

        public Money FixedAmount { get; private set; }

        public decimal Percentage { get; private set; }

        public ScholarshipAwardStatus Status { get; private set; }

        public DateTime? ApprovedAtUtc { get; private set; }

        public string ApprovedByUserId { get; private set; }

        public string AppliedAssessmentId { get; private set; }

        public string AppliedAdjustmentId { get; private set; }

        public DateTime? AppliedAtUtc { get; private set; }

        public string AppliedByUserId { get; private set; }

        public string CancellationReason { get; private set; }

        public void Approve(DateTime approvedAtUtc, string approvedByUserId)
        {
            if (Status != ScholarshipAwardStatus.Prepared)
            {
                throw new DomainValidationException(
                    "Only Prepared Scholarship Awards can be approved.");
            }

            ApprovedAtUtc = DomainGuard.RequireUtc(approvedAtUtc, nameof(approvedAtUtc));
            ApprovedByUserId = DomainGuard.RequiredActorIdentifier(
                approvedByUserId,
                nameof(approvedByUserId));
            Status = ScholarshipAwardStatus.Approved;
            RecordChange(approvedAtUtc, approvedByUserId);
        }

        public ScholarshipAwardEffect PreviewEffect(
            string assessmentId,
            Money eligibleChargeAmount)
        {
            if (Status != ScholarshipAwardStatus.Approved)
            {
                throw new DomainValidationException(
                    "Only Approved Scholarship Awards can produce an effect.");
            }

            if (eligibleChargeAmount == null || eligibleChargeAmount.Amount <= 0m)
            {
                throw new DomainValidationException(
                    "Eligible charge amount must be greater than zero.");
            }

            RequireCurrency(eligibleChargeAmount);
            var credit = CalculateCredit(eligibleChargeAmount);
            return new ScholarshipAwardEffect(
                Id,
                StudentId,
                assessmentId,
                credit,
                "Scholarship award " + Id + " credit effect.");
        }

        public void MarkApplied(
            string assessmentId,
            string adjustmentId,
            DateTime appliedAtUtc,
            string appliedByUserId)
        {
            if (Status != ScholarshipAwardStatus.Approved)
            {
                throw new DomainValidationException(
                    "Only an Approved Scholarship Award can be marked as applied.");
            }

            AppliedAssessmentId = RequireIdentifier(
                assessmentId,
                "ASM",
                nameof(assessmentId));
            AppliedAdjustmentId = RequireIdentifier(
                adjustmentId,
                "FAD",
                nameof(adjustmentId));
            AppliedAtUtc = DomainGuard.RequireUtc(appliedAtUtc, nameof(appliedAtUtc));
            AppliedByUserId = DomainGuard.RequiredActorIdentifier(
                appliedByUserId,
                nameof(appliedByUserId));
            Status = ScholarshipAwardStatus.Applied;
            RecordChange(appliedAtUtc, appliedByUserId);
        }

        public void Cancel(
            string reason,
            DateTime cancelledAtUtc,
            string cancelledByUserId)
        {
            if (Status == ScholarshipAwardStatus.Applied)
            {
                throw new DomainValidationException(
                    "Applied Scholarship Awards cannot be cancelled directly.");
            }

            if (Status == ScholarshipAwardStatus.Cancelled)
            {
                throw new DomainValidationException("The Scholarship Award is already cancelled.");
            }

            CancellationReason = TextNormalizer.Required(reason, nameof(reason), 500);
            Status = ScholarshipAwardStatus.Cancelled;
            RecordChange(cancelledAtUtc, cancelledByUserId);
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status == ScholarshipAwardStatus.Applied)
            {
                throw new DomainValidationException(
                    "Applied Scholarship Awards cannot be archived.");
            }

            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status == ScholarshipAwardStatus.Applied)
            {
                throw new DomainValidationException(
                    "Applied Scholarship Awards cannot be restored.");
            }

            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private void ConfigureEffect(Money fixedAmount, decimal percentage)
        {
            if (EffectKind == ScholarshipEffectKind.FixedAmount)
            {
                if (fixedAmount == null || fixedAmount.Amount <= 0m)
                {
                    throw new DomainValidationException(
                        "Fixed Scholarship Awards require a positive fixed amount.");
                }

                RequireCurrency(fixedAmount);
                if (percentage != 0m)
                {
                    throw new DomainValidationException(
                        "Fixed Scholarship Awards cannot also define a percentage.");
                }

                FixedAmount = fixedAmount;
                Percentage = 0m;
                return;
            }

            if (fixedAmount != null)
            {
                throw new DomainValidationException(
                    "Percentage or full Scholarship Awards cannot define a fixed amount.");
            }

            if (EffectKind == ScholarshipEffectKind.PercentageOfEligibleCharges)
            {
                if (percentage <= 0m || percentage > 100m)
                {
                    throw new DomainValidationException(
                        "Scholarship percentage must be greater than zero and at most 100.");
                }

                if (decimal.Round(percentage, 2, MidpointRounding.AwayFromZero) != percentage)
                {
                    throw new DomainValidationException(
                        "Scholarship percentage cannot contain more than two fractional digits.");
                }

                Percentage = percentage;
                return;
            }

            if (percentage != 0m)
            {
                throw new DomainValidationException(
                    "Full Scholarship Awards cannot define a separate percentage.");
            }
        }

        private Money CalculateCredit(Money eligibleChargeAmount)
        {
            if (EffectKind == ScholarshipEffectKind.FullEligibleCharges)
            {
                return eligibleChargeAmount;
            }

            if (EffectKind == ScholarshipEffectKind.PercentageOfEligibleCharges)
            {
                return eligibleChargeAmount.Multiply(Percentage / 100m);
            }

            return FixedAmount.Amount > eligibleChargeAmount.Amount
                ? eligibleChargeAmount
                : FixedAmount;
        }

        private void RequireCurrency(Money value)
        {
            if (!StringComparer.Ordinal.Equals(CurrencyCode, value.CurrencyCode))
            {
                throw new DomainValidationException(
                    "Scholarship monetary values must use the Award currency.");
            }
        }

        private static ScholarshipEffectKind RequireEffectKind(ScholarshipEffectKind value)
        {
            if (!Enum.IsDefined(typeof(ScholarshipEffectKind), value)
                || value == ScholarshipEffectKind.Unspecified)
            {
                throw new DomainValidationException("A defined Scholarship effect kind is required.");
            }

            return value;
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
    }
}