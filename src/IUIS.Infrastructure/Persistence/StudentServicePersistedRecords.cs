using System;
using System.Collections.Generic;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class PersistedLibraryBookCopyRecord
    {
        public string CopyId { get; set; }
        public string Barcode { get; set; }
        public string Condition { get; set; }
        public string Status { get; set; }
    }

    public sealed class PersistedLibraryBookRecord : PersistedEntityRecord
    {
        public PersistedLibraryBookRecord()
        {
            Copies = new List<PersistedLibraryBookCopyRecord>();
        }

        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public List<PersistedLibraryBookCopyRecord> Copies { get; set; }
    }

    public sealed class PersistedLibraryBorrowingRecord : PersistedEntityRecord
    {
        public string StudentId { get; set; }
        public string BookId { get; set; }
        public string CopyId { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? IssuedAtUtc { get; set; }
        public string IssuedByUserId { get; set; }
        public int RenewalCount { get; set; }
        public DateTime? ReturnedAtUtc { get; set; }
        public string ReturnedByUserId { get; set; }
        public string ReturnCondition { get; set; }
        public DateTime? LostAtUtc { get; set; }
        public string LostRecordedByUserId { get; set; }
    }

    public sealed class PersistedCounselingConfidentialSessionRecord
    {
        public string SessionId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string CounselorEmployeeId { get; set; }
        public string RiskLevel { get; set; }
        public string InternalNotes { get; set; }
    }

    public sealed class PersistedCounselingReleasedSummaryRecord
    {
        public string SummaryId { get; set; }
        public string SessionId { get; set; }
        public string ReleaseAuthorizationId { get; set; }
        public string Summary { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
        public string ReleasedByUserId { get; set; }
    }

    public sealed class PersistedCounselingCaseRecord : PersistedEntityRecord
    {
        public PersistedCounselingCaseRecord()
        {
            ConfidentialSessions = new List<PersistedCounselingConfidentialSessionRecord>();
            ReleasedSummaries = new List<PersistedCounselingReleasedSummaryRecord>();
        }

        public string StudentId { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public DateTime? ConfirmedAppointmentAtUtc { get; set; }
        public string AssignedCounselorEmployeeId { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public string ClosureSummary { get; set; }
        public List<PersistedCounselingConfidentialSessionRecord> ConfidentialSessions { get; set; }
        public List<PersistedCounselingReleasedSummaryRecord> ReleasedSummaries { get; set; }
    }

    public sealed class PersistedDisciplineEvidenceReferenceRecord
    {
        public string EvidenceId { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime AddedAtUtc { get; set; }
        public string AddedByUserId { get; set; }
    }

    public sealed class PersistedDisciplineViolationRecord
    {
        public string ViolationId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
    }

    public sealed class PersistedDisciplineReleasedNoticeRecord
    {
        public string NoticeId { get; set; }
        public string ReleasedSummary { get; set; }
        public string ResponseDueDate { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
        public string ReleasedByUserId { get; set; }
    }

    public sealed class PersistedDisciplineStudentResponseRecord
    {
        public string ResponseId { get; set; }
        public string ResponseText { get; set; }
        public string EvidenceReference { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
    }

    public sealed class PersistedDisciplineFindingRecord
    {
        public string FindingId { get; set; }
        public bool Substantiated { get; set; }
        public string InternalFinding { get; set; }
        public DateTime RecordedAtUtc { get; set; }
        public string RecordedByUserId { get; set; }
    }

    public sealed class PersistedDisciplineRestrictedDecisionRecord
    {
        public string DecisionId { get; set; }
        public string Outcome { get; set; }
        public string InternalRationale { get; set; }
        public string SanctionSummary { get; set; }
        public DateTime PreparedAtUtc { get; set; }
        public string PreparedByUserId { get; set; }
    }

    public sealed class PersistedDisciplineReleasedDecisionRecord
    {
        public string DecisionId { get; set; }
        public string ReleasedDecisionSummary { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
        public string ReleasedByUserId { get; set; }
    }

    public sealed class PersistedDisciplineCaseRecord : PersistedEntityRecord
    {
        public PersistedDisciplineCaseRecord()
        {
            RestrictedEvidence = new List<PersistedDisciplineEvidenceReferenceRecord>();
            StudentResponses = new List<PersistedDisciplineStudentResponseRecord>();
            RestrictedFindings = new List<PersistedDisciplineFindingRecord>();
        }

        public string StudentId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string Location { get; set; }
        public string InternalIncidentNarrative { get; set; }
        public string ReporterUserId { get; set; }
        public string Status { get; set; }
        public PersistedDisciplineViolationRecord Violation { get; set; }
        public PersistedDisciplineReleasedNoticeRecord ReleasedNotice { get; set; }
        public PersistedDisciplineRestrictedDecisionRecord RestrictedDecision { get; set; }
        public PersistedDisciplineReleasedDecisionRecord ReleasedDecision { get; set; }
        public List<PersistedDisciplineEvidenceReferenceRecord> RestrictedEvidence { get; set; }
        public List<PersistedDisciplineStudentResponseRecord> StudentResponses { get; set; }
        public List<PersistedDisciplineFindingRecord> RestrictedFindings { get; set; }
    }

    public sealed class PersistedClinicAppointmentRecord : PersistedEntityRecord
    {
        public string StudentId { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string ReleasedReasonSummary { get; set; }
        public string Status { get; set; }
        public DateTime? ScheduledAtUtc { get; set; }
        public string ClinicianEmployeeId { get; set; }
        public DateTime? CheckedInAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public string ConsultationId { get; set; }
        public string CancellationReason { get; set; }
    }

    public sealed class PersistedMedicalConfidentialConsultationRecord
    {
        public string ConsultationId { get; set; }
        public string AppointmentId { get; set; }
        public string ClinicianEmployeeId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string InternalClinicalNotes { get; set; }
        public string InternalAssessment { get; set; }
        public string InternalTreatmentPlan { get; set; }
    }

    public sealed class PersistedMedicalReleasedSummaryRecord
    {
        public string SummaryId { get; set; }
        public string ConsultationId { get; set; }
        public string ReleasedSummary { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
        public string ReleasedByUserId { get; set; }
    }

    public sealed class PersistedMedicalRecord : PersistedEntityRecord
    {
        public PersistedMedicalRecord()
        {
            ConfidentialConsultations = new List<PersistedMedicalConfidentialConsultationRecord>();
            ReleasedSummaries = new List<PersistedMedicalReleasedSummaryRecord>();
        }

        public string StudentId { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public List<PersistedMedicalConfidentialConsultationRecord> ConfidentialConsultations { get; set; }
        public List<PersistedMedicalReleasedSummaryRecord> ReleasedSummaries { get; set; }
    }

    public sealed class PersistedMedicalClearanceHistoryRecord
    {
        public string HistoryId { get; set; }
        public string Action { get; set; }
        public string FromStatus { get; set; }
        public string ToStatus { get; set; }
        public string Reason { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string ActorUserId { get; set; }
    }

    public sealed class PersistedMedicalClearanceRecord : PersistedEntityRecord
    {
        public PersistedMedicalClearanceRecord()
        {
            RestrictedHistory = new List<PersistedMedicalClearanceHistoryRecord>();
        }

        public string StudentId { get; set; }
        public string MedicalRecordId { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public string ReviewingClinicianEmployeeId { get; set; }
        public string ClearanceNumber { get; set; }
        public string ValidFrom { get; set; }
        public string ValidUntil { get; set; }
        public string ReleasedSummary { get; set; }
        public string RevocationReason { get; set; }
        public List<PersistedMedicalClearanceHistoryRecord> RestrictedHistory { get; set; }
    }
}