using System;
using System.IO;
using System.Linq;
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
    public sealed class Pass11CorrectiveClosureTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 21, 1, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void PersistedRecordSchemaVersionMustBeExactlyOne()
        {
            var mapper = new PaymentJsonMapper();
            var options = JsonOptions();
            foreach (var version in new[] { 0, -1, 2, 99 })
            {
                var record = new PersistedPaymentRecord
                {
                    RecordSchemaVersion = version
                };
                var json = JsonSerializer.SerializeToElement(record, options);
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    mapper.FromJson(json, options),
                    "Schema version " + version + " must fail closed.");
            }
        }

        [TestMethod]
        public void SpecializedMapperWritesSchemaVersionOne()
        {
            var json = new PaymentJsonMapper().ToJson(
                CreatePostedPayment(
                    "PAY-2026-000601",
                    "ASM-2026-000601",
                    "STU-2026-000601"),
                JsonOptions());

            Assert.AreEqual(
                1,
                json.GetProperty("recordSchemaVersion").GetInt32());
        }

        [TestMethod]
        public void PaymentPostingRejectsStaleAssessmentEntityVersionBeforeIdentifierAllocation()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var assessment = CreatePostedAssessment(
                    "ASM-2026-000601",
                    "ENR-2026-000601",
                    "STU-2026-000601");
                composition.TuitionAssessments.Write(
                    new[] { assessment },
                    0,
                    bootstrap.AdministratorUserId);
                var service = CreatePaymentPostingService(composition);

                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    service.Post(
                        "SES-2026-000601",
                        "token",
                        PaymentRequest(
                            assessment,
                            assessment.Version - 1L),
                        Now.AddHours(1)));

                Assert.AreEqual(0, composition.Payments.Read().Records.Count);
                Assert.AreEqual(
                    "PAY-2026-000001",
                    composition.IdentifierAllocator.Allocate(
                        "PAY",
                        2026,
                        bootstrap.AdministratorUserId));
                Assert.AreEqual(
                    "PAL-2026-000001",
                    composition.IdentifierAllocator.Allocate(
                        "PAL",
                        2026,
                        bootstrap.AdministratorUserId));
                Assert.AreEqual(
                    "RCT-2026-000001",
                    composition.IdentifierAllocator.Allocate(
                        "RCT",
                        2026,
                        bootstrap.AdministratorUserId));
            });
        }

        [TestMethod]
        public void PaymentPostingAcceptsMatchingAssessmentEntityVersion()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var assessment = CreatePostedAssessment(
                    "ASM-2026-000602",
                    "ENR-2026-000602",
                    "STU-2026-000602");
                composition.TuitionAssessments.Write(
                    new[] { assessment },
                    0,
                    bootstrap.AdministratorUserId);

                var result = CreatePaymentPostingService(composition).Post(
                    "SES-2026-000602",
                    "token",
                    PaymentRequest(assessment, assessment.Version),
                    Now.AddHours(1));

                var stored = composition.Payments.Read().Records.Single();
                Assert.AreEqual(PaymentStatus.Posted, stored.Status);
                Assert.AreEqual(
                    assessment.Id,
                    stored.Allocations.Single().AssessmentId);
                Assert.AreEqual(result.ReceiptNumber, stored.ReceiptNumber);
                Assert.AreEqual(3L, assessment.Version);
            });
        }

        [TestMethod]
        public void StudentFinanceProjectionExcludesUnreleasedLifecycleStates()
        {
            WithBootstrap((root, bootstrap) =>
            {
                const string studentId = "STU-2026-000701";
                var composition = new IuisCompositionRoot(root);
                var draftEnrollment = CreateDraftEnrollment(
                    "ENR-2026-000701",
                    studentId);
                var approvedEnrollment = CreateApprovedEnrollment(
                    "ENR-2026-000702",
                    studentId);
                var draftAssessment = CreateDraftAssessment(
                    "ASM-2026-000701",
                    draftEnrollment.Id,
                    studentId,
                    "USD");
                var postedAssessment = CreatePostedAssessment(
                    "ASM-2026-000702",
                    approvedEnrollment.Id,
                    studentId);
                var draftPayment = CreateDraftPayment(
                    "PAY-2026-000701",
                    studentId,
                    "USD");
                var postedPayment = CreatePostedPayment(
                    "PAY-2026-000702",
                    postedAssessment.Id,
                    studentId);
                var preparedAdjustment = CreatePreparedAdjustment(
                    "FAD-2026-000701",
                    draftAssessment.Id,
                    studentId,
                    "USD");
                var postedAdjustment = CreatePostedAdjustment(
                    "FAD-2026-000702",
                    postedAssessment.Id,
                    studentId);
                var preparedScholarship = CreatePreparedScholarship(
                    "SAW-2026-000701",
                    studentId,
                    "USD");
                var approvedScholarship = CreateApprovedScholarship(
                    "SAW-2026-000702",
                    studentId);

                composition.Enrollments.Write(
                    new[] { draftEnrollment, approvedEnrollment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.TuitionAssessments.Write(
                    new[] { draftAssessment, postedAssessment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.Payments.Write(
                    new[] { draftPayment, postedPayment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.FinancialAdjustments.Write(
                    new[] { preparedAdjustment, postedAdjustment },
                    0,
                    bootstrap.AdministratorUserId);
                composition.ScholarshipAwards.Write(
                    new[] { preparedScholarship, approvedScholarship },
                    0,
                    bootstrap.AdministratorUserId);

                var service = new StudentFinanceQueryService(
                    new SessionAwareRequestExecutor(
                        new FixedPrincipalProvider(StudentPrincipal(
                            studentId,
                            "student.finance.read")),
                        new PermissionResolver()),
                    composition.Enrollments,
                    composition.TuitionAssessments,
                    composition.Payments,
                    composition.FinancialAdjustments,
                    composition.ScholarshipAwards);
                var result = service.GetOwnOverview(
                    "SES-2026-000701",
                    "token",
                    Now.AddHours(2));

                Assert.AreEqual("PHP", result.CurrencyCode);
                Assert.AreEqual(1, result.Enrollments.Count);
                Assert.AreEqual(
                    approvedEnrollment.Id,
                    result.Enrollments.Single().EnrollmentId);
                Assert.AreEqual(1, result.Assessments.Count);
                Assert.AreEqual(
                    postedAssessment.Id,
                    result.Assessments.Single().AssessmentId);
                Assert.AreEqual(1, result.Payments.Count);
                Assert.AreEqual(
                    postedPayment.Id,
                    result.Payments.Single().PaymentId);
                Assert.AreEqual(1, result.Scholarships.Count);
                Assert.AreEqual(
                    approvedScholarship.Id,
                    result.Scholarships.Single().ScholarshipAwardId);
                Assert.AreEqual(1500m, result.PostedAssessmentTotal);
                Assert.AreEqual(100m, result.PostedAdjustmentCreditTotal);
                Assert.AreEqual(1500m, result.PostedPaymentTotal);
                Assert.AreEqual(-100m, result.Balance);
            });
        }

        [TestMethod]
        public void PostedPaymentRejectedMutationIsExceptionAtomic()
        {
            var payment = CreatePostedPayment(
                "PAY-2026-000801",
                "ASM-2026-000801",
                "STU-2026-000801");
            var version = payment.Version;
            var updatedAtUtc = payment.UpdatedAtUtc;
            var updatedByUserId = payment.UpdatedByUserId;
            var receiptNumber = payment.ReceiptNumber;
            var allocationId = payment.Allocations.Single().AllocationId;
            var allocatedAmount = payment.AllocatedAmount.Amount;

            Assert.ThrowsExactly<DomainValidationException>(() =>
                payment.AddAllocation(
                    new PaymentAllocation(
                        "PAL-2026-000802",
                        "ASM-2026-000802",
                        Money.PhilippinePeso(1m)),
                    Now.AddHours(3),
                    "USR-2026-000001"));

            Assert.AreEqual(PaymentStatus.Posted, payment.Status);
            Assert.AreEqual(version, payment.Version);
            Assert.AreEqual(updatedAtUtc, payment.UpdatedAtUtc);
            Assert.AreEqual(updatedByUserId, payment.UpdatedByUserId);
            Assert.AreEqual(receiptNumber, payment.ReceiptNumber);
            Assert.AreEqual(1, payment.Allocations.Count);
            Assert.AreEqual(
                allocationId,
                payment.Allocations.Single().AllocationId);
            Assert.AreEqual(allocatedAmount, payment.AllocatedAmount.Amount);
        }

        [TestMethod]
        public void PostedFinancialAdjustmentRejectedMutationIsExceptionAtomic()
        {
            var adjustment = CreatePostedAdjustment(
                "FAD-2026-000801",
                "ASM-2026-000801",
                "STU-2026-000801");
            var version = adjustment.Version;
            var updatedAtUtc = adjustment.UpdatedAtUtc;
            var updatedByUserId = adjustment.UpdatedByUserId;
            var postedAtUtc = adjustment.PostedAtUtc;
            var postedByUserId = adjustment.PostedByUserId;
            var reason = adjustment.Reason;
            var amount = adjustment.Amount.Amount;

            Assert.ThrowsExactly<DomainValidationException>(() =>
                adjustment.CancelPrepared(
                    "A posted adjustment cannot be cancelled as prepared.",
                    Now.AddHours(3),
                    "USR-2026-000001"));

            Assert.AreEqual(
                FinancialAdjustmentStatus.Posted,
                adjustment.Status);
            Assert.AreEqual(version, adjustment.Version);
            Assert.AreEqual(updatedAtUtc, adjustment.UpdatedAtUtc);
            Assert.AreEqual(updatedByUserId, adjustment.UpdatedByUserId);
            Assert.AreEqual(postedAtUtc, adjustment.PostedAtUtc);
            Assert.AreEqual(postedByUserId, adjustment.PostedByUserId);
            Assert.AreEqual(reason, adjustment.Reason);
            Assert.AreEqual(amount, adjustment.Amount.Amount);
        }

        private static PaymentPostingService CreatePaymentPostingService(
            IuisCompositionRoot composition)
        {
            return new PaymentPostingService(
                new SessionAwareRequestExecutor(
                    new FixedPrincipalProvider(AdminPrincipal(
                        "finance.payment.post")),
                    new PermissionResolver()),
                composition.TuitionAssessments,
                composition.Payments,
                composition.Transactions,
                composition.IdentifierAllocator);
        }

        private static PaymentPostingRequest PaymentRequest(
            TuitionAssessment assessment,
            long expectedAssessmentEntityVersion)
        {
            return new PaymentPostingRequest
            {
                ExpectedAssessmentRepositoryRevision = 1,
                ExpectedPaymentRepositoryRevision = 0,
                StudentId = assessment.StudentId,
                AcademicPeriodId = assessment.AcademicPeriodId,
                Amount = 1500m,
                CurrencyCode = "PHP",
                Method = PaymentMethod.Cash,
                ReceivedAtUtc = Now.AddMinutes(30),
                Allocations = new[]
                {
                    new PaymentAllocationInput
                    {
                        AssessmentId = assessment.Id,
                        ExpectedAssessmentEntityVersion =
                            expectedAssessmentEntityVersion,
                        Amount = 1500m
                    }
                }
            };
        }

        private static Enrollment CreateDraftEnrollment(
            string id,
            string studentId)
        {
            return new Enrollment(
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
        }

        private static Enrollment CreateApprovedEnrollment(
            string id,
            string studentId)
        {
            var value = CreateDraftEnrollment(id, studentId);
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

        private static TuitionAssessment CreateDraftAssessment(
            string id,
            string enrollmentId,
            string studentId,
            string currencyCode)
        {
            return new TuitionAssessment(
                id,
                studentId,
                enrollmentId,
                "APD-2026-000001",
                currencyCode,
                Now,
                "USR-2026-000001");
        }

        private static TuitionAssessment CreatePostedAssessment(
            string id,
            string enrollmentId,
            string studentId)
        {
            var value = CreateDraftAssessment(
                id,
                enrollmentId,
                studentId,
                "PHP");
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

        private static Payment CreateDraftPayment(
            string id,
            string studentId,
            string currencyCode)
        {
            return new Payment(
                id,
                studentId,
                "APD-2026-000001",
                new Money(50m, currencyCode),
                PaymentMethod.Cash,
                Now,
                null,
                Now,
                "USR-2026-000001");
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
                    "PAL-2026-000801",
                    assessmentId,
                    Money.PhilippinePeso(1500m)),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.Post(
                "RCT-2026-000801",
                Now.AddMinutes(2),
                "USR-2026-000001");
            return value;
        }

        private static FinancialAdjustment CreatePreparedAdjustment(
            string id,
            string assessmentId,
            string studentId,
            string currencyCode)
        {
            return new FinancialAdjustment(
                id,
                studentId,
                assessmentId,
                FinancialAdjustmentDirection.Credit,
                new Money(100m, currencyCode),
                FinancialAdjustmentSourceKind.AdministrativeCorrection,
                "ADM-2026-000001",
                "Approved credit",
                Now,
                "USR-2026-000001");
        }

        private static FinancialAdjustment CreatePostedAdjustment(
            string id,
            string assessmentId,
            string studentId)
        {
            var value = CreatePreparedAdjustment(
                id,
                assessmentId,
                studentId,
                "PHP");
            value.Post(Now.AddMinutes(1), "USR-2026-000001");
            return value;
        }

        private static ScholarshipAward CreatePreparedScholarship(
            string id,
            string studentId,
            string currencyCode)
        {
            return new ScholarshipAward(
                id,
                studentId,
                "SCP-2026-000001",
                "APD-2026-000001",
                ScholarshipEffectKind.FixedAmount,
                currencyCode,
                new Money(500m, currencyCode),
                0m,
                Now,
                "USR-2026-000001");
        }

        private static ScholarshipAward CreateApprovedScholarship(
            string id,
            string studentId)
        {
            var value = CreatePreparedScholarship(id, studentId, "PHP");
            value.Approve(Now.AddMinutes(1), "USR-2026-000001");
            return value;
        }

        private static AuthorizationPrincipal StudentPrincipal(
            string studentId,
            params string[] permissions)
        {
            return new AuthorizationPrincipal(
                "USR-2026-000701",
                studentId,
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                "stamp",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000701",
                        true,
                        permissions)
                },
                null,
                null);
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

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass11-Corrective-" + Guid.NewGuid().ToString("N"));
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

        private sealed class FixedPrincipalProvider :
            IAuthorizationPrincipalProvider
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
