using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Services;
using IUIS.Domain.Time;

namespace IUIS.Domain.Clinic
{
    public enum ClinicAppointmentStatus
    {
        Requested = 0,
        Scheduled = 1,
        Confirmed = 2,
        CheckedIn = 3,
        Completed = 4,
        Cancelled = 5,
        NoShow = 6
    }

    public enum MedicalRecordStatus
    {
        Active = 0,
        Closed = 1
    }

    public enum MedicalClearanceStatus
    {
        Requested = 0,
        UnderReview = 1,
        Issued = 2,
        Denied = 3,
        Revoked = 4
    }

    public enum MedicalClearanceHistoryAction
    {
        Requested = 0,
        ReviewStarted = 1,
        Issued = 2,
        Denied = 3,
        Revoked = 4
    }

    public sealed partial class ClinicAppointment : EntityBase
    {
        public ClinicAppointment(
            string id,
            string studentId,
            DateTime requestedAppointmentAtUtc,
            string releasedReasonSummary,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "CAP", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(createdByUserId, "USR", nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            RequestedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(requestedAppointmentAtUtc, nameof(requestedAppointmentAtUtc));
            ReleasedReasonSummary = ServiceDomainGuard.RequiredText(releasedReasonSummary, nameof(releasedReasonSummary), 500);
            Status = ClinicAppointmentStatus.Requested;
        }

        public string StudentId { get; }
        public DateTime RequestedAppointmentAtUtc { get; }
        public string ReleasedReasonSummary { get; }
        public ClinicAppointmentStatus Status { get; private set; }
        public DateTime? ScheduledAtUtc { get; private set; }
        public string ClinicianEmployeeId { get; private set; }
        public DateTime? CheckedInAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        public string ConsultationId { get; private set; }
        public string CancellationReason { get; private set; }

        public static ClinicAppointment Rehydrate(
            string id,
            string studentId,
            DateTime requestedAppointmentAtUtc,
            string releasedReasonSummary,
            ClinicAppointmentStatus status,
            DateTime? scheduledAtUtc,
            string clinicianEmployeeId,
            DateTime? checkedInAtUtc,
            DateTime? completedAtUtc,
            string consultationId,
            string cancellationReason,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            ValidatePersistedState(
                status,
                scheduledAtUtc,
                clinicianEmployeeId,
                checkedInAtUtc,
                completedAtUtc,
                consultationId,
                cancellationReason);

            var appointment = new ClinicAppointment(
                id,
                studentId,
                requestedAppointmentAtUtc,
                releasedReasonSummary,
                createdAtUtc,
                createdByUserId);
            appointment.Status = status;
            appointment.ScheduledAtUtc = scheduledAtUtc.HasValue
                ? ServiceDomainGuard.RequireUtc(scheduledAtUtc.Value, nameof(scheduledAtUtc))
                : (DateTime?)null;
            appointment.ClinicianEmployeeId = string.IsNullOrWhiteSpace(clinicianEmployeeId)
                ? null
                : ServiceDomainGuard.RequireIdentifier(clinicianEmployeeId, "EMP", nameof(clinicianEmployeeId));
            appointment.CheckedInAtUtc = checkedInAtUtc.HasValue
                ? ServiceDomainGuard.RequireUtc(checkedInAtUtc.Value, nameof(checkedInAtUtc))
                : (DateTime?)null;
            appointment.CompletedAtUtc = completedAtUtc.HasValue
                ? ServiceDomainGuard.RequireUtc(completedAtUtc.Value, nameof(completedAtUtc))
                : (DateTime?)null;
            appointment.ConsultationId = string.IsNullOrWhiteSpace(consultationId)
                ? null
                : ServiceDomainGuard.RequireIdentifier(consultationId, "CON", nameof(consultationId));
            appointment.CancellationReason = ServiceDomainGuard.OptionalText(
                cancellationReason,
                nameof(cancellationReason),
                500);

            if (appointment.CheckedInAtUtc.HasValue
                && appointment.CompletedAtUtc.HasValue
                && appointment.CompletedAtUtc.Value < appointment.CheckedInAtUtc.Value)
            {
                throw new DomainValidationException(
                    "A persisted Clinic Appointment completion cannot precede check-in.");
            }

            appointment.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return appointment;
        }

        public void Schedule(
            DateTime scheduledAtUtc,
            string clinicianEmployeeId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.Requested, "schedule");
            var normalizedScheduledAtUtc = ServiceDomainGuard.RequireUtc(
                scheduledAtUtc,
                nameof(scheduledAtUtc));
            var normalizedClinicianId = ServiceDomainGuard.RequireIdentifier(
                clinicianEmployeeId,
                "EMP",
                nameof(clinicianEmployeeId));
            ScheduledAtUtc = normalizedScheduledAtUtc;
            ClinicianEmployeeId = normalizedClinicianId;
            Status = ClinicAppointmentStatus.Scheduled;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void Confirm(DateTime changedAtUtc, string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.Scheduled, "confirm");
            Status = ClinicAppointmentStatus.Confirmed;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void CheckIn(DateTime checkedInAtUtc, string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.Confirmed, "check in");
            var normalizedCheckedInAtUtc = ServiceDomainGuard.RequireUtc(
                checkedInAtUtc,
                nameof(checkedInAtUtc));
            CheckedInAtUtc = normalizedCheckedInAtUtc;
            Status = ClinicAppointmentStatus.CheckedIn;
            RecordServiceChange(normalizedCheckedInAtUtc, changedByUserId);
        }

