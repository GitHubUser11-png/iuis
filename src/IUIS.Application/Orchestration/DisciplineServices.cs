using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Discipline;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Application.Orchestration
{
    public sealed class DisciplineReportRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public string StudentId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string Location { get; set; }
        public string InternalIncidentNarrative { get; set; }
    }

    public class DisciplineCaseMutationRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string CaseId { get; set; }
        public string ReleasedDecisionSummary { get; set; }
        public string ResponseText { get; set; }
        public string ResponseEvidenceReference { get; set; }
    }

    public sealed class DisciplineEvidenceRequest : DisciplineCaseMutationRequest
    {
        public string Reference { get; set; }
        public string Description { get; set; }
    }

    public sealed class DisciplineViolationRequest : DisciplineCaseMutationRequest
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public DisciplineSeverity Severity { get; set; }
    }

    public sealed class DisciplineNoticeRequest : DisciplineCaseMutationRequest
    {
        public string ReleasedSummary { get; set; }
        public InstitutionLocalDate ResponseDueDate { get; set; }
    }

    public sealed class DisciplineStudentResponseRequest : DisciplineCaseMutationRequest
    {
        public string EvidenceReference { get; set; }
    }

    public sealed class DisciplineFindingRequest : DisciplineCaseMutationRequest
    {
        public bool Substantiated { get; set; }
        public string InternalFinding { get; set; }
    }

    public sealed class DisciplineDecisionRequest : DisciplineCaseMutationRequest
    {
        public DisciplineDecisionOutcome Outcome { get; set; }
        public string InternalRationale { get; set; }
        public string SanctionSummary { get; set; }
    }

    public sealed class DisciplineReleaseDecisionRequest : DisciplineCaseMutationRequest
    {
    }

    public sealed class DisciplineDismissRequest : DisciplineCaseMutationRequest
    {
        public string InternalReason { get; set; }
    }

    public sealed class RestrictedDisciplineCaseQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IDisciplineCaseRepository _cases;

        public RestrictedDisciplineCaseQueryService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository cases)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _cases = cases ?? throw new ArgumentNullException(nameof(cases));
        }

        public RestrictedDisciplineCaseQueryService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository cases,
            RestrictedProjectionService projections)
            : this(executor, cases)
        {
            if (projections == null) throw new ArgumentNullException(nameof(projections));
        }

        public RestrictedDisciplineCaseViewDto Get(
            string sessionId,
            string sessionToken,
            string caseId,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "discipline.case.restricted.read",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _cases.Read();
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        caseId,
                        "Discipline Case");
                    return new RestrictedDisciplineCaseViewDto
                    {
                        RepositoryRevision = snapshot.Revision,
                        EntityVersion = value.Version,
                        Case = new DisciplineInternalCaseDto
                        {
                            CaseId = value.Id,
                            StudentId = value.StudentId,
                            Status = value.Status.ToString(),
                            InternalIncidentNarrative = value.InternalIncidentNarrative,
                            RestrictedEvidenceReferences = value.RestrictedEvidence
                                .Select(item => item.Reference + " | " + item.Description)
                                .ToList().AsReadOnly(),
                            RestrictedFindings = value.RestrictedFindings
                                .Select(item => item.InternalFinding)
                                .ToList().AsReadOnly(),
                            InternalDecisionRationale = value.Decision == null
                                ? null
                                : value.Decision.InternalRationale
                        }
                    };
                });
        }
    }

    public sealed class DisciplineCaseCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IDisciplineCaseRepository _cases;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public DisciplineCaseCommandService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository cases,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _cases = cases ?? throw new ArgumentNullException(nameof(cases));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public StudentServiceCommandResult Report(
            string sessionId,
            string sessionToken,
            DisciplineReportRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "discipline.case.report",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _cases.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _cases.RepositoryName);
                    var value = new DisciplineCase(
                        _ids.Allocate("DIN", utcNow.Year, principal.UserId),
                        request.StudentId,
                        request.OccurredAtUtc,
                        request.Location,
                        request.InternalIncidentNarrative,
                        principal.UserId,
                        utcNow,
                        principal.UserId);
                    var records = snapshot.Records.ToList();
                    records.Add(value);
                    return Commit(snapshot, records, value, principal.UserId);
                });
        }

        public StudentServiceCommandResult BeginReview(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.case.review",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.BeginReview(
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult AddEvidence(
            string sessionId,
            string sessionToken,
            DisciplineEvidenceRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.evidence.restricted.add",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.AddEvidenceReference(
                    _ids.Allocate("DEV", utcNow.Year, principal.UserId),
                    request.Reference,
                    request.Description,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult OpenViolation(
            string sessionId,
            string sessionToken,
            DisciplineViolationRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.violation.open",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.ConvertToViolation(
                    _ids.Allocate("VIO", utcNow.Year, principal.UserId),
                    request.Code,
                    request.Description,
                    request.Severity,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseNotice(
            string sessionId,
            string sessionToken,
            DisciplineNoticeRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.notice.release",
                ConfidentialityClassification.Internal,
                (value, principal) => value.ReleaseNotice(
                    _ids.Allocate("DNT", utcNow.Year, principal.UserId),
                    request.ReleasedSummary,
                    request.ResponseDueDate,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult SubmitOwnResponse(
            string sessionId,
            string sessionToken,
            DisciplineStudentResponseRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.discipline.response.submit",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var snapshot = _cases.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _cases.RepositoryName);
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.CaseId,
                        "Discipline Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Discipline Case");
                    if (!StringComparer.Ordinal.Equals(
                        value.StudentId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException(
                            "Students may respond only to their own Discipline Case.");
                    }
                    value.RecordStudentResponse(
                        _ids.Allocate("DSR", utcNow.Year, principal.UserId),
                        request.ResponseText,
                        request.EvidenceReference,
                        utcNow,
                        principal.UserId);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        public StudentServiceCommandResult RecordStudentResponse(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return SubmitOwnResponse(
                sessionId,
                sessionToken,
                new DisciplineStudentResponseRequest
                {
                    ExpectedRepositoryRevision = request.ExpectedRepositoryRevision,
                    ExpectedEntityVersion = request.ExpectedEntityVersion,
                    CaseId = request.CaseId,
                    ResponseText = request.ResponseText,
                    EvidenceReference = request.ResponseEvidenceReference
                },
                utcNow);
        }

        public StudentServiceCommandResult RecordFinding(
            string sessionId,
            string sessionToken,
            DisciplineFindingRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.finding.restricted.record",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.RecordFinding(
                    _ids.Allocate("DFN", utcNow.Year, principal.UserId),
                    request.Substantiated,
                    request.InternalFinding,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult PrepareDecision(
            string sessionId,
            string sessionToken,
            DisciplineDecisionRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.decision.restricted.prepare",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.PrepareDecision(
                    _ids.Allocate("DDC", utcNow.Year, principal.UserId),
                    request.Outcome,
                    request.InternalRationale,
                    request.SanctionSummary,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseDecision(
            string sessionId,
            string sessionToken,
            DisciplineReleaseDecisionRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.decision.release",
                ConfidentialityClassification.Internal,
                (value, principal) => value.ReleaseDecision(
                    request.ReleasedDecisionSummary,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseDecision(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return ReleaseDecision(
                sessionId,
                sessionToken,
                new DisciplineReleaseDecisionRequest
                {
                    ExpectedRepositoryRevision = request.ExpectedRepositoryRevision,
                    ExpectedEntityVersion = request.ExpectedEntityVersion,
                    CaseId = request.CaseId,
                    ReleasedDecisionSummary = request.ReleasedDecisionSummary
                },
                utcNow);
        }

        public StudentServiceCommandResult Close(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.case.close",
                ConfidentialityClassification.Internal,
                (value, principal) => value.Close(
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult Dismiss(
            string sessionId,
            string sessionToken,
            DisciplineDismissRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "discipline.case.dismiss",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.Dismiss(
                    request.InternalReason,
                    utcNow,
                    principal.UserId));
        }

        private StudentServiceCommandResult MutateInternal<TRequest>(
            string sessionId,
            string sessionToken,
            TRequest request,
            DateTime utcNow,
            string permission,
            ConfidentialityClassification confidentiality,
            Action<DisciplineCase, AuthorizationPrincipal> mutation)
            where TRequest : DisciplineCaseMutationRequest
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    permission,
                    confidentiality),
                principal =>
                {
                    var snapshot = _cases.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _cases.RepositoryName);
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.CaseId,
                        "Discipline Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Discipline Case");
                    mutation(value, principal);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        private StudentServiceCommandResult Commit(
            RepositorySnapshot<DisciplineCase> snapshot,
            IReadOnlyCollection<DisciplineCase> records,
            DisciplineCase value,
            string actorUserId)
        {
            var transactionId = _transactions.Execute(scope => scope.Stage(
                _cases,
                records,
                snapshot.Revision,
                actorUserId));
            return new StudentServiceCommandResult
            {
                TransactionId = transactionId,
                RecordId = value.Id,
                RepositoryRevision = checked(snapshot.Revision + 1L),
                EntityVersion = value.Version
            };
        }
    }
}
