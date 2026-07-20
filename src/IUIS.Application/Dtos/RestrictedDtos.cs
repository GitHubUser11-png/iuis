using System;
using System.Collections.Generic;

namespace IUIS.Application.Dtos
{
    public sealed class StudentOwnRecordDto
    {
        public string StudentId { get; set; }
        public string StudentNumber { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string CourseId { get; set; }
        public string Status { get; set; }
    }

    public sealed class EmployeeSelfServiceDto
    {
        public string EmployeeId { get; set; }
        public string EmployeeNumber { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string DepartmentId { get; set; }
        public string PositionTitle { get; set; }
        public bool IsFaculty { get; set; }
        public string Status { get; set; }
    }

    public sealed class ReleasedSummaryDto
    {
        public string SummaryId { get; set; }
        public string RelatedRecordId { get; set; }
        public string Summary { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
    }

    public sealed class CounselingReleasedCaseDto
    {
        public string CaseId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public string RequestReason { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public DateTime? ConfirmedAppointmentAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public IReadOnlyList<ReleasedSummaryDto> ReleasedSummaries { get; set; }
    }

    public sealed class CounselingInternalSessionDto
    {
        public string SessionId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string CounselorEmployeeId { get; set; }
        public string RiskLevel { get; set; }
        public string InternalNotes { get; set; }
    }

    public sealed class CounselingInternalCaseDto
    {
        public string CaseId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public string AssignedCounselorEmployeeId { get; set; }
        public string ClosureSummary { get; set; }
        public IReadOnlyList<CounselingInternalSessionDto> ConfidentialSessions { get; set; }
        public IReadOnlyList<ReleasedSummaryDto> ReleasedSummaries { get; set; }
    }

    public sealed class DisciplineReleasedCaseDto
    {
        public string CaseId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public string ViolationCode { get; set; }
        public string ViolationDescription { get; set; }
        public string ReleasedNoticeSummary { get; set; }
        public string ResponseDueDate { get; set; }
        public string ReleasedDecisionSummary { get; set; }
        public string SanctionSummary { get; set; }
    }

    public sealed class DisciplineInternalCaseDto
    {
        public string CaseId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public string InternalIncidentNarrative { get; set; }
        public IReadOnlyList<string> RestrictedEvidenceReferences { get; set; }
        public IReadOnlyList<string> RestrictedFindings { get; set; }
        public string InternalDecisionRationale { get; set; }
    }

    public sealed class MedicalReleasedRecordDto
    {
        public string MedicalRecordId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public IReadOnlyList<ReleasedSummaryDto> ReleasedSummaries { get; set; }
    }

    public sealed class MedicalInternalConsultationDto
    {
        public string ConsultationId { get; set; }
        public string AppointmentId { get; set; }
        public string ClinicianEmployeeId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string InternalClinicalNotes { get; set; }
        public string InternalAssessment { get; set; }
        public string InternalTreatmentPlan { get; set; }
    }

    public sealed class MedicalInternalRecordDto
    {
        public string MedicalRecordId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public IReadOnlyList<MedicalInternalConsultationDto> ConfidentialConsultations { get; set; }
        public IReadOnlyList<ReleasedSummaryDto> ReleasedSummaries { get; set; }
    }
}