        public void Complete(
            string consultationId,
            DateTime completedAtUtc,
            string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.CheckedIn, "complete");
            var normalizedConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            var normalizedCompletedAtUtc = ServiceDomainGuard.RequireUtc(
                completedAtUtc,
                nameof(completedAtUtc));
            if (CheckedInAtUtc.HasValue && normalizedCompletedAtUtc < CheckedInAtUtc.Value)
            {
                throw new DomainValidationException(
                    "Clinic Appointment completion cannot precede check-in.");
            }

            ConsultationId = normalizedConsultationId;
            CompletedAtUtc = normalizedCompletedAtUtc;
            Status = ClinicAppointmentStatus.Completed;
            RecordServiceChange(normalizedCompletedAtUtc, changedByUserId);
        }

        public void Cancel(
            string reason,
            DateTime cancelledAtUtc,
            string changedByUserId)
        {
            if (Status == ClinicAppointmentStatus.Completed
                || Status == ClinicAppointmentStatus.Cancelled
                || Status == ClinicAppointmentStatus.NoShow)
            {
                throw new DomainValidationException(
                    "The Clinic Appointment can no longer be cancelled.");
            }

            var normalizedReason = ServiceDomainGuard.RequiredText(
                reason,
                nameof(reason),
                500);
            ServiceDomainGuard.RequireUtc(cancelledAtUtc, nameof(cancelledAtUtc));
            CancellationReason = normalizedReason;
            Status = ClinicAppointmentStatus.Cancelled;
            RecordServiceChange(cancelledAtUtc, changedByUserId);
        }

        public void MarkNoShow(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != ClinicAppointmentStatus.Scheduled
                && Status != ClinicAppointmentStatus.Confirmed)
            {
                throw new DomainValidationException(
                    "Only a scheduled or confirmed Clinic Appointment can be marked No Show.");
            }

