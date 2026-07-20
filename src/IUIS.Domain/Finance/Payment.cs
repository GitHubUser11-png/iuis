using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class PaymentAllocation
    {
        public PaymentAllocation(string allocationId, string assessmentId, Money amount)
        {
            AllocationId = RequireIdentifier(allocationId, "PAL", nameof(allocationId));
            AssessmentId = RequireIdentifier(assessmentId, "ASM", nameof(assessmentId));
            if (amount == null || amount.Amount <= 0m)
                throw new DomainValidationException("Payment allocation amount must be greater than zero.");
            Amount = amount;
        }

        public string AllocationId { get; private set; }
        public string AssessmentId { get; private set; }
        public Money Amount { get; private set; }

        private static string RequireIdentifier(string value, string prefix, string parameterName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
                throw new DomainValidationException(
                    parameterName + " must use the " + prefix + " identifier prefix.");
            return identifier.Value;
        }
    }

    public sealed class Payment : EntityBase
    {
        private readonly List<PaymentAllocation> _allocations;

        private Payment()
        {
            StudentId = string.Empty;
            AcademicPeriodId = string.Empty;
            Amount = Money.PhilippinePeso(0m);
            _allocations = new List<PaymentAllocation>();
        }

        public Payment(
            string id,
            string studentId,
            string academicPeriodId,
            Money amount,
            PaymentMethod method,
            DateTime receivedAtUtc,
            string externalReference,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "PAY", nameof(id)), createdAtUtc, createdByUserId)
        {
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            AcademicPeriodId = RequireIdentifier(academicPeriodId, "APD", nameof(academicPeriodId));
            Amount = RequirePositive(amount, nameof(amount));
            Method = RequireMethod(method);
            ReceivedAtUtc = DomainGuard.RequireUtc(receivedAtUtc, nameof(receivedAtUtc));
            ExternalReference = TextNormalizer.Optional(externalReference, nameof(externalReference), 200);
            Status = PaymentStatus.Draft;
            _allocations = new List<PaymentAllocation>();
        }

        public string StudentId { get; private set; }
        public string AcademicPeriodId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentMethod Method { get; private set; }
        public DateTime ReceivedAtUtc { get; private set; }
        public string ExternalReference { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string ReceiptNumber { get; private set; }
        public DateTime? PostedAtUtc { get; private set; }
        public string PostedByUserId { get; private set; }
        public DateTime? VoidedAtUtc { get; private set; }
        public string VoidedByUserId { get; private set; }
        public string VoidReason { get; private set; }
        public IReadOnlyList<PaymentAllocation> Allocations
        {
            get { return _allocations.AsReadOnly(); }
        }

        public Money AllocatedAmount
        {
            get
            {
                var total = Money.Zero(Amount.CurrencyCode);
                foreach (var allocation in _allocations) total = total.Add(allocation.Amount);
                return total;
            }
        }

        public Money UnallocatedAmount
        {
            get { return Amount.Subtract(AllocatedAmount); }
        }

        public static Payment Rehydrate(
            string id,
            string studentId,
            string academicPeriodId,
            Money amount,
            PaymentMethod method,
            DateTime receivedAtUtc,
            string externalReference,
            IEnumerable<PaymentAllocation> allocations,
            PaymentStatus status,
            string receiptNumber,
            DateTime? postedAtUtc,
            string postedByUserId,
            DateTime? voidedAtUtc,
            string voidedByUserId,
            string voidReason,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            if (!Enum.IsDefined(typeof(PaymentStatus), status))
                throw new DomainValidationException("Persisted Payment status is invalid.");
            if (allocations == null)
                throw new DomainValidationException("Persisted Payment allocations are required.");

            var record = new Payment(
                id,
                studentId,
                academicPeriodId,
                amount,
                method,
                receivedAtUtc,
                externalReference,
                createdAtUtc,
                createdByUserId);
            foreach (var allocation in allocations)
            {
                if (allocation == null)
                    throw new DomainValidationException("Persisted Payment allocation is invalid.");
                record.RequireCurrency(allocation.Amount);
                if (record._allocations.Any(existing =>
                        StringComparer.Ordinal.Equals(existing.AllocationId, allocation.AllocationId))
                    || record._allocations.Any(existing =>
                        StringComparer.Ordinal.Equals(existing.AssessmentId, allocation.AssessmentId)))
                    throw new DomainValidationException("Persisted Payment allocations must have unique IDs and Assessment IDs.");
                if (record.AllocatedAmount.Add(allocation.Amount).Amount > record.Amount.Amount)
                    throw new DomainValidationException("Persisted Payment allocations cannot exceed the Payment amount.");
                record._allocations.Add(allocation);
            }

            if (status == PaymentStatus.Posted || status == PaymentStatus.Voided)
            {
                if (record._allocations.Count == 0 || record.AllocatedAmount.Amount != record.Amount.Amount)
                    throw new DomainValidationException("Posted or Voided Payments must be fully allocated.");
                record.ReceiptNumber = RequireIdentifier(receiptNumber, "RCT", nameof(receiptNumber));
                record.PostedAtUtc = RequireUtc(postedAtUtc, nameof(postedAtUtc));
                if (record.PostedAtUtc.Value < record.ReceivedAtUtc)
                    throw new DomainValidationException("Payment posting time cannot precede receipt time.");
                record.PostedByUserId = DomainGuard.RequiredActorIdentifier(
                    postedByUserId,
                    nameof(postedByUserId));
            }
            else if (!string.IsNullOrWhiteSpace(receiptNumber)
                || postedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(postedByUserId))
                throw new DomainValidationException("Draft Payments cannot contain posting metadata.");

            if (status == PaymentStatus.Voided)
            {
                record.VoidedAtUtc = RequireUtc(voidedAtUtc, nameof(voidedAtUtc));
                if (record.VoidedAtUtc.Value < record.PostedAtUtc.Value)
                    throw new DomainValidationException("Payment void time cannot precede posting time.");
                record.VoidedByUserId = DomainGuard.RequiredActorIdentifier(
                    voidedByUserId,
                    nameof(voidedByUserId));
                record.VoidReason = TextNormalizer.Required(voidReason, nameof(voidReason), 500);
            }
            else if (voidedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(voidedByUserId)
                || !string.IsNullOrWhiteSpace(voidReason))
                throw new DomainValidationException("Only Voided Payments may contain void metadata.");

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

        public void AddAllocation(
            PaymentAllocation allocation,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            if (allocation == null)
                throw new DomainValidationException("Payment allocation is required.");
            RequireCurrency(allocation.Amount);
            if (_allocations.Any(existing =>
                    StringComparer.Ordinal.Equals(existing.AllocationId, allocation.AllocationId)))
                throw new DomainValidationException("Payment allocation identifiers must be unique within a Payment.");
            if (_allocations.Any(existing =>
                    StringComparer.Ordinal.Equals(existing.AssessmentId, allocation.AssessmentId)))
                throw new DomainValidationException("A Payment can contain only one allocation for each Assessment.");
            if (AllocatedAmount.Add(allocation.Amount).Amount > Amount.Amount)
                throw new DomainValidationException("Payment allocations cannot exceed the Payment amount.");
            RecordChange(changedAtUtc, changedByUserId);
            _allocations.Add(allocation);
        }

        public void RemoveAllocation(
            string allocationId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireDraft();
            var canonicalId = RequireIdentifier(allocationId, "PAL", nameof(allocationId));
            var existing = _allocations.FirstOrDefault(allocation =>
                StringComparer.Ordinal.Equals(allocation.AllocationId, canonicalId));
            if (existing == null)
                throw new DomainValidationException("The Payment allocation does not exist.");
            RecordChange(changedAtUtc, changedByUserId);
            _allocations.Remove(existing);
        }

        public void Post(string receiptNumber, DateTime postedAtUtc, string postedByUserId)
        {
            RequireDraft();
            if (_allocations.Count == 0)
                throw new DomainValidationException("A Payment requires at least one allocation before posting.");
            if (AllocatedAmount.Amount != Amount.Amount)
                throw new DomainValidationException("A Payment must be fully allocated before posting.");
            var canonicalReceipt = RequireIdentifier(receiptNumber, "RCT", nameof(receiptNumber));
            var canonicalTime = DomainGuard.RequireUtc(postedAtUtc, nameof(postedAtUtc));
            if (canonicalTime < ReceivedAtUtc)
                throw new DomainValidationException("Payment posting time cannot precede receipt time.");
            var actor = DomainGuard.RequiredActorIdentifier(postedByUserId, nameof(postedByUserId));
            RecordChange(canonicalTime, actor);
            ReceiptNumber = canonicalReceipt;
            PostedAtUtc = canonicalTime;
            PostedByUserId = actor;
            Status = PaymentStatus.Posted;
        }

        public void Void(string reason, DateTime voidedAtUtc, string voidedByUserId)
        {
            if (Status != PaymentStatus.Posted)
                throw new DomainValidationException("Only Posted Payments can be voided.");
            var canonicalTime = DomainGuard.RequireUtc(voidedAtUtc, nameof(voidedAtUtc));
            if (!PostedAtUtc.HasValue || canonicalTime < PostedAtUtc.Value)
                throw new DomainValidationException("Payment void time cannot precede posting time.");
            var canonicalReason = TextNormalizer.Required(reason, nameof(reason), 500);
            var actor = DomainGuard.RequiredActorIdentifier(voidedByUserId, nameof(voidedByUserId));
            RecordChange(canonicalTime, actor);
            VoidReason = canonicalReason;
            VoidedAtUtc = canonicalTime;
            VoidedByUserId = actor;
            Status = PaymentStatus.Voided;
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (Status != PaymentStatus.Draft)
                throw new DomainValidationException("Posted or Voided Payments are retained and cannot be archived.");
            base.Archive(archivedAtUtc, archivedByUserId);
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (Status != PaymentStatus.Draft)
                throw new DomainValidationException("Posted or Voided Payments are retained and cannot be restored.");
            base.Restore(restoredAtUtc, restoredByUserId);
        }

        private void RequireDraft()
        {
            if (Status != PaymentStatus.Draft)
                throw new DomainValidationException("Only Draft Payments can be changed.");
        }

        private void RequireCurrency(Money value)
        {
            if (!StringComparer.Ordinal.Equals(Amount.CurrencyCode, value.CurrencyCode))
                throw new DomainValidationException("Payment allocation currency must match the Payment currency.");
        }

        private static PaymentMethod RequireMethod(PaymentMethod value)
        {
            if (!Enum.IsDefined(typeof(PaymentMethod), value)
                || value == PaymentMethod.Unspecified)
                throw new DomainValidationException("A defined Payment method is required.");
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
