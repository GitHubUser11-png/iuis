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
                throw new DomainValidationException("Charge-rule code cannot contain spaces.");
            return normalized;
        }

        private static AssessmentChargeCategory RequireCategory(AssessmentChargeCategory value)
        {
            if (!Enum.IsDefined(typeof(AssessmentChargeCategory), value)
                || value == AssessmentChargeCategory.Unspecified)
                throw new DomainValidationException("A defined assessment-charge category is required.");
            return value;
        }

        private static Money RequirePositive(Money value, string parameterName)
        {
            if (value == null || value.Amount <= 0m)
                throw new DomainValidationException(parameterName + " must be greater than zero.");
            return value;
        }

        private static string RequireIdentifier(string value, string prefix, string parameterName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
                throw new DomainValidationException(
                    parameterName + " must use the " + prefix + " identifier prefix.");
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
            AcademicPeriodId = RequireIdentifier(academicPeriodId, "APD", nameof(academicPeriodId));
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
                foreach (var line in _chargeLines) total = total.Add(line.Amount);
                return total;
            }
        }

        public static TuitionAssessment Rehydrate(
            string id,
            string studentId,
            string enrollmentId,
            string academicPeriodId,
            string currencyCode,
            IEnumerable<AssessmentChargeLine> chargeLines,
            TuitionAssessmentStatus status,
            DateTime? postedAtUtc,
            string postedByUserId,
            DateTime? cancelledAtUtc,
            string cancelledByUserId,
            string cancellationReason,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            if (!Enum.IsDefined(typeof(TuitionAssessmentStatus), status))
                throw new DomainValidationException("Persisted Tuition Assessment status is invalid.");
            if (chargeLines == null)
                throw new DomainValidationException("Persisted Assessment charge lines are required.");

            var record = new TuitionAssessment(
                id,
                studentId,
                enrollmentId,
                academicPeriodId,
                currencyCode,
                createdAtUtc,
                createdByUserId);
            foreach (var line in chargeLines)
            {
                if (line == null)
                    throw new DomainValidationException("Persisted Assessment charge line is invalid.");
                record.RequireCurrency(line.Amount);
                if (record._chargeLines.Any(existing =>
                    StringComparer.Ordinal.Equals(existing.LineId, line.LineId)))
                    throw new DomainValidationException("Persisted Assessment charge-line IDs must be unique.");
                record._chargeLines.Add(line);
            }

            if (status == TuitionAssessmentStatus.Posted)
            {
                if (record._chargeLines.Count == 0 || record.GrossAmount.Amount <= 0m)
                    throw new DomainValidationException("Posted Tuition Assessments require positive charge lines.");
                record.PostedAtUtc = RequireUtc(postedAtUtc, nameof(postedAtUtc));
                record.PostedByUserId = DomainGuard.RequiredActorIdentifier(
                    postedByUserId,
                    nameof(postedByUserId));
                if (cancelledAtUtc.HasValue || !string.IsNullOrWhiteSpace(cancelledByUserId)
                    || !string.IsNullOrWhiteSpace(cancellationReason))
                    throw new DomainValidationException("Posted Tuition Assessments cannot contain cancellation metadata.");
            }
            else if (status == TuitionAssessmentStatus.Cancelled)
            {
                record.CancelledAtUtc = RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
                record.CancelledByUserId = DomainGuard.RequiredActorIdentifier(
                    cancelledByUserId,
                    nameof(cancelledByUserId));
                record.CancellationReason = TextNormalizer.Required(
                    cancellationReason,
                    nameof(cancellationReason),
                    500);
                if (postedAtUtc.HasValue || !string.IsNullOrWhiteSpace(postedByUserId))
                    throw new DomainValidationException("Cancelled Tuition Assessments cannot contain posting metadata.");
            }
            else if (postedAtUtc.HasValue || cancelledAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(postedByUserId)
                || !string.IsNullOrWhiteSpace(cancelledByUserId)
                || !string.IsNullOrWhiteSpace(cancellationReason))
            {
                throw new DomainValidationException("Draft Tuition Assessments cannot contain lifecycle metadata.");
            }

            record.Status = status;
            record.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return record;
        }

        public void AddChargeLine(
            AssessmentChargeLine line,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            if (line == null)
                throw new DomainValidationException("Assessment charge line is required.");
            RequireCurrency(line.Amount);
            if (_chargeLines.Any(existing =>
                StringComparer.Ordinal.Equals(existing.LineId, line.LineId)))
                throw new DomainValidationException("Assessment charge-line identifiers must be unique within an Assessment.");
            RecordChange(changedAtUtc, changedByUserId);
            _chargeLines.Add(line);
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
                throw new DomainValidationException("The Assessment charge line does not exist.");
            RecordChange(changedAtUtc, changedByUserId);
            _chargeLines.Remove(existing);
        }

        public void Post(DateTime postedAtUtc, string postedByUserId)
        {
            RequireDraft();
            if (_chargeLines.Count == 0)
                throw new DomainValidationException("A Tuition Assessment requires at least one charge before posting.");
            if (GrossAmount.Amount <= 0m)
                throw new DomainValidationException("A Tuition Assessment gross amount must be greater than zero.");
            var canonicalTime = DomainGuard.RequireUtc(postedAtUtc, nameof(postedAtUtc));
            var actor = DomainGuard.RequiredActorIdentifier(postedByUserId, nameof(postedByUserId));
            RecordChange(canonicalTime, actor);
            PostedAtUtc = canonicalTime;
            PostedByUserId = actor;
            Status = TuitionAssessmentStatus.Posted;
        }

        public void CancelDraft(string reason, DateTime cancelledAtUtc, string cancelledByUserId)
        {
            RequireDraft();
            var canonicalReason = TextNormalizer.Required(reason, nameof(reason), 500);
            var canonicalTime = DomainGuard.RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
            var actor = DomainGuard.RequiredActorIdentifier(cancelledByUserId, nameof(cancelledByUserId));
            RecordChange(canonicalTime, actor);
            CancellationReason = canonicalReason;
            CancelledAtUtc = canonicalTime;
            CancelledByUserId = actor;
            Status = TuitionAssessmentStatus.Cancelled;
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status == TuitionAssessmentStatus.Posted)
                throw new DomainValidationException("Posted Tuition Assessments are immutable and cannot be archived.");
            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status == TuitionAssessmentStatus.Posted)
                throw new DomainValidationException("Posted Tuition Assessments are immutable and cannot be restored.");
            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private void RequireDraft()
        {
            if (Status != TuitionAssessmentStatus.Draft)
                throw new DomainValidationException("Only Draft Tuition Assessments can be changed.");
        }

        private void RequireCurrency(Money amount)
        {
            if (!StringComparer.Ordinal.Equals(CurrencyCode, amount.CurrencyCode))
                throw new DomainValidationException("Assessment charge currency must match the Assessment currency.");
        }

        private static DateTime RequireUtc(DateTime? value, string parameterName)
        {
            if (!value.HasValue)
                throw new DomainValidationException(parameterName + " is required.");
            return DomainGuard.RequireUtc(value.Value, parameterName);
        }

        private static string RequireIdentifier(string value, string prefix, string parameterName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
                throw new DomainValidationException(
                    parameterName + " must use the " + prefix + " identifier prefix.");
            return identifier.Value;
        }
    }
}
