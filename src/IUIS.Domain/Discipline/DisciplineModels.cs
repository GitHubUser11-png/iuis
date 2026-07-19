using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Services;
using IUIS.Domain.Time;

namespace IUIS.Domain.Discipline
{
    public enum DisciplineCaseStatus
    {
        Reported = 0,
        UnderReview = 1,
        ViolationOpened = 2,
        NoticeReleased = 3,
        DecisionPrepared = 4,
        DecisionReleased = 5,
        Closed = 6,
        Dismissed = 7
    }

    public enum DisciplineSeverity
    {
        Minor = 0,
        Moderate = 1,
        Major = 2,
        Critical = 3
    }

    public enum DisciplineDecisionOutcome
    {
        NotResponsible = 0,
        Warning = 1,
        CorrectiveAction = 2,
        SuspensionRecommendation = 3,
        ExpulsionRecommendation = 4
    }

    public sealed class DisciplineEvidenceReference
    {
        internal DisciplineEvidenceReference(
            string evidenceId,
            string reference,
            string description,
            DateTime addedAtUtc,
            string addedByUserId)
        {
            EvidenceId = ServiceDomainGuard.RequireIdentifier(
                evidenceId,
                "DEV",
                nameof(evidenceId));
            Reference = ServiceDomainGuard.RequiredText(reference, nameof(reference), 500);
            Description = ServiceDomainGuard.RequiredText(
                description,
                nameof(description),
                1000);
            AddedAtUtc = ServiceDomainGuard.RequireUtc(addedAtUtc, nameof(addedAtUtc));
            AddedByUserId = ServiceDomainGuard.RequireIdentifier(
                addedByUserId,
                "USR",
                nameof(addedByUserId));
        }

        public string EvidenceId { get; }
        public string Reference { get; }
        public string Description { get; }
        public DateTime AddedAtUtc { get; }
        public string AddedByUserId { get; }
    }

    public sealed class DisciplineViolation
    {
        internal DisciplineViolation(
            string violationId,
            string code,
            string description,
            DisciplineSeverity severity)
        {
            ViolationId = ServiceDomainGuard.RequireIdentifier(
                violationId,
                "VIO",
                nameof(violationId));
            Code = ServiceDomainGuard.RequiredCode(code, nameof(code), 64);
            Description = ServiceDomainGuard.RequiredText(
                description,
                nameof(description),
                1000);
            Severity = ServiceDomainGuard.RequireDefined(severity, nameof(severity));
        }

        public string ViolationId { get; }
        public string Code { get; }
        public string Description { get; }
        public DisciplineSeverity Severity { get; }
    }

