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

    public sealed class ClinicAppointment : EntityBase
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
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            RequestedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(
                requestedAppointmentAtUtc,
                nameof(requestedAppointmentAtUtc));
            ReleasedReasonSummary = ServiceDomainGuard.RequiredText(
                releasedReasonSummary,
                nameof(releasedReasonSummary),
                500);
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

        public void Schedule(
            DateTime scheduledAtUtc,
            string clinicianEmployeeId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.Requested, "schedule");
            ScheduledAtUtc = ServiceDomainGuard.RequireUtc(scheduledAtUtc, nameof(scheduledAtUtc));
            ClinicianEmployeeId = ServiceDomainGuard.RequireIdentifier(
                clinicianEmployeeId,
                "EMP",
                nameof(clinicianEmployeeId));
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
            CheckedInAtUtc = ServiceDomainGuard.RequireUtc(checkedInAtUtc, nameof(checkedInAtUtc));
            Status = ClinicAppointmentStatus.CheckedIn;
            RecordServiceChange(CheckedInAtUtc.Value, changedByUserId);
        }

        public void Complete(
            string consultationId,
            DateTime completedAtUtc,
            string changedByUserId)
        {
            RequireStatus(ClinicAppointmentStatus.CheckedIn, "complete");
            ConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            CompletedAtUtc = ServiceDomainGuard.RequireUtc(completedAtUtc, nameof(completedAtUtc));
            if (CheckedInAtUtc.HasValue && CompletedAtUtc.Value < CheckedInAtUtc.Value)
            {
                throw new DomainValidationException(
                    "Clinic Appointment completion cannot precede check-in.");
            }

            Status = ClinicAppointmentStatus.Completed;
            RecordServiceChange(CompletedAtUtc.Value, changedByUserId);
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

            CancellationReason = ServiceDomainGuard.RequiredText(reason, nameof(reason), 500);
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
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
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
            SummaryId = ServiceDomainGuard.RequireIdentifier(summaryId, "MRS", nameof(summaryId));
            ConsultationId = ServiceDomainGuard.RequireIdentifier(
                consultationId,
                "CON",
                nameof(consultationId));
            ReleasedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(releasedAtUtc, nameof(releasedAtUtc));
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

    public sealed class MedicalRecord : EntityBase
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
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
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
            if (_consultations.Any(
                item => StringComparer.Ordinal.Equals(
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
            if (!_consultations.Any(
                item => StringComparer.Ordinal.Equals(
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
            if (_releasedSummaries.Any(
                item => StringComparer.Ordinal.Equals(item.SummaryId, summary.SummaryId)))
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

            ClosedAtUtc = ServiceDomainGuard.RequireUtc(closedAtUtc, nameof(closedAtUtc));
            Status = MedicalRecordStatus.Closed;
            RecordServiceChange(ClosedAtUtc.Value, changedByUserId);
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
            HistoryId = ServiceDomainGuard.RequireIdentifier(historyId, "MCH", nameof(historyId));
            Action = ServiceDomainGuard.RequireDefined(action, nameof(action));
            FromStatus = ServiceDomainGuard.RequireDefined(fromStatus, nameof(fromStatus));
            ToStatus = ServiceDomainGuard.RequireDefined(toStatus, nameof(toStatus));
            Reason = ServiceDomainGuard.OptionalText(reason, nameof(reason), 1000);
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
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

    public sealed class MedicalClearance : EntityBase
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
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
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

            ClearanceNumber = ServiceDomainGuard.RequireIdentifier(
                clearanceNumber,
                "MCN",
                nameof(clearanceNumber));
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            ReleasedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Issued,
                MedicalClearanceStatus.Issued,
                ReleasedSummary,
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
            releasedReason = ServiceDomainGuard.RequiredText(
                releasedReason,
                nameof(releasedReason),
                1000);
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Denied,
                MedicalClearanceStatus.Denied,
                releasedReason,
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
            RevocationReason = ServiceDomainGuard.RequiredText(reason, nameof(reason), 1000);
            Transition(
                historyId,
                MedicalClearanceHistoryAction.Revoked,
                MedicalClearanceStatus.Revoked,
                RevocationReason,
                changedAtUtc,
                changedByUserId);
        }

        public bool IsValidOn(InstitutionLocalDate date)
        {
            if (Status != MedicalClearanceStatus.Issued || !ValidFrom.HasValue)
            {
                return false;
            }

            return date >= ValidFrom.Value
                && (!ValidUntil.HasValue || date <= ValidUntil.Value);
        }

        private void Transition(
            string historyId,
            MedicalClearanceHistoryAction action,
            MedicalClearanceStatus newStatus,
            string reason,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var previous = Status;
            AddHistory(
                historyId,
                action,
                previous,
                newStatus,
                reason,
                changedAtUtc,
                changedByUserId);
            Status = newStatus;
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId,
                    "USR",
                    nameof(changedByUserId)));
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
            if (_history.Any(item => StringComparer.Ordinal.Equals(item.HistoryId, entry.HistoryId)))
            {
                throw new DomainValidationException(
                    "The Medical Clearance already contains the History ID.");
            }

            _history.Add(entry);
        }

        private void RequireStatus(MedicalClearanceStatus expected, string operation)
        {
            if (Status != expected)
            {
                throw new DomainValidationException(
                    "The Medical Clearance cannot " + operation + " from its current status.");
            }
        }
    }
}
