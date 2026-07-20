using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Academic;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;

namespace IUIS.Application.Orchestration
{
    public sealed class StudentFinanceQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IEnrollmentRepository _enrollments;
        private readonly ITuitionAssessmentRepository _assessments;
        private readonly IPaymentRepository _payments;
        private readonly IFinancialAdjustmentRepository _adjustments;
        private readonly IScholarshipAwardRepository _scholarships;

        public StudentFinanceQueryService(
            SessionAwareRequestExecutor executor,
            IEnrollmentRepository enrollments,
            ITuitionAssessmentRepository assessments,
            IPaymentRepository payments,
            IFinancialAdjustmentRepository adjustments,
            IScholarshipAwardRepository scholarships)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _enrollments = enrollments ?? throw new ArgumentNullException(nameof(enrollments));
            _assessments = assessments ?? throw new ArgumentNullException(nameof(assessments));
            _payments = payments ?? throw new ArgumentNullException(nameof(payments));
            _adjustments = adjustments ?? throw new ArgumentNullException(nameof(adjustments));
            _scholarships = scholarships ?? throw new ArgumentNullException(nameof(scholarships));
        }

        public StudentFinanceOverviewDto GetOwnOverview(
            string sessionId,
            string sessionToken,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.finance.read",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal => Build(principal.PersonRecordId));
        }

        private StudentFinanceOverviewDto Build(string studentId)
        {
            var enrollmentSnapshot = _enrollments.Read();
            var assessmentSnapshot = _assessments.Read();
            var paymentSnapshot = _payments.Read();
            var adjustmentSnapshot = _adjustments.Read();
            var scholarshipSnapshot = _scholarships.Read();

            var enrollments = enrollmentSnapshot.Records
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(ToEnrollment)
                .ToList();
            var assessmentRecords = assessmentSnapshot.Records
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var paymentRecords = paymentSnapshot.Records
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.ReceivedAtUtc)
                .ToList();
            var adjustmentRecords = adjustmentSnapshot.Records
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();
            var scholarshipRecords = scholarshipSnapshot.Records
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .ToList();

            var currencyCode = ResolveCurrency(
                assessmentRecords,
                paymentRecords,
                adjustmentRecords,
                scholarshipRecords);
            EnsureSingleCurrency(
                currencyCode,
                assessmentRecords,
                paymentRecords,
                adjustmentRecords,
                scholarshipRecords);

            var postedAssessmentTotal = assessmentRecords
                .Where(item => item.Status == TuitionAssessmentStatus.Posted)
                .Sum(item => item.GrossAmount.Amount);
            var postedAdjustmentDebitTotal = adjustmentRecords
                .Where(item => item.Status == FinancialAdjustmentStatus.Posted
                    && item.Direction == FinancialAdjustmentDirection.Debit)
                .Sum(item => item.Amount.Amount);
            var postedAdjustmentCreditTotal = adjustmentRecords
                .Where(item => item.Status == FinancialAdjustmentStatus.Posted
                    && item.Direction == FinancialAdjustmentDirection.Credit)
                .Sum(item => item.Amount.Amount);
            var postedPaymentTotal = paymentRecords
                .Where(item => item.Status == PaymentStatus.Posted)
                .Sum(item => item.Amount.Amount);

            return new StudentFinanceOverviewDto
            {
                StudentId = studentId,
                CurrencyCode = currencyCode,
                PostedAssessmentTotal = postedAssessmentTotal,
                PostedAdjustmentDebitTotal = postedAdjustmentDebitTotal,
                PostedAdjustmentCreditTotal = postedAdjustmentCreditTotal,
                PostedPaymentTotal = postedPaymentTotal,
                Balance = postedAssessmentTotal
                    + postedAdjustmentDebitTotal
                    - postedAdjustmentCreditTotal
                    - postedPaymentTotal,
                EnrollmentRepositoryRevision = enrollmentSnapshot.Revision,
                AssessmentRepositoryRevision = assessmentSnapshot.Revision,
                PaymentRepositoryRevision = paymentSnapshot.Revision,
                AdjustmentRepositoryRevision = adjustmentSnapshot.Revision,
                ScholarshipRepositoryRevision = scholarshipSnapshot.Revision,
                Enrollments = enrollments.AsReadOnly(),
                Assessments = assessmentRecords.Select(ToAssessment).ToList().AsReadOnly(),
                Payments = paymentRecords.Select(ToPayment).ToList().AsReadOnly(),
                Scholarships = scholarshipRecords.Select(ToScholarship).ToList().AsReadOnly()
            };
        }

        private static StudentEnrollmentDto ToEnrollment(Enrollment value)
        {
            var releasedReason = value.Status == EnrollmentStatus.ReturnedForCorrection
                    || value.Status == EnrollmentStatus.Rejected
                    || value.Status == EnrollmentStatus.Withdrawn
                    || value.Status == EnrollmentStatus.Cancelled
                ? value.DecisionReason
                : null;
            return new StudentEnrollmentDto
            {
                EnrollmentId = value.Id,
                AcademicPeriodId = value.AcademicPeriodId,
                CourseCode = value.CourseCodeSnapshot,
                CourseName = value.CourseNameSnapshot,
                Status = value.Status.ToString(),
                TotalUnits = value.TotalUnits,
                SubmittedAtUtc = value.SubmittedAtUtc,
                ReleasedDecisionReason = releasedReason,
                EntityVersion = value.Version,
                SubjectLines = value.SubjectLines.Select(item =>
                    new StudentEnrollmentLineDto
                    {
                        SubjectId = item.SubjectId,
                        SubjectCode = item.SubjectCodeSnapshot,
                        SubjectTitle = item.SubjectTitleSnapshot,
                        Units = item.UnitsSnapshot,
                        SectionCode = item.SectionCode
                    }).ToList().AsReadOnly()
            };
        }

        private static StudentAssessmentDto ToAssessment(TuitionAssessment value)
        {
            return new StudentAssessmentDto
            {
                AssessmentId = value.Id,
                EnrollmentId = value.EnrollmentId,
                AcademicPeriodId = value.AcademicPeriodId,
                Status = value.Status.ToString(),
                GrossAmount = value.GrossAmount.Amount,
                CurrencyCode = value.CurrencyCode,
                PostedAtUtc = value.PostedAtUtc,
                EntityVersion = value.Version
            };
        }

        private static StudentPaymentDto ToPayment(Payment value)
        {
            return new StudentPaymentDto
            {
                PaymentId = value.Id,
                AcademicPeriodId = value.AcademicPeriodId,
                Amount = value.Amount.Amount,
                CurrencyCode = value.Amount.CurrencyCode,
                Method = value.Method.ToString(),
                Status = value.Status.ToString(),
                ReceiptNumber = value.ReceiptNumber,
                ReceivedAtUtc = value.ReceivedAtUtc,
                PostedAtUtc = value.PostedAtUtc,
                EntityVersion = value.Version
            };
        }

        private static StudentScholarshipDto ToScholarship(ScholarshipAward value)
        {
            return new StudentScholarshipDto
            {
                ScholarshipAwardId = value.Id,
                ScholarshipProgramId = value.ScholarshipProgramId,
                AcademicPeriodId = value.AcademicPeriodId,
                EffectKind = value.EffectKind.ToString(),
                FixedAmount = value.FixedAmount == null
                    ? (decimal?)null
                    : value.FixedAmount.Amount,
                Percentage = value.Percentage,
                CurrencyCode = value.CurrencyCode,
                Status = value.Status.ToString(),
                AppliedAssessmentId = value.AppliedAssessmentId,
                AppliedAtUtc = value.AppliedAtUtc,
                EntityVersion = value.Version
            };
        }

        private static string ResolveCurrency(
            IEnumerable<TuitionAssessment> assessments,
            IEnumerable<Payment> payments,
            IEnumerable<FinancialAdjustment> adjustments,
            IEnumerable<ScholarshipAward> scholarships)
        {
            var currency = assessments.Select(item => item.CurrencyCode)
                .Concat(payments.Select(item => item.Amount.CurrencyCode))
                .Concat(adjustments.Select(item => item.Amount.CurrencyCode))
                .Concat(scholarships.Select(item => item.CurrencyCode))
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            return currency ?? MoneyRules.PhilippinePesoCurrencyCode;
        }

        private static void EnsureSingleCurrency(
            string currencyCode,
            IEnumerable<TuitionAssessment> assessments,
            IEnumerable<Payment> payments,
            IEnumerable<FinancialAdjustment> adjustments,
            IEnumerable<ScholarshipAward> scholarships)
        {
            var currencies = assessments.Select(item => item.CurrencyCode)
                .Concat(payments.Select(item => item.Amount.CurrencyCode))
                .Concat(adjustments.Select(item => item.Amount.CurrencyCode))
                .Concat(scholarships.Select(item => item.CurrencyCode))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (currencies.Any(value => !StringComparer.Ordinal.Equals(value, currencyCode)))
            {
                throw new InvalidOperationException(
                    "Student financial projection contains multiple currencies.");
            }
        }
    }
}