    public sealed class DisciplineReleasedNotice
    {
        internal DisciplineReleasedNotice(
            string noticeId,
            string releasedSummary,
            InstitutionLocalDate responseDueDate,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            NoticeId = ServiceDomainGuard.RequireIdentifier(noticeId, "DNT", nameof(noticeId));
            ReleasedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary,
                nameof(releasedSummary),
                2000);
            ResponseDueDate = responseDueDate;
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(releasedAtUtc, nameof(releasedAtUtc));
            ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId,
                "USR",
                nameof(releasedByUserId));
        }

        public string NoticeId { get; }
        public string ReleasedSummary { get; }
        public InstitutionLocalDate ResponseDueDate { get; }
        public DateTime ReleasedAtUtc { get; }
        public string ReleasedByUserId { get; }
    }

    public sealed class DisciplineStudentResponse
    {
        internal DisciplineStudentResponse(
            string responseId,
            string responseText,
            string evidenceReference,
            DateTime submittedAtUtc)
        {
            ResponseId = ServiceDomainGuard.RequireIdentifier(
                responseId,
                "DSR",
                nameof(responseId));
            ResponseText = ServiceDomainGuard.RequiredText(
                responseText,
                nameof(responseText),
                4000);
            EvidenceReference = ServiceDomainGuard.OptionalText(
                evidenceReference,
                nameof(evidenceReference),
                500);
            SubmittedAtUtc = ServiceDomainGuard.RequireUtc(
                submittedAtUtc,
                nameof(submittedAtUtc));
        }

        public string ResponseId { get; }
        public string ResponseText { get; }
        public string EvidenceReference { get; }
        public DateTime SubmittedAtUtc { get; }
    }

    public sealed class DisciplineFinding
    {
        internal DisciplineFinding(
            string findingId,
            bool substantiated,
            string internalFinding,
            DateTime recordedAtUtc,
            string recordedByUserId)
        {
            FindingId = ServiceDomainGuard.RequireIdentifier(
                findingId,
                "DFN",
                nameof(findingId));
            Substantiated = substantiated;
            InternalFinding = ServiceDomainGuard.RequiredText(
                internalFinding,
                nameof(internalFinding),
                5000);
            RecordedAtUtc = ServiceDomainGuard.RequireUtc(
                recordedAtUtc,
                nameof(recordedAtUtc));
            RecordedByUserId = ServiceDomainGuard.RequireIdentifier(
                recordedByUserId,
                "USR",
                nameof(recordedByUserId));
        }

        public string FindingId { get; }
        public bool Substantiated { get; }
        public string InternalFinding { get; }
        public DateTime RecordedAtUtc { get; }
        public string RecordedByUserId { get; }
    }

    public sealed class DisciplineDecision
    {
        internal DisciplineDecision(
            string decisionId,
            DisciplineDecisionOutcome outcome,
            string internalRationale,
            string sanctionSummary,
            DateTime preparedAtUtc,
            string preparedByUserId)
        {
            DecisionId = ServiceDomainGuard.RequireIdentifier(
                decisionId,
                "DDC",
                nameof(decisionId));
            Outcome = ServiceDomainGuard.RequireDefined(outcome, nameof(outcome));
            InternalRationale = ServiceDomainGuard.RequiredText(
                internalRationale,
                nameof(internalRationale),
                5000);
            SanctionSummary = ServiceDomainGuard.OptionalText(
                sanctionSummary,
                nameof(sanctionSummary),
                2000);
            PreparedAtUtc = ServiceDomainGuard.RequireUtc(
                preparedAtUtc,
                nameof(preparedAtUtc));
            PreparedByUserId = ServiceDomainGuard.RequireIdentifier(
                preparedByUserId,
                "USR",
                nameof(preparedByUserId));
        }

        public string DecisionId { get; }
        public DisciplineDecisionOutcome Outcome { get; }
        public string InternalRationale { get; }
        public string SanctionSummary { get; }
        public DateTime PreparedAtUtc { get; }
        public string PreparedByUserId { get; }
        public string ReleasedDecisionSummary { get; internal set; }
        public DateTime? ReleasedAtUtc { get; internal set; }
        public string ReleasedByUserId { get; internal set; }
    }

    public sealed class DisciplineCase : EntityBase
    {
        private readonly List<DisciplineEvidenceReference> _evidence =
            new List<DisciplineEvidenceReference>();
        private readonly List<DisciplineStudentResponse> _responses =
            new List<DisciplineStudentResponse>();
        private readonly List<DisciplineFinding> _findings =
            new List<DisciplineFinding>();

        public DisciplineCase(
            string id,
            string studentId,
            DateTime occurredAtUtc,
            string location,
            string internalIncidentNarrative,
            string reporterUserId,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "DIN", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
            Location = ServiceDomainGuard.RequiredText(location, nameof(location), 300);
            InternalIncidentNarrative = ServiceDomainGuard.RequiredText(
                internalIncidentNarrative,
                nameof(internalIncidentNarrative),
                6000);
            ReporterUserId = ServiceDomainGuard.RequireIdentifier(
                reporterUserId,
                "USR",
                nameof(reporterUserId));
            Status = DisciplineCaseStatus.Reported;
        }

        public string StudentId { get; }
        public DateTime OccurredAtUtc { get; }
        public string Location { get; }
        public string InternalIncidentNarrative { get; }
        public string ReporterUserId { get; }
        public DisciplineCaseStatus Status { get; private set; }
        public DisciplineViolation Violation { get; private set; }
        public DisciplineReleasedNotice ReleasedNotice { get; private set; }
        public DisciplineDecision Decision { get; private set; }

        public IReadOnlyList<DisciplineEvidenceReference> RestrictedEvidence
        {
            get { return _evidence.AsReadOnly(); }
        }

        public IReadOnlyList<DisciplineStudentResponse> StudentResponses
        {
            get { return _responses.AsReadOnly(); }
        }

        public IReadOnlyList<DisciplineFinding> RestrictedFindings
        {
            get { return _findings.AsReadOnly(); }
        }

        public void BeginReview(DateTime changedAtUtc, string changedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.Reported, "begin review");
            Status = DisciplineCaseStatus.UnderReview;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void AddEvidenceReference(
            string evidenceId,
            string reference,
            string description,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != DisciplineCaseStatus.UnderReview
                && Status != DisciplineCaseStatus.ViolationOpened
                && Status != DisciplineCaseStatus.NoticeReleased)
            {
                throw new DomainValidationException(
                    "Evidence can be added only during an active Discipline investigation.");
            }

            var evidence = new DisciplineEvidenceReference(
                evidenceId,
                reference,
                description,
                changedAtUtc,
                changedByUserId);
            if (_evidence.Any(item => StringComparer.Ordinal.Equals(item.EvidenceId, evidence.EvidenceId)))
            {
                throw new DomainValidationException("The Discipline Case already contains the Evidence ID.");
            }

            _evidence.Add(evidence);
            RecordChange(changedAtUtc, evidence.AddedByUserId);
        }

        public void ConvertToViolation(
            string violationId,
            string code,
            string description,
            DisciplineSeverity severity,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.UnderReview, "open a Violation");
            Violation = new DisciplineViolation(violationId, code, description, severity);
            Status = DisciplineCaseStatus.ViolationOpened;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void ReleaseNotice(
            string noticeId,
            string releasedSummary,
            InstitutionLocalDate responseDueDate,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.ViolationOpened, "release a notice");
            ReleasedNotice = new DisciplineReleasedNotice(
                noticeId,
                releasedSummary,
                responseDueDate,
                releasedAtUtc,
                releasedByUserId);
            Status = DisciplineCaseStatus.NoticeReleased;
            RecordChange(releasedAtUtc, ReleasedNotice.ReleasedByUserId);
        }

        public void RecordStudentResponse(
            string responseId,
            string responseText,
            string evidenceReference,
            DateTime submittedAtUtc,
            string changedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.NoticeReleased, "record a Student response");
            var response = new DisciplineStudentResponse(
                responseId,
                responseText,
                evidenceReference,
                submittedAtUtc);
            if (_responses.Any(item => StringComparer.Ordinal.Equals(item.ResponseId, response.ResponseId)))
            {
                throw new DomainValidationException("The Discipline Case already contains the Response ID.");
            }

            _responses.Add(response);
            RecordServiceChange(submittedAtUtc, changedByUserId);
        }

        public void RecordFinding(
            string findingId,
            bool substantiated,
            string internalFinding,
            DateTime recordedAtUtc,
            string recordedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.NoticeReleased, "record a finding");
            var finding = new DisciplineFinding(
                findingId,
                substantiated,
                internalFinding,
                recordedAtUtc,
                recordedByUserId);
            if (_findings.Any(item => StringComparer.Ordinal.Equals(item.FindingId, finding.FindingId)))
            {
                throw new DomainValidationException("The Discipline Case already contains the Finding ID.");
            }

            _findings.Add(finding);
            RecordChange(recordedAtUtc, finding.RecordedByUserId);
        }

        public void PrepareDecision(
            string decisionId,
            DisciplineDecisionOutcome outcome,
            string internalRationale,
            string sanctionSummary,
            DateTime preparedAtUtc,
            string preparedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.NoticeReleased, "prepare a decision");
            if (_findings.Count == 0)
            {
                throw new DomainValidationException(
                    "A Discipline decision requires at least one recorded finding.");
            }

            Decision = new DisciplineDecision(
                decisionId,
                outcome,
                internalRationale,
                sanctionSummary,
                preparedAtUtc,
                preparedByUserId);
            Status = DisciplineCaseStatus.DecisionPrepared;
            RecordChange(preparedAtUtc, Decision.PreparedByUserId);
        }

        public void ReleaseDecision(
            string releasedDecisionSummary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.DecisionPrepared, "release a decision");
            Decision.ReleasedDecisionSummary = ServiceDomainGuard.RequiredText(
                releasedDecisionSummary,
                nameof(releasedDecisionSummary),
                2500);
            Decision.ReleasedAtUtc = ServiceDomainGuard.RequireUtc(releasedAtUtc, nameof(releasedAtUtc));
            Decision.ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId,
                "USR",
                nameof(releasedByUserId));
            Status = DisciplineCaseStatus.DecisionReleased;
            RecordChange(Decision.ReleasedAtUtc.Value, Decision.ReleasedByUserId);
        }

        public void Close(DateTime closedAtUtc, string changedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.DecisionReleased, "close the case");
            Status = DisciplineCaseStatus.Closed;
            RecordServiceChange(closedAtUtc, changedByUserId);
        }

        public void Dismiss(
            string internalReason,
            DateTime dismissedAtUtc,
            string changedByUserId)
        {
            RequireStatus(DisciplineCaseStatus.UnderReview, "dismiss the case");
            ServiceDomainGuard.RequiredText(internalReason, nameof(internalReason), 2000);
            Status = DisciplineCaseStatus.Dismissed;
            RecordServiceChange(dismissedAtUtc, changedByUserId);
        }

        private void RequireStatus(DisciplineCaseStatus expected, string operation)
        {
            if (Status != expected)
            {
                throw new DomainValidationException(
                    "The Discipline Case cannot " + operation + " from its current status.");
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
}
