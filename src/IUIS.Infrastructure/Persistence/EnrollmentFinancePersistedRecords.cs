using System;
using System.Collections.Generic;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class PersistedEnrollmentSubjectLine
    {
        public string SubjectId { get; set; }
        public string SubjectCodeSnapshot { get; set; }
        public string SubjectTitleSnapshot { get; set; }
        public decimal UnitsSnapshot { get; set; }
        public int YearLevelSnapshot { get; set; }
        public int TermNumberSnapshot { get; set; }
        public bool IsRequiredSnapshot { get; set; }
        public string SectionCode { get; set; }
    }

    public sealed class PersistedEnrollmentRecord : PersistedEntityRecord
    {
        public PersistedEnrollmentRecord()
        {
            SubjectLines = new List<PersistedEnrollmentSubjectLine>();
        }

        public string StudentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string CourseIdSnapshot { get; set; }
        public string CourseCodeSnapshot { get; set; }
        public string CourseNameSnapshot { get; set; }
        public string CurriculumIdSnapshot { get; set; }
        public string CurriculumVersionSnapshot { get; set; }
        public string Status { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public string SubmittedByUserId { get; set; }
        public DateTime? ReviewStartedAtUtc { get; set; }
        public string ReviewStartedByUserId { get; set; }
        public DateTime? DecisionAtUtc { get; set; }
        public string DecisionByUserId { get; set; }
        public string DecisionReason { get; set; }
        public List<PersistedEnrollmentSubjectLine> SubjectLines { get; set; }
    }

    public sealed class PersistedAssessmentChargeLine
    {
        public string LineId { get; set; }
        public string ChargeRuleId { get; set; }
        public string RuleCodeSnapshot { get; set; }
        public string DescriptionSnapshot { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public sealed class PersistedTuitionAssessmentRecord : PersistedEntityRecord
    {
        public PersistedTuitionAssessmentRecord()
        {
            ChargeLines = new List<PersistedAssessmentChargeLine>();
        }

        public string StudentId { get; set; }
        public string EnrollmentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string CurrencyCode { get; set; }
        public string Status { get; set; }
        public DateTime? PostedAtUtc { get; set; }
        public string PostedByUserId { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string CancelledByUserId { get; set; }
        public string CancellationReason { get; set; }
        public List<PersistedAssessmentChargeLine> ChargeLines { get; set; }
    }

    public sealed class PersistedPaymentAllocation
    {
        public string AllocationId { get; set; }
        public string AssessmentId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public sealed class PersistedPaymentRecord : PersistedEntityRecord
    {
        public PersistedPaymentRecord()
        {
            Allocations = new List<PersistedPaymentAllocation>();
        }

        public string StudentId { get; set; }
        public string AcademicPeriodId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Method { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string ExternalReference { get; set; }
        public string Status { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime? PostedAtUtc { get; set; }
        public string PostedByUserId { get; set; }
        public DateTime? VoidedAtUtc { get; set; }
        public string VoidedByUserId { get; set; }
        public string VoidReason { get; set; }
        public List<PersistedPaymentAllocation> Allocations { get; set; }
    }

    public sealed class PersistedFinancialAdjustmentRecord : PersistedEntityRecord
    {
        public string StudentId { get; set; }
        public string AssessmentId { get; set; }
        public string Direction { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string SourceKind { get; set; }
        public string SourceRecordId { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime? PostedAtUtc { get; set; }
        public string PostedByUserId { get; set; }
        public string CancellationReason { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string CancelledByUserId { get; set; }
    }

    public sealed class PersistedScholarshipAwardRecord : PersistedEntityRecord
    {
        public string StudentId { get; set; }
        public string ScholarshipProgramId { get; set; }
        public string AcademicPeriodId { get; set; }
        public string EffectKind { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? FixedAmount { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string ApprovedByUserId { get; set; }
        public string AppliedAssessmentId { get; set; }
        public string AppliedAdjustmentId { get; set; }
        public DateTime? AppliedAtUtc { get; set; }
        public string AppliedByUserId { get; set; }
        public string CancellationReason { get; set; }
    }
}
