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

        public static FinancialAdjustment CreateScholarshipCredit(
            string adjustmentId,
            ScholarshipAwardEffect effect,
            DateTime createdAtUtc,
            string createdByUserId)
        {
            if (effect == null)
            {
                throw new DomainValidationException("Scholarship Award effect is required.");
            }

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
            {
                throw new DomainValidationException(
                    "Only Prepared Financial Adjustments can be posted.");
            }

            PostedAtUtc = DomainGuard.RequireUtc(postedAtUtc, nameof(postedAtUtc));
            PostedByUserId = DomainGuard.RequiredActorIdentifier(
                postedByUserId,
                nameof(postedByUserId));
            Status = FinancialAdjustmentStatus.Posted;
            RecordChange(postedAtUtc, postedByUserId);
        }

        public void CancelPrepared(
            string reason,
            DateTime cancelledAtUtc,
            string cancelledByUserId)
        {
            if (Status != FinancialAdjustmentStatus.Prepared)
            {
                throw new DomainValidationException(
                    "Only Prepared Financial Adjustments can be cancelled.");
            }

            CancellationReason = TextNormalizer.Required(reason, nameof(reason), 500);
            CancelledAtUtc = DomainGuard.RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
            CancelledByUserId = DomainGuard.RequiredActorIdentifier(
                cancelledByUserId,
                nameof(cancelledByUserId));
            Status = FinancialAdjustmentStatus.Cancelled;
            RecordChange(cancelledAtUtc, cancelledByUserId);
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status == FinancialAdjustmentStatus.Posted)
            {
                throw new DomainValidationException(
                    "Posted Financial Adjustments are immutable and cannot be archived.");
            }

            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status == FinancialAdjustmentStatus.Posted)
            {
                throw new DomainValidationException(
                    "Posted Financial Adjustments are immutable and cannot be restored.");
            }

            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private static FinancialAdjustmentDirection RequireDirection(
            FinancialAdjustmentDirection value)
        {
            if (!Enum.IsDefined(typeof(FinancialAdjustmentDirection), value)
                || value == FinancialAdjustmentDirection.Unspecified)
            {
                throw new DomainValidationException(
                    "A defined Financial Adjustment direction is required.");
            }

            return value;
        }

        private static FinancialAdjustmentSourceKind RequireSourceKind(
            FinancialAdjustmentSourceKind value)
        {
            if (!Enum.IsDefined(typeof(FinancialAdjustmentSourceKind), value)
                || value == FinancialAdjustmentSourceKind.Unspecified)
            {
                throw new DomainValidationException(
                    "A defined Financial Adjustment source kind is required.");
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
}