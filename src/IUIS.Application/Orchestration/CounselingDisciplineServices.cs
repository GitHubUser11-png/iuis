using System;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Application.Orchestration
{
    public sealed class CounselingCaseRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string RequestReason { get; set; }
    }

    public sealed class CounselingCaseMutationRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string CaseId { get; set; }
        public DateTime ConfirmedAppointmentAtUtc { get; set; }
        public string CounselorEmployeeId { get; set; }
        public DateTime SessionOccurredAtUtc { get; set; }
        public CounselingRiskLevel RiskLevel { get; set; }
        public string InternalNotes { get; set; }
        public string SessionId { get; set; }
        public string ReleaseAuthorizationId { get; set; }
        public string ReleasedSummary { get; set; }
        public string ClosureSummary { get; set; }
        public string CancellationReason { get; set; }
    }

    public sealed class DisciplineReportRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public string StudentId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string Location { get; set; }
        public string InternalIncidentNarrative { get; set; }
    }

    public sealed class DisciplineCaseMutationRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string CaseId { get; set; }
        public string EvidenceReference { get; set; }
        public string EvidenceDescription { get; set; }
        public string ViolationCode { get; set; }
        public string ViolationDescription { get; set; }
        public DisciplineSeverity Severity { get; set; }
        public string ReleasedNoticeSummary { get; set; }
        public InstitutionLocalDate ResponseDueDate { get; set; }
        public string ResponseText { get; set; }
        public string ResponseEvidenceReference { get; set; }
        public bool FindingSubstantiated { get; set; }
        public string InternalFinding { get; set; }
        public DisciplineDecisionOutcome DecisionOutcome { get; set; }
        public string InternalRationale { get; set; }
        public string SanctionSummary { get; set; }
        public string ReleasedDecisionSummary { get; set; }
        public string DismissalReason { get; set; }
    }

    public sealed class DisciplineCounselingCoordinationRequest
    {
        public long ExpectedDisciplineRepositoryRevision { get; set; }
        public long ExpectedCounselingRepositoryRevision { get; set; }
        public long ExpectedDisciplineEntityVersion { get; set; }
        public string DisciplineCaseId { get; set; }
        public string ReleasedDecisionSummary { get; set; }
        public DateTime RequestedCounselingAppointmentAtUtc { get; set; }
        public string CounselingRequestReason { get; set; }
    }

    public sealed class StudentCounselingDisciplineQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _counseling;
        private readonly IDisciplineCaseRepository _discipline;
        private readonly RestrictedProjectionService _projections;

        public StudentCounselingDisciplineQueryService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository counseling,
            IDisciplineCaseRepository discipline,
            RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _counseling = counseling ?? throw new ArgumentNullException(nameof(counseling));
            _discipline = discipline ?? throw new ArgumentNullException(nameof(discipline));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public StudentCounselingDisciplineOverviewDto GetOwnOverview(
            string sessionId,
            string sessionToken,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.services.released.read",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var counseling = _counseling.Read();
                    var discipline = _discipline.Read();
                    var counselingItems = counseling.Records
                        .Where(item => !item.IsArchived)
                        .Where(item => StringComparer.Ordinal.Equals(
                            item.StudentId,
                            principal.PersonRecordId))
                        .OrderByDescending(item => item.UpdatedAtUtc)
                        .Select(_projections.ToCounselingReleased)
                        .ToList();
                    var disciplineItems = discipline.Records
                        .Where(item => !item.IsArchived)
                        .Where(item => StringComparer.Ordinal.Equals(
                            item.StudentId,
                            principal.PersonRecordId))
                        .Where(item => item.ReleasedNotice != null
                            || item.Decision != null)
                        .OrderByDescending(item => item.UpdatedAtUtc)
                        .Select(_projections.ToDisciplineReleased)
                        .ToList();
                    return new StudentCounselingDisciplineOverviewDto
                    {
                        StudentId = principal.PersonRecordId,
                        CounselingRepositoryRevision = counseling.Revision,
                        DisciplineRepositoryRevision = discipline.Revision,
                        CounselingCases = counselingItems.AsReadOnly(),
                        DisciplineCases = disciplineItems.AsReadOnly()
                    };
                });
        }
    }

    public sealed class RestrictedCounselingCaseQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _repository;
        private readonly RestrictedProjectionService _projections;

        public RestrictedCounselingCaseQueryService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository repository,
            RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public RestrictedCounselingCaseViewDto Get(
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
                    "counseling.case.restricted.read",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _repository.Read();
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        caseId,
                        "Counseling Case");
                    return new RestrictedCounselingCaseViewDto
                    {
                        RepositoryRevision = snapshot.Revision,
                        EntityVersion = value.Version,
                        Case = _projections.ToCounselingInternal(value)
                    };
                });
        }
    }

    public sealed class RestrictedDisciplineCaseQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IDisciplineCaseRepository _repository;
        private readonly RestrictedProjectionService _projections;

        public RestrictedDisciplineCaseQueryService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository repository,
            RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
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
                    var snapshot = _repository.Read();
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        caseId,
                        "Discipline Case");
                    return new RestrictedDisciplineCaseViewDto
                    {
                        RepositoryRevision = snapshot.Revision,
                        EntityVersion = value.Version,
                        Case = _projections.ToDisciplineInternal(value)
                    };
                });
        }
    }

    public sealed class CounselingCaseCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _repository;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public CounselingCaseCommandService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository repository,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public StudentServiceCommandResult Request(
            string sessionId,
            string sessionToken,
            CounselingCaseRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.counseling.request",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var snapshot = _repository.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _repository.RepositoryName);
                    var value = new CounselingCase(
                        _ids.Allocate("CNS", utcNow.Year, principal.UserId),
                        principal.PersonRecordId,
                        request.RequestedAppointmentAtUtc,
                        request.RequestReason,
                        utcNow,
                        principal.UserId);
                    var records = snapshot.Records.ToList();
                    records.Add(value);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _repository,
                            records,
                            snapshot.Revision,
                            principal.UserId));
                    return Result(
                        transactionId,
                        value,
                        checked(snapshot.Revision + 1L));
                });
        }

        public StudentServiceCommandResult ConfirmAndAssign(
            string sessionId,
            string sessionToken,
            CounselingCaseMutationRequest request,
            DateTime utcNow)
        {
            return Mutate(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.case.manage",
                ConfidentialityClassification.Internal,
                (value, principal) =>
                {
                    value.ConfirmAppointment(
                        request.ConfirmedAppointmentAtUtc,
                        utcNow,
                        principal.UserId);
                    value.AssignCounselor(
                        request.CounselorEmployeeId,
                        utcNow,
                        principal.UserId);
                });
        }

        public StudentServiceCommandResult RecordSession(
            string sessionId,
            string sessionToken,
            CounselingCaseMutationRequest request,
            DateTime utcNow)
        {
            return Mutate(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.session.restricted.record",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.RecordSession(
                    _ids.Allocate("CSN", utcNow.Year, principal.UserId),
                    request.SessionOccurredAtUtc,
                    request.RiskLevel,
                    request.InternalNotes,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseSummary(
            string sessionId,
            string sessionToken,
            CounselingCaseMutationRequest request,
            DateTime utcNow)
        {
            return Mutate(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.summary.release",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.ReleaseSessionSummary(
                    _ids.Allocate("CSR", utcNow.Year, principal.UserId),
                    request.SessionId,
                    request.ReleaseAuthorizationId,
                    request.ReleasedSummary,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult Close(
            string sessionId,
            string sessionToken,
            CounselingCaseMutationRequest request,
            DateTime utcNow)
        {
            return Mutate(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.case.manage",
                ConfidentialityClassification.Internal,
                (value, principal) => value.Close(
                    request.ClosureSummary,
                    utcNow,
                    principal.UserId));
        }

        private StudentServiceCommandResult Mutate(
            string sessionId,
            string sessionToken,
            CounselingCaseMutationRequest request,
            DateTime utcNow,
            string permission,
            ConfidentialityClassification confidentiality,
            Action<CounselingCase, AuthorizationPrincipal> mutation)
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
                    var snapshot = _repository.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _repository.RepositoryName);
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.CaseId,
                        "Counseling Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Counseling Case");
                    mutation(value, principal);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _repository,
                            snapshot.Records,
                            snapshot.Revision,
                            principal.UserId));
                    return Result(
                        transactionId,
                        value,
                        checked(snapshot.Revision + 1L));
                });
        }

        private static StudentServiceCommandResult Result(
            string transactionId,
            CounselingCase value,
            long repositoryRevision)
        {
            return new StudentServiceCommandResult
            {
                TransactionId = transactionId,
                RecordId = value.Id,
                RepositoryRevision = repositoryRevision,
                EntityVersion = value.Version
            };
        }
    }

    public sealed class DisciplineCaseCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IDisciplineCaseRepository _repository;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public DisciplineCaseCommandService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository repository,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
                    var snapshot = _repository.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _repository.RepositoryName);
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
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _repository,
                            records,
                            snapshot.Revision,
                            principal.UserId));
                    return Result(
                        transactionId,
                        value,
                        checked(snapshot.Revision + 1L));
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
                "discipline.case.manage",
                (value, principal) => value.BeginReview(
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult AddEvidence(
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
                "discipline.evidence.restricted.record",
                (value, principal) => value.AddEvidenceReference(
                    _ids.Allocate("DEV", utcNow.Year, principal.UserId),
                    request.EvidenceReference,
                    request.EvidenceDescription,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult OpenViolation(
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
                "discipline.case.manage",
                (value, principal) => value.ConvertToViolation(
                    _ids.Allocate("VIO", utcNow.Year, principal.UserId),
                    request.ViolationCode,
                    request.ViolationDescription,
                    request.Severity,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseNotice(
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
                "discipline.notice.release",
                (value, principal) => value.ReleaseNotice(
                    _ids.Allocate("DNT", utcNow.Year, principal.UserId),
                    request.ReleasedNoticeSummary,
                    request.ResponseDueDate,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult RecordStudentResponse(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
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
                    var snapshot = _repository.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _repository.RepositoryName);
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.CaseId,
                        "Discipline Case");
                    if (!StringComparer.Ordinal.Equals(
                        value.StudentId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException(
                            "record-ownership-mismatch");
                    }
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Discipline Case");
                    value.RecordStudentResponse(
                        _ids.Allocate("DSR", utcNow.Year, principal.UserId),
                        request.ResponseText,
                        request.ResponseEvidenceReference,
                        utcNow,
                        principal.UserId);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _repository,
                            snapshot.Records,
                            snapshot.Revision,
                            principal.UserId));
                    return Result(
                        transactionId,
                        value,
                        checked(snapshot.Revision + 1L));
                });
        }

        public StudentServiceCommandResult RecordFinding(
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
                "discipline.finding.restricted.record",
                (value, principal) => value.RecordFinding(
                    _ids.Allocate("DFN", utcNow.Year, principal.UserId),
                    request.FindingSubstantiated,
                    request.InternalFinding,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult PrepareDecision(
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
                "discipline.decision.restricted.prepare",
                (value, principal) => value.PrepareDecision(
                    _ids.Allocate("DDC", utcNow.Year, principal.UserId),
                    request.DecisionOutcome,
                    request.InternalRationale,
                    request.SanctionSummary,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult ReleaseDecision(
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
                "discipline.decision.release",
                (value, principal) => value.ReleaseDecision(
                    request.ReleasedDecisionSummary,
                    utcNow,
                    principal.UserId));
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
                "discipline.case.manage",
                (value, principal) => value.Close(
                    utcNow,
                    principal.UserId));
        }

        private StudentServiceCommandResult MutateInternal(
            string sessionId,
            string sessionToken,
            DisciplineCaseMutationRequest request,
            DateTime utcNow,
            string permission,
            Action<DisciplineCase, AuthorizationPrincipal> mutation)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    permission,
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _repository.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _repository.RepositoryName);
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.CaseId,
                        "Discipline Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Discipline Case");
                    mutation(value, principal);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _repository,
                            snapshot.Records,
                            snapshot.Revision,
                            principal.UserId));
                    return Result(
                        transactionId,
                        value,
                        checked(snapshot.Revision + 1L));
                });
        }

        private static StudentServiceCommandResult Result(
            string transactionId,
            DisciplineCase value,
            long repositoryRevision)
        {
            return new StudentServiceCommandResult
            {
                TransactionId = transactionId,
                RecordId = value.Id,
                RepositoryRevision = repositoryRevision,
                EntityVersion = value.Version
            };
        }
    }

    public sealed class DisciplineCounselingCoordinationService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IDisciplineCaseRepository _discipline;
        private readonly ICounselingCaseRepository _counseling;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public DisciplineCounselingCoordinationService(
            SessionAwareRequestExecutor executor,
            IDisciplineCaseRepository discipline,
            ICounselingCaseRepository counseling,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _discipline = discipline ?? throw new ArgumentNullException(nameof(discipline));
            _counseling = counseling ?? throw new ArgumentNullException(nameof(counseling));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public StudentServiceCommandResult ReleaseDecisionWithCounselingReferral(
            string sessionId,
            string sessionToken,
            DisciplineCounselingCoordinationRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "discipline.decision.counseling-referral",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var disciplineSnapshot = _discipline.Read();
                    var counselingSnapshot = _counseling.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedDisciplineRepositoryRevision,
                        disciplineSnapshot.Revision,
                        _discipline.RepositoryName);
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedCounselingRepositoryRevision,
                        counselingSnapshot.Revision,
                        _counseling.RepositoryName);
                    var disciplineCase = MedicalCommandGuard.Find(
                        disciplineSnapshot.Records,
                        request.DisciplineCaseId,
                        "Discipline Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedDisciplineEntityVersion,
                        disciplineCase.Version,
                        "Discipline Case");

                    disciplineCase.ReleaseDecision(
                        request.ReleasedDecisionSummary,
                        utcNow,
                        principal.UserId);
                    var counselingCase = new CounselingCase(
                        _ids.Allocate("CNS", utcNow.Year, principal.UserId),
                        disciplineCase.StudentId,
                        request.RequestedCounselingAppointmentAtUtc,
                        request.CounselingRequestReason,
                        utcNow,
                        principal.UserId);
                    var counselingRecords =
                        counselingSnapshot.Records.ToList();
                    counselingRecords.Add(counselingCase);

                    var transactionId = _transactions.Execute(scope =>
                    {
                        scope.Stage(
                            _discipline,
                            disciplineSnapshot.Records,
                            disciplineSnapshot.Revision,
                            principal.UserId);
                        scope.Stage(
                            _counseling,
                            counselingRecords,
                            counselingSnapshot.Revision,
                            principal.UserId);
                    });

                    return new StudentServiceCommandResult
                    {
                        TransactionId = transactionId,
                        RecordId = disciplineCase.Id,
                        SecondaryRecordId = counselingCase.Id,
                        RepositoryRevision = checked(
                            disciplineSnapshot.Revision + 1L),
                        SecondaryRepositoryRevision = checked(
                            counselingSnapshot.Revision + 1L),
                        EntityVersion = disciplineCase.Version,
                        SecondaryEntityVersion = counselingCase.Version
                    };
                });
        }
    }
}
