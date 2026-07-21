using System;
using System.Linq;

using IUIS.Domain.Clinic;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.People;

namespace IUIS.Application.Dtos
{
    public sealed class RestrictedProjectionService
    {
        public StudentOwnRecordDto ToStudentOwnRecord(StudentRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            return new StudentOwnRecordDto
            {
                StudentId = record.Id,
                StudentNumber = record.StudentNumber,
                DisplayName = record.Name.DisplayName,
                EmailAddress = record.Contact.EmailAddress,
                MobileNumber = record.Contact.MobileNumber,
                CourseId = record.CourseId,
                Status = record.Status.ToString()
            };
        }

        public EmployeeSelfServiceDto ToEmployeeSelfService(EmployeeRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            return new EmployeeSelfServiceDto
            {
                EmployeeId = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                DisplayName = record.Name.DisplayName,
                EmailAddress = record.Contact.EmailAddress,
                MobileNumber = record.Contact.MobileNumber,
                DepartmentId = record.DepartmentId,
                PositionTitle = record.PositionTitle,
                IsFaculty = record.IsFaculty,
                Status = record.Status.ToString()
            };
        }

        public CounselingReleasedCaseDto ToCounselingReleased(
            CounselingCase value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new CounselingReleasedCaseDto
            {
                CaseId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                RequestReason = value.RequestReason,
                RequestedAppointmentAtUtc = value.RequestedAppointmentAtUtc,
                ConfirmedAppointmentAtUtc = value.ConfirmedAppointmentAtUtc,
                ClosedAtUtc = value.ClosedAtUtc,
                EntityVersion = value.Version,
                ReleasedSummaries = value.ReleasedSummaries
                    .Select(item => new ReleasedSummaryDto
                    {
                        SummaryId = item.SummaryId,
                        RelatedRecordId = item.SessionId,
                        Summary = item.Summary,
                        ReleasedAtUtc = item.ReleasedAtUtc
                    })
                    .ToList()
                    .AsReadOnly()
            };
        }

        public CounselingInternalCaseDto ToCounselingInternal(
            CounselingCase value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new CounselingInternalCaseDto
            {
                CaseId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                AssignedCounselorEmployeeId = value.AssignedCounselorEmployeeId,
                ClosureSummary = value.ClosureSummary,
                ConfidentialSessions = value.ConfidentialSessions
                    .Select(item => new CounselingInternalSessionDto
                    {
                        SessionId = item.SessionId,
                        OccurredAtUtc = item.OccurredAtUtc,
                        CounselorEmployeeId = item.CounselorEmployeeId,
                        RiskLevel = item.RiskLevel.ToString(),
                        InternalNotes = item.InternalNotes
                    })
                    .ToList()
                    .AsReadOnly(),
                ReleasedSummaries = value.ReleasedSummaries
                    .Select(item => new ReleasedSummaryDto
                    {
                        SummaryId = item.SummaryId,
                        RelatedRecordId = item.SessionId,
                        Summary = item.Summary,
                        ReleasedAtUtc = item.ReleasedAtUtc
                    })
                    .ToList()
                    .AsReadOnly()
            };
        }

        public DisciplineReleasedCaseDto ToDisciplineReleased(
            DisciplineCase value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var hasReleasedDecision = value.Decision != null
                && value.Decision.ReleasedAtUtc.HasValue;
            return new DisciplineReleasedCaseDto
            {
                CaseId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                ViolationCode = value.Violation == null
                    ? null
                    : value.Violation.Code,
                ViolationDescription = value.Violation == null
                    ? null
                    : value.Violation.Description,
                ReleasedNoticeSummary = value.ReleasedNotice == null
                    ? null
                    : value.ReleasedNotice.ReleasedSummary,
                ResponseDueDate = value.ReleasedNotice == null
                    ? null
                    : value.ReleasedNotice.ResponseDueDate.ToString(),
                ReleasedDecisionSummary = hasReleasedDecision
                    ? value.Decision.ReleasedDecisionSummary
                    : null,
                SanctionSummary = hasReleasedDecision
                    ? value.Decision.SanctionSummary
                    : null,
                EntityVersion = value.Version
            };
        }

        public DisciplineInternalCaseDto ToDisciplineInternal(
            DisciplineCase value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new DisciplineInternalCaseDto
            {
                CaseId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                InternalIncidentNarrative = value.InternalIncidentNarrative,
                RestrictedEvidenceReferences = value.RestrictedEvidence
                    .Select(item => item.Reference + " | " + item.Description)
                    .ToList()
                    .AsReadOnly(),
                RestrictedFindings = value.RestrictedFindings
                    .Select(item => item.InternalFinding)
                    .ToList()
                    .AsReadOnly(),
                InternalDecisionRationale = value.Decision == null
                    ? null
                    : value.Decision.InternalRationale
            };
        }

        public MedicalReleasedRecordDto ToMedicalReleased(MedicalRecord value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new MedicalReleasedRecordDto
            {
                MedicalRecordId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                ReleasedSummaries = value.ReleasedSummaries
                    .Select(item => new ReleasedSummaryDto
                    {
                        SummaryId = item.SummaryId,
                        RelatedRecordId = item.ConsultationId,
                        Summary = item.ReleasedSummary,
                        ReleasedAtUtc = item.ReleasedAtUtc
                    })
                    .ToList()
                    .AsReadOnly()
            };
        }

        public MedicalInternalRecordDto ToMedicalInternal(MedicalRecord value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new MedicalInternalRecordDto
            {
                MedicalRecordId = value.Id,
                StudentId = value.StudentId,
                Status = value.Status.ToString(),
                ConfidentialConsultations = value.ConfidentialConsultations
                    .Select(item => new MedicalInternalConsultationDto
                    {
                        ConsultationId = item.ConsultationId,
                        AppointmentId = item.AppointmentId,
                        ClinicianEmployeeId = item.ClinicianEmployeeId,
                        OccurredAtUtc = item.OccurredAtUtc,
                        InternalClinicalNotes = item.InternalClinicalNotes,
                        InternalAssessment = item.InternalAssessment,
                        InternalTreatmentPlan = item.InternalTreatmentPlan
                    })
                    .ToList()
                    .AsReadOnly(),
                ReleasedSummaries = value.ReleasedSummaries
                    .Select(item => new ReleasedSummaryDto
                    {
                        SummaryId = item.SummaryId,
                        RelatedRecordId = item.ConsultationId,
                        Summary = item.ReleasedSummary,
                        ReleasedAtUtc = item.ReleasedAtUtc
                    })
                    .ToList()
                    .AsReadOnly()
            };
        }
    }
}
