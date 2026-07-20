using System;
using System.Collections.Generic;

namespace IUIS.Application.Dtos
{
    public sealed class StudentEnrollmentLineDto
    {
        public string SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectTitle { get; set; }
        public decimal Units { get; set; }
        public string SectionCode { get; set; }
    }

    public sealed class StudentEnrollmentDto
    {
        public string EnrollmentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public decimal TotalUnits { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public string ReleasedDecisionReason { get; set; }
        public long EntityVersion { get; set; }
        public IReadOnlyList<StudentEnrollmentLineDto> SubjectLines { get; set; }
    }

    public sealed class StudentAssessmentDto
    {
        public string AssessmentId { get; set; }
        public string EnrollmentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string Status { get; set; }
        public decimal GrossAmount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? PostedAtUtc { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentPaymentDto
    {
        public string PaymentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public DateTime? PostedAtUtc { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentScholarshipDto
    {
        public string ScholarshipAwardId { get; set; }
        public string ScholarshipProgramId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string EffectKind { get; set; }
        public decimal? FixedAmount { get; set; }
        public decimal Percentage { get; set; }
        public string CurrencyCode { get; set; }
        public string Status { get; set; }
        public string AppliedAssessmentId { get; set; }
        public DateTime? AppliedAtUtc { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentFinanceOverviewDto
    {
        public string StudentId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal PostedAssessmentTotal { get; set; }
        public decimal PostedAdjustmentDebitTotal { get; set; }
        public decimal PostedAdjustmentCreditTotal { get; set; }
        public decimal PostedPaymentTotal { get; set; }
        public decimal Balance { get; set; }
        public long EnrollmentRepositoryRevision { get; set; }
        public long AssessmentRepositoryRevision { get; set; }
        public long PaymentRepositoryRevision { get; set; }
        public long AdjustmentRepositoryRevision { get; set; }
        public long ScholarshipRepositoryRevision { get; set; }
        public IReadOnlyList<StudentEnrollmentDto> Enrollments { get; set; }
        public IReadOnlyList<StudentAssessmentDto> Assessments { get; set; }
        public IReadOnlyList<StudentPaymentDto> Payments { get; set; }
        public IReadOnlyList<StudentScholarshipDto> Scholarships { get; set; }
    }

    public sealed class FinanceCommandResult
    {
        public string TransactionId { get; set; }
        public string RecordId { get; set; }
        public string SecondaryRecordId { get; set; }
        public string ReceiptNumber { get; set; }
        public long RepositoryRevision { get; set; }
        public long SecondaryRepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
    }
}
