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
                evidenceId, "DEV", nameof(evidenceId));
            Reference = ServiceDomainGuard.RequiredText(
                reference, nameof(reference), 500);
            Description = ServiceDomainGuard.RequiredText(
                description, nameof(description), 1000);
            AddedAtUtc = ServiceDomainGuard.RequireUtc(
                addedAtUtc, nameof(addedAtUtc));
            AddedByUserId = ServiceDomainGuard.RequireIdentifier(
                addedByUserId, "USR", nameof(addedByUserId));
        }

        public string EvidenceId { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime AddedAtUtc { get; private set; }
        public string AddedByUserId { get; private set; }

        public static DisciplineEvidenceReference Rehydrate(
            string evidenceId,
            string reference,
            string description,
            DateTime addedAtUtc,
            string addedByUserId)
        {
            return new DisciplineEvidenceReference(
                evidenceId,
                reference,
                description,
                addedAtUtc,
                addedByUserId);
        }
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
                violationId, "VIO", nameof(violationId));
            Code = ServiceDomainGuard.RequiredCode(code, nameof(code), 64);
            Description = ServiceDomainGuard.RequiredText(
                description, nameof(description), 1000);
            Severity = ServiceDomainGuard.RequireDefined(
                severity, nameof(severity));
        }

        public string ViolationId { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
        public DisciplineSeverity Severity { get; private set; }

        public static DisciplineViolation Rehydrate(
            string violationId,
            string code,
            string description,
            DisciplineSeverity severity)
        {
            return new DisciplineViolation(
                violationId,
                code,
                description,
                severity);
        }
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
            NoticeId = ServiceDomainGuard.RequireIdentifier(
                noticeId, "DNT", nameof(noticeId));
            ReleasedSummary = ServiceDomainGuard.RequiredText(
                releasedSummary, nameof(releasedSummary), 2000);
            ResponseDueDate = responseDueDate;
            ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                releasedAtUtc, nameof(releasedAtUtc));
            ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId, "USR", nameof(releasedByUserId));
        }

        public string NoticeId { get; private set; }
        public string ReleasedSummary { get; private set; }
        public InstitutionLocalDate ResponseDueDate { get; private set; }
        public DateTime ReleasedAtUtc { get; private set; }
        public string ReleasedByUserId { get; private set; }

        public static DisciplineReleasedNotice Rehydrate(
            string noticeId,
            string releasedSummary,
            InstitutionLocalDate responseDueDate,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            return new DisciplineReleasedNotice(
                noticeId,
                releasedSummary,
                responseDueDate,
                releasedAtUtc,
                releasedByUserId);
        }
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
                responseId, "DSR", nameof(responseId));
            ResponseText = ServiceDomainGuard.RequiredText(
                responseText, nameof(responseText), 4000);
            EvidenceReference = ServiceDomainGuard.OptionalText(
                evidenceReference, nameof(evidenceReference), 500);
            SubmittedAtUtc = ServiceDomainGuard.RequireUtc(
                submittedAtUtc, nameof(submittedAtUtc));
        }

        public string ResponseId { get; private set; }
        public string ResponseText { get; private set; }
        public string EvidenceReference { get; private set; }
        public DateTime SubmittedAtUtc { get; private set; }

        public static DisciplineStudentResponse Rehydrate(
            string responseId,
            string responseText,
            string evidenceReference,
            DateTime submittedAtUtc)
        {
            return new DisciplineStudentResponse(
                responseId,
                responseText,
                evidenceReference,
                submittedAtUtc);
        }
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
                findingId, "DFN", nameof(findingId));
            Substantiated = substantiated;
            InternalFinding = ServiceDomainGuard.RequiredText(
                internalFinding, nameof(internalFinding), 5000);
            RecordedAtUtc = ServiceDomainGuard.RequireUtc(
                recordedAtUtc, nameof(recordedAtUtc));
            RecordedByUserId = ServiceDomainGuard.RequireIdentifier(
                recordedByUserId, "USR", nameof(recordedByUserId));
        }

        public string FindingId { get; private set; }
        public bool Substantiated { get; private set; }
        public string InternalFinding { get; private set; }
        public DateTime RecordedAtUtc { get; private set; }
        public string RecordedByUserId { get; private set; }

        public static DisciplineFinding Rehydrate(
            string findingId,
            bool substantiated,
            string internalFinding,
            DateTime recordedAtUtc,
            string recordedByUserId)
        {
            return new DisciplineFinding(
                findingId,
                substantiated,
                internalFinding,
                recordedAtUtc,
                recordedByUserId);
        }
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
                decisionId, "DDC", nameof(decisionId));
            Outcome = ServiceDomainGuard.RequireDefined(
                outcome, nameof(outcome));
            InternalRationale = ServiceDomainGuard.RequiredText(
                internalRationale, nameof(internalRationale), 5000);
            SanctionSummary = ServiceDomainGuard.OptionalText(
                sanctionSummary, nameof(sanctionSummary), 2000);
            PreparedAtUtc = ServiceDomainGuard.RequireUtc(
                preparedAtUtc, nameof(preparedAtUtc));
            PreparedByUserId = ServiceDomainGuard.RequireIdentifier(
                preparedByUserId, "USR", nameof(preparedByUserId));
        }

        public string DecisionId { get; private set; }
        public DisciplineDecisionOutcome Outcome { get; private set; }
        public string InternalRationale { get; private set; }
        public string SanctionSummary { get; private set; }
        public DateTime PreparedAtUtc { get; private set; }
        public string PreparedByUserId { get; private set; }
        public string ReleasedDecisionSummary { get; internal set; }
        public DateTime? ReleasedAtUtc { get; internal set; }
        public string ReleasedByUserId { get; internal set; }

        public static DisciplineDecision Rehydrate(
            string decisionId,
            DisciplineDecisionOutcome outcome,
            string internalRationale,
            string sanctionSummary,
            DateTime preparedAtUtc,
            string preparedByUserId,
            string releasedDecisionSummary,
            DateTime? releasedAtUtc,
            string releasedByUserId)
        {
            var value = new DisciplineDecision(
                decisionId,
                outcome,
                internalRationale,
                sanctionSummary,
                preparedAtUtc,
                preparedByUserId);
            var hasSummary = !string.IsNullOrWhiteSpace(
                releasedDecisionSummary);
            var hasTime = releasedAtUtc.HasValue;
            var hasActor = !string.IsNullOrWhiteSpace(
                releasedByUserId);
            if (hasSummary || hasTime || hasActor)
            {
                if (!(hasSummary && hasTime && hasActor))
                    throw new DomainValidationException(
                        "Persisted Discipline Decision release metadata must be complete.");
                value.ReleasedDecisionSummary =
                    ServiceDomainGuard.RequiredText(
                        releasedDecisionSummary,
                        nameof(releasedDecisionSummary),
                        2500);
                value.ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                    releasedAtUtc.Value,
                    nameof(releasedAtUtc));
                value.ReleasedByUserId =
                    ServiceDomainGuard.RequireIdentifier(
                        releasedByUserId,
                        "USR",
                        nameof(releasedByUserId));
            }
            return value;
        }
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
                    createdByUserId, "USR", nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(
                studentId, "STU", nameof(studentId));
            OccurredAtUtc = ServiceDomainGuard.RequireUtc(
                occurredAtUtc, nameof(occurredAtUtc));
            Location = ServiceDomainGuard.RequiredText(
                location, nameof(location), 300);
            InternalIncidentNarrative = ServiceDomainGuard.RequiredText(
                internalIncidentNarrative,
                nameof(internalIncidentNarrative),
                6000);
            ReporterUserId = ServiceDomainGuard.RequireIdentifier(
                reporterUserId, "USR", nameof(reporterUserId));
            Status = DisciplineCaseStatus.Reported;
        }

        public string StudentId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string Location { get; private set; }
        public string InternalIncidentNarrative { get; private set; }
        public string ReporterUserId { get; private set; }
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

        public static DisciplineCase Rehydrate(
            string id,
            string studentId,
            DateTime occurredAtUtc,
            string location,
            string internalIncidentNarrative,
            string reporterUserId,
            DisciplineCaseStatus status,
            DisciplineViolation violation,
            DisciplineReleasedNotice releasedNotice,
            DisciplineDecision decision,
            IEnumerable<DisciplineEvidenceReference> restrictedEvidence,
            IEnumerable<DisciplineStudentResponse> studentResponses,
            IEnumerable<DisciplineFinding> restrictedFindings,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            var value = new DisciplineCase(
                id,
                studentId,
                occurredAtUtc,
                location,
                internalIncidentNarrative,
                reporterUserId,
                createdAtUtc,
                createdByUserId);
            value.Status = ServiceDomainGuard.RequireDefined(
                status, nameof(status));
            value.Violation = violation;
            value.ReleasedNotice = releasedNotice;
            value.Decision = decision;

            var evidence = restrictedEvidence == null
                ? new List<DisciplineEvidenceReference>()
                : restrictedEvidence.ToList();
            var responses = studentResponses == null
                ? new List<DisciplineStudentResponse>()
                : studentResponses.ToList();
            var findings = restrictedFindings == null
                ? new List<DisciplineFinding>()
                : restrictedFindings.ToList();

            if (evidence.Any(item => item == null)
                || responses.Any(item => item == null)
                || findings.Any(item => item == null))
            {
                throw new DomainValidationException(
                    "Persisted Discipline collections cannot contain null records.");
            }

            RequireUnique(
                evidence.Select(item => item.EvidenceId),
                evidence.Count,
                "Evidence");
            RequireUnique(
                responses.Select(item => item.ResponseId),
                responses.Count,
                "Student Response");
            RequireUnique(
                findings.Select(item => item.FindingId),
                findings.Count,
                "Finding");

            value._evidence.AddRange(evidence);
            value._responses.AddRange(responses);
            value._findings.AddRange(findings);
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

        public void BeginReview(
            DateTime changedAtUtc,
            string changedByUserId)
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
            if (_evidence.Any(item => StringComparer.Ordinal.Equals(
                item.EvidenceId, evidence.EvidenceId)))
            {
                throw new DomainValidationException(
                    "The Discipline Case already contains the Evidence ID.");
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
            RequireStatus(
                DisciplineCaseStatus.UnderReview,
                "open a Violation");
            Violation = new DisciplineViolation(
                violationId, code, description, severity);
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
            RequireStatus(
                DisciplineCaseStatus.ViolationOpened,
                "release a notice");
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
            RequireStatus(
                DisciplineCaseStatus.NoticeReleased,
                "record a Student response");
            var response = new DisciplineStudentResponse(
                responseId,
                responseText,
                evidenceReference,
                submittedAtUtc);
            if (_responses.Any(item => StringComparer.Ordinal.Equals(
                item.ResponseId, response.ResponseId)))
            {
                throw new DomainValidationException(
                    "The Discipline Case already contains the Response ID.");
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
            RequireStatus(
                DisciplineCaseStatus.NoticeReleased,
                "record a finding");
            var finding = new DisciplineFinding(
                findingId,
                substantiated,
                internalFinding,
                recordedAtUtc,
                recordedByUserId);
            if (_findings.Any(item => StringComparer.Ordinal.Equals(
                item.FindingId, finding.FindingId)))
            {
                throw new DomainValidationException(
                    "The Discipline Case already contains the Finding ID.");
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
            RequireStatus(
                DisciplineCaseStatus.NoticeReleased,
                "prepare a decision");
            if (_findings.Count == 0)
                throw new DomainValidationException(
                    "A Discipline decision requires at least one recorded finding.");

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
            RequireStatus(
                DisciplineCaseStatus.DecisionPrepared,
                "release a decision");
            Decision.ReleasedDecisionSummary = ServiceDomainGuard.RequiredText(
                releasedDecisionSummary,
                nameof(releasedDecisionSummary),
                2500);
            Decision.ReleasedAtUtc = ServiceDomainGuard.RequireUtc(
                releasedAtUtc, nameof(releasedAtUtc));
            Decision.ReleasedByUserId = ServiceDomainGuard.RequireIdentifier(
                releasedByUserId, "USR", nameof(releasedByUserId));
            Status = DisciplineCaseStatus.DecisionReleased;
            RecordChange(
                Decision.ReleasedAtUtc.Value,
                Decision.ReleasedByUserId);
        }

        public void Close(
            DateTime closedAtUtc,
            string changedByUserId)
        {
            RequireStatus(
                DisciplineCaseStatus.DecisionReleased,
                "close the case");
            Status = DisciplineCaseStatus.Closed;
            RecordServiceChange(closedAtUtc, changedByUserId);
        }

        public void Dismiss(
            string internalReason,
            DateTime dismissedAtUtc,
            string changedByUserId)
        {
            RequireStatus(
                DisciplineCaseStatus.UnderReview,
                "dismiss the case");
            ServiceDomainGuard.RequiredText(
                internalReason, nameof(internalReason), 2000);
            Status = DisciplineCaseStatus.Dismissed;
            RecordServiceChange(dismissedAtUtc, changedByUserId);
        }

        private static void RequireUnique(
            IEnumerable<string> values,
            int expectedCount,
            string label)
        {
            if (values.Distinct(StringComparer.Ordinal).Count()
                != expectedCount)
            {
                throw new DomainValidationException(
                    "Persisted Discipline " + label
                    + " records require unique identifiers.");
            }
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
                    "Persisted Discipline update time cannot precede creation.");
            if (OccurredAtUtc > created)
                throw new DomainValidationException(
                    "A persisted Discipline incident cannot occur after case creation.");

            var violationRequired =
                Status == DisciplineCaseStatus.ViolationOpened
                || Status == DisciplineCaseStatus.NoticeReleased
                || Status == DisciplineCaseStatus.DecisionPrepared
                || Status == DisciplineCaseStatus.DecisionReleased
                || Status == DisciplineCaseStatus.Closed;
            var noticeRequired =
                Status == DisciplineCaseStatus.NoticeReleased
                || Status == DisciplineCaseStatus.DecisionPrepared
                || Status == DisciplineCaseStatus.DecisionReleased
                || Status == DisciplineCaseStatus.Closed;
            var decisionRequired =
                Status == DisciplineCaseStatus.DecisionPrepared
                || Status == DisciplineCaseStatus.DecisionReleased
                || Status == DisciplineCaseStatus.Closed;
            var decisionReleased =
                Status == DisciplineCaseStatus.DecisionReleased
                || Status == DisciplineCaseStatus.Closed;

            if (violationRequired != (Violation != null))
                throw new DomainValidationException(
                    "Persisted Discipline Violation metadata contradicts status.");
            if (noticeRequired != (ReleasedNotice != null))
                throw new DomainValidationException(
                    "Persisted Discipline Notice metadata contradicts status.");
            if (decisionRequired != (Decision != null))
                throw new DomainValidationException(
                    "Persisted Discipline Decision metadata contradicts status.");

            if (Decision != null)
            {
                var hasReleasedSummary =
                    !string.IsNullOrWhiteSpace(
                        Decision.ReleasedDecisionSummary);
                var hasReleasedAt = Decision.ReleasedAtUtc.HasValue;
                var hasReleasedBy =
                    !string.IsNullOrWhiteSpace(
                        Decision.ReleasedByUserId);
                if (decisionReleased
                    != (hasReleasedSummary && hasReleasedAt && hasReleasedBy))
                {
                    throw new DomainValidationException(
                        "Persisted Discipline released Decision metadata contradicts status.");
                }
                if (!decisionReleased
                    && (hasReleasedSummary || hasReleasedAt || hasReleasedBy))
                {
                    throw new DomainValidationException(
                        "An unreleased Discipline Decision cannot retain release metadata.");
                }
                if (_findings.Count == 0)
                    throw new DomainValidationException(
                        "Persisted Discipline Decisions require findings.");
            }

            if (Status == DisciplineCaseStatus.Reported
                && (_evidence.Count != 0
                    || _responses.Count != 0
                    || _findings.Count != 0))
            {
                throw new DomainValidationException(
                    "Reported Discipline Cases cannot retain investigation records.");
            }

            if ((Status == DisciplineCaseStatus.UnderReview
                    || Status == DisciplineCaseStatus.ViolationOpened
                    || Status == DisciplineCaseStatus.Dismissed)
                && (_responses.Count != 0 || _findings.Count != 0))
            {
                throw new DomainValidationException(
                    "Discipline responses and findings require a released Notice.");
            }

            if (Status == DisciplineCaseStatus.Dismissed
                && (Violation != null
                    || ReleasedNotice != null
                    || Decision != null))
            {
                throw new DomainValidationException(
                    "Dismissed Discipline Cases cannot retain Violation or Decision state.");
            }

            foreach (var item in _evidence)
            {
                if (item.AddedAtUtc < created
                    || item.AddedAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Discipline Evidence chronology is invalid.");
                }
            }
            foreach (var item in _responses)
            {
                if (item.SubmittedAtUtc < created
                    || item.SubmittedAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Discipline Response chronology is invalid.");
                }
            }
            foreach (var item in _findings)
            {
                if (item.RecordedAtUtc < created
                    || item.RecordedAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Discipline Finding chronology is invalid.");
                }
            }

            if (ReleasedNotice != null
                && (ReleasedNotice.ReleasedAtUtc < created
                    || ReleasedNotice.ReleasedAtUtc > updated))
            {
                throw new DomainValidationException(
                    "Persisted Discipline Notice chronology is invalid.");
            }

            if (Decision != null)
            {
                if (Decision.PreparedAtUtc < created
                    || Decision.PreparedAtUtc > updated)
                {
                    throw new DomainValidationException(
                        "Persisted Discipline Decision chronology is invalid.");
                }
                if (Decision.ReleasedAtUtc.HasValue
                    && (Decision.ReleasedAtUtc.Value
                        < Decision.PreparedAtUtc
                        || Decision.ReleasedAtUtc.Value > updated))
                {
                    throw new DomainValidationException(
                        "Persisted Discipline Decision release chronology is invalid.");
                }
            }
        }

        private void RequireStatus(
            DisciplineCaseStatus expected,
            string operation)
        {
            if (Status != expected)
                throw new DomainValidationException(
                    "The Discipline Case cannot " + operation
                    + " from its current status.");
        }

        private void RecordServiceChange(
            DateTime changedAtUtc,
            string changedByUserId)
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
