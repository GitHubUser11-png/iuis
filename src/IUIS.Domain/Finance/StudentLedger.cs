using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Finance
{
    public sealed class StudentLedgerEntry
    {
        public StudentLedgerEntry(
            string entryId,
            string studentId,
            StudentLedgerEntryKind kind,
            string referenceId,
            DateTime occurredAtUtc,
            string description,
            Money debit,
            Money credit)
        {
            EntryId = TextNormalizer.Required(entryId, nameof(entryId), 200);
            StudentId = RequireIdentifier(studentId, "STU", nameof(studentId));
            Kind = RequireKind(kind);
            ReferenceId = InstitutionIdentifier.Parse(referenceId).Value;
            OccurredAtUtc = DomainGuard.RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
            Description = TextNormalizer.Required(description, nameof(description), 500);
            Debit = debit ?? throw new DomainValidationException("Ledger debit is required.");
            Credit = credit ?? throw new DomainValidationException("Ledger credit is required.");

            if (!StringComparer.Ordinal.Equals(Debit.CurrencyCode, Credit.CurrencyCode))
            {
                throw new DomainValidationException(
                    "Ledger debit and credit must use the same currency.");
            }

            var hasDebit = Debit.Amount > 0m;
            var hasCredit = Credit.Amount > 0m;
            if (hasDebit == hasCredit || Debit.Amount < 0m || Credit.Amount < 0m)
            {
                throw new DomainValidationException(
                    "A Ledger entry must contain exactly one positive debit or credit amount.");
            }
        }

        public string EntryId { get; private set; }

        public string StudentId { get; private set; }

        public StudentLedgerEntryKind Kind { get; private set; }

        public string ReferenceId { get; private set; }

        public DateTime OccurredAtUtc { get; private set; }

        public string Description { get; private set; }

        public Money Debit { get; private set; }

        public Money Credit { get; private set; }

        private static StudentLedgerEntryKind RequireKind(StudentLedgerEntryKind value)
        {
            if (!Enum.IsDefined(typeof(StudentLedgerEntryKind), value)
                || value == StudentLedgerEntryKind.Unspecified)
            {
                throw new DomainValidationException("A defined Ledger entry kind is required.");
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

    public sealed class StudentLedgerSnapshot
    {
        public StudentLedgerSnapshot(
            string studentId,
            string currencyCode,
            IEnumerable<StudentLedgerEntry> entries)
        {
            StudentId = InstitutionIdentifier.Parse(studentId).Value;
            CurrencyCode = Money.Zero(currencyCode).CurrencyCode;
            var ordered = (entries ?? Enumerable.Empty<StudentLedgerEntry>())
                .OrderBy(entry => entry.OccurredAtUtc)
                .ThenBy(entry => entry.EntryId, StringComparer.Ordinal)
                .ToList();

            foreach (var entry in ordered)
            {
                if (!StringComparer.Ordinal.Equals(StudentId, entry.StudentId))
                {
                    throw new DomainValidationException(
                        "Every Ledger entry must belong to the requested Student.");
                }

                if (!StringComparer.Ordinal.Equals(CurrencyCode, entry.Debit.CurrencyCode))
                {
                    throw new DomainValidationException(
                        "Every Ledger entry must use the requested currency.");
                }
            }

            Entries = ordered.AsReadOnly();
            var debits = Money.Zero(CurrencyCode);
            var credits = Money.Zero(CurrencyCode);
            foreach (var entry in ordered)
            {
                debits = debits.Add(entry.Debit);
                credits = credits.Add(entry.Credit);
            }

            TotalDebits = debits;
            TotalCredits = credits;
            Balance = debits.Subtract(credits);
        }

        public string StudentId { get; private set; }

        public string CurrencyCode { get; private set; }

        public IReadOnlyList<StudentLedgerEntry> Entries { get; private set; }

        public Money TotalDebits { get; private set; }

        public Money TotalCredits { get; private set; }

        public Money Balance { get; private set; }
    }

    public static class StudentLedgerDeriver
    {
        public static StudentLedgerSnapshot Derive(
            string studentId,
            string currencyCode,
            IEnumerable<TuitionAssessment> assessments,
            IEnumerable<FinancialAdjustment> adjustments,
            IEnumerable<Payment> payments)
        {
            var canonicalStudentId = RequireStudentId(studentId);
            var canonicalCurrency = Money.Zero(currencyCode).CurrencyCode;
            var entries = new List<StudentLedgerEntry>();

            AddAssessmentEntries(
                entries,
                canonicalStudentId,
                canonicalCurrency,
                assessments ?? Enumerable.Empty<TuitionAssessment>());
            AddAdjustmentEntries(
                entries,
                canonicalStudentId,
                canonicalCurrency,
                adjustments ?? Enumerable.Empty<FinancialAdjustment>());
            AddPaymentEntries(
                entries,
                canonicalStudentId,
                canonicalCurrency,
                payments ?? Enumerable.Empty<Payment>());

            return new StudentLedgerSnapshot(
                canonicalStudentId,
                canonicalCurrency,
                entries);
        }

        private static void AddAssessmentEntries(
            ICollection<StudentLedgerEntry> entries,
            string studentId,
            string currencyCode,
            IEnumerable<TuitionAssessment> assessments)
        {
            foreach (var assessment in assessments)
            {
                if (assessment == null
                    || assessment.Status != TuitionAssessmentStatus.Posted
                    || !StringComparer.Ordinal.Equals(studentId, assessment.StudentId))
                {
                    continue;
                }

                RequireCurrency(currencyCode, assessment.GrossAmount);
                entries.Add(new StudentLedgerEntry(
                    assessment.Id + ":POST",
                    studentId,
                    StudentLedgerEntryKind.AssessmentDebit,
                    assessment.Id,
                    assessment.PostedAtUtc.Value,
                    "Posted Tuition Assessment " + assessment.Id + ".",
                    assessment.GrossAmount,
                    Money.Zero(currencyCode)));
            }
        }

        private static void AddAdjustmentEntries(
            ICollection<StudentLedgerEntry> entries,
            string studentId,
            string currencyCode,
            IEnumerable<FinancialAdjustment> adjustments)
        {
            foreach (var adjustment in adjustments)
            {
                if (adjustment == null
                    || adjustment.Status != FinancialAdjustmentStatus.Posted
                    || !StringComparer.Ordinal.Equals(studentId, adjustment.StudentId))
                {
                    continue;
                }

                RequireCurrency(currencyCode, adjustment.Amount);
                var isDebit = adjustment.Direction == FinancialAdjustmentDirection.Debit;
                entries.Add(new StudentLedgerEntry(
                    adjustment.Id + ":POST",
                    studentId,
                    isDebit
                        ? StudentLedgerEntryKind.AdjustmentDebit
                        : StudentLedgerEntryKind.AdjustmentCredit,
                    adjustment.Id,
                    adjustment.PostedAtUtc.Value,
                    adjustment.Reason,
                    isDebit ? adjustment.Amount : Money.Zero(currencyCode),
                    isDebit ? Money.Zero(currencyCode) : adjustment.Amount));
            }
        }

        private static void AddPaymentEntries(
            ICollection<StudentLedgerEntry> entries,
            string studentId,
            string currencyCode,
            IEnumerable<Payment> payments)
        {
            foreach (var payment in payments)
            {
                if (payment == null
                    || payment.Status == PaymentStatus.Draft
                    || !StringComparer.Ordinal.Equals(studentId, payment.StudentId))
                {
                    continue;
                }

                RequireCurrency(currencyCode, payment.Amount);
                entries.Add(new StudentLedgerEntry(
                    payment.Id + ":POST",
                    studentId,
                    StudentLedgerEntryKind.PaymentCredit,
                    payment.Id,
                    payment.PostedAtUtc.Value,
                    "Posted Payment " + payment.ReceiptNumber + ".",
                    Money.Zero(currencyCode),
                    payment.Amount));

                if (payment.Status == PaymentStatus.Voided)
                {
                    entries.Add(new StudentLedgerEntry(
                        payment.Id + ":VOID",
                        studentId,
                        StudentLedgerEntryKind.PaymentVoidDebit,
                        payment.Id,
                        payment.VoidedAtUtc.Value,
                        "Voided Payment " + payment.ReceiptNumber + ": " + payment.VoidReason,
                        payment.Amount,
                        Money.Zero(currencyCode)));
                }
            }
        }

        private static void RequireCurrency(string currencyCode, Money value)
        {
            if (!StringComparer.Ordinal.Equals(currencyCode, value.CurrencyCode))
            {
                throw new DomainValidationException(
                    "Ledger source currency does not match the requested Ledger currency.");
            }
        }

        private static string RequireStudentId(string value)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, "STU"))
            {
                throw new DomainValidationException(
                    "studentId must use the STU identifier prefix.");
            }

            return identifier.Value;
        }
    }
}