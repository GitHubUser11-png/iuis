using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Orchestration;
using IUIS.Domain.Academic;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass11EnvelopeTokenFinanceTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 20, 15, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void BootstrapProducesExactlyFortyNineCanonicalEnvelopes()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var files = Directory.GetFiles(root, "*.json");
                Assert.AreEqual(49, files.Length);
                var expected = new[]
                {
                    "records",
                    "repositoryName",
                    "revision",
                    "schemaVersion",
                    "updatedAtUtc",
                    "updatedByUserId"
                };
                foreach (var file in files)
                {
                    using (var document = JsonDocument.Parse(File.ReadAllText(file)))
                    {
                        var actual = document.RootElement.EnumerateObject()
                            .Select(item => item.Name)
                            .OrderBy(item => item, StringComparer.Ordinal)
                            .ToArray();
                        CollectionAssert.AreEqual(expected, actual);
                        Assert.AreEqual(
                            Path.GetFileNameWithoutExtension(file),
                            document.RootElement.GetProperty("repositoryName").GetString());
                    }
                }
            });
        }

        [TestMethod]
        public void LegacyEnvelopeReaderProducesCanonicalOnlyWrites()
        {
            const string legacy = "{"
                + "\"repository\":\"courses\","
                + "\"schemaVersion\":1,"
                + "\"revision\":4,"
                + "\"createdAtUtc\":\"2026-01-01T00:00:00Z\","
                + "\"updatedAtUtc\":\"2026-01-02T00:00:00Z\","
                + "\"updatedByUserId\":\"USR-2026-000001\","
                + "\"records\":[]}";
            var options = JsonOptions();
            var envelope = RepositoryEnvelopeJson.Deserialize<JsonElement>(legacy, options);
            Assert.AreEqual("courses", envelope.RepositoryName);
            Assert.AreEqual(4L, envelope.Revision);
            Assert.IsTrue(RepositoryEnvelopeJson.IsLegacy(legacy));

            var canonical = RepositoryEnvelopeJson.Serialize(envelope, options);
            Assert.IsFalse(RepositoryEnvelopeJson.IsLegacy(canonical));
            using (var document = JsonDocument.Parse(canonical))
            {
                Assert.IsTrue(document.RootElement.TryGetProperty("repositoryName", out _));
                Assert.IsFalse(document.RootElement.TryGetProperty("repository", out _));
                Assert.IsFalse(document.RootElement.TryGetProperty("createdAtUtc", out _));
            }
        }

        [TestMethod]
        public void JournaledMigrationRewritesAllFortyNineAndIsIdempotent()
        {
            WithBootstrap((root, bootstrap) =>
            {
                ConvertAllRepositoriesToLegacy(root);
                var service = new RepositoryEnvelopeMigrationService(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root));
                var result = service.MigrateAll(
                    Now.AddMinutes(1),
                    bootstrap.AdministratorUserId);

                Assert.IsFalse(result.WasNoOp);
                Assert.AreEqual(49, result.MigratedRepositoryCount);
                Assert.AreEqual(49, result.MigratedRepositoryNames.Count);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.AuditRecordId));
                foreach (var descriptor in new ProductionRepositoryCatalog().All)
                {
                    Assert.IsFalse(
                        RepositoryEnvelopeJson.IsLegacy(File.ReadAllText(
                            Path.Combine(root, descriptor.FileName))),
                        descriptor.Name + " was not canonicalized.");
                }

                var second = service.MigrateAll(
                    Now.AddMinutes(2),
                    bootstrap.AdministratorUserId);
                Assert.IsTrue(second.WasNoOp);
                Assert.AreEqual(0, second.MigratedRepositoryCount);
            });
        }

        [TestMethod]
        public void MigrationPreservesBusinessRepositoryRevisionAndRecords()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var store = NewStore(root);
                var courses = store.Read<JsonElement>("courses");
                courses.Records.Add(JsonSerializer.SerializeToElement(
                    new { id = "CRS-2026-009999", name = "Legacy payload" },
                    JsonOptions()));
                courses.UpdatedByUserId = bootstrap.AdministratorUserId;
                store.Write("courses", courses, courses.Revision);
                var before = store.Read<JsonElement>("courses");

                ConvertAllRepositoriesToLegacy(root);
                new RepositoryEnvelopeMigrationService(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .MigrateAll(Now.AddMinutes(1), bootstrap.AdministratorUserId);

                var after = store.Read<JsonElement>("courses");
                Assert.AreEqual(before.Revision, after.Revision);
                Assert.AreEqual(1, after.Records.Count);
                Assert.AreEqual(
                    "CRS-2026-009999",
                    after.Records[0].GetProperty("id").GetString());
            });
        }

        [TestMethod]
        public void AuthenticationPersistsOnlyDigestAndReturnsRawTokenOnce()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var result = new AuthenticationService(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Authenticate(
                        "root.admin",
                        "Temporary-Admin-Password-1",
                        "AdministratorApplication",
                        Now.AddMinutes(1));

                Assert.IsTrue(result.Succeeded);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.SessionToken));
                var raw = File.ReadAllText(Path.Combine(root, "sessions.json"));
                Assert.IsFalse(raw.Contains(result.SessionToken));
                Assert.IsFalse(raw.Contains("\"tokenHash\""));

                var session = NewStore(root)
                    .Read<PersistedSessionRecord>("sessions")
                    .Records.Single(item => item.Id == result.SessionId);
                var protector = new SessionTokenProtector();
                Assert.AreEqual(
                    SessionTokenProtector.CurrentDigestVersion,
                    session.TokenDigestVersion);
                Assert.IsTrue(protector.Verify(session.TokenDigest, result.SessionToken));
                Assert.IsFalse(protector.Verify(
                    session.TokenDigest,
                    result.SessionToken + "tampered"));
            });
        }

        [TestMethod]
        public void LegacySessionIsRejectedThenRevokedAndBearerMaterialRemoved()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var store = NewStore(root);
                var sessions = store.Read<PersistedSessionRecord>("sessions");
                var user = store.Read<PersistedUserAccount>("users").Records.Single();
                sessions.Records.Add(new PersistedSessionRecord
                {
                    Id = "SES-2026-009999",
                    UserId = bootstrap.AdministratorUserId,
                    TokenDigestVersion = 0,
                    TokenDigest = null,
                    LegacyTokenHash = "legacy-active-bearer",
                    SecurityStampSnapshot = user.SecurityStamp,
                    ApplicationKind = SessionApplicationKind.AdministratorApplication.ToString(),
                    Purpose = SessionPurpose.FullAccess.ToString(),
                    Status = UserSessionStatus.Active.ToString(),
                    IssuedAtUtc = Now,
                    LastActivityAtUtc = Now,
                    InactivityExpiresAtUtc = Now.AddHours(1),
                    AbsoluteExpiresAtUtc = Now.AddHours(8)
                });
                sessions.UpdatedByUserId = bootstrap.AdministratorUserId;
                store.Write("sessions", sessions, sessions.Revision);

                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new JsonAuthorizationPrincipalProvider(
                        new ProductionRepositoryCatalog(),
                        new JsonInfrastructureOptions(root))
                        .Load(
                            "SES-2026-009999",
                            "legacy-active-bearer",
                            Now.AddMinutes(1)));

                var result = new SessionSecurityMigrationService(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .RevokeLegacySessions(
                        Now.AddMinutes(2),
                        bootstrap.AdministratorUserId);
                Assert.AreEqual(1, result.LegacySessionCount);
                Assert.AreEqual(1, result.RevokedActiveSessionCount);

                var migrated = store.Read<PersistedSessionRecord>("sessions")
                    .Records.Single(item => item.Id == "SES-2026-009999");
                Assert.AreEqual(UserSessionStatus.Revoked.ToString(), migrated.Status);
                Assert.IsNull(migrated.TokenDigest);
                Assert.IsNull(migrated.LegacyTokenHash);
                Assert.IsFalse(File.ReadAllText(Path.Combine(root, "sessions.json"))
                    .Contains("legacy-active-bearer"));
            });
        }

        [TestMethod]
        public void FiveNewSpecializedMappersRoundTripLifecycleAndImmutableState()
        {
            var options = JsonOptions();
            var enrollment = CreateApprovedEnrollment(
                "ENR-2026-000201",
                "STU-2026-000201");
            var assessment = CreatePostedAssessment(
                "ASM-2026-000201",
                enrollment.Id,
                enrollment.StudentId);
            var payment = CreatePostedPayment(
                "PAY-2026-000201",
                assessment.Id,
                assessment.StudentId);
            var adjustment = CreatePostedAdjustment(
                "FAD-2026-000201",
                assessment.Id,
                assessment.StudentId);
            var award = CreateApprovedScholarship(
                "SAW-2026-000201",
                assessment.StudentId);

            var restoredEnrollment = new EnrollmentJsonMapper().FromJson(
                new EnrollmentJsonMapper().ToJson(enrollment, options), options);
            var restoredAssessment = new TuitionAssessmentJsonMapper().FromJson(
                new TuitionAssessmentJsonMapper().ToJson(assessment, options), options);
            var restoredPayment = new PaymentJsonMapper().FromJson(
                new PaymentJsonMapper().ToJson(payment, options), options);
            var restoredAdjustment = new FinancialAdjustmentJsonMapper().FromJson(
                new FinancialAdjustmentJsonMapper().ToJson(adjustment, options), options);
            var restoredAward = new ScholarshipAwardJsonMapper().FromJson(
                new ScholarshipAwardJsonMapper().ToJson(award, options), options);

            Assert.AreEqual(EnrollmentStatus.Approved, restoredEnrollment.Status);
            Assert.AreEqual(enrollment.Version, restoredEnrollment.Version);
            Assert.AreEqual(1, restoredEnrollment.SubjectLines.Count);
            Assert.AreEqual(TuitionAssessmentStatus.Posted, restoredAssessment.Status);
            Assert.AreEqual(1500m, restoredAssessment.GrossAmount.Amount);
            Assert.AreEqual(PaymentStatus.Posted, restoredPayment.Status);
            Assert.AreEqual("RCT-2026-000201", restoredPayment.ReceiptNumber);
            Assert.AreEqual(1, restoredPayment.Allocations.Count);
            Assert.AreEqual(FinancialAdjustmentStatus.Posted, restoredAdjustment.Status);
            Assert.AreEqual(FinancialAdjustmentDirection.Credit, restoredAdjustment.Direction);
            Assert.AreEqual(ScholarshipAwardStatus.Approved, restoredAward.Status);
            Assert.AreEqual(500m, restoredAward.FixedAmount.Amount);
        }

        [TestMethod]
        public void UnsupportedFinanceRecordVersionFailsClosed()
        {
            var record = new PersistedPaymentRecord
            {
                RecordSchemaVersion = 99,
                Id = "PAY-2026-000202",
                Version = 1,
                CreatedAtUtc = Now,
                CreatedByUserId = "USR-2026-000001",
                UpdatedAtUtc = Now,
                UpdatedByUserId = "USR-2026-000001",
                StudentId = "STU-2026-000201",
                AcademicPeriodId = "APD-2026-000001",
                Amount = 100m,
                CurrencyCode = "PHP",
                Method = PaymentMethod.Cash.ToString(),
                ReceivedAtUtc = Now,
                Status = PaymentStatus.Draft.ToString(),
                Allocations = new List<PersistedPaymentAllocation>()
            };
            var json = JsonSerializer.SerializeToElement(record, JsonOptions());
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                new PaymentJsonMapper().FromJson(json, JsonOptions()));
        }

        [TestMethod]
        public void ReadinessCatalogReflectsFinalPass12Activation()
        {
            Assert.AreEqual(
                18,
                AggregateMapperReadinessCatalog.All.Count(item =>
                    item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted));
            Assert.AreEqual(
                0,
                AggregateMapperReadinessCatalog.All.Count(item =>
                    item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason));
        }

        [TestMethod]
        public void StudentFinanceOverviewUsesSessionDerivedOwnershipOnly()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                composition.Enrollments.Write(
                    new[]
                    {
                        CreateApprovedEnrollment("ENR-2026-000301", "STU-2026-000301"),
                        CreateApprovedEnrollment("ENR-2026-000302", "STU-2026-000302")
                    },
                    0,
                    bootstrap.AdministratorUserId);
                var service = new StudentFinanceQueryService(
                    new SessionAwareRequestExecutor(
                        new FixedPrincipalProvider(StudentPrincipal(
                            "STU-2026-000301",
                            "student.finance.read")),
                        new PermissionResolver()),
                    composition.Enrollments,
                    composition.TuitionAssessments,
                    composition.Payments,
                    composition.FinancialAdjustments,
                    composition.ScholarshipAwards);

                var result = service.GetOwnOverview(
                    "SES-2026-000301",
                    "token",
                    Now.AddHours(1));
                Assert.AreEqual("STU-2026-000301", result.StudentId);
                Assert.AreEqual(1, result.Enrollments.Count);
                Assert.AreEqual("ENR-2026-000301", result.Enrollments.Single().EnrollmentId);
            });
        }

        [TestMethod]
        public void StalePaymentRepositoryRevisionPreventsAnyPaymentWrite()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var assessment = CreatePostedAssessment(
                    "ASM-2026-000401",
                    "ENR-2026-000401",
                    "STU-2026-000401");
                composition.TuitionAssessments.Write(
                    new[] { assessment },
                    0,
                    bootstrap.AdministratorUserId);
                var service = new PaymentPostingService(
                    new SessionAwareRequestExecutor(
                        new FixedPrincipalProvider(AdminPrincipal("finance.payment.post")),
                        new PermissionResolver()),
                    composition.TuitionAssessments,
                    composition.Payments,
                    composition.Transactions,
                    composition.IdentifierAllocator);

                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    service.Post(
                        "SES-2026-000401",
                        "token",
                        new PaymentPostingRequest
                        {
                            ExpectedAssessmentRepositoryRevision = 1,
                            ExpectedPaymentRepositoryRevision = 1,
                            StudentId = assessment.StudentId,
                            AcademicPeriodId = assessment.AcademicPeriodId,
                            Amount = 1500m,
                            CurrencyCode = "PHP",
                            Method = PaymentMethod.Cash,
                            ReceivedAtUtc = Now.AddHours(1),
                            Allocations = new[]
                            {
                                new PaymentAllocationInput
                                {
                                    AssessmentId = assessment.Id,
                                    Amount = 1500m
                                }
                            }
                        },
                        Now.AddHours(1).AddMinutes(1)));
                Assert.AreEqual(0, composition.Payments.Read().Records.Count);
            });
        }

        [TestMethod]
        public void ScholarshipApplicationCommitsAwardAndAdjustmentTogether()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var assessment = CreatePostedAssessment(
                    "ASM-2026-000501",
                    "ENR-2026-000501",
                    "STU-2026-000501");
                var award = CreateApprovedScholarship(
                    "SAW-2026-000501",
                    assessment.StudentId);
                composition.TuitionAssessments.Write(
                    new[] { assessment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.ScholarshipAwards.Write(
                    new[] { award },
                    0,
                    bootstrap.AdministratorUserId);

                var service = new ScholarshipAwardApplicationService(
                    new SessionAwareRequestExecutor(
                        new FixedPrincipalProvider(AdminPrincipal(
                            "finance.scholarship.apply",
                            "confidentiality.restricted")),
                        new PermissionResolver()),
                    composition.ScholarshipAwards,
                    composition.TuitionAssessments,
                    composition.FinancialAdjustments,
                    composition.Transactions,
                    composition.IdentifierAllocator);
                var result = service.Apply(
                    "SES-2026-000501",
                    "token",
                    new ScholarshipApplicationRequest
                    {
                        ExpectedScholarshipRepositoryRevision = 1,
                        ExpectedAssessmentRepositoryRevision = 1,
                        ExpectedAdjustmentRepositoryRevision = 0,
                        ExpectedScholarshipEntityVersion = award.Version,
                        ExpectedAssessmentEntityVersion = assessment.Version,
                        ScholarshipAwardId = award.Id,
                        AssessmentId = assessment.Id,
                        EligibleChargeAmount = assessment.GrossAmount.Amount
                    },
                    Now.AddHours(1));

                Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
                Assert.AreEqual(2L, result.RepositoryRevision);
                Assert.AreEqual(1L, result.SecondaryRepositoryRevision);
                Assert.AreEqual(
                    ScholarshipAwardStatus.Applied,
                    composition.ScholarshipAwards.Read().Records.Single().Status);
                Assert.AreEqual(
                    FinancialAdjustmentStatus.Posted,
                    composition.FinancialAdjustments.Read().Records.Single().Status);
            });
        }

        private static Enrollment CreateApprovedEnrollment(string id, string studentId)
        {
            var value = new Enrollment(
                id,
                studentId,
                "APD-2026-000001",
                "CRS-2026-000001",
                "BSIT",
                "Bachelor of Science in Information Technology",
                "CUR-2026-000001",
                "V1",
                Now,
                "USR-2026-000001");
            value.AddSubjectLine(
                new EnrollmentSubjectLine(
                    "SUB-2026-000001",
                    "IT-332",
                    "Integrative Programming",
                    3m,
                    3,
                    1,
                    true,
                    "IT3-A"),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.Submit(Now.AddMinutes(2), "USR-2026-000001");
            value.BeginReview(Now.AddMinutes(3), "USR-2026-000001");
            value.Approve(null, Now.AddMinutes(4), "USR-2026-000001");
            return value;
        }

        private static TuitionAssessment CreatePostedAssessment(
            string id,
            string enrollmentId,
            string studentId)
        {
            var value = new TuitionAssessment(
                id,
                studentId,
                enrollmentId,
                "APD-2026-000001",
                "PHP",
                Now,
                "USR-2026-000001");
            value.AddChargeLine(
                new AssessmentChargeLine(
                    "ACL-2026-000001",
                    "ACR-2026-000001",
                    "TUITION",
                    "Tuition charge",
                    AssessmentChargeCategory.Tuition,
                    Money.PhilippinePeso(1500m)),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.Post(Now.AddMinutes(2), "USR-2026-000001");
            return value;
        }

        private static Payment CreatePostedPayment(
            string id,
            string assessmentId,
            string studentId)
        {
            var value = new Payment(
                id,
                studentId,
                "APD-2026-000001",
                Money.PhilippinePeso(1500m),
                PaymentMethod.Cash,
                Now,
                null,
                Now,
                "USR-2026-000001");
            value.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-000201",
                    assessmentId,
                    Money.PhilippinePeso(1500m)),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.Post(
                "RCT-2026-000201",
                Now.AddMinutes(2),
                "USR-2026-000001");
            return value;
        }

        private static FinancialAdjustment CreatePostedAdjustment(
            string id,
            string assessmentId,
            string studentId)
        {
            var value = new FinancialAdjustment(
                id,
                studentId,
                assessmentId,
                FinancialAdjustmentDirection.Credit,
                Money.PhilippinePeso(100m),
                FinancialAdjustmentSourceKind.AdministrativeCorrection,
                "ADM-2026-000001",
                "Approved credit",
                Now,
                "USR-2026-000001");
            value.Post(Now.AddMinutes(1), "USR-2026-000001");
            return value;
        }

        private static ScholarshipAward CreateApprovedScholarship(string id, string studentId)
        {
            var value = new ScholarshipAward(
                id,
                studentId,
                "SCP-2026-000001",
                "APD-2026-000001",
                ScholarshipEffectKind.FixedAmount,
                "PHP",
                Money.PhilippinePeso(500m),
                0m,
                Now,
                "USR-2026-000001");
            value.Approve(Now.AddMinutes(1), "USR-2026-000001");
            return value;
        }

        private static AuthorizationPrincipal StudentPrincipal(
            string studentId,
            params string[] permissions)
        {
            return new AuthorizationPrincipal(
                "USR-2026-000301",
                studentId,
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                "stamp",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000301",
                        true,
                        permissions)
                },
                null,
                null);
        }

        private static AuthorizationPrincipal AdminPrincipal(params string[] permissions)
        {
            return new AuthorizationPrincipal(
                "USR-2026-000001",
                "EMP-2026-000001",
                PrimaryRole.Administrator,
                SessionApplicationKind.AdministratorApplication,
                SessionPurpose.FullAccess,
                "stamp",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000001",
                        true,
                        permissions)
                },
                null,
                null);
        }

        private static void ConvertAllRepositoriesToLegacy(string root)
        {
            var options = JsonOptions();
            foreach (var descriptor in new ProductionRepositoryCatalog().All)
            {
                var path = Path.Combine(root, descriptor.FileName);
                using (var document = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    var source = document.RootElement;
                    var legacy = new Dictionary<string, object>
                    {
                        { "repository", source.GetProperty("repositoryName").GetString() },
                        { "schemaVersion", source.GetProperty("schemaVersion").GetInt32() },
                        { "revision", source.GetProperty("revision").GetInt64() },
                        { "createdAtUtc", "2026-01-01T00:00:00Z" },
                        { "updatedAtUtc", source.GetProperty("updatedAtUtc").GetDateTime() },
                        { "updatedByUserId", source.GetProperty("updatedByUserId").GetString() },
                        { "records", source.GetProperty("records").Clone() }
                    };
                    File.WriteAllText(path, JsonSerializer.Serialize(legacy, options));
                }
            }
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static JsonRepositoryStore NewStore(string root)
        {
            return new JsonRepositoryStore(
                new ProductionRepositoryCatalog(),
                new JsonInfrastructureOptions(root));
        }

        private static void WithBootstrap(Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass11-" + Guid.NewGuid().ToString("N"));
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

        private sealed class FixedPrincipalProvider : IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;
            public FixedPrincipalProvider(AuthorizationPrincipal principal)
            {
                _principal = principal;
            }
            public AuthorizationPrincipal Load(
                string sessionId,
                string sessionToken,
                DateTime utcNow)
            {
                return _principal;
            }
        }
    }
}
