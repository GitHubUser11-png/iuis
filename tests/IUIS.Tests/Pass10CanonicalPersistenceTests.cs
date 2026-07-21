using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Domain.Academic;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;
using IUIS.Domain.People;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass10CanonicalPersistenceTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 20, 6, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void BootstrapSeedsCanonicalAdministratorEmployeeRecord()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var store = NewStore(root);
                var envelope = store.Read<JsonElement>("employees");
                Assert.AreEqual(1L, envelope.Revision);
                Assert.AreEqual(1, envelope.Records.Count);
                Assert.AreEqual(1, envelope.Records[0].GetProperty("recordSchemaVersion").GetInt32());

                var employee = new IuisCompositionRoot(root).Employees
                    .FindById(bootstrap.AdministratorEmployeeId);
                Assert.IsNotNull(employee);
                Assert.AreEqual("admin@example.edu", employee.Contact.EmailAddress);
                Assert.AreEqual("PH", employee.Address.CountryCode);
                Assert.AreEqual(EmploymentStatus.Active, employee.Status);
            });
        }

        [TestMethod]
        public void SixActivatedAdaptersReadBootstrapAndEmptyRepositories()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                Assert.AreEqual(0, composition.Students.Read().Records.Count);
                Assert.AreEqual(1, composition.Employees.Read().Records.Count);
                Assert.AreEqual(0, composition.Courses.Read().Records.Count);
                Assert.AreEqual(0, composition.Subjects.Read().Records.Count);
                Assert.AreEqual(0, composition.AcademicPeriods.Read().Records.Count);
                Assert.AreEqual(0, composition.AssessmentChargeRules.Read().Records.Count);
            });
        }

        [TestMethod]
        public void StudentMapperRoundTripPreservesValueObjectsAndMetadata()
        {
            var student = CreateStudent("STU-2026-000010");
            student.UpdateContact(
                new ContactInformation("updated.student@example.edu", "+639181112222", null),
                new PostalAddress("2 University Road", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                Now.AddMinutes(1),
                "USR-2026-000010");
            student.Archive(Now.AddMinutes(2), "USR-2026-000010");

            var mapper = new StudentRecordJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(student, options), options);

            Assert.AreEqual(student.Id, restored.Id);
            Assert.AreEqual(student.Version, restored.Version);
            Assert.AreEqual(student.IsArchived, restored.IsArchived);
            Assert.AreEqual(student.ArchivedAtUtc, restored.ArchivedAtUtc);
            Assert.AreEqual(student.UpdatedByUserId, restored.UpdatedByUserId);
            Assert.AreEqual(student.Name, restored.Name);
            Assert.AreEqual(student.Contact, restored.Contact);
            Assert.AreEqual(student.Address, restored.Address);
            Assert.AreEqual(student.BirthDate, restored.BirthDate);
        }

        [TestMethod]
        public void EmployeeMapperRoundTripPreservesAssignmentAndMetadata()
        {
            var employee = CreateEmployee("EMP-2026-000010");
            employee.ChangeAssignment(
                "DEPT-REGISTRAR",
                "Senior Registrar",
                false,
                Now.AddMinutes(1),
                "USR-2026-000010");
            var mapper = new EmployeeRecordJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(employee, options), options);

            Assert.AreEqual(employee.Id, restored.Id);
            Assert.AreEqual(employee.Version, restored.Version);
            Assert.AreEqual("DEPT-REGISTRAR", restored.DepartmentId);
            Assert.AreEqual("Senior Registrar", restored.PositionTitle);
            Assert.IsFalse(restored.IsFaculty);
            Assert.AreEqual(employee.Name, restored.Name);
            Assert.AreEqual(employee.Contact, restored.Contact);
            Assert.AreEqual(employee.Address, restored.Address);
        }

        [TestMethod]
        public void AcademicMappersRestoreLifecycleDatesAndPrerequisites()
        {
            var course = new Course(
                "CRS-2026-000010",
                "BSIT",
                "Bachelor of Science in Information Technology",
                "DEPT-IT",
                4,
                Now,
                "USR-2026-000001");
            course.ChangeStatus(CourseStatus.Active, Now.AddMinutes(1), "USR-2026-000001");
            var subject = new Subject(
                "SUB-2026-000010",
                "IT-332",
                "Integrative Programming",
                3m,
                Now,
                "USR-2026-000001");
            subject.AddPrerequisite(
                new SubjectPrerequisite("SUB-2026-000009"),
                Now.AddMinutes(1),
                "USR-2026-000001");
            subject.ChangeStatus(SubjectStatus.Active, Now.AddMinutes(2), "USR-2026-000001");
            var period = new AcademicPeriod(
                "APD-2026-000010",
                "AY2026-T1",
                "Academic Year 2026 Term 1",
                new InstitutionLocalDate(2026, 7, 1),
                new InstitutionLocalDate(2026, 7, 20),
                new InstitutionLocalDate(2026, 8, 1),
                new InstitutionLocalDate(2026, 12, 15),
                Now,
                "USR-2026-000001");
            period.TransitionTo(AcademicPeriodStatus.Scheduled, Now.AddMinutes(1), "USR-2026-000001");

            var options = JsonOptions();
            var restoredCourse = new CourseJsonMapper().FromJson(
                new CourseJsonMapper().ToJson(course, options), options);
            var restoredSubject = new SubjectJsonMapper().FromJson(
                new SubjectJsonMapper().ToJson(subject, options), options);
            var restoredPeriod = new AcademicPeriodJsonMapper().FromJson(
                new AcademicPeriodJsonMapper().ToJson(period, options), options);

            Assert.AreEqual(CourseStatus.Active, restoredCourse.Status);
            Assert.AreEqual(course.Version, restoredCourse.Version);
            Assert.AreEqual(SubjectStatus.Active, restoredSubject.Status);
            Assert.AreEqual("SUB-2026-000009", restoredSubject.Prerequisites.Single().PrerequisiteSubjectId);
            Assert.AreEqual(AcademicPeriodStatus.Scheduled, restoredPeriod.Status);
            Assert.AreEqual(period.StartDate, restoredPeriod.StartDate);
            Assert.AreEqual(period.EndDate, restoredPeriod.EndDate);
        }

        [TestMethod]
        public void ChargeRuleMapperPreservesMoneyCalculationAndLifecycle()
        {
            var rule = new AssessmentChargeRule(
                "ACR-2026-000010",
                "TUITION-PER-UNIT",
                "Tuition charge per academic unit",
                AssessmentChargeCategory.Tuition,
                ChargeCalculationKind.PerAcademicUnit,
                Money.PhilippinePeso(1250.50m),
                Now,
                "USR-2026-000001");
            rule.Activate(Now.AddMinutes(1), "USR-2026-000001");
            var mapper = new AssessmentChargeRuleJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(rule, options), options);

            Assert.AreEqual(ChargeRuleStatus.Active, restored.Status);
            Assert.AreEqual(1250.50m, restored.Rate.Amount);
            Assert.AreEqual("PHP", restored.Rate.CurrencyCode);
            Assert.AreEqual(3751.50m, restored.Calculate(3m).Amount);
        }

        [TestMethod]
        public void MapperRejectsUnversionedCanonicalRecord()
        {
            var options = JsonOptions();
            var source = JsonSerializer.SerializeToElement(
                new PersistedCourseRecord
                {
                    RecordSchemaVersion = 0,
                    Id = "CRS-2026-000011",
                    Version = 1,
                    CreatedAtUtc = Now,
                    CreatedByUserId = "USR-2026-000001",
                    UpdatedAtUtc = Now,
                    UpdatedByUserId = "USR-2026-000001",
                    Code = "BSCS",
                    Name = "Bachelor of Science in Computer Science",
                    DepartmentId = "DEPT-CS",
                    DurationYears = 4,
                    Status = CourseStatus.Draft.ToString()
                },
                options);
            Assert.ThrowsException<InvalidOperationException>(() =>
                new CourseJsonMapper().FromJson(source, options));
        }

        [TestMethod]
        public void MapperRejectsUnsupportedRecordSchemaVersion()
        {
            var options = JsonOptions();
            var source = JsonSerializer.SerializeToElement(
                new PersistedCourseRecord
                {
                    RecordSchemaVersion = 99,
                    Id = "CRS-2026-000012",
                    Version = 1,
                    CreatedAtUtc = Now,
                    CreatedByUserId = "USR-2026-000001",
                    UpdatedAtUtc = Now,
                    UpdatedByUserId = "USR-2026-000001",
                    Code = "BSA",
                    Name = "Bachelor of Science in Accountancy",
                    DepartmentId = "DEPT-ACCOUNTING",
                    DurationYears = 4,
                    Status = CourseStatus.Draft.ToString()
                },
                options);
            Assert.ThrowsException<InvalidOperationException>(() =>
                new CourseJsonMapper().FromJson(source, options));
        }

        [TestMethod]
        public void ReadinessCatalogActivatesExactlyEighteenSpecializedMappers()
        {
            var records = AggregateMapperReadinessCatalog.All;
            Assert.AreEqual(18, records.Count);
            Assert.AreEqual(
                18,
                records.Count(item => item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted));
            Assert.AreEqual(
                0,
                records.Count(item => item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason));
            Assert.IsFalse(records.Any(item =>
                item.Readiness == AggregateMapperReadiness.GenericMapperCompatible
                || item.Readiness == AggregateMapperReadiness.RequiresSpecializedMapper));
        }

        [TestMethod]
        public void CompositionRootProvidesRealJsonBackedStudentReadModel()
        {
            WithStudentSession((root, credentials, student) =>
            {
                var result = new IuisCompositionRoot(root).StudentOwnRecords.GetOwnRecord(
                    credentials.SessionId,
                    credentials.Token,
                    Now.AddMinutes(5));
                Assert.AreEqual(student.Id, result.StudentId);
                Assert.AreEqual(student.Version, result.EntityVersion);
                Assert.AreEqual(1L, result.RepositoryRevision);
                Assert.AreEqual(student.Contact.EmailAddress, result.EmailAddress);
            });
        }

        [TestMethod]
        public void CompositionRootProvidesRealJsonBackedEmployeeReadModel()
        {
            WithEmployeeSession((root, credentials, employee) =>
            {
                var result = new IuisCompositionRoot(root).EmployeeSelfService.GetOwnRecord(
                    credentials.SessionId,
                    credentials.Token,
                    Now.AddMinutes(5));
                Assert.AreEqual(employee.Id, result.EmployeeId);
                Assert.AreEqual(employee.Version, result.EntityVersion);
                Assert.AreEqual(2L, result.RepositoryRevision);
                Assert.AreEqual(employee.DepartmentId, result.DepartmentId);
            });
        }

        [TestMethod]
        public void StudentContactUpdatePersistsWithAuditMetadata()
        {
            WithStudentSession((root, credentials, student) =>
            {
                var composition = new IuisCompositionRoot(root);
                var read = composition.StudentOwnRecords.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(5));
                var result = composition.StudentContactUpdates.UpdateOwnContact(
                    credentials.SessionId,
                    credentials.Token,
                    ContactRequest(read.RepositoryRevision, read.EntityVersion, "student.changed@example.edu"),
                    Now.AddMinutes(6));
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
                Assert.AreEqual(2L, result.RepositoryRevision);
                Assert.AreEqual(2L, result.EntityVersion);
                Assert.AreEqual(credentials.UserId, result.UpdatedByUserId);

                var persisted = new IuisCompositionRoot(root).StudentOwnRecords.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(7));
                Assert.AreEqual("student.changed@example.edu", persisted.EmailAddress);
                Assert.AreEqual(2L, persisted.RepositoryRevision);
                Assert.AreEqual(2L, persisted.EntityVersion);
            });
        }

        [TestMethod]
        public void EmployeeContactUpdatePersistsWithAuditMetadata()
        {
            WithEmployeeSession((root, credentials, employee) =>
            {
                var composition = new IuisCompositionRoot(root);
                var read = composition.EmployeeSelfService.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(5));
                var result = composition.EmployeeContactUpdates.UpdateOwnContact(
                    credentials.SessionId,
                    credentials.Token,
                    ContactRequest(read.RepositoryRevision, read.EntityVersion, "employee.changed@example.edu"),
                    Now.AddMinutes(6));
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
                Assert.AreEqual(3L, result.RepositoryRevision);
                Assert.AreEqual(2L, result.EntityVersion);
                Assert.AreEqual(credentials.UserId, result.UpdatedByUserId);

                var persisted = new IuisCompositionRoot(root).EmployeeSelfService.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(7));
                Assert.AreEqual("employee.changed@example.edu", persisted.EmailAddress);
                Assert.AreEqual(3L, persisted.RepositoryRevision);
            });
        }

        [TestMethod]
        public void StaleContactUpdateTokensCannotOverwriteCommittedState()
        {
            WithStudentSession((root, credentials, student) =>
            {
                var composition = new IuisCompositionRoot(root);
                var read = composition.StudentOwnRecords.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(5));
                composition.StudentContactUpdates.UpdateOwnContact(
                    credentials.SessionId,
                    credentials.Token,
                    ContactRequest(read.RepositoryRevision, read.EntityVersion, "first.change@example.edu"),
                    Now.AddMinutes(6));
                Assert.ThrowsException<InvalidOperationException>(() =>
                    composition.StudentContactUpdates.UpdateOwnContact(
                        credentials.SessionId,
                        credentials.Token,
                        ContactRequest(read.RepositoryRevision, read.EntityVersion, "stale.change@example.edu"),
                        Now.AddMinutes(7)));

                var persisted = new IuisCompositionRoot(root).StudentOwnRecords.GetOwnRecord(
                    credentials.SessionId, credentials.Token, Now.AddMinutes(8));
                Assert.AreEqual("first.change@example.edu", persisted.EmailAddress);
                Assert.AreEqual(2L, persisted.RepositoryRevision);
                Assert.AreEqual(2L, persisted.EntityVersion);
            });
        }

        [TestMethod]
        public void ReleasedDtoBoundaryRemainsFreeOfInternalFields()
        {
            var types = new[]
            {
                typeof(StudentOwnRecordDto),
                typeof(EmployeeSelfServiceDto),
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

        private static StudentRecord CreateStudent(string id)
        {
            return new StudentRecord(
                id,
                id,
                new PersonName("Ada", null, "Lovelace", null),
                new ContactInformation("ada.lovelace@example.edu", "+639171234567", null),
                new PostalAddress("1 University Road", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                new InstitutionLocalDate(2000, 1, 1),
                "CRS-2026-000001",
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
                new ContactInformation("grace.hopper@example.edu", "+639181234567", null),
                new PostalAddress("3 Faculty Road", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                new InstitutionLocalDate(1985, 12, 9),
                "DEPT-IT",
                "Faculty Member",
                EmploymentStatus.Active,
                true,
                Now,
                "USR-2026-000001");
        }

        private static ContactUpdateRequest ContactRequest(
            long expectedRepositoryRevision,
            long expectedEntityVersion,
            string emailAddress)
        {
            return new ContactUpdateRequest
            {
                ExpectedRepositoryRevision = expectedRepositoryRevision,
                ExpectedEntityVersion = expectedEntityVersion,
                EmailAddress = emailAddress,
                MobileNumber = "+639199998888",
                AddressLine1 = "9 Updated Road",
                Barangay = "Poblacion",
                CityMunicipality = "Malvar",
                Province = "Batangas",
                PostalCode = "4233",
                CountryCode = "PH"
            };
        }

        private static void WithStudentSession(Action<string, SessionCredentials, StudentRecord> action)
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var student = CreateStudent("STU-2026-000020");
                composition.Students.Write(new[] { student }, 0, bootstrap.AdministratorUserId);
                var credentials = SeedUserSession(
                    root,
                    "student.20",
                    "USR-2026-000020",
                    student.Id,
                    PrimaryRole.Student,
                    PersonRecordKind.Student,
                    new[] { "student.profile.read", "student.profile.contact.update" });
                action(root, credentials, student);
            });
        }

        private static void WithEmployeeSession(Action<string, SessionCredentials, EmployeeRecord> action)
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var employee = CreateEmployee("EMP-2026-000020");
                var existing = composition.Employees.Read();
                var records = existing.Records.ToList();
                records.Add(employee);
                composition.Employees.Write(records, existing.Revision, bootstrap.AdministratorUserId);
                var credentials = SeedUserSession(
                    root,
                    "employee.20",
                    "USR-2026-000021",
                    employee.Id,
                    PrimaryRole.EmployeeFaculty,
                    PersonRecordKind.EmployeeFaculty,
                    new[] { "employee.profile.read", "employee.profile.contact.update" });
                action(root, credentials, employee);
            });
        }

        private static SessionCredentials SeedUserSession(
            string root,
            string loginId,
            string userId,
            string personRecordId,
            PrimaryRole role,
            PersonRecordKind personRecordKind,
            IEnumerable<string> permissions)
        {
            var store = NewStore(root);
            var token = "token-" + userId;
            var sessionId = "SES-" + userId.Substring(4);
            var securityStamp = "stamp-" + userId;
            var users = store.Read<PersistedUserAccount>("users");
            users.Records.Add(new PersistedUserAccount
            {
                Id = userId,
                LoginId = loginId,
                PrimaryRole = role.ToString(),
                PersonRecordKind = personRecordKind.ToString(),
                PersonRecordId = personRecordId,
                CredentialHash = "not-used-by-session-test",
                SecurityStamp = securityStamp,
                Status = "Active",
                MustChangePassword = false,
                CreatedAtUtc = Now,
                UpdatedAtUtc = Now,
                Version = 1,
                PermissionProfileIds = new List<string>(),
                DirectPermissionGrants = permissions.ToList(),
                DirectPermissionRestrictions = new List<string>()
            });
            store.Write("users", users, users.Revision);
            var sessions = store.Read<PersistedSessionRecord>("sessions");
            sessions.Records.Add(new PersistedSessionRecord
            {
                Id = sessionId,
                UserId = userId,
                TokenHash = "sha256:" + token,
                SecurityStampSnapshot = securityStamp,
                ApplicationKind = SessionApplicationKind.UserApplication.ToString(),
                Purpose = SessionPurpose.FullAccess.ToString(),
                Status = UserSessionStatus.Active.ToString(),
                IssuedAtUtc = Now,
                LastActivityAtUtc = Now,
                InactivityExpiresAtUtc = Now.AddHours(1),
                AbsoluteExpiresAtUtc = Now.AddHours(8)
            });
            store.Write("sessions", sessions, sessions.Revision);
            return new SessionCredentials { UserId = userId, SessionId = sessionId, Token = token };
        }

        private static void WithBootstrap(Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass10-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var result = new ProductionBootstrapper(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Initialize(CreateBootstrapRequest());
                action(root, result);
            }
            finally
            {
                try { Directory.Delete(root, true); }
                catch { }
            }
        }

        private static ProductionBootstrapRequest CreateBootstrapRequest()
        {
            return new ProductionBootstrapRequest
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
            };
        }

        private sealed class SessionCredentials
        {
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string Token { get; set; }
        }
    }
}
