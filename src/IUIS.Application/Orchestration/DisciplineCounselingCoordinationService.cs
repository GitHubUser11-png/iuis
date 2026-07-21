using System;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Counseling;
using IUIS.Domain.Identity;

namespace IUIS.Application.Orchestration
{
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
                principal => new AuthorizationRequest(
                    "discipline.decision.counseling-referral",
                    principal.PrimaryRole == PrimaryRole.Administrator
                        ? SessionApplicationKind.AdministratorApplication
                        : SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.Restricted,
                    null,
                    new[] { PrimaryRole.EmployeeFaculty, PrimaryRole.Administrator }),
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
                    var counselingRecords = counselingSnapshot.Records.ToList();
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
