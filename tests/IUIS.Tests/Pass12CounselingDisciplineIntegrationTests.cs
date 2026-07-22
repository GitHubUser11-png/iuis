using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Application.Repositories;
using IUIS.Domain.Common;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass12CounselingDisciplineIntegrationTests
    {
        private static readonly DateTime StartUtc =
            new DateTime(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc);
        private const string ActorUserId = "USR-2026-000001";
        private const string CounselorEmployeeId = "EMP-2026-000001";
        private const string StudentId = "STU-2026-000001";

        [TestMethod]
        public void CounselingMapperRoundTripPreservesConfidentialAndReleasedState()
        {
            var value = ClosedCounselingCase(
                "CNS-2026-000001", StudentId,
                "CSN-2026-000001", "CSR-2026-000001");
            var mapper = new CounselingCaseJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(value, options), options);

            Assert.AreEqual(CounselingCaseStatus.Closed, restored.Status);
            Assert.AreEqual(value.Version, restored.Version);
            Assert.AreEqual(1, restored.ConfidentialSessions.Count);
            Assert.AreEqual(1, restored.ReleasedSummaries.Count);
            Assert.AreEqual("Restricted counseling notes",
                restored.ConfidentialSessions[0].InternalNotes);
            Assert.AreEqual("Released counseling summary",
                restored.ReleasedSummaries[0].Summary);
            Assert.AreEqual("Counseling objectives completed",
                restored.ClosureSummary);
        }

        [TestMethod]
        public void AssignedCancellationRoundTripPreservesPriorWorkflowMetadata()
        {
            var value = new CounselingCase(
                "CNS-2026-000002", StudentId,
                StartUtc.AddMinutes(5), "Student requested support",
                StartUtc, ActorUserId);
            value.ConfirmAppointment(
                StartUtc.AddMinutes(5), StartUtc.AddMinutes(1), ActorUserId);
            value.AssignCounselor(
                CounselorEmployeeId, StartUtc.AddMinutes(2), ActorUserId);
            value.Cancel(
                "Student rescheduled outside the current case.",
                StartUtc.AddMinutes(6), ActorUserId);
            var mapper = new CounselingCaseJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(value, options), options);

            Assert.AreEqual(CounselingCaseStatus.Cancelled, restored.Status);
            Assert.AreEqual(value.ConfirmedAppointmentAtUtc,
                restored.ConfirmedAppointmentAtUtc);
            Assert.AreEqual(CounselorEmployeeId,
                restored.AssignedCounselorEmployeeId);
            Assert.AreEqual(0, restored.ConfidentialSessions.Count);
            Assert.AreEqual(0, restored.ReleasedSummaries.Count);
        }

        [TestMethod]
        public void DisciplineMapperRoundTripPreservesRestrictedAndReleasedDecisionShapes()
        {
            var value = ClosedDisciplineCase("DIN-2026-000001", StudentId);
            var mapper = new DisciplineCaseJsonMapper();
            var options = JsonOptions();
            var json = mapper.ToJson(value, options);
            var restored = mapper.FromJson(json, options);

            Assert.AreEqual(DisciplineCaseStatus.Closed, restored.Status);
            Assert.AreEqual(value.Version, restored.Version);
            Assert.AreEqual(1, restored.RestrictedEvidence.Count);
            Assert.AreEqual(1, restored.StudentResponses.Count);
            Assert.AreEqual(1, restored.RestrictedFindings.Count);
            Assert.AreEqual("Restricted decision rationale",
                restored.Decision.InternalRationale);
            Assert.AreEqual("Released decision summary",
                restored.Decision.ReleasedDecisionSummary);
            Assert.IsTrue(json.TryGetProperty("restrictedDecision", out _));
            Assert.IsTrue(json.TryGetProperty("releasedDecision", out _));
        }

        [TestMethod]
        public void FinalMappersRejectUnsupportedSchemaAndContradictoryState()
        {
            var options = JsonOptions();
            var unsupported = new PersistedCounselingCaseRecord
            {
                RecordSchemaVersion = 2,
                Id = "CNS-2026-000003"
            };
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                new CounselingCaseJsonMapper().FromJson(
                    JsonSerializer.SerializeToElement(unsupported, options),
                    options));

            Assert.ThrowsExactly<DomainValidationException>(() =>
                CounselingCase.Rehydrate(
                    "CNS-2026-000004", StudentId, StartUtc,
                    "Support request", CounselingCaseStatus.Active,
                    null, null, null, null,
                    new CounselingSessionRecord[0],
                    new CounselingReleasedSummary[0],
                    1, false, StartUtc, ActorUserId,
                    StartUtc, ActorUserId, null, null));

            Assert.ThrowsExactly<DomainValidationException>(() =>
                DisciplineCase.Rehydrate(
                    "DIN-2026-000004", StudentId,
                    StartUtc.AddMinutes(-1), "Campus",
                    "Restricted incident narrative", ActorUserId,
                    DisciplineCaseStatus.DecisionReleased,
                    null, null, null,
                    new DisciplineEvidenceReference[0],
                    new DisciplineStudentResponse[0],
                    new DisciplineFinding[0],
                    1, false, StartUtc, ActorUserId,
                    StartUtc, ActorUserId, null, null));
        }

        [TestMethod]
        public void MapperReadinessIsEighteenCompletedAndZeroDeferred()
        {
            var all = AggregateMapperReadinessCatalog.All;
            var completed = all.Where(item => item.Readiness
                    == AggregateMapperReadiness.SpecializedMapperCompleted)
                .Select(item => item.AdapterName).ToList();
            var deferred = all.Where(item => item.Readiness
                    == AggregateMapperReadiness.DeferredWithExplicitReason)
                .ToList();

            Assert.AreEqual(18, all.Count);
            Assert.AreEqual(18, completed.Count);
            Assert.AreEqual(0, deferred.Count);
            CollectionAssert.Contains(completed,
                "CounselingCaseRepositoryAdapter");
            CollectionAssert.Contains(completed,
                "DisciplineCaseRepositoryAdapter");
        }

        [TestMethod]
        public void CompositionRootRestartPreservesFinalRepositoriesAndServices()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var first = new IuisCompositionRoot(root);
                var counseling = ClosedCounselingCase(
                    "CNS-2026-000101", StudentId,
                    "CSN-2026-000101", "CSR-2026-000101");
                var discipline = ClosedDisciplineCase(
                    "DIN-2026-000101", StudentId);
                first.CounselingCases.Write(new[] { counseling }, 0,
                    bootstrap.AdministratorUserId);
                first.DisciplineCases.Write(new[] { discipline }, 0,
                    bootstrap.AdministratorUserId);

                var restarted = new IuisCompositionRoot(root);
                Assert.AreEqual(CounselingCaseStatus.Closed,
                    restarted.CounselingCases.FindById(counseling.Id).Status);
                Assert.AreEqual(DisciplineCaseStatus.Closed,
                    restarted.DisciplineCases.FindById(discipline.Id).Status);
                Assert.IsNotNull(restarted.StudentCounselingDiscipline);
                Assert.IsNotNull(restarted.RestrictedCounselingCases);
                Assert.IsNotNull(restarted.RestrictedDisciplineCases);
                Assert.IsNotNull(restarted.CounselingCommands);
                Assert.IsNotNull(restarted.DisciplineCommands);
                Assert.IsNotNull(restarted.DisciplineCounselingCoordination);
            });
        }

        [TestMethod]
        public void StudentProjectionUsesSessionOwnershipAndReleasedAllowlist()
        {
            var ownCounseling = ClosedCounselingCase(
                "CNS-2026-000201", StudentId,
                "CSN-2026-000201", "CSR-2026-000201");
            var otherCounseling = ClosedCounselingCase(
                "CNS-2026-000202", "STU-2026-000002",
                "CSN-2026-000202", "CSR-2026-000202");
            var prepared = DecisionPreparedDisciplineCase(
                "DIN-2026-000201", StudentId);
            var other = ClosedDisciplineCase(
                "DIN-2026-000202", "STU-2026-000002");
            var service = new StudentCounselingDisciplineQueryService(
                Executor(StudentPrincipal(StudentId)),
                new CounselingRepository(3,
                    new[] { ownCounseling, otherCounseling }),
                new DisciplineRepository(4, new[] { prepared, other }));

            var result = service.GetOwnOverview(
                "SES-2026-000201", "token", StartUtc.AddHours(10));

            Assert.AreEqual(StudentId, result.StudentId);
            Assert.AreEqual(1, result.CounselingCases.Count);
            Assert.AreEqual(1, result.DisciplineCases.Count);
            Assert.AreEqual(ownCounseling.Id,
                result.CounselingCases[0].CaseId);
            Assert.AreEqual(prepared.Id,
                result.DisciplineCases[0].CaseId);
            Assert.IsNull(result.DisciplineCases[0].SanctionSummary);
            Assert.IsNull(result.DisciplineCases[0].ReleasedDecisionSummary);
            Assert.IsNull(typeof(CounselingReleasedCaseDto)
                .GetProperty("ConfidentialSessions"));
            Assert.IsNull(typeof(DisciplineReleasedCaseDto)
                .GetProperty("InternalIncidentNarrative"));
            Assert.IsNull(typeof(DisciplineReleasedCaseDto)
                .GetProperty("InternalDecisionRationale"));
        }

        [TestMethod]
        public void RestrictedQueriesRequireExplicitConfidentialityPermission()
        {
            var counseling = ClosedCounselingCase(
                "CNS-2026-000301", StudentId,
                "CSN-2026-000301", "CSR-2026-000301");
            var discipline = ClosedDisciplineCase(
                "DIN-2026-000301", StudentId);

            var deniedCounseling = new RestrictedCounselingCaseQueryService(
                Executor(AdminPrincipal("counseling.case.restricted.read")),
                new CounselingRepository(1, new[] { counseling }));
            Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                deniedCounseling.Get("SES-2026-000301", "token",
                    counseling.Id, StartUtc.AddHours(10)));

            var allowedCounseling = new RestrictedCounselingCaseQueryService(
                Executor(AdminPrincipal(
                    "counseling.case.restricted.read",
                    "confidentiality.restricted")),
                new CounselingRepository(1, new[] { counseling }));
            Assert.AreEqual("Restricted counseling notes",
                allowedCounseling.Get("SES-2026-000302", "token",
                    counseling.Id, StartUtc.AddHours(10))
                    .Case.ConfidentialSessions[0].InternalNotes);

            var deniedDiscipline = new RestrictedDisciplineCaseQueryService(
                Executor(AdminPrincipal("discipline.case.restricted.read")),
                new DisciplineRepository(1, new[] { discipline }));
            Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                deniedDiscipline.Get("SES-2026-000303", "token",
                    discipline.Id, StartUtc.AddHours(10)));
        }

        [TestMethod]
        public void StaleRepositoryAndEntityTokensFailBeforeMutation()
        {
            var value = DecisionPreparedDisciplineCase(
                "DIN-2026-000401", StudentId);
            var repository = new DisciplineRepository(7, new[] { value });
            var coordinator = new ImmediateCoordinator();
            var service = new DisciplineCaseCommandService(
                Executor(EmployeePrincipal(
                    "discipline.decision.release")),
                repository, coordinator, new FakeAllocator());

            Assert.ThrowsExactly<InvalidOperationException>(() =>
                service.ReleaseDecision("SES-2026-000401", "token",
                    new DisciplineReleaseDecisionRequest
                    {
                        ExpectedRepositoryRevision = 6,
                        ExpectedEntityVersion = value.Version,
                        CaseId = value.Id,
                        ReleasedDecisionSummary = "Released decision"
                    }, StartUtc.AddHours(10)));
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                service.ReleaseDecision("SES-2026-000401", "token",
                    new DisciplineReleaseDecisionRequest
                    {
                        ExpectedRepositoryRevision = 7,
                        ExpectedEntityVersion = value.Version + 1L,
                        CaseId = value.Id,
                        ReleasedDecisionSummary = "Released decision"
                    }, StartUtc.AddHours(10)));

            Assert.AreEqual(0, coordinator.ExecutionCount);
            Assert.AreEqual(DisciplineCaseStatus.DecisionPrepared,
                repository.FindById(value.Id).Status);
        }

        [TestMethod]
        public void StudentCannotSubmitResponseForAnotherStudentsCase()
        {
            var value = NoticeReleasedDisciplineCase(
                "DIN-2026-000501", "STU-2026-000002");
            var repository = new DisciplineRepository(2, new[] { value });
            var coordinator = new ImmediateCoordinator();
            var service = new DisciplineCaseCommandService(
                Executor(StudentPrincipal(StudentId,
                    "student.discipline.response.submit")),
                repository, coordinator, new FakeAllocator());

            var exception = Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                service.SubmitOwnResponse("SES-2026-000501", "token",
                    new DisciplineStudentResponseRequest
                    {
                        ExpectedRepositoryRevision = 2,
                        ExpectedEntityVersion = value.Version,
                        CaseId = value.Id,
                        ResponseText = "Cross-record response"
                    }, StartUtc.AddHours(10)));

            Assert.AreEqual(
                "Students may respond only to their own Discipline Case.",
                exception.ReasonCode);
            Assert.AreEqual(0, coordinator.ExecutionCount);
            Assert.AreEqual(0,
                repository.FindById(value.Id).StudentResponses.Count);
        }

        [TestMethod]
        public void DecisionReleaseAndCounselingReferralCommitTogether()
        {
            var discipline = DecisionPreparedDisciplineCase(
                "DIN-2026-000601", StudentId);
            var disciplineRepository = new DisciplineRepository(
                5, new[] { discipline });
            var counselingRepository = new CounselingRepository(
                8, new CounselingCase[0]);
            var coordinator = new ImmediateCoordinator();
            var service = new DisciplineCounselingCoordinationService(
                Executor(EmployeePrincipal(
                    "discipline.decision.counseling-referral",
                    "confidentiality.restricted")),
                disciplineRepository, counselingRepository,
                coordinator, new FakeAllocator());

            var result = service.ReleaseDecisionWithCounselingReferral(
                "SES-2026-000601", "token",
                new DisciplineCounselingCoordinationRequest
                {
                    ExpectedDisciplineRepositoryRevision = 5,
                    ExpectedCounselingRepositoryRevision = 8,
                    ExpectedDisciplineEntityVersion = discipline.Version,
                    DisciplineCaseId = discipline.Id,
                    ReleasedDecisionSummary = "Released corrective decision",
                    RequestedCounselingAppointmentAtUtc =
                        StartUtc.AddHours(11),
                    CounselingRequestReason =
                        "Decision-linked counseling referral"
                }, StartUtc.AddHours(10));

            Assert.AreEqual(1, coordinator.ExecutionCount);
            Assert.AreEqual(2, coordinator.LastStageCount);
            Assert.AreEqual(6L, disciplineRepository.Read().Revision);
            Assert.AreEqual(9L, counselingRepository.Read().Revision);
            Assert.AreEqual(DisciplineCaseStatus.DecisionReleased,
                disciplineRepository.FindById(discipline.Id).Status);
            Assert.AreEqual(1, counselingRepository.Read().Records.Count);
            Assert.AreEqual(StudentId,
                counselingRepository.Read().Records[0].StudentId);
            Assert.AreEqual(result.SecondaryRecordId,
                counselingRepository.Read().Records[0].Id);
        }

        [TestMethod]
        public void DeterministicRelatedMutationFailureRollsBackBothFilesByteForByte()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var store = new JsonRepositoryStore(catalog, options);
                var disciplineRepository =
                    new DisciplineCaseRepositoryAdapter(store);
                var counselingRepository =
                    new CounselingCaseRepositoryAdapter(store);
                var discipline = DecisionPreparedDisciplineCase(
                    "DIN-2026-000701", StudentId);
                disciplineRepository.Write(new[] { discipline }, 0,
                    bootstrap.AdministratorUserId);
                var disciplinePath = Path.Combine(root,
                    "discipline_incidents.json");
                var counselingPath = Path.Combine(root, "counseling.json");
                var beforeDiscipline = File.ReadAllBytes(disciplinePath);
                var beforeCounseling = File.ReadAllBytes(counselingPath);
                var transactions = new JournaledApplicationTransactionCoordinator(
                    new JournaledTransactionCoordinator(catalog, options,
                        new FailAfterFirstMutation()));
                var service = new DisciplineCounselingCoordinationService(
                    Executor(EmployeePrincipal(
                        "discipline.decision.counseling-referral",
                        "confidentiality.restricted")),
                    disciplineRepository, counselingRepository,
                    transactions,
                    new ApplicationIdentifierAllocator(catalog, options));

                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    service.ReleaseDecisionWithCounselingReferral(
                        "SES-2026-000701", "token",
                        new DisciplineCounselingCoordinationRequest
                        {
                            ExpectedDisciplineRepositoryRevision = 1,
                            ExpectedCounselingRepositoryRevision = 0,
                            ExpectedDisciplineEntityVersion = discipline.Version,
                            DisciplineCaseId = discipline.Id,
                            ReleasedDecisionSummary = "Released decision",
                            RequestedCounselingAppointmentAtUtc =
                                StartUtc.AddHours(11),
                            CounselingRequestReason = "Required referral"
                        }, StartUtc.AddHours(10)));

                CollectionAssert.AreEqual(beforeDiscipline,
                    File.ReadAllBytes(disciplinePath));
                CollectionAssert.AreEqual(beforeCounseling,
                    File.ReadAllBytes(counselingPath));
                var restarted = new IuisCompositionRoot(root);
                Assert.AreEqual(DisciplineCaseStatus.DecisionPrepared,
                    restarted.DisciplineCases.FindById(discipline.Id).Status);
                Assert.AreEqual(0,
                    restarted.CounselingCases.Read().Records.Count);
            });
        }

        private static CounselingCase ClosedCounselingCase(
            string id, string studentId, string sessionId, string summaryId)
        {
            var value = new CounselingCase(id, studentId,
                StartUtc.AddMinutes(1),
                "Student requested counseling support",
                StartUtc, ActorUserId);
            value.ConfirmAppointment(StartUtc.AddMinutes(1),
                StartUtc.AddMinutes(1), ActorUserId);
            value.AssignCounselor(CounselorEmployeeId,
                StartUtc.AddMinutes(2), ActorUserId);
            value.RecordSession(sessionId, StartUtc.AddMinutes(3),
                CounselingRiskLevel.Elevated,
                "Restricted counseling notes",
                StartUtc.AddMinutes(3), ActorUserId);
            value.ReleaseSessionSummary(summaryId, sessionId,
                "CRL-2026-000001", "Released counseling summary",
                StartUtc.AddMinutes(4), ActorUserId);
            value.Close("Counseling objectives completed",
                StartUtc.AddMinutes(5), ActorUserId);
            return value;
        }

        private static DisciplineCase NoticeReleasedDisciplineCase(
            string id, string studentId)
        {
            var value = new DisciplineCase(id, studentId,
                StartUtc.AddMinutes(-1), "Main Campus",
                "Restricted incident narrative", ActorUserId,
                StartUtc, ActorUserId);
            value.BeginReview(StartUtc.AddMinutes(1), ActorUserId);
            value.AddEvidenceReference("DEV-2026-000001",
                "restricted-evidence-reference",
                "Restricted evidence description",
                StartUtc.AddMinutes(2), ActorUserId);
            value.ConvertToViolation("VIO-2026-000001", "CODE-1",
                "Released violation description",
                DisciplineSeverity.Moderate,
                StartUtc.AddMinutes(3), ActorUserId);
            value.ReleaseNotice("DNT-2026-000001",
                "Released notice summary",
                new InstitutionLocalDate(2026, 8, 1),
                StartUtc.AddMinutes(4), ActorUserId);
            return value;
        }

        private static DisciplineCase DecisionPreparedDisciplineCase(
            string id, string studentId)
        {
            var value = NoticeReleasedDisciplineCase(id, studentId);
            value.RecordStudentResponse("DSR-2026-000001",
                "Student response", "student-evidence-reference",
                StartUtc.AddMinutes(5), ActorUserId);
            value.RecordFinding("DFN-2026-000001", true,
                "Restricted finding",
                StartUtc.AddMinutes(6), ActorUserId);
            value.PrepareDecision("DDC-2026-000001",
                DisciplineDecisionOutcome.CorrectiveAction,
                "Restricted decision rationale",
                "Complete corrective counseling",
                StartUtc.AddMinutes(7), ActorUserId);
            return value;
        }

        private static DisciplineCase ClosedDisciplineCase(
            string id, string studentId)
        {
            var value = DecisionPreparedDisciplineCase(id, studentId);
            value.ReleaseDecision("Released decision summary",
                StartUtc.AddMinutes(8), ActorUserId);
            value.Close(StartUtc.AddMinutes(9), ActorUserId);
            return value;
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static SessionAwareRequestExecutor Executor(
            AuthorizationPrincipal principal)
        {
            return new SessionAwareRequestExecutor(
                new FixedPrincipalProvider(principal),
                new PermissionResolver());
        }

        private static AuthorizationPrincipal StudentPrincipal(
            string studentId, params string[] permissions)
        {
            return Principal("USR-2026-000002", studentId,
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                permissions.Length == 0
                    ? new[] { "student.services.released.read" }
                    : permissions);
        }

        private static AuthorizationPrincipal EmployeePrincipal(
            params string[] permissions)
        {
            return Principal(ActorUserId, CounselorEmployeeId,
                PrimaryRole.EmployeeFaculty,
                SessionApplicationKind.UserApplication, permissions);
        }

        private static AuthorizationPrincipal AdminPrincipal(
            params string[] permissions)
        {
            return Principal(ActorUserId, CounselorEmployeeId,
                PrimaryRole.Administrator,
                SessionApplicationKind.AdministratorApplication,
                permissions);
        }

        private static AuthorizationPrincipal Principal(
            string userId, string personId, PrimaryRole role,
            SessionApplicationKind applicationKind,
            string[] permissions)
        {
            return new AuthorizationPrincipal(userId, personId, role,
                applicationKind, SessionPurpose.FullAccess,
                "security-stamp",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000001", true, permissions)
                }, null, null);
        }

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(Path.GetTempPath(),
                "IUIS-Pass12-Unit4-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var result = new ProductionBootstrapper(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Initialize(new ProductionBootstrapRequest
                    {
                        AdministratorLoginId = "root.admin",
                        AdministratorInitialPassword =
                            "Temporary-Admin-Password-1",
                        AdministratorGivenName = "Initial",
                        AdministratorFamilyName = "Administrator",
                        AdministratorEmailAddress = "admin@example.edu",
                        AdministratorMobileNumber = "+639171234567",
                        AdministratorAddressLine1 = "1 University Road",
                        AdministratorBarangay = "Poblacion",
                        AdministratorCityMunicipality = "Malvar",
                        AdministratorProvince = "Batangas",
                        AdministratorPostalCode = "4233",
                        AdministratorCountryCode = "PH",
                        AdministratorBirthDate = "1990-01-01",
                        DepartmentId = "DEPT-ADMIN",
                        PositionTitle = "System Administrator",
                        BootstrapAtUtc = StartUtc
                    });
                action(root, result);
            }
            finally
            {
                try { Directory.Delete(root, true); }
                catch { }
            }
        }

        private sealed class FixedPrincipalProvider :
            IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;
            public FixedPrincipalProvider(AuthorizationPrincipal principal)
            {
                _principal = principal;
            }
            public AuthorizationPrincipal Load(
                string sessionId, string sessionToken, DateTime utcNow)
            {
                return _principal;
            }
        }

        private sealed class FakeAllocator : IApplicationIdentifierAllocator
        {
            private int _next;
            public string Allocate(string prefix, int year, string actorUserId)
            {
                _next++;
                return prefix + "-" + year + "-"
                    + _next.ToString("000000");
            }
        }

        private abstract class RepositoryBase<T> where T : class, IEntity
        {
            private List<T> _records;
            private long _revision;
            protected RepositoryBase(string name, long revision,
                IEnumerable<T> records)
            {
                RepositoryName = name;
                _revision = revision;
                _records = records.ToList();
            }
            public string RepositoryName { get; private set; }
            protected RepositorySnapshot<T> ReadCore()
            {
                return new RepositorySnapshot<T>(RepositoryName,
                    _revision, _records.ToList());
            }
            protected T FindCore(string id)
            {
                return _records.SingleOrDefault(item =>
                    StringComparer.Ordinal.Equals(item.Id, id));
            }
            protected void WriteCore(IReadOnlyCollection<T> records,
                long expectedRevision)
            {
                if (_revision != expectedRevision)
                    throw new InvalidOperationException(
                        "Repository revision conflict for "
                        + RepositoryName + ".");
                _records = records.ToList();
                _revision = checked(_revision + 1L);
            }
        }

        private sealed class CounselingRepository :
            RepositoryBase<CounselingCase>, ICounselingCaseRepository
        {
            public CounselingRepository(long revision,
                IEnumerable<CounselingCase> records)
                : base("counseling", revision, records) { }
            public RepositorySnapshot<CounselingCase> Read()
            { return ReadCore(); }
            public CounselingCase FindById(string id)
            { return FindCore(id); }
            public void Write(IReadOnlyCollection<CounselingCase> records,
                long expectedRevision, string updatedByUserId)
            { WriteCore(records, expectedRevision); }
        }

        private sealed class DisciplineRepository :
            RepositoryBase<DisciplineCase>, IDisciplineCaseRepository
        {
            public DisciplineRepository(long revision,
                IEnumerable<DisciplineCase> records)
                : base("discipline_incidents", revision, records) { }
            public RepositorySnapshot<DisciplineCase> Read()
            { return ReadCore(); }
            public DisciplineCase FindById(string id)
            { return FindCore(id); }
            public void Write(IReadOnlyCollection<DisciplineCase> records,
                long expectedRevision, string updatedByUserId)
            { WriteCore(records, expectedRevision); }
        }

        private sealed class ImmediateCoordinator :
            IApplicationTransactionCoordinator
        {
            public int ExecutionCount { get; private set; }
            public int LastStageCount { get; private set; }
            public string Execute(Action<IRepositoryTransactionScope> stageMutations)
            {
                var scope = new ImmediateScope();
                stageMutations(scope);
                ExecutionCount++;
                LastStageCount = scope.Actions.Count;
                foreach (var action in scope.Actions) action();
                return "TXN-2026-000001";
            }

            private sealed class ImmediateScope : IRepositoryTransactionScope
            {
                public readonly List<Action> Actions = new List<Action>();
                public void Stage<T>(IVersionedRepository<T> repository,
                    IReadOnlyCollection<T> records, long expectedRevision,
                    string updatedByUserId) where T : class, IEntity
                {
                    Actions.Add(() => repository.Write(records,
                        expectedRevision, updatedByUserId));
                }
            }
        }

        private sealed class FailAfterFirstMutation :
            ITransactionFailureInjector
        {
            public void OnStage(TransactionExecutionContext context)
            {
                if (context.Stage == TransactionExecutionStage.MutationApplied
                    && context.AppliedMutationCount == 1)
                {
                    throw new InvalidOperationException(
                        "Deterministic Pass 12 Unit 4 transaction failure.");
                }
            }
        }
    }
}
