using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class FinancialAdjustment : EntityBase
    {
        private FinancialAdjustment()
        {
            StudentId = string.Empty;
            AssessmentId = string.Empty;
            SourceRecordId = string.Empty;
            Reason = string.Empty;
            Amount = Money.PhilippinePeso(0m);
        }

        public FinancialAdjustment(
            string id,
            string studentId,
            string assessmentId,
            FinancialAdjustmentDirection direction,
            Money amount,
            FinancialAdjustmentSourceKind sourceKind,
            string sourceRecordId,
            string reason,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "FAD", nameof(id)), createdAtUtc, createdByUserId)
        {
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            AssessmentId = RequireIdentifier(assessmentId, "ASM", nameof(assessmentId));
            Direction = RequireDirection(direction);
            Amount = RequirePositive(amount, nameof(amount));
            SourceKind = RequireSourceKind(sourceKind);
            SourceRecordId = InstitutionIdentifier.Parse(sourceRecordId).Value;
            Reason = TextNormalizer.Required(reason, nameof(reason), 500);
            Status = FinancialAdjustmentStatus.Prepared;
        }

        public string StudentId { get; private set; }
        public string AssessmentId { get; private set; }
        public FinancialAdjustmentDirection Direction { get; private set; }
        public Money Amount { get; private set; }
        public FinancialAdjustmentSourceKind SourceKind { get; private set; }
        public string SourceRecordId { get; private set; }
        public string Reason { get; private set; }
        public FinancialAdjustmentStatus Status { get; private set; }
        public DateTime? PostedAtUtc { get; private set; }
        public string PostedByUserId { get; private set; }
        public string CancellationReason { get; private set; }
        public DateTime? CancelledAtUtc { get; private set; }
        public string CancelledByUserId { get; private set; }

        public static FinancialAdjustment Rehydrate(
            string id,
            string studentId,
            string assessmentId,
            FinancialAdjustmentDirection direction,
            Money amount,
            FinancialAdjustmentSourceKind sourceKind,
            string sourceRecordId,
            string reason,
            FinancialAdjustmentStatus status,
            DateTime? postedAtUtc,
            string postedByUserId,
            string cancellationReason,
            DateTime? cancelledAtUtc,
            string cancelledByUserId,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            if (!Enum.IsDefined(typeof(FinancialAdjustmentStatus), status))
                throw new DomainValidationException("Persisted Financial Adjustment status is invalid.");

            var record = new FinancialAdjustment(
                id,
                studentId,
                assessmentId,
                direction,
                amount,
                sourceKind,
                sourceRecordId,
                reason,
                createdAtUtc,
                createdByUserId);

            if (status == FinancialAdjustmentStatus.Posted)
            {
                record.PostedAtUtc = RequireUtc(postedAtUtc, nameof(postedAtUtc));
                record.PostedByUserId = DomainGuard.RequiredActorIdentifier(
                    postedByUserId,
                    nameof(postedByUserId));
                if (cancelledAtUtc.HasValue || !string.IsNullOrWhiteSpace(cancelledByUserId)
                    || !string.IsNullOrWhiteSpace(cancellationReason))
                    throw new DomainValidationException("Posted Financial Adjustments cannot contain cancellation metadata.");
            }
            else if (status == FinancialAdjustmentStatus.Cancelled)
            {
                record.CancellationReason = TextNormalizer.Required(
                    cancellationReason,
                    nameof(cancellationReason),
                    500);
                record.CancelledAtUtc = RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
                record.CancelledByUserId = DomainGuard.RequiredActorIdentifier(
                    cancelledByUserId,
                    nameof(cancelledByUserId));
                if (postedAtUtc.HasValue || !string.IsNullOrWhiteSpace(postedByUserId))
                    throw new DomainValidationException("Cancelled Financial Adjustments cannot contain posting metadata.");
            }
            else if (postedAtUtc.HasValue || cancelledAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(postedByUserId)
                || !string.IsNullOrWhiteSpace(cancelledByUserId)
                || !string.IsNullOrWhiteSpace(cancellationReason))
            {
                throw new DomainValidationException("Prepared Financial Adjustments cannot contain decision metadata.");
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

        public static FinancialAdjustment CreateScholarshipCredit(
            string adjustmentId,
            ScholarshipAwardEffect effect,
            DateTime createdAtUtc,
            string createdByUserId)
        {
            if (effect == null)
                throw new DomainValidationException("Scholarship Award effect is required.");
            return new FinancialAdjustment(
                adjustmentId,
                effect.StudentId,
                effect.AssessmentId,
                FinancialAdjustmentDirection.Credit,
                effect.CreditAmount,
                FinancialAdjustmentSourceKind.ScholarshipAward,
                effect.ScholarshipAwardId,
                effect.Description,
                createdAtUtc,
                createdByUserId);
        }

        public void Post(DateTime postedAtUtc, string postedByUserId)
        {
            if (Status != FinancialAdjustmentStatus.Prepared)
                throw new DomainValidationException("Only Prepared Financial Adjustments can be posted.");
            var canonicalTime = DomainGuard.RequireUtc(postedAtUtc, nameof(postedAtUtc));
            var actor = DomainGuard.RequiredActorIdentifier(postedByUserId, nameof(postedByUserId));
            RecordChange(canonicalTime, actor);
            PostedAtUtc = canonicalTime;
            PostedByUserId = actor;
            Status = FinancialAdjustmentStatus.Posted;
        }

        public void CancelPrepared(
            string reason,
            DateTime cancelledAtUtc,
            string cancelledByUserId)
        {
            if (Status != FinancialAdjustmentStatus.Prepared)
                throw new DomainValidationException("Only Prepared Financial Adjustments can be cancelled.");
            var canonicalReason = TextNormalizer.Required(reason, nameof(reason), 500);
            var canonicalTime = DomainGuard.RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
            var actor = DomainGuard.RequiredActorIdentifier(cancelledByUserId, nameof(cancelledByUserId));
            RecordChange(canonicalTime, actor);
            CancellationReason = canonicalReason;
            CancelledAtUtc = canonicalTime;
            CancelledByUserId = actor;
            Status = FinancialAdjustmentStatus.Cancelled;
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status == FinancialAdjustmentStatus.Posted)
                throw new DomainValidationException("Posted Financial Adjustments are immutable and cannot be archived.");
            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status == FinancialAdjustmentStatus.Posted)
                throw new DomainValidationException("Posted Financial Adjustments are immutable and cannot be restored.");
            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private static FinancialAdjustmentDirection RequireDirection(FinancialAdjustmentDirection value)
        {
            if (!Enum.IsDefined(typeof(FinancialAdjustmentDirection), value)
                || value == FinancialAdjustmentDirection.Unspecified)
                throw new DomainValidationException("A defined Financial Adjustment direction is required.");
            return value;
        }

        private static FinancialAdjustmentSourceKind RequireSourceKind(FinancialAdjustmentSourceKind value)
        {
            if (!Enum.IsDefined(typeof(FinancialAdjustmentSourceKind), value)
                || value == FinancialAdjustmentSourceKind.Unspecified)
                throw new DomainValidationException("A defined Financial Adjustment source kind is required.");
            return value;
        }

        private static Money RequirePositive(Money value, string parameterName)
        {
            if (value == null || value.Amount <= 0m)
                throw new DomainValidationException(parameterName + " must be greater than zero.");
            return value;
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
