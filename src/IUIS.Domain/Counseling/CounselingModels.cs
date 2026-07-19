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
                sessionId,
                "CSN",
                nameof(sessionId));
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(
                occurredAtUtc,
                nameof(occurredAtUtc));
            CounselorEmployeeId = ServiceDomainGuard.RequireIdentifier(
                counselorEmployeeId,
                "EMP",
                nameof(counselorEmployeeId));
            RiskLevel = ServiceDomainGuard.RequireDefined(riskLevel, nameof(riskLevel));
            InternalNotes = ServiceDomainGuard.RequiredText(
                internalNotes,
                nameof(internalNotes),
                8000);
        }

        public string SessionId { get; }

        public DateTime OccurredAtUtc { get; }

        public string CounselorEmployeeId { get; }

        public CounselingRiskLevel RiskLevel { get; }

        public string InternalNotes { get; }
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
                summaryId,
                "CSR",
                nameof(summaryId));
            SessionId = ServiceDomainGuard.RequireIdentifier(
                sessionId,
                "CSN",
                nameof(sessionId));
            ReleaseAuthorizationId = ServiceDomainGuard.RequireIdentifier(
                releaseAuthorizationId,
                "CRL",
                nameof(releaseAuthorizationId));
            Summary = ServiceDomainGuard.RequiredText(summary, nameof(summary), 2000);
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                releasedAtUtc,
                nameof(releasedAtUtc));
            ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId,
                "USR",
                nameof(releasedByUserId));
        }

        public string SummaryId { get; }

        public string SessionId { get; }

        public string ReleaseAuthorizationId { get; }

        public string Summary { get; }

        public DateTime ReleasedAtUtc { get; }

        public string ReleasedByUserId { get; }
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
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(
                studentId,
                "STU",
                nameof(studentId));
            RequestedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(
                requestedAppointmentAtUtc,
                nameof(requestedAppointmentAtUtc));
            RequestReason = ServiceDomainGuard.RequiredText(
                requestReason,
                nameof(requestReason),
                500);
            Status = CounselingCaseStatus.Requested;
        }

        public string StudentId { get; }

        public DateTime RequestedAppointmentAtUtc { get; }

        public string RequestReason { get; }

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

        public void ConfirmAppointment(
            DateTime confirmedAppointmentAtUtc,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Requested)
            {
                throw new DomainValidationException(
                    "Only a requested Counseling Case can be confirmed.");
            }

            ConfirmedAppointmentAtUtc = ServiceDomainGuard.RequireUtc(
                confirmedAppointmentAtUtc,
                nameof(confirmedAppointmentAtUtc));
            Status = CounselingCaseStatus.Confirmed;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void AssignCounselor(
            string counselorEmployeeId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != CounselingCaseStatus.Confirmed)
            {
                throw new DomainValidationException(
                    "A Counseling Case must be confirmed before counselor assignment.");
            }

            AssignedCounselorEmployeeId = ServiceDomainGuard.RequireIdentifier(
                counselorEmployeeId,
                "EMP",
                nameof(counselorEmployeeId));
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

            if (_sessions.Any(
                item => StringComparer.Ordinal.Equals(item.SessionId, session.SessionId)))
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
                sessionId,
                "CSN",
                nameof(sessionId));
            if (!_sessions.Any(
                item => StringComparer.Ordinal.Equals(item.SessionId, normalizedSessionId)))
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
            if (_releasedSummaries.Any(
                item => StringComparer.Ordinal.Equals(item.SummaryId, summary.SummaryId)))
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
            {
                throw new DomainValidationException(
                    "Only an active Counseling Case can be closed.");
            }

            if (_sessions.Count == 0)
            {
                throw new DomainValidationException(
                    "A Counseling Case requires a recorded session before closure.");
            }

            ClosureSummary = ServiceDomainGuard.RequiredText(
                closureSummary,
                nameof(closureSummary),
                2000);
            ClosedAtUtc = ServiceDomainGuard.RequireUtc(closedAtUtc, nameof(closedAtUtc));
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
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            CounselingCaseId = ServiceDomainGuard.RequireIdentifier(
                counselingCaseId,
                "CNS",
                nameof(counselingCaseId));
            StudentId = ServiceDomainGuard.RequireIdentifier(
                studentId,
                "STU",
                nameof(studentId));
            Recipient = ServiceDomainGuard.RequiredText(recipient, nameof(recipient), 200);
            Purpose = ServiceDomainGuard.RequiredText(purpose, nameof(purpose), 500);
            Scope = ServiceDomainGuard.RequireDefined(scope, nameof(scope));
            ValidFromUtc = ServiceDomainGuard.RequireUtc(validFromUtc, nameof(validFromUtc));
            ValidUntilUtc = ServiceDomainGuard.RequireUtc(validUntilUtc, nameof(validUntilUtc));
            if (ValidUntilUtc <= ValidFromUtc)
            {
                throw new DomainValidationException(
                    "Counseling release authorization must have a positive validity window.");
            }

            Status = CounselingReleaseStatus.Active;
        }

        public string CounselingCaseId { get; }

        public string StudentId { get; }

        public string Recipient { get; }

        public string Purpose { get; }

        public CounselingReleaseScope Scope { get; }

        public DateTime ValidFromUtc { get; }

        public DateTime ValidUntilUtc { get; }

        public CounselingReleaseStatus Status { get; private set; }

        public DateTime? RevokedAtUtc { get; private set; }

        public string RevocationReason { get; private set; }

        public bool Allows(CounselingReleaseScope requestedScope, DateTime atUtc)
        {
            requestedScope = ServiceDomainGuard.RequireDefined(
                requestedScope,
                nameof(requestedScope));
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
            {
                throw new DomainValidationException(
                    "Only an active Counseling Release Authorization can be revoked.");
            }

            RevocationReason = ServiceDomainGuard.RequiredText(
                reason,
                nameof(reason),
                500);
            RevokedAtUtc = ServiceDomainGuard.RequireUtc(revokedAtUtc, nameof(revokedAtUtc));
            Status = CounselingReleaseStatus.Revoked;
            RecordChange(
                RevokedAtUtc.Value,
                ServiceDomainGuard.RequireIdentifier(
                    revokedByUserId,
                    "USR",
                    nameof(revokedByUserId)));
        }
    }
}
