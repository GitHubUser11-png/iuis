using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Services;

namespace IUIS.Domain.Counseling
{
    public enum CounselingCaseStatus
    {
        Requested = 0,
        Confirmed = 1,
        Assigned = 2,
        Active = 3,
        Closed = 4,
        Cancelled = 5
    }

    public enum CounselingRiskLevel
    {
        Routine = 0,
        Elevated = 1,
        Urgent = 2
    }

    public enum CounselingReleaseScope
    {
        AttendanceConfirmation = 0,
        ReferralSummary = 1,
        ReleasedSessionSummary = 2,
        CaseClosureSummary = 3
    }

    public enum CounselingReleaseStatus
    {
        Active = 0,
        Revoked = 1
    }

    public sealed class CounselingSessionRecord
    {
        internal CounselingSessionRecord(
            string sessionId,
            DateTime occurredAtUtc,
            string counselorEmployeeId,
            CounselingRiskLevel riskLevel,
            string internalNotes)
        {
            SessionId = ServiceDomainGuard.RequireIdentifier(
                sessionId, "CSN", nameof(sessionId));
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(
                occurredAtUtc, nameof(occurredAtUtc));
            CounselorEmployeeId = ServiceDomainGuard.RequireIdentifier(
                counselorEmployeeId, "EMP", nameof(counselorEmployeeId));
            RiskLevel = ServiceDomainGuard.RequireDefined(
                riskLevel, nameof(riskLevel));
            InternalNotes = ServiceDomainGuard.RequiredText(
                internalNotes, nameof(internalNotes), 8000);
        }

        public string SessionId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string CounselorEmployeeId { get; private set; }
        public CounselingRiskLevel RiskLevel { get; private set; }
        public string InternalNotes { get; private set; }

        public static CounselingSessionRecord Rehydrate(
            string sessionId,
            DateTime occurredAtUtc,
            string counselorEmployeeId,
            CounselingRiskLevel riskLevel,
            string internalNotes)
        {
            return new CounselingSessionRecord(
                sessionId,
                occurredAtUtc,
                counselorEmployeeId,
                riskLevel,
                internalNotes);
        }
    }

