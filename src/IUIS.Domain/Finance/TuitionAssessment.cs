using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class AssessmentChargeLine
    {
        public AssessmentChargeLine(
            string lineId,
            string chargeRuleId,
            string ruleCodeSnapshot,
            string descriptionSnapshot,
            AssessmentChargeCategory category,
            Money amount)
        {
            LineId = RequireIdentifier(lineId, "ACL", nameof(lineId));
            ChargeRuleId = RequireIdentifier(chargeRuleId, "ACR", nameof(chargeRuleId));
            RuleCodeSnapshot = NormalizeCode(ruleCodeSnapshot);
            DescriptionSnapshot = TextNormalizer.Required(
                descriptionSnapshot,
                nameof(descriptionSnapshot),
                200);
            Category = RequireCategory(category);
            Amount = RequirePositive(amount, nameof(amount));
        }

        public string LineId { get; private set; }

        public string ChargeRuleId { get; private set; }

        public string RuleCodeSnapshot { get; private set; }

        public string DescriptionSnapshot { get; private set; }

        public AssessmentChargeCategory Category { get; private set; }

        public Money Amount { get; private set; }

        private static string NormalizeCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 50).ToUpperInvariant();
            if (normalized.Contains(" "))
            {
                throw new DomainValidationException("Charge-rule code cannot contain spaces.");
            }

            return normalized;
        }

        private static AssessmentChargeCategory RequireCategory(AssessmentChargeCategory value)
        {
            if (!Enum.IsDefined(typeof(AssessmentChargeCategory), value)
                || value == AssessmentChargeCategory.Unspecified)
            {
                throw new DomainValidationException("A defined assessment-charge category is required.");
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

    public sealed class TuitionAssessment : EntityBase
    {
        private readonly List<AssessmentChargeLine> _chargeLines;

        private TuitionAssessment()
        {
            StudentId = string.Empty;
            EnrollmentId = string.Empty;
            AcademicPeriodId = string.Empty;
            CurrencyCode = MoneyRules.PhilippinePesoCurrencyCode;
            _chargeLines = new List<AssessmentChargeLine>();
        }

        public TuitionAssessment(
            string id,
            string studentId,
            string enrollmentId,
            string academicPeriodId,
            string currencyCode,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "ASM", nameof(id)), createdAtUtc, createdByUserId)
        {
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            EnrollmentId = RequireIdentifier(enrollmentId, "ENR", nameof(enrollmentId));
            AcademicPeriodId = RequireIdentifier(
                academicPeriodId,
                "APD",
                nameof(academicPeriodId));
            CurrencyCode = Money.Zero(currencyCode).CurrencyCode;
            Status = TuitionAssessmentStatus.Draft;
            _chargeLines = new List<AssessmentChargeLine>();
        }

        public string StudentId { get; private set; }

        public string EnrollmentId { get; private set; }

        public string AcademicPeriodId { get; private set; }

        public string CurrencyCode { get; private set; }

        public TuitionAssessmentStatus Status { get; private set; }

        public DateTime? PostedAtUtc { get; private set; }

        public string PostedByUserId { get; private set; }

        public DateTime? CancelledAtUtc { get; private set; }

        public string CancelledByUserId { get; private set; }

        public string CancellationReason { get; private set; }

        public IReadOnlyList<AssessmentChargeLine> ChargeLines
        {
            get { return _chargeLines.AsReadOnly(); }
        }

        public Money GrossAmount
        {
            get
            {
                var total = Money.Zero(CurrencyCode);
                foreach (var line in _chargeLines)
                {
                    total = total.Add(line.Amount);
                }

                return total;
            }
        }

        public void AddChargeLine(
            AssessmentChargeLine line,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            if (line == null)
            {
                throw new DomainValidationException("Assessment charge line is required.");
            }

            RequireCurrency(line.Amount);
            if (_chargeLines.Any(existing =>
                StringComparer.Ordinal.Equals(existing.LineId, line.LineId)))
            {
                throw new DomainValidationException(
                    "Assessment charge-line identifiers must be unique within an Assessment.");
            }

            _chargeLines.Add(line);
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void RemoveChargeLine(
            string lineId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            var canonicalLineId = RequireIdentifier(lineId, "ACL", nameof(lineId));
            var existing = _chargeLines.FirstOrDefault(line =>
                StringComparer.Ordinal.Equals(line.LineId, canonicalLineId));
            if (existing == null)
            {
                throw new DomainValidationException("The Assessment charge line does not exist.");
            }

            _chargeLines.Remove(existing);
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void Post(DateTime postedAtUtc, string postedByUserId)
        {
            RequireDraft();
            if (_chargeLines.Count == 0)
            {
                throw new DomainValidationException(
                    "A Tuition Assessment requires at least one charge before posting.");
            }

            if (GrossAmount.Amount <= 0m)
            {
                throw new DomainValidationException(
                    "A Tuition Assessment gross amount must be greater than zero.");
            }

            PostedAtUtc = DomainGuard.RequireUtc(postedAtUtc, nameof(postedAtUtc));
            PostedByUserId = DomainGuard.RequiredActorIdentifier(
                postedByUserId,
                nameof(postedByUserId));
            Status = TuitionAssessmentStatus.Posted;
            RecordChange(postedAtUtc, postedByUserId);
        }

        public void CancelDraft(
            string reason,
            DateTime cancelledAtUtc,
            string cancelledByUserId)
        {
            RequireDraft();
            CancellationReason = TextNormalizer.Required(reason, nameof(reason), 500);
            CancelledAtUtc = DomainGuard.RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
            CancelledByUserId = DomainGuard.RequiredActorIdentifier(
                cancelledByUserId,
                nameof(cancelledByUserId));
            Status = TuitionAssessmentStatus.Cancelled;
            RecordChange(cancelledAtUtc, cancelledByUserId);
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status == TuitionAssessmentStatus.Posted)
            {
                throw new DomainValidationException(
                    "Posted Tuition Assessments are immutable and cannot be archived.");
            }

            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status == TuitionAssessmentStatus.Posted)
            {
                throw new DomainValidationException(
                    "Posted Tuition Assessments are immutable and cannot be restored.");
            }

            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private void RequireDraft()
        {
            if (Status != TuitionAssessmentStatus.Draft)
            {
                throw new DomainValidationException(
                    "Only Draft Tuition Assessments can be changed.");
            }
        }

        private void RequireCurrency(Money amount)
        {
            if (!StringComparer.Ordinal.Equals(CurrencyCode, amount.CurrencyCode))
            {
                throw new DomainValidationException(
                    "Assessment charge currency must match the Assessment currency.");
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
    }
}