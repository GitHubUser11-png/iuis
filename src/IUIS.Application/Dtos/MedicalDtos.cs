using System;
using System.Collections.Generic;

namespace IUIS.Application.Dtos
{
    public sealed class StudentClinicAppointmentDto
    {
        public string AppointmentId { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string ReleasedReasonSummary { get; set; }
        public string Status { get; set; }
        public DateTime? ScheduledAtUtc { get; set; }
        public DateTime? CheckedInAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public string CancellationReason { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentMedicalReleasedSummaryDto
    {
        public string SummaryId { get; set; }
        public string ReleasedSummary { get; set; }
        public DateTime ReleasedAtUtc { get; set; }
    }

    public sealed class StudentMedicalClearanceDto
    {
        public string ClearanceId { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public string ClearanceNumber { get; set; }
        public string ValidFrom { get; set; }
        public string ValidUntil { get; set; }
        public string ReleasedSummary { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentMedicalOverviewDto
    {
        public string StudentId { get; set; }
        public long AppointmentRepositoryRevision { get; set; }
        public long MedicalRecordRepositoryRevision { get; set; }
        public long MedicalClearanceRepositoryRevision { get; set; }
        public IReadOnlyList<StudentClinicAppointmentDto> Appointments { get; set; }
        public IReadOnlyList<StudentMedicalReleasedSummaryDto> ReleasedSummaries { get; set; }
        public IReadOnlyList<StudentMedicalClearanceDto> Clearances { get; set; }
    }

    public sealed class RestrictedMedicalConsultationDto
    {
        public string ConsultationId { get; set; }
        public string AppointmentId { get; set; }
        public string ClinicianEmployeeId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string InternalClinicalNotes { get; set; }
        public string InternalAssessment { get; set; }
        public string InternalTreatmentPlan { get; set; }
    }

    public sealed class RestrictedMedicalRecordDto
    {
        public string MedicalRecordId { get; set; }
        public string StudentId { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public long RepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public IReadOnlyList<RestrictedMedicalConsultationDto> ConfidentialConsultations { get; set; }
    }

    public sealed class ClinicMedicalCommandResult
    {
        public string TransactionId { get; set; }
        public string AppointmentId { get; set; }
        public string MedicalRecordId { get; set; }
        public string MedicalClearanceId { get; set; }
        public long AppointmentRepositoryRevision { get; set; }
        public long MedicalRecordRepositoryRevision { get; set; }
        public long MedicalClearanceRepositoryRevision { get; set; }
        public long AppointmentEntityVersion { get; set; }
        public long MedicalRecordEntityVersion { get; set; }
        public long MedicalClearanceEntityVersion { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
    }
}