            Status = ClinicAppointmentStatus.NoShow;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        private static void ValidatePersistedState(
            ClinicAppointmentStatus status,
            DateTime? scheduledAtUtc,
            string clinicianEmployeeId,
            DateTime? checkedInAtUtc,
            DateTime? completedAtUtc,
            string consultationId,
            string cancellationReason)
        {
            var hasSchedule = scheduledAtUtc.HasValue
                && !string.IsNullOrWhiteSpace(clinicianEmployeeId);
            var hasCompletion = completedAtUtc.HasValue
                && !string.IsNullOrWhiteSpace(consultationId);
            var hasCancellation = !string.IsNullOrWhiteSpace(cancellationReason);

            if (scheduledAtUtc.HasValue
                != !string.IsNullOrWhiteSpace(clinicianEmployeeId))
            {
                throw new DomainValidationException(
                    "A persisted Clinic Appointment schedule requires both timestamp and clinician.");
            }

            if (completedAtUtc.HasValue
                != !string.IsNullOrWhiteSpace(consultationId))
            {
                throw new DomainValidationException(
                    "A persisted completed Clinic Appointment requires both timestamp and Consultation ID.");
            }

            switch (status)
            {
                case ClinicAppointmentStatus.Requested:
                    if (hasSchedule || checkedInAtUtc.HasValue || hasCompletion || hasCancellation)
                    {
                        throw new DomainValidationException(
                            "A requested Clinic Appointment contains later workflow state.");
                    }
                    break;
                case ClinicAppointmentStatus.Scheduled:
                case ClinicAppointmentStatus.Confirmed:
                    if (!hasSchedule || checkedInAtUtc.HasValue || hasCompletion || hasCancellation)
                    {
                        throw new DomainValidationException(
                            "A scheduled or confirmed Clinic Appointment is inconsistent.");
                    }
                    break;
                case ClinicAppointmentStatus.CheckedIn:
                    if (!hasSchedule || !checkedInAtUtc.HasValue || hasCompletion || hasCancellation)
                    {
                        throw new DomainValidationException(
                            "A checked-in Clinic Appointment is inconsistent.");
                    }
                    break;
                case ClinicAppointmentStatus.Completed:
                    if (!hasSchedule || !checkedInAtUtc.HasValue || !hasCompletion || hasCancellation)
                    {
                        throw new DomainValidationException(
                            "A completed Clinic Appointment is inconsistent.");
                    }
                    break;
                case ClinicAppointmentStatus.Cancelled:
                    if (!hasCancellation || hasCompletion)
                    {
                        throw new DomainValidationException(
                            "A cancelled Clinic Appointment is inconsistent.");
                    }
                    break;
                case ClinicAppointmentStatus.NoShow:
                    if (!hasSchedule || checkedInAtUtc.HasValue || hasCompletion || hasCancellation)
                    {
                        throw new DomainValidationException(
                            "A no-show Clinic Appointment is inconsistent.");
                    }
                    break;
                default:
                    throw new DomainValidationException(
                        "The persisted Clinic Appointment status is invalid.");
            }
        }

        private void RequireStatus(ClinicAppointmentStatus expected, string operation)
        {
            if (Status != expected)
            {
                throw new DomainValidationException(
                    "The Clinic Appointment cannot " + operation + " from its current status.");
            }
        }

