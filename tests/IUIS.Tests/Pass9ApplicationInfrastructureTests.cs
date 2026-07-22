using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Application.Repositories;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.People;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass9ApplicationInfrastructureTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 20, 3, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void ActiveProfilePermissionAuthorizesEmployee()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.EmployeeFaculty,
                    SessionApplicationKind.UserApplication,
                    SessionPurpose.FullAccess,
                    "EMP-2026-000001",
                    new[] { "registrar.student.read" },
                    null,
                    null),
                Request(
                    "registrar.student.read",
                    ConfidentialityClassification.Internal,
                    SessionApplicationKind.UserApplication,
                    null,
                    PrimaryRole.EmployeeFaculty));
            Assert.IsTrue(decision.IsAllowed);
        }

        [TestMethod]
        public void DirectRestrictionOverridesProfileAndDirectGrant()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.EmployeeFaculty,
                    SessionApplicationKind.UserApplication,
                    SessionPurpose.FullAccess,
                    "EMP-2026-000001",
                    new[] { "finance.payment.post" },
                    new[] { "finance.*" },
                    new[] { "finance.payment.post" }),
                Request(
                    "finance.payment.post",
                    ConfidentialityClassification.Internal,
                    SessionApplicationKind.UserApplication,
                    null,
                    PrimaryRole.EmployeeFaculty));
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual("direct-restriction", decision.ReasonCode);
        }

        [TestMethod]
        public void AdministratorCannotBypassRestrictedConfidentiality()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.Administrator,
                    SessionApplicationKind.AdministratorApplication,
                    SessionPurpose.FullAccess,
                    "EMP-2026-000001",
                    new[] { "counseling.case.internal.read" },
                    null,
                    null),
                Request(
                    "counseling.case.internal.read",
                    ConfidentialityClassification.Restricted,
                    SessionApplicationKind.AdministratorApplication,
                    null,
                    PrimaryRole.Administrator));
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual(
                "restricted-confidentiality-permission-missing",
                decision.ReasonCode);
        }

        [TestMethod]
        public void AdministratorWithExplicitRestrictedPermissionIsAuthorized()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.Administrator,
                    SessionApplicationKind.AdministratorApplication,
                    SessionPurpose.FullAccess,
                    "EMP-2026-000001",
                    new[]
                    {
                        "counseling.case.internal.read",
                        "confidentiality.restricted"
                    },
                    null,
                    null),
                Request(
                    "counseling.case.internal.read",
                    ConfidentialityClassification.Restricted,
                    SessionApplicationKind.AdministratorApplication,
                    null,
                    PrimaryRole.Administrator));
            Assert.IsTrue(decision.IsAllowed);
        }

        [TestMethod]
        public void StudentOwnRecordIsAuthorized()
        {
            var decision = new PermissionResolver().Resolve(
                StudentPrincipal("STU-2026-000001"),
                Request(
                    "student.profile.read",
                    ConfidentialityClassification.OwnRecord,
                    SessionApplicationKind.UserApplication,
                    "STU-2026-000001",
                    PrimaryRole.Student));
            Assert.IsTrue(decision.IsAllowed);
        }

        [TestMethod]
        public void StudentCrossRecordAccessIsDenied()
        {
            var decision = new PermissionResolver().Resolve(
                StudentPrincipal("STU-2026-000001"),
                Request(
                    "student.profile.read",
                    ConfidentialityClassification.OwnRecord,
                    SessionApplicationKind.UserApplication,
                    "STU-2026-000002",
                    PrimaryRole.Student));
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual("record-ownership-mismatch", decision.ReasonCode);
        }

        [TestMethod]
        public void FirstLoginSessionBlocksOrdinaryQueries()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.Administrator,
                    SessionApplicationKind.AdministratorApplication,
                    SessionPurpose.FirstLoginPasswordChange,
                    "EMP-2026-000001",
                    new[] { "admin.user.read", "security.password.change" },
                    null,
                    null),
                Request(
                    "admin.user.read",
                    ConfidentialityClassification.Internal,
                    SessionApplicationKind.AdministratorApplication,
                    null,
                    PrimaryRole.Administrator));
            Assert.IsFalse(decision.IsAllowed);
            Assert.AreEqual("session-purpose-restricted", decision.ReasonCode);
        }

        [TestMethod]
        public void FirstLoginSessionAllowsOnlyPasswordChange()
        {
            var decision = new PermissionResolver().Resolve(
                CreatePrincipal(
                    PrimaryRole.Administrator,
                    SessionApplicationKind.AdministratorApplication,
                    SessionPurpose.FirstLoginPasswordChange,
                    "EMP-2026-000001",
                    new[] { "security.password.change" },
                    null,
                    null),
                Request(
                    "security.password.change",
                    ConfidentialityClassification.Internal,
                    SessionApplicationKind.AdministratorApplication,
                    null,
                    PrimaryRole.Administrator));
            Assert.IsTrue(decision.IsAllowed);
        }

        [TestMethod]
        public void ReleasedDtosExposeNoInternalOrConfidentialProperties()
        {
            var types = new[]
            {
                typeof(CounselingReleasedCaseDto),
                typeof(DisciplineReleasedCaseDto),
                typeof(MedicalReleasedRecordDto)
            };
            foreach (var type in types)
            {
                Assert.IsFalse(type.GetProperties().Any(property =>
                    property.Name.IndexOf("Internal", StringComparison.OrdinalIgnoreCase) >= 0
                    || property.Name.IndexOf("Confidential", StringComparison.OrdinalIgnoreCase) >= 0));
            }
        }

        [TestMethod]
        public void StudentOwnRecordQueryDerivesStudentIdFromSessionPrincipal()
        {
            var student = CreateStudent("STU-2026-000001");
            var service = new StudentOwnRecordQueryService(
                new SessionAwareRequestExecutor(
                    new FixedPrincipalProvider(StudentPrincipal(student.Id)),
                    new PermissionResolver()),
                new InMemoryStudentRepository(student),
                new RestrictedProjectionService());
            var result = service.GetOwnRecord("SES-2026-000001", "token", Now);
            Assert.AreEqual(student.Id, result.StudentId);
            Assert.AreEqual(student.Version, result.EntityVersion);
        }

        [TestMethod]
        public void EmployeeQueryRejectsStudentRoleBeforeRepositoryRead()
        {
            var repository = new InMemoryEmployeeRepository();
            var service = new EmployeeRecordQueryService(
                new SessionAwareRequestExecutor(
                    new FixedPrincipalProvider(StudentPrincipal("STU-2026-000001")),
                    new PermissionResolver()),
                repository,
                new RestrictedProjectionService());
            Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                service.GetEmployee(
                    "SES-2026-000001",
                    "token",
                    "EMP-2026-000001",
                    Now));
            Assert.AreEqual(0, repository.ReadCount);
        }

        [TestMethod]
        public void MappedRepositoryRoundTripsTypedRecords()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var repository = new MappedJsonRepository<TestAggregate>(
                    "courses",
                    NewStore(root),
                    new SystemTextJsonRecordMapper<TestAggregate>());
                repository.Write(
                    new[]
                    {
                        new TestAggregate { Id = "TST-2026-000001", Value = "Alpha" }
                    },
                    0,
                    bootstrap.AdministratorUserId);
                var snapshot = repository.Read();
                Assert.AreEqual(1L, snapshot.Revision);
                Assert.AreEqual("Alpha", snapshot.Records.Single().Value);

                var readiness = AggregateMapperReadinessCatalog.All;
                Assert.AreEqual(18, readiness.Count);
                Assert.AreEqual(
                    18,
                    readiness.Select(item => item.AdapterName)
                        .Distinct(StringComparer.Ordinal)
                        .Count());
                Assert.AreEqual(
                    18,
                    readiness.Count(item =>
                        item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted));
                Assert.AreEqual(
                    0,
                    readiness.Count(item =>
                        item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason));
                Assert.IsFalse(readiness.Any(item =>
                    item.Readiness == AggregateMapperReadiness.GenericMapperCompatible
                    || item.Readiness == AggregateMapperReadiness.RequiresSpecializedMapper));
            });
        }

        [TestMethod]
        public void MappedRepositoryRejectsStaleExpectedRevision()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var repository = new MappedJsonRepository<TestAggregate>(
                    "courses",
                    NewStore(root),
                    new SystemTextJsonRecordMapper<TestAggregate>());
                var records = new[]
                {
                    new TestAggregate { Id = "TST-2026-000001", Value = "Alpha" }
                };
                repository.Write(records, 0, bootstrap.AdministratorUserId);
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    repository.Write(records, 0, bootstrap.AdministratorUserId));
            });
        }

        [TestMethod]
        public void ApplicationTransactionRoutesRelatedWritesThroughJournalCoordinator()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var store = new JsonRepositoryStore(catalog, options);
                var courses = new MappedJsonRepository<TestAggregate>(
                    "courses",
                    store,
                    new SystemTextJsonRecordMapper<TestAggregate>());
                var subjects = new MappedJsonRepository<TestAggregate>(
                    "subjects",
                    store,
                    new SystemTextJsonRecordMapper<TestAggregate>());
                var transaction = new JournaledApplicationTransactionCoordinator(
                    new JournaledTransactionCoordinator(catalog, options));
                var id = transaction.Execute(scope =>
                {
                    scope.Stage(
                        courses,
                        new[] { new TestAggregate { Id = "TST-2026-000001", Value = "Course" } },
                        0,
                        bootstrap.AdministratorUserId);
                    scope.Stage(
                        subjects,
                        new[] { new TestAggregate { Id = "TST-2026-000002", Value = "Subject" } },
                        0,
                        bootstrap.AdministratorUserId);
                });
                Assert.IsFalse(string.IsNullOrWhiteSpace(id));
                Assert.AreEqual(1L, courses.Read().Revision);
                Assert.AreEqual(1L, subjects.Read().Revision);

                using (var stageReadCompleted = new ManualResetEventSlim(false))
                using (var releaseStage = new ManualResetEventSlim(false))
                {
                    var blockingCourses = new MappedJsonRepository<TestAggregate>(
                        "courses",
                        store,
                        new BlockingJsonRecordMapper(stageReadCompleted, releaseStage));
                    Exception transactionFailure = null;
                    var staleTask = Task.Run(() =>
                    {
                        try
                        {
                            transaction.Execute(scope => scope.Stage(
                                blockingCourses,
                                new[]
                                {
                                    new TestAggregate
                                    {
                                        Id = "TST-2026-000001",
                                        Value = "Stale staged value"
                                    }
                                },
                                1,
                                bootstrap.AdministratorUserId));
                        }
                        catch (Exception exception)
                        {
                            transactionFailure = exception;
                        }
                    });
                    Assert.IsTrue(stageReadCompleted.Wait(TimeSpan.FromSeconds(10)));
                    courses.Write(
                        new[]
                        {
                            new TestAggregate
                            {
                                Id = "TST-2026-000001",
                                Value = "Concurrent committed value"
                            }
                        },
                        1,
                        bootstrap.AdministratorUserId);
                    releaseStage.Set();
                    Assert.IsTrue(staleTask.Wait(TimeSpan.FromSeconds(10)));
                    Assert.IsInstanceOfType(transactionFailure, typeof(InvalidOperationException));
                    var finalSnapshot = courses.Read();
                    Assert.AreEqual(2L, finalSnapshot.Revision);
                    Assert.AreEqual("Concurrent committed value", finalSnapshot.Records.Single().Value);
                }
            });
        }

        [TestMethod]
        public void JsonPrincipalProviderLoadsProfilesGrantsAndRestrictions()
        {
            WithFullAdminSession((root, bootstrap, authentication) =>
            {
                var store = NewStore(root);
                var users = store.Read<PersistedUserAccount>("users");
                var account = users.Records.Single(item => item.Id == bootstrap.AdministratorUserId);
                account.PermissionProfileIds = new List<string> { "PPR-2026-000001" };
                account.DirectPermissionGrants = new List<string> { "admin.report.read" };
                account.DirectPermissionRestrictions = new List<string> { "admin.user.delete" };
                store.Write("users", users, users.Revision);
                var profiles = store.Read<PersistedPermissionProfileRecord>("permission_profiles");
                profiles.Records.Add(new PersistedPermissionProfileRecord
                {
                    Id = "PPR-2026-000001",
                    Name = "Security Operations",
                    IsActive = true,
                    Permissions = new List<string> { "admin.user.read" },
                    UpdatedAtUtc = Now,
                    UpdatedByUserId = bootstrap.AdministratorUserId,
                    Version = 1
                });
                store.Write("permission_profiles", profiles, profiles.Revision);

                var principal = new JsonAuthorizationPrincipalProvider(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Load(authentication.SessionId, authentication.SessionToken, Now.AddMinutes(4));
                var effective = new PermissionResolver().ResolveEffectivePermissions(principal);
                Assert.IsTrue(effective.Contains("admin.user.read"));
                Assert.IsTrue(effective.Contains("admin.report.read"));
                Assert.IsTrue(principal.DirectRestrictions.Contains("admin.user.delete"));
            });
        }

        [TestMethod]
        public void JsonPrincipalProviderRejectsExpiredSession()
        {
            WithFullAdminSession((root, bootstrap, authentication) =>
            {
                var provider = new JsonAuthorizationPrincipalProvider(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root));
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    provider.Load(authentication.SessionId, authentication.SessionToken, Now.AddHours(10)));
            });
        }

        [TestMethod]
        public void JsonPrincipalProviderRejectsSecurityStampMismatch()
        {
            WithFullAdminSession((root, bootstrap, authentication) =>
            {
                var store = NewStore(root);
                var users = store.Read<PersistedUserAccount>("users");
                users.Records.Single(item => item.Id == bootstrap.AdministratorUserId)
                    .SecurityStamp = Guid.NewGuid().ToString("N");
                store.Write("users", users, users.Revision);
                var provider = new JsonAuthorizationPrincipalProvider(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root));
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    provider.Load(authentication.SessionId, authentication.SessionToken, Now.AddMinutes(4)));
            });
        }

        private static AuthorizationRequest Request(
            string permission,
            ConfidentialityClassification confidentiality,
            SessionApplicationKind applicationKind,
            string ownerId,
            params PrimaryRole[] roles)
        {
            return new AuthorizationRequest(permission, applicationKind, confidentiality, ownerId, roles);
        }

        private static AuthorizationPrincipal StudentPrincipal(string studentId)
        {
            return CreatePrincipal(
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                studentId,
                new[] { "student.profile.read" },
                null,
                null);
        }

        private static AuthorizationPrincipal CreatePrincipal(
            PrimaryRole role,
            SessionApplicationKind applicationKind,
            SessionPurpose purpose,
            string personId,
            IEnumerable<string> profilePermissions,
            IEnumerable<string> directGrants,
            IEnumerable<string> restrictions)
        {
            return new AuthorizationPrincipal(
                "USR-2026-000001",
                personId,
                role,
                applicationKind,
                purpose,
                "security-stamp",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000001",
                        true,
                        profilePermissions ?? new string[0])
                },
                directGrants,
                restrictions);
        }

        private static StudentRecord CreateStudent(string id)
        {
            return new StudentRecord(
                id,
                id,
                new PersonName("Ada", null, "Lovelace", null),
                new ContactInformation("ada@example.edu", "+639171234567", null),
                new PostalAddress("1 University Road", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                new InstitutionLocalDate(2000, 1, 1),
                "CRS-2026-000001",
                StudentStatus.Active,
                Now,
                "USR-2026-000001");
        }

        private static JsonRepositoryStore NewStore(string root)
        {
            return new JsonRepositoryStore(
                new ProductionRepositoryCatalog(),
                new JsonInfrastructureOptions(root));
        }

        private static void WithFullAdminSession(
            Action<string, ProductionBootstrapResult, AuthenticationResult> action)
        {
            WithBootstrap((root, bootstrap) =>
            {
                var service = new AuthenticationService(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root));
                var restricted = service.Authenticate(
                    "root.admin",
                    "Temporary-Admin-Password-1",
                    "AdministratorApplication",
                    Now.AddMinutes(1));
                var full = service.CompleteForcedPasswordChange(
                    bootstrap.AdministratorUserId,
                    restricted.SessionId,
                    "Permanent-Admin-Password-2",
                    Now.AddMinutes(2));
                action(root, bootstrap, full);
            });
        }

        private static void WithBootstrap(Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass9-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var result = new ProductionBootstrapper(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Initialize(new ProductionBootstrapRequest
                    {
                        AdministratorLoginId = "root.admin",
                        AdministratorInitialPassword = "Temporary-Admin-Password-1",
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
                        BootstrapAtUtc = Now
                    });
                action(root, result);
            }
            finally
            {
                try { Directory.Delete(root, true); }
                catch { }
            }
        }

        private sealed class BlockingJsonRecordMapper : IJsonRecordMapper<TestAggregate>
        {
            private readonly ManualResetEventSlim _stageReadCompleted;
            private readonly ManualResetEventSlim _releaseStage;
            private readonly SystemTextJsonRecordMapper<TestAggregate> _inner =
                new SystemTextJsonRecordMapper<TestAggregate>();

            public BlockingJsonRecordMapper(
                ManualResetEventSlim stageReadCompleted,
                ManualResetEventSlim releaseStage)
            {
                _stageReadCompleted = stageReadCompleted;
                _releaseStage = releaseStage;
            }

            public TestAggregate FromJson(
                System.Text.Json.JsonElement element,
                System.Text.Json.JsonSerializerOptions options)
            {
                return _inner.FromJson(element, options);
            }

            public System.Text.Json.JsonElement ToJson(
                TestAggregate value,
                System.Text.Json.JsonSerializerOptions options)
            {
                _stageReadCompleted.Set();
                if (!_releaseStage.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The test did not release the staged mutation.");
                return _inner.ToJson(value, options);
            }
        }

        private sealed class FixedPrincipalProvider : IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;
            public FixedPrincipalProvider(AuthorizationPrincipal principal) { _principal = principal; }
            public AuthorizationPrincipal Load(string sessionId, string sessionToken, DateTime utcNow)
            {
                return _principal;
            }
        }

        private sealed class InMemoryStudentRepository : IStudentRecordRepository
        {
            private readonly StudentRecord _record;
            public InMemoryStudentRepository(StudentRecord record) { _record = record; }
            public string RepositoryName { get { return "students"; } }
            public RepositorySnapshot<StudentRecord> Read()
            {
                return new RepositorySnapshot<StudentRecord>(RepositoryName, 0, new[] { _record });
            }
            public StudentRecord FindById(string id)
            {
                return string.Equals(_record.Id, id, StringComparison.Ordinal) ? _record : null;
            }
            public void Write(
                IReadOnlyCollection<StudentRecord> records,
                long expectedRevision,
                string updatedByUserId)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class InMemoryEmployeeRepository : IEmployeeRecordRepository
        {
            public int ReadCount { get; private set; }
            public string RepositoryName { get { return "employees"; } }
            public RepositorySnapshot<EmployeeRecord> Read()
            {
                ReadCount++;
                return new RepositorySnapshot<EmployeeRecord>(
                    RepositoryName,
                    0,
                    new EmployeeRecord[0]);
            }
            public EmployeeRecord FindById(string id)
            {
                ReadCount++;
                return null;
            }
            public void Write(
                IReadOnlyCollection<EmployeeRecord> records,
                long expectedRevision,
                string updatedByUserId)
            {
                throw new NotSupportedException();
            }
        }

        public sealed class TestAggregate : IEntity
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }
    }
}
