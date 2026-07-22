using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Orchestration;
using IUIS.Domain.Academic;
using IUIS.Domain.Common;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass11CorrectiveAuditUnit2Tests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 21, 2, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void AllFortyNineMigrationFailureRollsBackExactBusinessBytesAndRecordsJournalEvidence()
        {
            WithBootstrap((root, bootstrap) =>
            {
                ConvertAllRepositoriesToLegacy(root);
                var catalog = new ProductionRepositoryCatalog();
                var before = catalog.All
                    .Where(item => !StringComparer.OrdinalIgnoreCase.Equals(
                        item.Name,
                        "transaction_journal"))
                    .ToDictionary(
                        item => item.Name,
                        item => File.ReadAllText(Path.Combine(root, item.FileName)),
                        StringComparer.OrdinalIgnoreCase);
                var injector = new ThrowAfterAppliedMutation(17);
                var service = new RepositoryEnvelopeMigrationService(
                    catalog,
                    new JsonInfrastructureOptions(root),
                    injector,
                    null);

                Assert.ThrowsExactly<InjectedTransactionFailureException>(() =>
                    service.MigrateAll(
                        Now.AddMinutes(1),
                        bootstrap.AdministratorUserId));

                Assert.IsFalse(string.IsNullOrWhiteSpace(injector.TransactionId));
                foreach (var descriptor in catalog.All.Where(item =>
                    !StringComparer.OrdinalIgnoreCase.Equals(
                        item.Name,
                        "transaction_journal")))
                {
                    var current = File.ReadAllText(
                        Path.Combine(root, descriptor.FileName));
                    Assert.AreEqual(
                        before[descriptor.Name],
                        current,
                        descriptor.Name + " was not restored byte-for-byte.");
                    Assert.IsTrue(
                        RepositoryEnvelopeJson.IsLegacy(current),
                        descriptor.Name + " no longer contains its original legacy envelope.");
                }

                var journal = ReadLatestJournal(root, injector.TransactionId);
                Assert.AreEqual(
                    TransactionJournalStatus.RolledBack,
                    journal.Status);
                Assert.AreEqual(17, journal.AppliedMutationCount);
                Assert.IsFalse(string.IsNullOrWhiteSpace(
                    journal.LastAppliedRepositoryName));
                Assert.AreEqual(
                    typeof(InjectedTransactionFailureException).FullName,
                    journal.FailureType);
                StringAssert.Contains(
                    journal.FailureMessage,
                    "Injected transaction failure");
                Assert.AreEqual(48, journal.Entries.Count);
                foreach (var entry in journal.Entries)
                    Assert.IsFalse(File.Exists(entry.BackupPath));

                var retry = new RepositoryEnvelopeMigrationService(
                    catalog,
                    new JsonInfrastructureOptions(root))
                    .MigrateAll(
                        Now.AddMinutes(2),
                        bootstrap.AdministratorUserId);
                Assert.IsFalse(retry.WasNoOp);
                Assert.AreEqual(48, retry.MigratedRepositoryCount);
                Assert.AreEqual(
                    RepositoryEnvelopeMigrationAuditStatus.Registered,
                    retry.AuditStatus);
                foreach (var descriptor in catalog.All)
                {
                    Assert.IsFalse(
                        RepositoryEnvelopeJson.IsLegacy(File.ReadAllText(
                            Path.Combine(root, descriptor.FileName))),
                        descriptor.Name + " was not canonical after retry.");
                }
            });
        }

        [TestMethod]
        public void MigrationAuditRegistrationFailureIsClassifiedAndRecoveredIdempotently()
        {
            WithBootstrap((root, bootstrap) =>
            {
                ConvertRepositoryToLegacy(root, "courses");
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var service = new RepositoryEnvelopeMigrationService(
                    catalog,
                    options,
                    null,
                    new ThrowBeforeAuditRegistration());

                var failure = Assert.ThrowsExactly<
                    RepositoryEnvelopeMigrationAuditRegistrationException>(() =>
                        service.MigrateAll(
                            Now.AddMinutes(3),
                            bootstrap.AdministratorUserId));

                Assert.IsFalse(failure.Result.WasNoOp);
                Assert.AreEqual(1, failure.Result.MigratedRepositoryCount);
                Assert.AreEqual(
                    RepositoryEnvelopeMigrationAuditStatus.RecoveryRequired,
                    failure.Result.AuditStatus);
                Assert.IsFalse(RepositoryEnvelopeJson.IsLegacy(
                    File.ReadAllText(Path.Combine(root, "courses.json"))));
                Assert.AreEqual(
                    0,
                    CountAuditRecords(root, failure.Result.TransactionId));

                var recoveryService = new RepositoryEnvelopeMigrationService(
                    catalog,
                    options);
                var recovered = recoveryService.RecoverAuditRegistration(
                    failure.Result);
                Assert.AreEqual(
                    RepositoryEnvelopeMigrationAuditStatus.Registered,
                    recovered.AuditStatus);
                Assert.AreEqual(
                    1,
                    CountAuditRecords(root, recovered.TransactionId));

                recoveryService.RecoverAuditRegistration(recovered);
                Assert.AreEqual(
                    1,
                    CountAuditRecords(root, recovered.TransactionId));
            });
        }

        [TestMethod]
        public void FivePass11MappersPreserveEveryPersistedFieldAcrossRoundTrip()
        {
            var options = JsonOptions();

            var enrollment = CreateRejectedArchivedEnrollment();
            var enrollmentJson = AssertExactRoundTrip(
                enrollment,
                new EnrollmentJsonMapper(),
                typeof(PersistedEnrollmentRecord),
                options);
            AssertPropertySet(
                enrollmentJson.GetProperty("subjectLines")[0],
                typeof(PersistedEnrollmentSubjectLine),
                options);

            var assessment = CreateCancelledArchivedAssessment();
            var assessmentJson = AssertExactRoundTrip(
                assessment,
                new TuitionAssessmentJsonMapper(),
                typeof(PersistedTuitionAssessmentRecord),
                options);
            AssertPropertySet(
                assessmentJson.GetProperty("chargeLines")[0],
                typeof(PersistedAssessmentChargeLine),
                options);

            var payment = CreateVoidedPayment();
            var paymentJson = AssertExactRoundTrip(
                payment,
                new PaymentJsonMapper(),
                typeof(PersistedPaymentRecord),
                options);
            AssertPropertySet(
                paymentJson.GetProperty("allocations")[0],
                typeof(PersistedPaymentAllocation),
                options);

            AssertExactRoundTrip(
                CreateCancelledArchivedAdjustment(),
                new FinancialAdjustmentJsonMapper(),
                typeof(PersistedFinancialAdjustmentRecord),
                options);
            AssertExactRoundTrip(
                CreateAppliedPercentageScholarship(),
                new ScholarshipAwardJsonMapper(),
                typeof(PersistedScholarshipAwardRecord),
                options);
        }

        [TestMethod]
        public void ScholarshipApplicationFailureRollsBackBothRepositoriesAndCanRetry()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var composition = new IuisCompositionRoot(root);
                var assessment = CreatePostedAssessment(
                    "ASM-2026-000901",
                    "ENR-2026-000901",
                    "STU-2026-000901");
                var award = CreateApprovedScholarship(
                    "SAW-2026-000901",
                    assessment.StudentId);
                composition.TuitionAssessments.Write(
                    new[] { assessment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.ScholarshipAwards.Write(
                    new[] { award },
                    0,
                    bootstrap.AdministratorUserId);

                var scholarshipPath = Path.Combine(
                    root,
                    "scholarship_awards.json");
                var adjustmentPath = Path.Combine(
                    root,
                    "financial_adjustments.json");
                var scholarshipBefore = File.ReadAllText(scholarshipPath);
                var adjustmentBefore = File.ReadAllText(adjustmentPath);
                var injector = new ThrowAfterAppliedMutation(1);
                var failingTransactions =
                    new JournaledApplicationTransactionCoordinator(
                        new JournaledTransactionCoordinator(
                            catalog,
                            options,
                            injector));
                var service = CreateScholarshipService(
                    composition,
                    failingTransactions);
                var request = ScholarshipRequest(assessment, award);

                Assert.ThrowsExactly<InjectedTransactionFailureException>(() =>
                    service.Apply(
                        "SES-2026-000901",
                        "token",
                        request,
                        Now.AddHours(1)));

                Assert.AreEqual(
                    scholarshipBefore,
                    File.ReadAllText(scholarshipPath));
                Assert.AreEqual(
                    adjustmentBefore,
                    File.ReadAllText(adjustmentPath));
                Assert.AreEqual(
                    ScholarshipAwardStatus.Approved,
                    composition.ScholarshipAwards.Read()
                        .Records.Single().Status);
                Assert.AreEqual(
                    0,
                    composition.FinancialAdjustments.Read().Records.Count);

                var journal = ReadLatestJournal(root, injector.TransactionId);
                Assert.AreEqual(
                    TransactionJournalStatus.RolledBack,
                    journal.Status);
                Assert.AreEqual(1, journal.AppliedMutationCount);
                Assert.AreEqual(2, journal.Entries.Count);
                Assert.AreEqual(
                    typeof(InjectedTransactionFailureException).FullName,
                    journal.FailureType);

                var retry = CreateScholarshipService(
                    composition,
                    composition.Transactions)
                    .Apply(
                        "SES-2026-000902",
                        "token",
                        request,
                        Now.AddHours(2));
                Assert.IsFalse(string.IsNullOrWhiteSpace(
                    retry.TransactionId));
                Assert.AreEqual(
                    ScholarshipAwardStatus.Applied,
                    composition.ScholarshipAwards.Read()
                        .Records.Single().Status);
                Assert.AreEqual(
                    FinancialAdjustmentStatus.Posted,
                    composition.FinancialAdjustments.Read()
                        .Records.Single().Status);
            });
        }

        [TestMethod]
        public void SourceTreeValidatorScansEveryUiCSharpFile()
        {
            var root = FindRepositoryRoot();
            var script = File.ReadAllText(Path.Combine(
                root,
                "build",
                "Test-IuisSourceTree.ps1"));

            StringAssert.Contains(script, "'src\\IUIS.SharedUI'");
            StringAssert.Contains(script, "'src\\IUIS.UserApp'");
            StringAssert.Contains(script, "'src\\IUIS.AdminApp'");
            StringAssert.Contains(script, "-Filter '*.cs'");
            StringAssert.Contains(script, "System\\.IO");
            StringAssert.Contains(script, "System\\.Text\\.Json");
            Assert.IsFalse(script.Contains("-Filter '*Form.cs'"));
        }

        private static JsonElement AssertExactRoundTrip<T>(
            T value,
            IJsonRecordMapper<T> mapper,
            Type persistedRecordType,
            JsonSerializerOptions options)
            where T : class, IEntity
        {
            var first = mapper.ToJson(value, options);
            AssertPropertySet(first, persistedRecordType, options);
            var restored = mapper.FromJson(first, options);
            var second = mapper.ToJson(restored, options);
            Assert.AreEqual(
                first.GetRawText(),
                second.GetRawText(),
                persistedRecordType.Name
                    + " did not preserve every serialized field.");
            return first;
        }

        private static void AssertPropertySet(
            JsonElement element,
            Type persistedType,
            JsonSerializerOptions options)
        {
            var expected = persistedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => options.PropertyNamingPolicy == null
                    ? property.Name
                    : options.PropertyNamingPolicy.ConvertName(property.Name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var actual = element.EnumerateObject()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            CollectionAssert.AreEqual(
                expected,
                actual,
                persistedType.Name + " JSON field set is incomplete.");
        }

        private static Enrollment CreateRejectedArchivedEnrollment()
        {
            var value = new Enrollment(
                "ENR-2026-001001",
                "STU-2026-001001",
                "APD-2026-001001",
                "CRS-2026-001001",
                "BSIT",
                "Bachelor of Science in Information Technology",
                "CUR-2026-001001",
                "V2026.1",
                Now,
                "USR-2026-000001");
            value.AddSubjectLine(
                new EnrollmentSubjectLine(
                    "SUB-2026-001001",
                    "IT-332",
                    "Integrative Programming",
                    3m,
                    3,
                    1,
                    true,
                    "IT3-A"),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.AddSubjectLine(
                new EnrollmentSubjectLine(
                    "SUB-2026-001002",
                    "IT-333",
                    "Systems Integration",
                    2.5m,
                    3,
                    1,
                    false,
                    "IT3-B"),
                Now.AddMinutes(2),
                "USR-2026-000001");
            value.Submit(
                Now.AddMinutes(3),
                "USR-2026-001001");
            value.BeginReview(
                Now.AddMinutes(4),
                "USR-2026-000002");
            value.Reject(
                "Prerequisite evidence requires correction.",
                Now.AddMinutes(5),
                "USR-2026-000002");
            value.Archive(
                Now.AddMinutes(6),
                "USR-2026-000003");
            return value;
        }

        private static TuitionAssessment CreateCancelledArchivedAssessment()
        {
            var value = new TuitionAssessment(
                "ASM-2026-001001",
                "STU-2026-001001",
                "ENR-2026-001001",
                "APD-2026-001001",
                "PHP",
                Now,
                "USR-2026-000001");
            value.AddChargeLine(
                new AssessmentChargeLine(
                    "ACL-2026-001001",
                    "ACR-2026-001001",
                    "TUITION",
                    "Tuition charge",
                    AssessmentChargeCategory.Tuition,
                    Money.PhilippinePeso(1500.25m)),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.AddChargeLine(
                new AssessmentChargeLine(
                    "ACL-2026-001002",
                    "ACR-2026-001002",
                    "LAB",
                    "Laboratory charge",
                    AssessmentChargeCategory.Laboratory,
                    Money.PhilippinePeso(250.75m)),
                Now.AddMinutes(2),
                "USR-2026-000001");
            value.CancelDraft(
                "Assessment superseded before posting.",
                Now.AddMinutes(3),
                "USR-2026-000002");
            value.Archive(
                Now.AddMinutes(4),
                "USR-2026-000003");
            return value;
        }

        private static Payment CreateVoidedPayment()
        {
            var value = new Payment(
                "PAY-2026-001001",
                "STU-2026-001001",
                "APD-2026-001001",
                Money.PhilippinePeso(1751m),
                PaymentMethod.BankTransfer,
                Now.AddMinutes(1),
                "BANK-TRACE-2026-001001",
                Now,
                "USR-2026-000001");
            value.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-001001",
                    "ASM-2026-001001",
                    Money.PhilippinePeso(1500.25m)),
                Now.AddMinutes(2),
                "USR-2026-000001");
            value.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-001002",
                    "ASM-2026-001002",
                    Money.PhilippinePeso(250.75m)),
                Now.AddMinutes(3),
                "USR-2026-000001");
            value.Post(
                "RCT-2026-001001",
                Now.AddMinutes(4),
                "USR-2026-000002");
            value.Void(
                "Bank transfer was reversed.",
                Now.AddMinutes(5),
                "USR-2026-000003");
            return value;
        }

        private static FinancialAdjustment
            CreateCancelledArchivedAdjustment()
        {
            var value = new FinancialAdjustment(
                "FAD-2026-001001",
                "STU-2026-001001",
                "ASM-2026-001001",
                FinancialAdjustmentDirection.Debit,
                Money.PhilippinePeso(125.50m),
                FinancialAdjustmentSourceKind.AdministrativeCorrection,
                "ADM-2026-001001",
                "Corrective debit with complete audit metadata.",
                Now,
                "USR-2026-000001");
            value.CancelPrepared(
                "Correction request was withdrawn.",
                Now.AddMinutes(1),
                "USR-2026-000002");
            value.Archive(
                Now.AddMinutes(2),
                "USR-2026-000003");
            return value;
        }

        private static ScholarshipAward
            CreateAppliedPercentageScholarship()
        {
            var value = new ScholarshipAward(
                "SAW-2026-001001",
                "STU-2026-001001",
                "SCP-2026-001001",
                "APD-2026-001001",
                ScholarshipEffectKind.PercentageOfEligibleCharges,
                "PHP",
                null,
                37.5m,
                Now,
                "USR-2026-000001");
            value.Approve(
                Now.AddMinutes(1),
                "USR-2026-000002");
            value.MarkApplied(
                "ASM-2026-001001",
                "FAD-2026-001001",
                Now.AddMinutes(2),
                "USR-2026-000003");
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
                    "ACL-2026-000901",
                    "ACR-2026-000901",
                    "TUITION",
                    "Tuition charge",
                    AssessmentChargeCategory.Tuition,
                    Money.PhilippinePeso(1500m)),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.Post(
                Now.AddMinutes(2),
                "USR-2026-000001");
            return value;
        }

        private static ScholarshipAward CreateApprovedScholarship(
            string id,
            string studentId)
        {
            var value = new ScholarshipAward(
                id,
                studentId,
                "SCP-2026-000901",
                "APD-2026-000001",
                ScholarshipEffectKind.FixedAmount,
                "PHP",
                Money.PhilippinePeso(500m),
                0m,
                Now,
                "USR-2026-000001");
            value.Approve(
                Now.AddMinutes(1),
                "USR-2026-000001");
            return value;
        }

        private static ScholarshipAwardApplicationService
            CreateScholarshipService(
                IuisCompositionRoot composition,
                IUIS.Application.Repositories.IApplicationTransactionCoordinator
                    transactions)
        {
            return new ScholarshipAwardApplicationService(
                new SessionAwareRequestExecutor(
                    new FixedPrincipalProvider(AdminPrincipal(
                        "finance.scholarship.apply",
                        "confidentiality.restricted")),
                    new PermissionResolver()),
                composition.ScholarshipAwards,
                composition.TuitionAssessments,
                composition.FinancialAdjustments,
                transactions,
                composition.IdentifierAllocator);
        }

        private static ScholarshipApplicationRequest ScholarshipRequest(
            TuitionAssessment assessment,
            ScholarshipAward award)
        {
            return new ScholarshipApplicationRequest
            {
                ExpectedScholarshipRepositoryRevision = 1,
                ExpectedAssessmentRepositoryRevision = 1,
                ExpectedAdjustmentRepositoryRevision = 0,
                ExpectedScholarshipEntityVersion = award.Version,
                ExpectedAssessmentEntityVersion = assessment.Version,
                ScholarshipAwardId = award.Id,
                AssessmentId = assessment.Id,
                EligibleChargeAmount = assessment.GrossAmount.Amount
            };
        }

        private static AuthorizationPrincipal AdminPrincipal(
            params string[] permissions)
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

        private static TransactionJournalRecord ReadLatestJournal(
            string root,
            string transactionId)
        {
            var envelope = RepositoryEnvelopeJson
                .Deserialize<TransactionJournalRecord>(
                    File.ReadAllText(Path.Combine(
                        root,
                        "transaction_journal.json")),
                    JsonOptions());
            return envelope.Records.Single(item =>
                StringComparer.Ordinal.Equals(
                    item.TransactionId,
                    transactionId));
        }

        private static int CountAuditRecords(
            string root,
            string transactionId)
        {
            return NewStore(root)
                .Read<JsonElement>("operational_report_runs")
                .Records.Count(item =>
                {
                    JsonElement property;
                    return item.ValueKind == JsonValueKind.Object
                        && item.TryGetProperty(
                            "transactionId",
                            out property)
                        && property.ValueKind == JsonValueKind.String
                        && StringComparer.Ordinal.Equals(
                            property.GetString(),
                            transactionId);
                });
        }

        private static void ConvertAllRepositoriesToLegacy(string root)
        {
            foreach (var descriptor in
                new ProductionRepositoryCatalog().All)
                ConvertRepositoryToLegacy(root, descriptor.Name);
        }

        private static void ConvertRepositoryToLegacy(
            string root,
            string repositoryName)
        {
            var catalog = new ProductionRepositoryCatalog();
            var descriptor = catalog.Get(repositoryName);
            var path = Path.Combine(root, descriptor.FileName);
            var options = JsonOptions();
            using (var document = JsonDocument.Parse(
                File.ReadAllText(path)))
            {
                var source = document.RootElement;
                var legacy = new Dictionary<string, object>
                {
                    {
                        "repository",
                        source.GetProperty("repositoryName").GetString()
                    },
                    {
                        "schemaVersion",
                        source.GetProperty("schemaVersion").GetInt32()
                    },
                    {
                        "revision",
                        source.GetProperty("revision").GetInt64()
                    },
                    {
                        "createdAtUtc",
                        "2026-01-01T00:00:00Z"
                    },
                    {
                        "updatedAtUtc",
                        source.GetProperty("updatedAtUtc").GetDateTime()
                    },
                    {
                        "updatedByUserId",
                        source.GetProperty("updatedByUserId").GetString()
                    },
                    {
                        "records",
                        source.GetProperty("records").Clone()
                    }
                };
                File.WriteAllText(
                    path,
                    JsonSerializer.Serialize(legacy, options));
            }
        }

        private static string FindRepositoryRoot()
        {
            var candidates = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                var directory = new DirectoryInfo(candidate);
                while (directory != null)
                {
                    if (File.Exists(Path.Combine(
                        directory.FullName,
                        "IUIS.sln")))
                        return directory.FullName;
                    directory = directory.Parent;
                }
            }

            throw new DirectoryNotFoundException(
                "The IUIS repository root could not be located.");
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

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass11-Audit2-"
                    + Guid.NewGuid().ToString("N"));
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

        private sealed class ThrowAfterAppliedMutation
            : ITransactionFailureInjector
        {
            private readonly int _appliedMutationCount;

            public ThrowAfterAppliedMutation(int appliedMutationCount)
            {
                _appliedMutationCount = appliedMutationCount;
            }

            public string TransactionId { get; private set; }

            public void OnStage(TransactionExecutionContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                TransactionId = context.TransactionId;
                if (context.Stage == TransactionExecutionStage.MutationApplied
                    && context.AppliedMutationCount == _appliedMutationCount)
                    throw new InjectedTransactionFailureException(
                        "Injected transaction failure after mutation "
                        + _appliedMutationCount + ".");
            }
        }

        private sealed class ThrowBeforeAuditRegistration
            : IRepositoryEnvelopeMigrationAuditFailureInjector
        {
            public void BeforeAuditRegistration(
                RepositoryEnvelopeMigrationAuditRecord record)
            {
                throw new InjectedAuditFailureException(
                    "Injected migration audit-registration failure.");
            }
        }

        private sealed class InjectedTransactionFailureException
            : InvalidOperationException
        {
            public InjectedTransactionFailureException(string message)
                : base(message)
            {
            }
        }

        private sealed class InjectedAuditFailureException
            : InvalidOperationException
        {
            public InjectedAuditFailureException(string message)
                : base(message)
            {
            }
        }

        private sealed class FixedPrincipalProvider
            : IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;

            public FixedPrincipalProvider(
                AuthorizationPrincipal principal)
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