        private void RecordServiceChange(DateTime changedAtUtc, string changedByUserId)
        {
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId,
                    "USR",
                    nameof(changedByUserId)));
        }
    }

    public sealed class MedicalConsultationRecord
    {
        internal MedicalConsultationRecord(
            string consultationId,
            string appointmentId,
            string clinicianEmployeeId,
            DateTime occurredAtUtc,
            string internalClinicalNotes,
            string internalAssessment,
            string internalTreatmentPlan)
        {
            ConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            AppointmentId = ServiceDomainGuard.RequireIdentifier(
                appointmentId,
                "CAP",
                nameof(appointmentId));
            ClinicianEmployeeId = ServiceDomainGuard.RequireIdentifier(
                clinicianEmployeeId,
                "EMP",
                nameof(clinicianEmployeeId));
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(
                occurredAtUtc,
                nameof(occurredAtUtc));
            InternalClinicalNotes = ServiceDomainGuard.RequiredText(
                internalClinicalNotes,
                nameof(internalClinicalNotes),
                8000);
            InternalAssessment = ServiceDomainGuard.RequiredText(
                internalAssessment,
                nameof(internalAssessment),
                4000);
            InternalTreatmentPlan = ServiceDomainGuard.OptionalText(
                internalTreatmentPlan,
                nameof(internalTreatmentPlan),
                4000);
        }

        public string ConsultationId { get; }
        public string AppointmentId { get; }
        public string ClinicianEmployeeId { get; }
        public DateTime OccurredAtUtc { get; }
        public string InternalClinicalNotes { get; }
        public string InternalAssessment { get; }
        public string InternalTreatmentPlan { get; }
    }

    public sealed class MedicalReleasedSummary
    {
        internal MedicalReleasedSummary(
            string summaryId,
            string consultationId,
            string releasedSummary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            SummaryId = ServiceDomainGuard.RequireIdentifier(
                summaryId,
                "MRS",
                nameof(summaryId));
            ConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            ReleasedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                releasedAtUtc,
                nameof(releasedAtUtc));
            ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId,
                "USR",
                nameof(releasedByUserId));
        }

        public string SummaryId { get; }
        public string ConsultationId { get; }
        public string ReleasedSummary { get; }
        public DateTime ReleasedAtUtc { get; }
        public string ReleasedByUserId { get; }
    }

    public sealed partial class MedicalRecord : EntityBase
    {
        private readonly List<MedicalConsultationRecord> _consultations =
            new List<MedicalConsultationRecord>();
        private readonly List<MedicalReleasedSummary> _releasedSummaries =
            new List<MedicalReleasedSummary>();

        public MedicalRecord(
            string id,
            string studentId,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "MDR", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(createdByUserId, "USR", nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            Status = MedicalRecordStatus.Active;
        }

        public string StudentId { get; }
        public MedicalRecordStatus Status { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }

        public IReadOnlyList<MedicalConsultationRecord> ConfidentialConsultations
        {
            get { return _consultations.AsReadOnly(); }
        }

        public IReadOnlyList<MedicalReleasedSummary> ReleasedSummaries
        {
            get { return _releasedSummaries.AsReadOnly(); }
        }

        public static MedicalRecord Rehydrate(
            string id,
            string studentId,
            MedicalRecordStatus status,
            DateTime? closedAtUtc,
            IEnumerable<MedicalConsultationRecord> consultations,
            IEnumerable<MedicalReleasedSummary> releasedSummaries,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            if (consultations == null)
            {
                throw new DomainValidationException(
                    "Persisted Medical consultations are required.");
            }
            if (releasedSummaries == null)
            {
                throw new DomainValidationException(
                    "Persisted Medical released summaries are required.");
            }
            if (isArchived
                || archivedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(archivedByUserId))
            {
                throw new DomainValidationException(
                    "Medical Records are retained and cannot contain archive state.");
            }

            var record = new MedicalRecord(
                id,
                studentId,
                createdAtUtc,
                createdByUserId);
            foreach (var consultation in consultations)
            {
                if (consultation == null)
                {
                    throw new DomainValidationException(
                        "A persisted Medical consultation is invalid.");
                }
                if (record._consultations.Any(item =>
                    StringComparer.Ordinal.Equals(
                        item.ConsultationId,
                        consultation.ConsultationId)))
                {
                    throw new DomainValidationException(
                        "Persisted Medical Consultation IDs must be unique.");
                }
                record._consultations.Add(consultation);
            }

            foreach (var summary in releasedSummaries)
            {
                if (summary == null)
                {
                    throw new DomainValidationException(
                        "A persisted Medical released summary is invalid.");
                }
                if (record._releasedSummaries.Any(item =>
                    StringComparer.Ordinal.Equals(item.SummaryId, summary.SummaryId)))
                {
                    throw new DomainValidationException(
                        "Persisted Medical Released Summary IDs must be unique.");
                }
                if (!record._consultations.Any(item =>
                    StringComparer.Ordinal.Equals(
                        item.ConsultationId,
                        summary.ConsultationId)))
                {
                    throw new DomainValidationException(
                        "A persisted Medical released summary must reference an existing Consultation.");
                }
                record._releasedSummaries.Add(summary);
            }

            if (status == MedicalRecordStatus.Active && closedAtUtc.HasValue)
            {
                throw new DomainValidationException(
                    "An active persisted Medical Record cannot contain closure metadata.");
            }
            if (status == MedicalRecordStatus.Closed)
            {
                if (!closedAtUtc.HasValue || record._consultations.Count == 0)
                {
                    throw new DomainValidationException(
                        "A closed persisted Medical Record requires a closure timestamp and Consultation.");
                }
                record.ClosedAtUtc = ServiceDomainGuard.RequireUtc(
                    closedAtUtc.Value,
                    nameof(closedAtUtc));
                if (record.ClosedAtUtc.Value > updatedAtUtc)
                {
                    throw new DomainValidationException(
                        "Medical Record closure cannot follow its latest update.");
                }
            }

            record.Status = status;
            record.RestorePersistenceState(
                version,
                false,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                null,
                null);
            return record;
        }

        public void AddConsultation(
            string consultationId,
            string appointmentId,
            string clinicianEmployeeId,
            DateTime occurredAtUtc,
            string internalClinicalNotes,
            string internalAssessment,
            string internalTreatmentPlan,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureActive();
            var consultation = new MedicalConsultationRecord(
                consultationId,
                appointmentId,
                clinicianEmployeeId,
                occurredAtUtc,
                internalClinicalNotes,
                internalAssessment,
                internalTreatmentPlan);
            if (_consultations.Any(item =>
                StringComparer.Ordinal.Equals(
                    item.ConsultationId,
                    consultation.ConsultationId)))
            {
                throw new DomainValidationException(
                    "The Medical Record already contains the Consultation ID.");
            }
            _consultations.Add(consultation);
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void ReleaseConsultationSummary(
            string summaryId,
            string consultationId,
            string releasedSummary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            var normalizedConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            if (!_consultations.Any(item =>
                StringComparer.Ordinal.Equals(
                    item.ConsultationId,
                    normalizedConsultationId)))
            {
                throw new DomainValidationException(
                    "A released Medical summary must reference an existing Consultation.");
            }

            var summary = new MedicalReleasedSummary(
                summaryId,
                normalizedConsultationId,
                releasedSummary,
                releasedAtUtc,
                releasedByUserId);
            if (_releasedSummaries.Any(item =>
                StringComparer.Ordinal.Equals(item.SummaryId, summary.SummaryId)))
            {
                throw new DomainValidationException(
                    "The Medical Record already contains the Released Summary ID.");
            }
            _releasedSummaries.Add(summary);
            RecordChange(releasedAtUtc, summary.ReleasedByUserId);
        }

        public void Close(DateTime closedAtUtc, string changedByUserId)
        {
            EnsureActive();
            if (_consultations.Count == 0)
            {
                throw new DomainValidationException(
                    "A Medical Record requires a Consultation before closure.");
            }
            var normalized = ServiceDomainGuard.RequireUtc(
                closedAtUtc,
                nameof(closedAtUtc));
            ClosedAtUtc = normalized;
            Status = MedicalRecordStatus.Closed;
            RecordServiceChange(normalized, changedByUserId);
        }

        public override void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            throw new DomainValidationException(
                "Medical Records are retained and cannot be archived through Domain behavior.");
        }

        public override void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            throw new DomainValidationException(
                "Medical Records are retained and cannot be restored through Domain behavior.");
        }

        private void EnsureActive()
        {
            if (Status != MedicalRecordStatus.Active)
            {
                throw new DomainValidationException(
                    "Only an active Medical Record can accept clinical changes.");
            }
        }

        private void RecordServiceChange(DateTime changedAtUtc, string changedByUserId)
        {
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId,
                    "USR",
                    nameof(changedByUserId)));
        }
    }

    public sealed class MedicalClearanceHistoryEntry
    {
        internal MedicalClearanceHistoryEntry(
            string historyId,
            MedicalClearanceHistoryAction action,
            MedicalClearanceStatus fromStatus,
            MedicalClearanceStatus toStatus,
            string reason,
            DateTime occurredAtUtc,
            string actorUserId)
        {
            HistoryId = ServiceDomainGuard.RequireIdentifier(
                historyId,
                "MCH",
                nameof(historyId));
            Action = ServiceDomainGuard.RequireDefined(action, nameof(action));
            FromStatus = ServiceDomainGuard.RequireDefined(fromStatus, nameof(fromStatus));
            ToStatus = ServiceDomainGuard.RequireDefined(toStatus, nameof(toStatus));
            Reason = ServiceDomainGuard.OptionalText(reason, nameof(reason), 1000);
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(
                occurredAtUtc,
                nameof(occurredAtUtc));
            ActorUserId = ServiceDomainGuard.RequireIdentifier(
                actorUserId,
                "USR",
                nameof(actorUserId));
        }

        public string HistoryId { get; }
        public MedicalClearanceHistoryAction Action { get; }
        public MedicalClearanceStatus FromStatus { get; }
        public MedicalClearanceStatus ToStatus { get; }
        public string Reason { get; }
        public DateTime OccurredAtUtc { get; }
        public string ActorUserId { get; }
    }

    public sealed partial class MedicalClearance : EntityBase
    {
        private readonly List<MedicalClearanceHistoryEntry> _history =
            new List<MedicalClearanceHistoryEntry>();

        public MedicalClearance(
            string id,
            string requestHistoryId,
            string studentId,
            string medicalRecordId,
            string requestReason,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "MCL", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(createdByUserId, "USR", nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            MedicalRecordId = ServiceDomainGuard.RequireIdentifier(
                medicalRecordId,
                "MDR",
                nameof(medicalRecordId));
            RequestReason = ServiceDomainGuard.RequiredText(
                requestReason,
                nameof(requestReason),
                1000);
            Status = MedicalClearanceStatus.Requested;
            AddHistory(
                requestHistoryId,
                MedicalClearanceHistoryAction.Requested,
                MedicalClearanceStatus.Requested,
                MedicalClearanceStatus.Requested,
                requestReason,
                createdAtUtc,
                createdByUserId);
        }

        public string StudentId { get; }
        public string MedicalRecordId { get; }
        public string RequestReason { get; }
        public MedicalClearanceStatus Status { get; private set; }
        public string ReviewingClinicianEmployeeId { get; private set; }
        public string ClearanceNumber { get; private set; }
        public InstitutionLocalDate? ValidFrom { get; private set; }
        public InstitutionLocalDate? ValidUntil { get; private set; }
        public string ReleasedSummary { get; private set; }
        public string RevocationReason { get; private set; }

        public IReadOnlyList<MedicalClearanceHistoryEntry> History
        {
            get { return _history.AsReadOnly(); }
        }

        public static MedicalClearance Rehydrate(
            string id,
            string studentId,
            string medicalRecordId,
            string requestReason,
            MedicalClearanceStatus status,
            string reviewingClinicianEmployeeId,
            string clearanceNumber,
            InstitutionLocalDate? validFrom,
            InstitutionLocalDate? validUntil,
            string releasedSummary,
            string revocationReason,
            IEnumerable<MedicalClearanceHistoryEntry> history,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            if (history == null)
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance history is required.");
            }
            var entries = history.ToList();
            if (entries.Count == 0)
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance history cannot be empty.");
            }
            if (entries.Any(item => item == null))
            {
                throw new DomainValidationException(
                    "A persisted Medical Clearance history entry is invalid.");
            }
            if (entries.Select(item => item.HistoryId)
                .Distinct(StringComparer.Ordinal).Count() != entries.Count)
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance History IDs must be unique.");
            }

            ValidateHistory(
                entries,
                status,
                createdAtUtc,
                createdByUserId);
            ValidatePersistedState(
                status,
                reviewingClinicianEmployeeId,
                clearanceNumber,
                validFrom,
                validUntil,
                releasedSummary,
                revocationReason);

            var clearance = new MedicalClearance(
                id,
                entries[0].HistoryId,
                studentId,
                medicalRecordId,
                requestReason,
                createdAtUtc,
                createdByUserId);
            clearance._history.Clear();
            clearance._history.AddRange(entries);
            clearance.Status = status;
            clearance.ReviewingClinicianEmployeeId =
                string.IsNullOrWhiteSpace(reviewingClinicianEmployeeId)
                    ? null
                    : ServiceDomainGuard.RequireIdentifier(
                        reviewingClinicianEmployeeId,
                        "EMP",
                        nameof(reviewingClinicianEmployeeId));
            clearance.ClearanceNumber =
                string.IsNullOrWhiteSpace(clearanceNumber)
                    ? null
                    : ServiceDomainGuard.RequireIdentifier(
                        clearanceNumber,
                        "MCN",
                        nameof(clearanceNumber));
            clearance.ValidFrom = validFrom;
            clearance.ValidUntil = validUntil;
            clearance.ReleasedSummary = ServiceDomainGuard.OptionalText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            clearance.RevocationReason = ServiceDomainGuard.OptionalText(
                revocationReason,
                nameof(revocationReason),
                1000);
            clearance.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return clearance;
        }

        public void BeginReview(
            string historyId,
            string clinicianEmployeeId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(MedicalClearanceStatus.Requested, "begin review");
            ReviewingClinicianEmployeeId = ServiceDomainGuard.RequireIdentifier(
                clinicianEmployeeId,
                "EMP",
                nameof(clinicianEmployeeId));
            Transition(
                historyId,
                MedicalClearanceHistoryAction.ReviewStarted,
                MedicalClearanceStatus.UnderReview,
                null,
                changedAtUtc,
                changedByUserId);
        }

        public void Issue(
            string historyId,
            string clearanceNumber,
            InstitutionLocalDate validFrom,
            InstitutionLocalDate? validUntil,
            string releasedSummary,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(MedicalClearanceStatus.UnderReview, "issue");
            if (validUntil.HasValue && validUntil.Value < validFrom)
            {
                throw new DomainValidationException(
                    "Medical Clearance validity cannot end before it begins.");
            }

            var normalizedNumber = ServiceDomainGuard.RequireIdentifier(
                clearanceNumber,
                "MCN",
                nameof(clearanceNumber));
            var normalizedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            ClearanceNumber = normalizedNumber;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            ReleasedSummary = normalizedSummary;
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Issued,
                MedicalClearanceStatus.Issued,
                normalizedSummary,
                changedAtUtc,
                changedByUserId);
        }

        public void Deny(
            string historyId,
            string releasedReason,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(MedicalClearanceStatus.UnderReview, "deny");
            var normalizedReason = ServiceDomainGuard.RequiredText(
                releasedReason,
                nameof(releasedReason),
                1000);
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Denied,
                MedicalClearanceStatus.Denied,
                normalizedReason,
                changedAtUtc,
                changedByUserId);
        }

        public void Revoke(
            string historyId,
            string reason,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(MedicalClearanceStatus.Issued, "revoke");
            var normalizedReason = ServiceDomainGuard.RequiredText(
                reason,
                nameof(reason),
                1000);
            RevocationReason = normalizedReason;
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Revoked,
                MedicalClearanceStatus.Revoked,
                normalizedReason,
                changedAtUtc,
                changedByUserId);
        }

        public bool IsValidOn(InstitutionLocalDate date)
        {
            if (Status != MedicalClearanceStatus.Issued
                || !ValidFrom.HasValue)
            {
                return false;
            }
            return date >= ValidFrom.Value
                && (!ValidUntil.HasValue || date <= ValidUntil.Value);
        }

        private static void ValidatePersistedState(
            MedicalClearanceStatus status,
            string reviewer,
            string clearanceNumber,
            InstitutionLocalDate? validFrom,
            InstitutionLocalDate? validUntil,
            string releasedSummary,
            string revocationReason)
        {
            var hasReviewer = !string.IsNullOrWhiteSpace(reviewer);
            var hasNumber = !string.IsNullOrWhiteSpace(clearanceNumber);
            var hasSummary = !string.IsNullOrWhiteSpace(releasedSummary);
            var hasRevocation = !string.IsNullOrWhiteSpace(revocationReason);
            if (validUntil.HasValue
                && (!validFrom.HasValue || validUntil.Value < validFrom.Value))
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance validity is invalid.");
            }

            switch (status)
            {
                case MedicalClearanceStatus.Requested:
                    if (hasReviewer || hasNumber || validFrom.HasValue
                        || validUntil.HasValue || hasSummary || hasRevocation)
                    {
                        throw new DomainValidationException(
                            "A requested Medical Clearance contains later workflow state.");
                    }
                    break;
                case MedicalClearanceStatus.UnderReview:
                    if (!hasReviewer || hasNumber || validFrom.HasValue
                        || validUntil.HasValue || hasSummary || hasRevocation)
                    {
                        throw new DomainValidationException(
                            "An under-review Medical Clearance is inconsistent.");
                    }
                    break;
                case MedicalClearanceStatus.Issued:
                    if (!hasReviewer || !hasNumber || !validFrom.HasValue
                        || !hasSummary || hasRevocation)
                    {
                        throw new DomainValidationException(
                            "An issued Medical Clearance is inconsistent.");
                    }
                    break;
                case MedicalClearanceStatus.Denied:
                    if (!hasReviewer || hasNumber || validFrom.HasValue
                        || validUntil.HasValue || hasSummary || hasRevocation)
                    {
                        throw new DomainValidationException(
                            "A denied Medical Clearance is inconsistent.");
                    }
                    break;
                case MedicalClearanceStatus.Revoked:
                    if (!hasReviewer || !hasNumber || !validFrom.HasValue
                        || !hasSummary || !hasRevocation)
                    {
                        throw new DomainValidationException(
                            "A revoked Medical Clearance is inconsistent.");
                    }
                    break;
                default:
                    throw new DomainValidationException(
                        "The persisted Medical Clearance status is invalid.");
            }
        }

        private static void ValidateHistory(
            IList<MedicalClearanceHistoryEntry> entries,
            MedicalClearanceStatus finalStatus,
            DateTime createdAtUtc,
            string createdByUserId)
        {
            var first = entries[0];
            if (first.Action != MedicalClearanceHistoryAction.Requested
                || first.FromStatus != MedicalClearanceStatus.Requested
                || first.ToStatus != MedicalClearanceStatus.Requested
                || first.OccurredAtUtc != createdAtUtc
                || !StringComparer.Ordinal.Equals(first.ActorUserId, createdByUserId))
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance history must begin with the creation request.");
            }

            var current = MedicalClearanceStatus.Requested;
            var previousAt = first.OccurredAtUtc;
            for (var index = 1; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry.FromStatus != current
                    || entry.OccurredAtUtc < previousAt)
                {
                    throw new DomainValidationException(
                        "Persisted Medical Clearance history is not chronological or continuous.");
                }
                var expected = ExpectedToStatus(entry.Action);
                if (entry.ToStatus != expected)
                {
                    throw new DomainValidationException(
                        "Persisted Medical Clearance history action and status are inconsistent.");
                }
                current = entry.ToStatus;
                previousAt = entry.OccurredAtUtc;
            }

            if (current != finalStatus)
            {
                throw new DomainValidationException(
                    "Persisted Medical Clearance history does not match the current status.");
            }
        }

        private static MedicalClearanceStatus ExpectedToStatus(
            MedicalClearanceHistoryAction action)
        {
            switch (action)
            {
                case MedicalClearanceHistoryAction.Requested:
                    return MedicalClearanceStatus.Requested;
                case MedicalClearanceHistoryAction.ReviewStarted:
                    return MedicalClearanceStatus.UnderReview;
                case MedicalClearanceHistoryAction.Issued:
                    return MedicalClearanceStatus.Issued;
                case MedicalClearanceHistoryAction.Denied:
                    return MedicalClearanceStatus.Denied;
                case MedicalClearanceHistoryAction.Revoked:
                    return MedicalClearanceStatus.Revoked;
                default:
                    throw new DomainValidationException(
                        "The persisted Medical Clearance history action is invalid.");
            }
        }

        private void Transition(
            string historyId,
            MedicalClearanceHistoryAction action,
            MedicalClearanceStatus newStatus,
            string reason,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedActor = ServiceDomainGuard.RequireIdentifier(
                changedByUserId,
                "USR",
                nameof(changedByUserId));
            var normalizedTime = ServiceDomainGuard.RequireUtc(
                changedAtUtc,
                nameof(changedAtUtc));
            var previous = Status;
            AddHistory(
                historyId,
                action,
                previous,
                newStatus,
                reason,
                normalizedTime,
                normalizedActor);
            Status = newStatus;
            RecordChange(normalizedTime, normalizedActor);
        }

        private void AddHistory(
            string historyId,
            MedicalClearanceHistoryAction action,
            MedicalClearanceStatus fromStatus,
            MedicalClearanceStatus toStatus,
            string reason,
            DateTime occurredAtUtc,
            string actorUserId)
        {
            var entry = new MedicalClearanceHistoryEntry(
                historyId,
                action,
                fromStatus,
                toStatus,
                reason,
                occurredAtUtc,
                actorUserId);
            if (_history.Any(item =>
                StringComparer.Ordinal.Equals(item.HistoryId, entry.HistoryId)))
            {
                throw new DomainValidationException(
                    "The Medical Clearance already contains the History ID.");
            }
            _history.Add(entry);
        }

        private void RequireStatus(
            MedicalClearanceStatus expected,
            string operation)
        {
            if (Status != expected)
            {
                throw new DomainValidationException(
                    "The Medical Clearance cannot " + operation + " from its current status.");
            }
        }
    }
}
