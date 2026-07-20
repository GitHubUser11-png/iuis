using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Domain.Academic;
using IUIS.Domain.Common;
using IUIS.Domain.Finance;
using IUIS.Domain.People;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass10ClosureIntegrityTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void AllSixActivatedRepositoriesRoundTripAcrossCompositionRootRestart()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var student = CreateStudent("STU-2026-000101");
                var employee = CreateEmployee("EMP-2026-000101");
                var course = CreateCourse("CRS-2026-000101");
                var subject = CreateSubject("SUB-2026-000101");
                var period = CreateAcademicPeriod("APD-2026-000101");
                var rule = CreateChargeRule("ACR-2026-000101");

                composition.Students.Write(
                    new[] { student },
                    0,
                    bootstrap.AdministratorUserId);

                var employees = composition.Employees.Read();
                var employeeRecords = employees.Records.ToList();
                employeeRecords.Add(employee);
                composition.Employees.Write(
                    employeeRecords,
                    employees.Revision,
                    bootstrap.AdministratorUserId);

                composition.Courses.Write(
                    new[] { course },
                    0,
                    bootstrap.AdministratorUserId);
                composition.Subjects.Write(
                    new[] { subject },
                    0,
                    bootstrap.AdministratorUserId);
                composition.AcademicPeriods.Write(
                    new[] { period },
                    0,
                    bootstrap.AdministratorUserId);
                composition.AssessmentChargeRules.Write(
                    new[] { rule },
                    0,
                    bootstrap.AdministratorUserId);

                var restarted = new IuisCompositionRoot(root);
                Assert.AreEqual(student.Id, restarted.Students.Read().Records.Single().Id);
                Assert.IsTrue(restarted.Employees.Read().Records.Any(
                    item => item.Id == employee.Id));
                Assert.AreEqual(course.Id, restarted.Courses.Read().Records.Single().Id);
                Assert.AreEqual(subject.Id, restarted.Subjects.Read().Records.Single().Id);
                Assert.AreEqual(period.Id, restarted.AcademicPeriods.Read().Records.Single().Id);
                Assert.AreEqual(
                    rule.Id,
                    restarted.AssessmentChargeRules.Read().Records.Single().Id);
            });
        }

        [TestMethod]
        public void ArchivedStudentRoundTripPreservesVersionAndArchiveMetadata()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var student = CreateStudent("STU-2026-000102");
                student.UpdateContact(
                    new ContactInformation(
                        "archived.student@example.edu",
                        "+639171010202",
                        null),
                    student.Address,
                    Now.AddMinutes(1),
                    bootstrap.AdministratorUserId);
                student.Archive(
                    Now.AddMinutes(2),
                    bootstrap.AdministratorUserId);

                var composition = new IuisCompositionRoot(root);
                composition.Students.Write(
                    new[] { student },
                    0,
                    bootstrap.AdministratorUserId);

                var restored = new IuisCompositionRoot(root)
                    .Students.Read().Records.Single();
                Assert.AreEqual(student.Id, restored.Id);
                Assert.AreEqual(student.Version, restored.Version);
                Assert.IsTrue(restored.IsArchived);
                Assert.AreEqual(student.ArchivedAtUtc, restored.ArchivedAtUtc);
                Assert.AreEqual(student.ArchivedByUserId, restored.ArchivedByUserId);
                Assert.AreEqual(student.UpdatedAtUtc, restored.UpdatedAtUtc);
                Assert.AreEqual(student.UpdatedByUserId, restored.UpdatedByUserId);
            });
        }

        [TestMethod]
        public void UnsupportedCanonicalRecordVersionFailsThroughActivatedAdapter()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var store = new JsonRepositoryStore(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root));
                var envelope = store.Read<JsonElement>("courses");
                envelope.Records.Add(JsonSerializer.SerializeToElement(
                    new PersistedCourseRecord
                    {
                        RecordSchemaVersion = 99,
                        Id = "CRS-2026-000103",
                        Version = 1,
                        CreatedAtUtc = Now,
                        CreatedByUserId = bootstrap.AdministratorUserId,
                        UpdatedAtUtc = Now,
                        UpdatedByUserId = bootstrap.AdministratorUserId,
                        Code = "BSIS",
                        Name = "Bachelor of Science in Information Systems",
                        DepartmentId = "DEPT-IT",
                        DurationYears = 4,
                        Status = CourseStatus.Draft.ToString()
                    },
                    JsonOptions()));
                store.Write("courses", envelope, envelope.Revision);

                Assert.ThrowsException<InvalidOperationException>(() =>
                    new IuisCompositionRoot(root).Courses.Read());
            });
        }

        [TestMethod]
        public void IncompleteArchiveMetadataFailsClosedDuringHydration()
        {
            var persisted = new PersistedStudentRecord
            {
                RecordSchemaVersion = 1,
                Id = "STU-2026-000104",
                StudentNumber = "STU-2026-000104",
                Name = new PersistedPersonName
                {
                    GivenName = "Katherine",
                    FamilyName = "Johnson"
                },
                Contact = new PersistedContactInformation
                {
                    EmailAddress = "katherine.johnson@example.edu",
                    MobileNumber = "+639171010204"
                },
                Address = new PersistedPostalAddress
                {
                    AddressLine1 = "4 University Road",
                    Barangay = "Poblacion",
                    CityMunicipality = "Malvar",
                    Province = "Batangas",
                    PostalCode = "4233",
                    CountryCode = "PH"
                },
                BirthDate = "2001-08-26",
                CourseId = "CRS-2026-000101",
                Status = StudentStatus.Active.ToString(),
                Version = 2,
                IsArchived = true,
                CreatedAtUtc = Now,
                CreatedByUserId = "USR-2026-000001",
                UpdatedAtUtc = Now.AddMinutes(1),
                UpdatedByUserId = "USR-2026-000001",
                ArchivedAtUtc = Now.AddMinutes(1),
                ArchivedByUserId = null
            };

            var json = JsonSerializer.SerializeToElement(persisted, JsonOptions());
            Assert.ThrowsException<DomainValidationException>(() =>
                new StudentRecordJsonMapper().FromJson(json, JsonOptions()));
        }

        [TestMethod]
        public void ChargeRuleRejectedMutationsAreExceptionAtomic()
        {
            var rule = CreateChargeRule("ACR-2026-000105");
            var originalDescription = rule.Description;
            var originalCategory = rule.Category;
            var originalCalculation = rule.CalculationKind;
            var originalRate = rule.Rate;
            var originalVersion = rule.Version;
            var originalUpdatedAtUtc = rule.UpdatedAtUtc;

            Assert.ThrowsException<DomainValidationException>(() =>
                rule.UpdateDraftDetails(
                    "Changed before validation",
                    AssessmentChargeCategory.Miscellaneous,
                    ChargeCalculationKind.FixedAmount,
                    Money.PhilippinePeso(99m),
                    Now.AddMinutes(-1),
                    "USR-2026-000001"));

            Assert.AreEqual(originalDescription, rule.Description);
            Assert.AreEqual(originalCategory, rule.Category);
            Assert.AreEqual(originalCalculation, rule.CalculationKind);
            Assert.AreEqual(originalRate, rule.Rate);
            Assert.AreEqual(originalVersion, rule.Version);
            Assert.AreEqual(originalUpdatedAtUtc, rule.UpdatedAtUtc);

            rule.Archive(Now.AddMinutes(1), "USR-2026-000001");
            var archivedVersion = rule.Version;
            var archivedUpdatedAtUtc = rule.UpdatedAtUtc;
            Assert.ThrowsException<DomainValidationException>(() =>
                rule.Activate(Now.AddMinutes(2), "USR-2026-000001"));
            Assert.AreEqual(ChargeRuleStatus.Draft, rule.Status);
            Assert.AreEqual(archivedVersion, rule.Version);
            Assert.AreEqual(archivedUpdatedAtUtc, rule.UpdatedAtUtc);
        }

        [TestMethod]
        public void ReadinessCatalogMatchesCompositionRootActivationSet()
        {
            var completed = AggregateMapperReadinessCatalog.All
                .Where(item =>
                    item.Readiness
                    == AggregateMapperReadiness.SpecializedMapperCompleted)
                .Select(item => item.RepositoryName)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    "academic_periods",
                    "assessment_charge_rules",
                    "assessments",
                    "courses",
                    "employees",
                    "enrollments",
                    "financial_adjustments",
                    "payments",
                    "scholarship_awards",
                    "students",
                    "subjects"
                },
                completed);
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static StudentRecord CreateStudent(string id)
        {
            return new StudentRecord(
                id,
                id,
                new PersonName("Ada", null, "Lovelace", null),
                new ContactInformation(
                    "ada.lovelace@example.edu",
                    "+639171234567",
                    null),
                new PostalAddress(
                    "1 University Road",
                    null,
                    "Poblacion",
                    "Malvar",
                    "Batangas",
                    "4233",
                    "PH"),
                new InstitutionLocalDate(2000, 1, 1),
                "CRS-2026-000101",
                StudentStatus.Active,
                Now,
                "USR-2026-000001");
        }

        private static EmployeeRecord CreateEmployee(string id)
        {
            return new EmployeeRecord(
                id,
                id,
                new PersonName("Grace", null, "Hopper", null),
                new ContactInformation(
                    "grace.hopper@example.edu",
                    "+639181234567",
                    null),
                new PostalAddress(
                    "3 Faculty Road",
                    null,
                    "Poblacion",
                    "Malvar",
                    "Batangas",
                    "4233",
                    "PH"),
                new InstitutionLocalDate(1985, 12, 9),
                "DEPT-IT",
                "Faculty Member",
                EmploymentStatus.Active,
                true,
                Now,
                "USR-2026-000001");
        }

        private static Course CreateCourse(string id)
        {
            var value = new Course(
                id,
                "BSIT",
                "Bachelor of Science in Information Technology",
                "DEPT-IT",
                4,
                Now,
                "USR-2026-000001");
            value.ChangeStatus(
                CourseStatus.Active,
                Now.AddMinutes(1),
                "USR-2026-000001");
            return value;
        }

        private static Subject CreateSubject(string id)
        {
            var value = new Subject(
                id,
                "IT-332",
                "Integrative Programming",
                3m,
                Now,
                "USR-2026-000001");
            value.AddPrerequisite(
                new SubjectPrerequisite("SUB-2026-000100"),
                Now.AddMinutes(1),
                "USR-2026-000001");
            value.ChangeStatus(
                SubjectStatus.Active,
                Now.AddMinutes(2),
                "USR-2026-000001");
            return value;
        }

        private static AcademicPeriod CreateAcademicPeriod(string id)
        {
            var value = new AcademicPeriod(
                id,
                "AY2026-T1",
                "Academic Year 2026 Term 1",
                new InstitutionLocalDate(2026, 7, 1),
                new InstitutionLocalDate(2026, 7, 20),
                new InstitutionLocalDate(2026, 8, 1),
                new InstitutionLocalDate(2026, 12, 15),
                Now,
                "USR-2026-000001");
            value.TransitionTo(
                AcademicPeriodStatus.Scheduled,
                Now.AddMinutes(1),
                "USR-2026-000001");
            return value;
        }

        private static AssessmentChargeRule CreateChargeRule(string id)
        {
            return new AssessmentChargeRule(
                id,
                "TUITION-PER-UNIT",
                "Tuition charge per academic unit",
                AssessmentChargeCategory.Tuition,
                ChargeCalculationKind.PerAcademicUnit,
                Money.PhilippinePeso(1250.50m),
                Now,
                "USR-2026-000001");
        }

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass10-Closure-" + Guid.NewGuid().ToString("N"));
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
    }
}