    public sealed class CounselingReleasedSummary
    {
        internal CounselingReleasedSummary(
            string summaryId,
            string sessionId,
            string releaseAuthorizationId,
            string summary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            SummaryId = ServiceDomainGuard.RequireIdentifier(
                summaryId, "CSR", nameof(summaryId));
            SessionId = ServiceDomainGuard.RequireIdentifier(
                sessionId, "CSN", nameof(sessionId));
            ReleaseAuthorizationId = ServiceDomainGuard.RequireIdentifier(
                releaseAuthorizationId, "CRL", nameof(releaseAuthorizationId));
            Summary = ServiceDomainGuard.RequiredText(
                summary, nameof(summary), 2000);
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                releasedAtUtc, nameof(releasedAtUtc));
            ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId, "USR", nameof(releasedByUserId));
        }

        public string SummaryId { get; private set; }
        public string SessionId { get; private set; }
        public string ReleaseAuthorizationId { get; private set; }
        public string Summary { get; private set; }
        public DateTime ReleasedAtUtc { get; private set; }
        public string ReleasedByUserId { get; private set; }

        public static CounselingReleasedSummary Rehydrate(
            string summaryId,
            string sessionId,
            string releaseAuthorizationId,
            string summary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            return new CounselingReleasedSummary(
                summaryId,
                sessionId,
                releaseAuthorizationId,
                summary,
                releasedAtUtc,
                releasedByUserId);
        }
    }

    public sealed class CounselingCase : EntityBase
    {
        private readonly List<CounselingSessionRecord> _sessions =
            new List<CounselingSessionRecord>();
        private readonly List<CounselingReleasedSummary> _releasedSummaries =
            new List<CounselingReleasedSummary>();

        public CounselingCase(
            string id,
            string studentId,
            DateTime requestedAppointmentAtUtc,
            string requestReason,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "CNS", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId, "USR", nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(
                studentId, "STU", nameof(studentId));
            RequestedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(
                requestedAppointmentAtUtc, nameof(requestedAppointmentAtUtc));
            RequestReason = ServiceDomainGuard.RequiredText(
                requestReason, nameof(requestReason), 500);
            Status = CounselingCaseStatus.Requested;
        }

        public string StudentId { get; private set; }
        public DateTime RequestedAppointmentAtUtc { get; private set; }
        public string RequestReason { get; private set; }
        public CounselingCaseStatus Status { get; private set; }
        public DateTime? ConfirmedAppointmentAtUtc { get; private set; }
        public string AssignedCounselorEmployeeId { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }
        public string ClosureSummary { get; private set; }

        public IReadOnlyList<CounselingSessionRecord> ConfidentialSessions
        {
            get { return _sessions.AsReadOnly(); }
        }

        public IReadOnlyList<CounselingReleasedSummary> ReleasedSummaries
        {
            get { return _releasedSummaries.AsReadOnly(); }
        }

        public static CounselingCase Rehydrate(
            string id,
            string studentId,
            DateTime requestedAppointmentAtUtc,
            string requestReason,
            CounselingCaseStatus status,
            DateTime? confirmedAppointmentAtUtc,
            string assignedCounselorEmployeeId,
            DateTime? closedAtUtc,
            string closureSummary,
            IEnumerable<CounselingSessionRecord> confidentialSessions,
            IEnumerable<CounselingReleasedSummary> releasedSummaries,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            var value = new CounselingCase(
                id,
                studentId,
                requestedAppointmentAtUtc,
                requestReason,
                createdAtUtc,
                createdByUserId);
            value.Status = ServiceDomainGuard.RequireDefined(
                status, nameof(status));
            value.ConfirmedAppointmentAtUtc = confirmedAppointmentAtUtc.HasValue
                ? ServiceDomainGuard.RequireUtc(
                    confirmedAppointmentAtUtc.Value,
                    nameof(confirmedAppointmentAtUtc))
                : (DateTime?)null;
            value.AssignedCounselorEmployeeId = string.IsNullOrWhiteSpace(
                assignedCounselorEmployeeId)
                ? null
                : ServiceDomainGuard.RequireIdentifier(
                    assignedCounselorEmployeeId,
                    "EMP",
                    nameof(assignedCounselorEmployeeId));
            value.ClosedAtUtc = closedAtUtc.HasValue
                ? ServiceDomainGuard.RequireUtc(
                    closedAtUtc.Value,
                    nameof(closedAtUtc))
                : (DateTime?)null;
            value.ClosureSummary = ServiceDomainGuard.OptionalText(
                closureSummary, nameof(closureSummary), 2000);

            var sessions = (confidentialSessions
                ?? Enumerable.Empty<CounselingSessionRecord>()).ToList();
            var summaries = (releasedSummaries
                ?? Enumerable.Empty<CounselingReleasedSummary>()).ToList();
            if (sessions.Any(item => item == null)
                || summaries.Any(item => item == null))
            {
                throw new DomainValidationException(
                    "Persisted Counseling collections cannot contain null records.");
            }
            if (sessions.Select(item => item.SessionId)
                .Distinct(StringComparer.Ordinal).Count() != sessions.Count)
            {
                throw new DomainValidationException(
                    "Persisted Counseling Sessions require unique identifiers.");
            }
            if (summaries.Select(item => item.SummaryId)
                .Distinct(StringComparer.Ordinal).Count() != summaries.Count)
            {
                throw new DomainValidationException(
                    "Persisted Counseling summaries require unique identifiers.");
            }

            value._sessions.AddRange(sessions);
            value._releasedSummaries.AddRange(summaries);
            value.ValidatePersistedState(createdAtUtc, updatedAtUtc);
            value.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return value;
        }

        public void ConfirmAppointment(
            DateTime confirmedAppointmentAtUtc,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Requested)
                throw new DomainValidationException(
                    "Only a requested Counseling Case can be confirmed.");
            ConfirmedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(
                confirmedAppointmentAtUtc, nameof(confirmedAppointmentAtUtc));
            Status = CounselingCaseStatus.Confirmed;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void AssignCounselor(
            string counselorEmployeeId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Confirmed)
                throw new DomainValidationException(
                    "A Counseling Case must be confirmed before counselor assignment.");
            AssignedCounselorEmployeeId = ServiceDomainGuard.RequireIdentifier(
                counselorEmployeeId, "EMP", nameof(counselorEmployeeId));
            Status = CounselingCaseStatus.Assigned;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void RecordSession(
            string sessionId,
            DateTime occurredAtUtc,
            CounselingRiskLevel riskLevel,
            string internalNotes,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Assigned
                && Status != CounselingCaseStatus.Active)
            {
                throw new DomainValidationException(
                    "A Counseling Case must have an assigned counselor before a session is recorded.");
            }
            var session = new CounselingSessionRecord(
                sessionId,
                occurredAtUtc,
                AssignedCounselorEmployeeId,
                riskLevel,
                internalNotes);
            if (_sessions.Any(item => StringComparer.Ordinal.Equals(
                item.SessionId, session.SessionId)))
            {
                throw new DomainValidationException(
                    "The Counseling Case already contains the Session ID.");
            }
            _sessions.Add(session);
            Status = CounselingCaseStatus.Active;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void ReleaseSessionSummary(
            string summaryId,
            string sessionId,
            string releaseAuthorizationId,
            string releasedSummary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            if (Status != CounselingCaseStatus.Active
                && Status != CounselingCaseStatus.Closed)
            {
                throw new DomainValidationException(
                    "Only an active or closed Counseling Case can release a summary.");
            }
            var normalizedSessionId = ServiceDomainGuard.RequireIdentifier(
                sessionId, "CSN", nameof(sessionId));
            if (!_sessions.Any(item => StringComparer.Ordinal.Equals(
                item.SessionId, normalizedSessionId)))
            {
                throw new DomainValidationException(
                    "A released Counseling summary must reference an existing session.");
            }
            var summary = new CounselingReleasedSummary(
                summaryId,
                normalizedSessionId,
                releaseAuthorizationId,
                releasedSummary,
                releasedAtUtc,
                releasedByUserId);
            if (_releasedSummaries.Any(item => StringComparer.Ordinal.Equals(
                item.SummaryId, summary.SummaryId)))
            {
                throw new DomainValidationException(
                    "The Counseling Case already contains the Released Summary ID.");
            }
            _releasedSummaries.Add(summary);
            RecordChange(releasedAtUtc, summary.ReleasedByUserId);
        }

        public void Close(
            string closureSummary,
            DateTime closedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Active)
                throw new DomainValidationException(
                    "Only an active Counseling Case can be closed.");
            if (_sessions.Count == 0)
                throw new DomainValidationException(
                    "A Counseling Case requires a recorded session before closure.");
            ClosureSummary = ServiceDomainGuard.RequiredText(
                closureSummary, nameof(closureSummary), 2000);
            ClosedAtUtc = ServiceDomainGuard.RequireUtc(
                closedAtUtc, nameof(closedAtUtc));
            Status = CounselingCaseStatus.Closed;
            RecordServiceChange(ClosedAtUtc.Value, changedByUserId);
        }

        public void Cancel(
            string reason,
            DateTime cancelledAtUtc,
            string changedByUserId)
        {
            if (Status == CounselingCaseStatus.Active
                || Status == CounselingCaseStatus.Closed
                || Status == CounselingCaseStatus.Cancelled)
            {
                throw new DomainValidationException(
                    "The Counseling Case can no longer be cancelled.");
            }
            ServiceDomainGuard.RequiredText(reason, nameof(reason), 500);
            Status = CounselingCaseStatus.Cancelled;
            RecordServiceChange(cancelledAtUtc, changedByUserId);
        }

        private void ValidatePersistedState(
            DateTime createdAtUtc,
            DateTime updatedAtUtc)
        {
            var created = ServiceDomainGuard.RequireUtc(
                createdAtUtc, nameof(createdAtUtc));
            var updated = ServiceDomainGuard.RequireUtc(
                updatedAtUtc, nameof(updatedAtUtc));
            if (updated < created)
                throw new DomainValidationException(
                    "Persisted Counseling update time cannot precede creation.");

            var hasConfirmation = ConfirmedAppointmentAtUtc.HasValue;
            var hasAssignment = !string.IsNullOrWhiteSpace(
                AssignedCounselorEmployeeId);
            if (hasAssignment && !hasConfirmation)
                throw new DomainValidationException(
                    "Persisted Counseling assignment requires confirmation metadata.");

            if (Status == CounselingCaseStatus.Requested
                && (hasConfirmation || hasAssignment))
            {
                throw new DomainValidationException(
                    "Requested Counseling Cases cannot retain confirmation or assignment metadata.");
            }
            if (Status == CounselingCaseStatus.Confirmed
                && (!hasConfirmation || hasAssignment))
            {
                throw new DomainValidationException(
                    "Confirmed Counseling metadata contradicts status.");
            }
            if ((Status == CounselingCaseStatus.Assigned
                    || Status == CounselingCaseStatus.Active
                    || Status == CounselingCaseStatus.Closed)
                && (!hasConfirmation || !hasAssignment))
            {
                throw new DomainValidationException(
                    "Assigned, active, or closed Counseling Cases require confirmation and assignment metadata.");
            }

            if ((Status == CounselingCaseStatus.Active
                    || Status == CounselingCaseStatus.Closed)
                && _sessions.Count == 0)
            {
                throw new DomainValidationException(
                    "Active or closed Counseling Cases require confidential sessions.");
            }
            if (Status != CounselingCaseStatus.Active
                && Status != CounselingCaseStatus.Closed
                && _sessions.Count != 0)
            {
                throw new DomainValidationException(
                    "Persisted Counseling Sessions contradict the case status.");
            }

            if (Status == CounselingCaseStatus.Closed)
            {
                if (!ClosedAtUtc.HasValue
                    || string.IsNullOrWhiteSpace(ClosureSummary))
                {
                    throw new DomainValidationException(
                        "Closed Counseling Cases require closure metadata.");
                }
            }
            else if (ClosedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(ClosureSummary))
            {
                throw new DomainValidationException(
                    "Non-closed Counseling Cases cannot retain closure metadata.");
            }

            if (Status == CounselingCaseStatus.Requested
                || Status == CounselingCaseStatus.Confirmed
                || Status == CounselingCaseStatus.Assigned
                || Status == CounselingCaseStatus.Cancelled)
            {
                if (_releasedSummaries.Count != 0)
                    throw new DomainValidationException(
                        "Unstarted or cancelled Counseling Cases cannot retain released summaries.");
            }

            foreach (var session in _sessions)
            {
                if (!StringComparer.Ordinal.Equals(
                    session.CounselorEmployeeId,
                    AssignedCounselorEmployeeId))
                {
                    throw new DomainValidationException(
                        "Persisted Counseling Sessions must belong to the assigned counselor.");
                }
                if (session.OccurredAtUtc < created
                    || session.OccurredAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Counseling Session chronology is invalid.");
                }
            }

            foreach (var summary in _releasedSummaries)
            {
                var session = _sessions.SingleOrDefault(item =>
                    StringComparer.Ordinal.Equals(
                        item.SessionId, summary.SessionId));
                if (session == null)
                    throw new DomainValidationException(
                        "Persisted Counseling summary references an unavailable session.");
                if (summary.ReleasedAtUtc < session.OccurredAtUtc
                    || summary.ReleasedAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Counseling summary chronology is invalid.");
                }
            }

            if (ConfirmedAppointmentAtUtc.HasValue
                && ConfirmedAppointmentAtUtc.Value > updated)
            {
                throw new DomainValidationException(
                    "Persisted Counseling confirmation cannot follow the latest update.");
            }
            if (ClosedAtUtc.HasValue
                && ClosedAtUtc.Value > updated)
            {
                throw new DomainValidationException(
                    "Persisted Counseling closure cannot follow the latest update.");
            }
        }

        private void RecordServiceChange(
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId, "USR", nameof(changedByUserId)));
        }
    }

    public sealed class CounselingReleaseAuthorization : EntityBase
    {
        public CounselingReleaseAuthorization(
            string id,
            string counselingCaseId,
            string studentId,
            string recipient,
            string purpose,
            CounselingReleaseScope scope,
            DateTime validFromUtc,
            DateTime validUntilUtc,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "CRL", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId, "USR", nameof(createdByUserId)))
        {
            CounselingCaseId = ServiceDomainGuard.RequireIdentifier(
                counselingCaseId, "CNS", nameof(counselingCaseId));
            StudentId = ServiceDomainGuard.RequireIdentifier(
                studentId, "STU", nameof(studentId));
            Recipient = ServiceDomainGuard.RequiredText(
                recipient, nameof(recipient), 200);
            Purpose = ServiceDomainGuard.RequiredText(
                purpose, nameof(purpose), 500);
            Scope = ServiceDomainGuard.RequireDefined(scope, nameof(scope));
            ValidFromUtc = ServiceDomainGuard.RequireUtc(
                validFromUtc, nameof(validFromUtc));
            ValidUntilUtc = ServiceDomainGuard.RequireUtc(
                validUntilUtc, nameof(validUntilUtc));
            if (ValidUntilUtc <= ValidFromUtc)
                throw new DomainValidationException(
                    "Counseling release authorization must have a positive validity window.");
            Status = CounselingReleaseStatus.Active;
        }

        public string CounselingCaseId { get; private set; }
        public string StudentId { get; private set; }
        public string Recipient { get; private set; }
        public string Purpose { get; private set; }
        public CounselingReleaseScope Scope { get; private set; }
        public DateTime ValidFromUtc { get; private set; }
        public DateTime ValidUntilUtc { get; private set; }
        public CounselingReleaseStatus Status { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public string RevocationReason { get; private set; }

        public bool Allows(
            CounselingReleaseScope requestedScope,
            DateTime atUtc)
        {
            requestedScope = ServiceDomainGuard.RequireDefined(
                requestedScope, nameof(requestedScope));
            atUtc = ServiceDomainGuard.RequireUtc(atUtc, nameof(atUtc));
            return Status == CounselingReleaseStatus.Active
                && Scope == requestedScope
                && atUtc >= ValidFromUtc
                && atUtc <= ValidUntilUtc;
        }

        public void Revoke(
            string reason,
            DateTime revokedAtUtc,
            string revokedByUserId)
        {
            if (Status != CounselingReleaseStatus.Active)
                throw new DomainValidationException(
                    "Only an active Counseling Release Authorization can be revoked.");
            RevocationReason = ServiceDomainGuard.RequiredText(
                reason, nameof(reason), 500);
            RevokedAtUtc = ServiceDomainGuard.RequireUtc(
                revokedAtUtc, nameof(revokedAtUtc));
            Status = CounselingReleaseStatus.Revoked;
            RecordChange(
                RevokedAtUtc.Value,
                ServiceDomainGuard.RequireIdentifier(
                    revokedByUserId, "USR", nameof(revokedByUserId)));
        }
    }
}
