using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Counseling;
using IUIS.Domain.Identity;

namespace IUIS.Application.Orchestration
{
    public sealed class CounselingCaseRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string RequestReason { get; set; }
    }

    public class CounselingCaseMutationRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string CaseId { get; set; }
    }

    public sealed class CounselingConfirmRequest : CounselingCaseMutationRequest
    {
        public DateTime ConfirmedAppointmentAtUtc { get; set; }
    }

    public sealed class CounselingAssignRequest : CounselingCaseMutationRequest
    {
        public string CounselorEmployeeId { get; set; }
    }

    public sealed class CounselingSessionRequest : CounselingCaseMutationRequest
    {
        public DateTime OccurredAtUtc { get; set; }
        public CounselingRiskLevel RiskLevel { get; set; }
        public string InternalNotes { get; set; }
    }

    public sealed class CounselingCloseRequest : CounselingCaseMutationRequest
    {
        public string ClosureSummary { get; set; }
    }

    public sealed class CounselingCancelRequest : CounselingCaseMutationRequest
    {
        public string Reason { get; set; }
    }

    public sealed class StudentCounselingDisciplineQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _counseling;
        private readonly IDisciplineCaseRepository _discipline;

        public StudentCounselingDisciplineQueryService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository counseling,
            IDisciplineCaseRepository discipline)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _counseling = counseling ?? throw new ArgumentNullException(nameof(counseling));
            _discipline = discipline ?? throw new ArgumentNullException(nameof(discipline));
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
                principal => Build(principal.PersonRecordId));
        }

        private StudentCounselingDisciplineOverviewDto Build(string studentId)
        {
            var counseling = _counseling.Read();
            var discipline = _discipline.Read();
            var counselingCases = counseling.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(item => new CounselingReleasedCaseDto
                {
                    CaseId = item.Id,
                    StudentId = item.StudentId,
                    Status = item.Status.ToString(),
                    RequestReason = item.RequestReason,
                    RequestedAppointmentAtUtc = item.RequestedAppointmentAtUtc,
                    ConfirmedAppointmentAtUtc = item.ConfirmedAppointmentAtUtc,
                    ClosedAtUtc = item.ClosedAtUtc,
                    EntityVersion = item.Version,
                    ReleasedSummaries = item.ReleasedSummaries
                        .OrderByDescending(summary => summary.ReleasedAtUtc)
                        .Select(summary => new ReleasedSummaryDto
                        {
                            SummaryId = summary.SummaryId,
                            RelatedRecordId = summary.SessionId,
                            Summary = summary.Summary,
                            ReleasedAtUtc = summary.ReleasedAtUtc
                        }).ToList().AsReadOnly()
                }).ToList();
            var disciplineCases = discipline.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .Where(item => item.ReleasedNotice != null)
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(item => new DisciplineReleasedCaseDto
                {
                    CaseId = item.Id,
                    StudentId = item.StudentId,
                    Status = item.Status.ToString(),
                    ViolationCode = item.Violation == null ? null : item.Violation.Code,
                    ViolationDescription = item.Violation == null ? null : item.Violation.Description,
                    ReleasedNoticeSummary = item.ReleasedNotice.ReleasedSummary,
                    ResponseDueDate = item.ReleasedNotice.ResponseDueDate.ToString(),
                    ReleasedDecisionSummary = item.Decision == null
                        ? null
                        : item.Decision.ReleasedDecisionSummary,
                    SanctionSummary = item.Decision == null
                        ? null
                        : item.Decision.SanctionSummary,
                    EntityVersion = item.Version
                }).ToList();
            return new StudentCounselingDisciplineOverviewDto
            {
                StudentId = studentId,
                CounselingRepositoryRevision = counseling.Revision,
                DisciplineRepositoryRevision = discipline.Revision,
                CounselingCases = counselingCases.AsReadOnly(),
                DisciplineCases = disciplineCases.AsReadOnly()
            };
        }
    }

    public sealed class RestrictedCounselingCaseQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _cases;

        public RestrictedCounselingCaseQueryService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository cases)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _cases = cases ?? throw new ArgumentNullException(nameof(cases));
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
                    var snapshot = _cases.Read();
                    var value = MedicalCommandGuard.Find(
                        snapshot.Records,
                        caseId,
                        "Counseling Case");
                    return new RestrictedCounselingCaseViewDto
                    {
                        RepositoryRevision = snapshot.Revision,
                        EntityVersion = value.Version,
                        Case = new CounselingInternalCaseDto
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
                                }).ToList().AsReadOnly(),
                            ReleasedSummaries = value.ReleasedSummaries
                                .Select(item => new ReleasedSummaryDto
                                {
                                    SummaryId = item.SummaryId,
                                    RelatedRecordId = item.SessionId,
                                    Summary = item.Summary,
                                    ReleasedAtUtc = item.ReleasedAtUtc
                                }).ToList().AsReadOnly()
                        }
                    };
                });
        }
    }

    public sealed class CounselingCaseCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _cases;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public CounselingCaseCommandService(
            SessionAwareRequestExecutor executor,
            ICounselingCaseRepository cases,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _cases = cases ?? throw new ArgumentNullException(nameof(cases));
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
                    var snapshot = _cases.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _cases.RepositoryName);
                    var value = new CounselingCase(
                        _ids.Allocate("CNS", utcNow.Year, principal.UserId),
                        principal.PersonRecordId,
                        request.RequestedAppointmentAtUtc,
                        request.RequestReason,
                        utcNow,
                        principal.UserId);
                    var records = snapshot.Records.ToList();
                    records.Add(value);
                    return Commit(snapshot, records, value, principal.UserId);
                });
        }

        public StudentServiceCommandResult Confirm(
            string sessionId,
            string sessionToken,
            CounselingConfirmRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.appointment.confirm",
                ConfidentialityClassification.Internal,
                (value, principal) => value.ConfirmAppointment(
                    request.ConfirmedAppointmentAtUtc,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult Assign(
            string sessionId,
            string sessionToken,
            CounselingAssignRequest request,
            DateTime utcNow)
        {
            return MutateInternal(
                sessionId,
                sessionToken,
                request,
                utcNow,
                "counseling.case.assign",
                ConfidentialityClassification.Restricted,
                (value, principal) => value.AssignCounselor(
                    request.CounselorEmployeeId,
                    utcNow,
                    principal.UserId));
        }

        public StudentServiceCommandResult RecordSession(
            string sessionId,
            string sessionToken,
            CounselingSessionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "counseling.session.restricted.record",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.Restricted,
                    null,
                    new[] { PrimaryRole.EmployeeFaculty }),
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
                        "Counseling Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Counseling Case");
                    if (!StringComparer.Ordinal.Equals(
                        value.AssignedCounselorEmployeeId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException(
                            "Only the assigned counselor may record a confidential Session.");
                    }
                    value.RecordSession(
                        _ids.Allocate("CSN", utcNow.Year, principal.UserId),
                        request.OccurredAtUtc,
                        request.RiskLevel,
                        request.InternalNotes,
                        utcNow,
                        principal.UserId);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        public StudentServiceCommandResult Close(
            string sessionId,
            string sessionToken,
            CounselingCloseRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "counseling.case.close",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.Restricted,
                    null,
                    new[] { PrimaryRole.EmployeeFaculty }),
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
                        "Counseling Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Counseling Case");
                    if (!StringComparer.Ordinal.Equals(
                        value.AssignedCounselorEmployeeId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException(
                            "Only the assigned counselor may close the Counseling Case.");
                    }
                    value.Close(request.ClosureSummary, utcNow, principal.UserId);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        public StudentServiceCommandResult CancelOwn(
            string sessionId,
            string sessionToken,
            CounselingCancelRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.counseling.cancel",
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
                        "Counseling Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Counseling Case");
                    if (!StringComparer.Ordinal.Equals(
                        value.StudentId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException(
                            "Students may cancel only their own Counseling Case.");
                    }
                    value.Cancel(request.Reason, utcNow, principal.UserId);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        private StudentServiceCommandResult MutateInternal<TRequest>(
            string sessionId,
            string sessionToken,
            TRequest request,
            DateTime utcNow,
            string permission,
            ConfidentialityClassification confidentiality,
            Action<CounselingCase, AuthorizationPrincipal> mutation)
            where TRequest : CounselingCaseMutationRequest
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
                        "Counseling Case");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        value.Version,
                        "Counseling Case");
                    mutation(value, principal);
                    return Commit(
                        snapshot,
                        snapshot.Records.ToList(),
                        value,
                        principal.UserId);
                });
        }

        private StudentServiceCommandResult Commit(
            RepositorySnapshot<CounselingCase> snapshot,
            IReadOnlyCollection<CounselingCase> records,
            CounselingCase value,
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
