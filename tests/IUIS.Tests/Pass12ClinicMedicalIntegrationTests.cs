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
using IUIS.Domain.Clinic;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass12ClinicMedicalIntegrationTests
    {
        private static readonly DateTime StartUtc =
            new DateTime(2026, 7, 21, 7, 0, 0, DateTimeKind.Utc);
        private const string ActorUserId = "USR-2026-000001";
        private const string ClinicianEmployeeId = "EMP-2026-000001";
        private const string StudentId = "STU-2026-000001";

        [TestMethod]
        public void ClinicAppointmentMapperRoundTripPreservesCompletedLifecycle()
        {
            var appointment = CheckedInAppointment("CAP-2026-000001", StudentId);
            appointment.Complete("CON-2026-000001", StartUtc.AddHours(2), ActorUserId);
            var mapper = new ClinicAppointmentJsonMapper();
            var options = JsonOptions();

            var restored = mapper.FromJson(mapper.ToJson(appointment, options), options);

            Assert.AreEqual(ClinicAppointmentStatus.Completed, restored.Status);
            Assert.AreEqual(appointment.Version, restored.Version);
            Assert.AreEqual(appointment.StudentId, restored.StudentId);
            Assert.AreEqual(appointment.ScheduledAtUtc, restored.ScheduledAtUtc);
            Assert.AreEqual(appointment.CheckedInAtUtc, restored.CheckedInAtUtc);
            Assert.AreEqual(appointment.CompletedAtUtc, restored.CompletedAtUtc);
            Assert.AreEqual(appointment.ConsultationId, restored.ConsultationId);
        }

        [TestMethod]
        public void MedicalRecordMapperRoundTripPreservesConfidentialAndReleasedSegregation()
        {
            var record = MedicalRecordWithReleasedSummary(
                "MDR-2026-000001",
                StudentId,
                "CAP-2026-000001",
                "CON-2026-000001");
            var mapper = new MedicalRecordJsonMapper();
            var options = JsonOptions();

            var restored = mapper.FromJson(mapper.ToJson(record, options), options);

            Assert.AreEqual(1, restored.ConfidentialConsultations.Count);
            Assert.AreEqual(1, restored.ReleasedSummaries.Count);
            Assert.AreEqual("Internal clinical notes", restored.ConfidentialConsultations[0].InternalClinicalNotes);
            Assert.AreEqual("Released recovery summary", restored.ReleasedSummaries[0].ReleasedSummary);
            Assert.AreEqual(record.Version, restored.Version);
            Assert.AreEqual(MedicalRecordStatus.Active, restored.Status);
        }

        [TestMethod]
        public void MedicalClearanceMapperRoundTripPreservesIssuedHistoryAndValidity()
        {
            var clearance = IssuedClearance(
                "MCL-2026-000001",
                StudentId,
                "MDR-2026-000001");
            var mapper = new MedicalClearanceJsonMapper();
            var options = JsonOptions();

            var restored = mapper.FromJson(mapper.ToJson(clearance, options), options);

            Assert.AreEqual(MedicalClearanceStatus.Issued, restored.Status);
            Assert.AreEqual(3, restored.History.Count);
            Assert.AreEqual(clearance.ClearanceNumber, restored.ClearanceNumber);
            Assert.AreEqual(clearance.ValidFrom, restored.ValidFrom);
            Assert.AreEqual(clearance.ValidUntil, restored.ValidUntil);
            Assert.AreEqual(clearance.ReleasedSummary, restored.ReleasedSummary);
            Assert.IsTrue(restored.IsValidOn(new InstitutionLocalDate(2026, 8, 1)));
        }

        [TestMethod]
        public void ClinicMedicalMappersRejectUnsupportedSchemasAndContradictoryLifecycle()
        {
            var options = JsonOptions();
            var invalidSchema = new PersistedClinicAppointmentRecord
            {
                RecordSchemaVersion = 2,
                Id = "CAP-2026-000001"
            };
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                new ClinicAppointmentJsonMapper().FromJson(
                    JsonSerializer.SerializeToElement(invalidSchema, options),
                    options));

            Assert.ThrowsExactly<DomainValidationException>(() =>
                ClinicAppointment.Rehydrate(
                    "CAP-2026-000001",
                    StudentId,
                    StartUtc,
                    "Released reason",
                    ClinicAppointmentStatus.Completed,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    1,
                    false,
                    StartUtc,
                    ActorUserId,
                    StartUtc,
                    ActorUserId,
                    null,
                    null));

            Assert.ThrowsExactly<DomainValidationException>(() =>
                MedicalRecord.Rehydrate(
                    "MDR-2026-000001",
                    StudentId,
                    MedicalRecordStatus.Closed,
                    StartUtc.AddMinutes(1),
                    new MedicalConsultationRecord[0],
                    new MedicalReleasedSummary[0],
                    1,
                    false,
                    StartUtc,
                    ActorUserId,
                    StartUtc.AddMinutes(1),
                    ActorUserId,
                    null,
                    null));
        }

        [TestMethod]
        public void MapperReadinessMovesToEighteenCompletedAndZeroDeferred()
        {
            var completed = AggregateMapperReadinessCatalog.All
                .Where(item => item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted)
                .Select(item => item.AdapterName)
                .ToList();
            var deferred = AggregateMapperReadinessCatalog.All
                .Where(item => item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason)
                .ToList();

            Assert.AreEqual(18, completed.Count);
            CollectionAssert.Contains(completed, "ClinicAppointmentRepositoryAdapter");
            CollectionAssert.Contains(completed, "MedicalRecordRepositoryAdapter");
            CollectionAssert.Contains(completed, "MedicalClearanceRepositoryAdapter");
            CollectionAssert.Contains(completed, "CounselingCaseRepositoryAdapter");
            CollectionAssert.Contains(completed, "DisciplineCaseRepositoryAdapter");
            Assert.AreEqual(0, deferred.Count);
        }

        [TestMethod]
        public void CompositionRootRestartPreservesClinicMedicalRepositories()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var first = new IuisCompositionRoot(root);
                var appointment = CheckedInAppointment("CAP-2026-000101", StudentId);
                var record = MedicalRecordWithReleasedSummary(
                    "MDR-2026-000101",
                    StudentId,
                    appointment.Id,
                    "CON-2026-000101");
                var clearance = IssuedClearance(
                    "MCL-2026-000101",
                    StudentId,
                    record.Id);
                first.ClinicAppointments.Write(
                    new[] { appointment },
                    0,
                    bootstrap.AdministratorUserId);
                first.MedicalRecords.Write(
                    new[] { record },
                    0,
                    bootstrap.AdministratorUserId);
                first.MedicalClearances.Write(
                    new[] { clearance },
                    0,
                    bootstrap.AdministratorUserId);

                var restarted = new IuisCompositionRoot(root);

                Assert.AreEqual(ClinicAppointmentStatus.CheckedIn,
                    restarted.ClinicAppointments.FindById(appointment.Id).Status);
                Assert.AreEqual(1,
                    restarted.MedicalRecords.FindById(record.Id).ReleasedSummaries.Count);
                Assert.AreEqual(MedicalClearanceStatus.Issued,
                    restarted.MedicalClearances.FindById(clearance.Id).Status);
                Assert.IsNotNull(restarted.StudentMedicalServices);
                Assert.IsNotNull(restarted.RestrictedMedicalRecords);
                Assert.IsNotNull(restarted.ClinicAppointmentCommands);
                Assert.IsNotNull(restarted.MedicalRecordCommands);
                Assert.IsNotNull(restarted.MedicalClearanceCommands);
            });
        }

        [TestMethod]
        public void StudentMedicalProjectionUsesSessionOwnershipAndExcludesConfidentialFields()
        {
            var ownAppointment = CheckedInAppointment("CAP-2026-000201", StudentId);
            var otherAppointment = CheckedInAppointment("CAP-2026-000202", "STU-2026-000002");
            var ownRecord = MedicalRecordWithReleasedSummary(
                "MDR-2026-000201",
                StudentId,
                ownAppointment.Id,
                "CON-2026-000201");
            var otherRecord = MedicalRecordWithReleasedSummary(
                "MDR-2026-000202",
                "STU-2026-000002",
                otherAppointment.Id,
                "CON-2026-000202");
            var ownClearance = IssuedClearance(
                "MCL-2026-000201",
                StudentId,
                ownRecord.Id);
            var otherClearance = IssuedClearance(
                "MCL-2026-000202",
                "STU-2026-000002",
                otherRecord.Id);
            var service = new StudentMedicalServicesQueryService(
                Executor(StudentPrincipal(StudentId)),
                new AppointmentRepository(3, new[] { ownAppointment, otherAppointment }),
                new MedicalRepository(4, new[] { ownRecord, otherRecord }),
                new ClearanceRepository(5, new[] { ownClearance, otherClearance }));

            var result = service.GetOwnOverview(
                "SES-2026-000001",
                "token",
                StartUtc.AddHours(3));

            Assert.AreEqual(StudentId, result.StudentId);
            Assert.AreEqual(1, result.Appointments.Count);
            Assert.AreEqual(1, result.ReleasedSummaries.Count);
            Assert.AreEqual(1, result.Clearances.Count);
            Assert.IsNull(typeof(StudentMedicalReleasedSummaryDto).GetProperty("InternalClinicalNotes"));
            Assert.IsNull(typeof(StudentMedicalReleasedSummaryDto).GetProperty("InternalAssessment"));
            Assert.IsNull(typeof(StudentMedicalClearanceDto).GetProperty("RestrictedHistory"));
            Assert.IsNull(typeof(StudentClinicAppointmentDto).GetProperty("ClinicianEmployeeId"));
        }

        [TestMethod]
        public void AdministratorRequiresExplicitRestrictedPermissionForClinicalRecords()
        {
            var record = MedicalRecordWithReleasedSummary(
                "MDR-2026-000301",
                StudentId,
                "CAP-2026-000301",
                "CON-2026-000301");
            var repository = new MedicalRepository(1, new[] { record });
            var denied = new RestrictedMedicalRecordQueryService(
                Executor(AdminPrincipal("clinic.medical.restricted.read")),
                repository);
            Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                denied.Get(
                    "SES-2026-000301",
                    "token",
                    record.Id,
                    StartUtc.AddHours(3)));

            var allowed = new RestrictedMedicalRecordQueryService(
                Executor(AdminPrincipal(
                    "clinic.medical.restricted.read",
                    "confidentiality.restricted")),
                repository);
            var result = allowed.Get(
                "SES-2026-000302",
                "token",
                record.Id,
                StartUtc.AddHours(3));

            Assert.AreEqual(1, result.ConfidentialConsultations.Count);
            Assert.AreEqual("Internal clinical notes",
                result.ConfidentialConsultations[0].InternalClinicalNotes);
        }

        [TestMethod]
        public void StaleAppointmentRevisionAndEntityVersionFailBeforeTransaction()
        {
            var appointment = new ClinicAppointment(
                "CAP-2026-000401",
                StudentId,
                StartUtc.AddHours(1),
                "Released reason",
                StartUtc,
                ActorUserId);
            var appointments = new AppointmentRepository(4, new[] { appointment });
            var coordinator = new ImmediateCoordinator();
            var service = new ClinicAppointmentCommandService(
                Executor(EmployeePrincipal("clinic.appointment.schedule")),
                appointments,
                new MedicalRepository(0, new MedicalRecord[0]),
                coordinator,
                new FakeAllocator());

            Assert.ThrowsExactly<InvalidOperationException>(() =>
                service.Schedule(
                    "SES-2026-000401",
                    "token",
                    new ClinicAppointmentScheduleRequest
                    {
                        ExpectedRepositoryRevision = 3,
                        ExpectedEntityVersion = appointment.Version,
                        AppointmentId = appointment.Id,
                        ScheduledAtUtc = StartUtc.AddHours(2),
                        ClinicianEmployeeId = ClinicianEmployeeId
                    },
                    StartUtc.AddMinutes(5)));
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                service.Schedule(
                    "SES-2026-000401",
                    "token",
                    new ClinicAppointmentScheduleRequest
                    {
                        ExpectedRepositoryRevision = 4,
                        ExpectedEntityVersion = appointment.Version + 1L,
                        AppointmentId = appointment.Id,
                        ScheduledAtUtc = StartUtc.AddHours(2),
                        ClinicianEmployeeId = ClinicianEmployeeId
                    },
                    StartUtc.AddMinutes(5)));

            Assert.AreEqual(0, coordinator.ExecutionCount);
            Assert.AreEqual(ClinicAppointmentStatus.Requested, appointment.Status);
            Assert.AreEqual(4L, appointments.Read().Revision);
        }

        [TestMethod]
        public void ConsultationCompletionCoordinatesAppointmentAndMedicalRecord()
        {
            var appointment = CheckedInAppointment("CAP-2026-000501", StudentId);
            var medicalRecord = new MedicalRecord(
                "MDR-2026-000501",
                StudentId,
                StartUtc,
                ActorUserId);
            var appointments = new AppointmentRepository(2, new[] { appointment });
            var records = new MedicalRepository(3, new[] { medicalRecord });
            var coordinator = new ImmediateCoordinator();
            var service = new ClinicAppointmentCommandService(
                Executor(EmployeePrincipal(
                    "clinic.consultation.record",
                    "confidentiality.restricted")),
                appointments,
                records,
                coordinator,
                new FakeAllocator());

            var result = service.CompleteWithConsultation(
                "SES-2026-000501",
                "token",
                new MedicalConsultationCompletionRequest
                {
                    ExpectedAppointmentRepositoryRevision = 2,
                    ExpectedMedicalRecordRepositoryRevision = 3,
                    ExpectedAppointmentEntityVersion = appointment.Version,
                    ExpectedMedicalRecordEntityVersion = medicalRecord.Version,
                    AppointmentId = appointment.Id,
                    ClinicianEmployeeId = ClinicianEmployeeId,
                    InternalClinicalNotes = "Internal clinical notes",
                    InternalAssessment = "Internal assessment",
                    InternalTreatmentPlan = "Internal treatment plan"
                },
                StartUtc.AddHours(2));

            Assert.AreEqual(1, coordinator.ExecutionCount);
            Assert.AreEqual(2, coordinator.LastStageCount);
            Assert.AreEqual(ClinicAppointmentStatus.Completed,
                appointments.FindById(appointment.Id).Status);
            Assert.AreEqual(1,
                records.FindById(medicalRecord.Id).ConfidentialConsultations.Count);
            Assert.AreEqual(3L, result.AppointmentRepositoryRevision);
            Assert.AreEqual(4L, result.MedicalRecordRepositoryRevision);
        }

        [TestMethod]
        public void StudentCannotRequestClearanceForAnotherStudentsMedicalRecord()
        {
            var otherRecord = new MedicalRecord(
                "MDR-2026-000601",
                "STU-2026-000002",
                StartUtc,
                ActorUserId);
            var coordinator = new ImmediateCoordinator();
            var service = new MedicalClearanceCommandService(
                Executor(StudentPrincipal(StudentId)),
                new MedicalRepository(1, new[] { otherRecord }),
                new ClearanceRepository(0, new MedicalClearance[0]),
                coordinator,
                new FakeAllocator());

            Assert.ThrowsExactly<AuthorizationDeniedException>(() =>
                service.Request(
                    "SES-2026-000601",
                    "token",
                    new MedicalClearanceRequest
                    {
                        ExpectedMedicalRecordRepositoryRevision = 1,
                        ExpectedClearanceRepositoryRevision = 0,
                        ExpectedMedicalRecordEntityVersion = otherRecord.Version,
                        MedicalRecordId = otherRecord.Id,
                        RequestReason = "Enrollment clearance"
                    },
                    StartUtc.AddMinutes(5)));
            Assert.AreEqual(0, coordinator.ExecutionCount);
        }

        [TestMethod]
        public void ClearanceRequestReviewAndIssueUseControlledLifecycle()
        {
            var record = new MedicalRecord(
                "MDR-2026-000701",
                StudentId,
                StartUtc,
                ActorUserId);
            var records = new MedicalRepository(1, new[] { record });
            var clearances = new ClearanceRepository(0, new MedicalClearance[0]);
            var coordinator = new ImmediateCoordinator();
            var allocator = new FakeAllocator();
            var studentService = new MedicalClearanceCommandService(
                Executor(StudentPrincipal(StudentId)),
                records,
                clearances,
                coordinator,
                allocator);
            var requestResult = studentService.Request(
                "SES-2026-000701",
                "token",
                new MedicalClearanceRequest
                {
                    ExpectedMedicalRecordRepositoryRevision = 1,
                    ExpectedClearanceRepositoryRevision = 0,
                    ExpectedMedicalRecordEntityVersion = record.Version,
                    MedicalRecordId = record.Id,
                    RequestReason = "Enrollment clearance"
                },
                StartUtc.AddMinutes(5));

            var employeeService = new MedicalClearanceCommandService(
                Executor(EmployeePrincipal(
                    "clinic.medical.clearance.review",
                    "clinic.medical.clearance.issue",
                    "confidentiality.restricted")),
                records,
                clearances,
                coordinator,
                allocator);
            var clearance = clearances.FindById(requestResult.MedicalClearanceId);
            employeeService.BeginReview(
                "SES-2026-000702",
                "token",
                new MedicalClearanceReviewRequest
                {
                    ExpectedRepositoryRevision = 1,
                    ExpectedEntityVersion = clearance.Version,
                    ClearanceId = clearance.Id,
                    ClinicianEmployeeId = ClinicianEmployeeId
                },
                StartUtc.AddMinutes(10));
            clearance = clearances.FindById(clearance.Id);
            employeeService.Issue(
                "SES-2026-000703",
                "token",
                new MedicalClearanceIssueRequest
                {
                    ExpectedRepositoryRevision = 2,
                    ExpectedEntityVersion = clearance.Version,
                    ClearanceId = clearance.Id,
                    ValidFrom = new InstitutionLocalDate(2026, 7, 22),
                    ValidUntil = new InstitutionLocalDate(2026, 12, 31),
                    ReleasedSummary = "Cleared for enrollment"
                },
                StartUtc.AddMinutes(15));

            clearance = clearances.FindById(clearance.Id);
            Assert.AreEqual(MedicalClearanceStatus.Issued, clearance.Status);
            Assert.AreEqual(3, clearance.History.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(clearance.ClearanceNumber));
        }

        [TestMethod]
        public void DeterministicConsultationFailureRollsBackBothRepositoriesByteForByte()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var store = new JsonRepositoryStore(catalog, options);
                var appointments = new ClinicAppointmentRepositoryAdapter(store);
                var medicalRecords = new MedicalRecordRepositoryAdapter(store);
                var appointment = CheckedInAppointment(
                    "CAP-2026-000801",
                    StudentId);
                appointments.Write(
                    new[] { appointment },
                    0,
                    bootstrap.AdministratorUserId);
                var appointmentPath = Path.Combine(root, "appointments.json");
                var recordPath = Path.Combine(root, "medical_records.json");
                var beforeAppointment = File.ReadAllBytes(appointmentPath);
                var beforeRecord = File.ReadAllBytes(recordPath);
                var service = new ClinicAppointmentCommandService(
                    Executor(EmployeePrincipal(
                        "clinic.consultation.record",
                        "confidentiality.restricted")),
                    appointments,
                    medicalRecords,
                    new JournaledApplicationTransactionCoordinator(
                        new JournaledTransactionCoordinator(
                            catalog,
                            options,
                            new FailAfterFirstMutation())),
                    new ApplicationIdentifierAllocator(catalog, options));

                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    service.CompleteWithConsultation(
                        "SES-2026-000801",
                        "token",
                        new MedicalConsultationCompletionRequest
                        {
                            ExpectedAppointmentRepositoryRevision = 1,
                            ExpectedMedicalRecordRepositoryRevision = 0,
                            ExpectedAppointmentEntityVersion = appointment.Version,
                            ExpectedMedicalRecordEntityVersion = 0,
                            AppointmentId = appointment.Id,
                            ClinicianEmployeeId = ClinicianEmployeeId,
                            InternalClinicalNotes = "Internal clinical notes",
                            InternalAssessment = "Internal assessment",
                            InternalTreatmentPlan = "Internal treatment plan"
                        },
                        StartUtc.AddHours(2)));

                CollectionAssert.AreEqual(beforeAppointment, File.ReadAllBytes(appointmentPath));
                CollectionAssert.AreEqual(beforeRecord, File.ReadAllBytes(recordPath));
                var restarted = new IuisCompositionRoot(root);
                Assert.AreEqual(ClinicAppointmentStatus.CheckedIn,
                    restarted.ClinicAppointments.FindById(appointment.Id).Status);
                Assert.AreEqual(0, restarted.MedicalRecords.Read().Records.Count);
            });
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        private static ClinicAppointment CheckedInAppointment(string id, string studentId)
        {
            var appointment = new ClinicAppointment(
                id,
                studentId,
                StartUtc.AddHours(1),
                "Released reason",
                StartUtc,
                ActorUserId);
            appointment.Schedule(
                StartUtc.AddHours(1),
                ClinicianEmployeeId,
                StartUtc.AddMinutes(5),
                ActorUserId);
            appointment.Confirm(StartUtc.AddMinutes(10), ActorUserId);
            appointment.CheckIn(StartUtc.AddHours(1), ActorUserId);
            return appointment;
        }

        private static MedicalRecord MedicalRecordWithReleasedSummary(
            string id,
            string studentId,
            string appointmentId,
            string consultationId)
        {
            var record = new MedicalRecord(id, studentId, StartUtc, ActorUserId);
            record.AddConsultation(
                consultationId,
                appointmentId,
                ClinicianEmployeeId,
                StartUtc.AddMinutes(10),
                "Internal clinical notes",
                "Internal assessment",
                "Internal treatment plan",
                StartUtc.AddMinutes(10),
                ActorUserId);
            record.ReleaseConsultationSummary(
                "MRS-2026-" + id.Substring(id.Length - 6),
                consultationId,
                "Released recovery summary",
                StartUtc.AddMinutes(20),
                ActorUserId);
            return record;
        }

        private static MedicalClearance IssuedClearance(
            string id,
            string studentId,
            string medicalRecordId)
        {
            var suffix = id.Substring(id.Length - 6);
            var clearance = new MedicalClearance(
                id,
                "MCH-2026-" + suffix,
                studentId,
                medicalRecordId,
                "Enrollment clearance",
                StartUtc,
                ActorUserId);
            clearance.BeginReview(
                "MCH-2025-" + suffix,
                ClinicianEmployeeId,
                StartUtc.AddMinutes(5),
                ActorUserId);
            clearance.Issue(
                "MCH-2024-" + suffix,
                "MCN-2026-" + suffix,
                new InstitutionLocalDate(2026, 7, 22),
                new InstitutionLocalDate(2026, 12, 31),
                "Cleared for enrollment",
                StartUtc.AddMinutes(10),
                ActorUserId);
            return clearance;
        }

        private static SessionAwareRequestExecutor Executor(AuthorizationPrincipal principal)
        {
            return new SessionAwareRequestExecutor(
                new FixedPrincipalProvider(principal),
                new PermissionResolver());
        }

        private static AuthorizationPrincipal StudentPrincipal(string studentId)
        {
            return Principal(
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                studentId,
                new[]
                {
                    "student.medical.read",
                    "student.clinic.appointment.request",
                    "student.medical.clearance.request"
                });
        }

        private static AuthorizationPrincipal EmployeePrincipal(params string[] permissions)
        {
            return Principal(
                PrimaryRole.EmployeeFaculty,
                SessionApplicationKind.UserApplication,
                ClinicianEmployeeId,
                permissions);
        }

        private static AuthorizationPrincipal AdminPrincipal(params string[] permissions)
        {
            return Principal(
                PrimaryRole.Administrator,
                SessionApplicationKind.AdministratorApplication,
                ClinicianEmployeeId,
                permissions);
        }

        private static AuthorizationPrincipal Principal(
            PrimaryRole role,
            SessionApplicationKind applicationKind,
            string personId,
            IEnumerable<string> permissions)
        {
            return new AuthorizationPrincipal(
                ActorUserId,
                personId,
                role,
                applicationKind,
                SessionPurpose.FullAccess,
                "SST-2026-000001",
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

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass12-Clinic-" + Guid.NewGuid().ToString("N"));
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

        private abstract class MemoryRepository<T> : IVersionedRepository<T>
            where T : class, IEntity
        {
            private List<T> _records;

            protected MemoryRepository(string repositoryName, long revision, IEnumerable<T> records)
            {
                RepositoryName = repositoryName;
                Revision = revision;
                _records = (records ?? new T[0]).ToList();
            }

            public string RepositoryName { get; private set; }
            public long Revision { get; private set; }

            public RepositorySnapshot<T> Read()
            {
                return new RepositorySnapshot<T>(RepositoryName, Revision, _records);
            }

            public T FindById(string id)
            {
                return _records.SingleOrDefault(item =>
                    StringComparer.Ordinal.Equals(item.Id, id));
            }

            public void Write(
                IReadOnlyCollection<T> records,
                long expectedRevision,
                string updatedByUserId)
            {
                if (expectedRevision != Revision)
                    throw new InvalidOperationException("Stale memory repository revision.");
                _records = records.ToList();
                Revision = checked(Revision + 1L);
            }
        }

        private sealed class AppointmentRepository :
            MemoryRepository<ClinicAppointment>,
            IClinicAppointmentRepository
        {
            public AppointmentRepository(long revision, IEnumerable<ClinicAppointment> records)
                : base("appointments", revision, records) { }
        }

        private sealed class MedicalRepository :
            MemoryRepository<MedicalRecord>,
            IMedicalRecordRepository
        {
            public MedicalRepository(long revision, IEnumerable<MedicalRecord> records)
                : base("medical_records", revision, records) { }
        }

        private sealed class ClearanceRepository :
            MemoryRepository<MedicalClearance>,
            IMedicalClearanceRepository
        {
            public ClearanceRepository(long revision, IEnumerable<MedicalClearance> records)
                : base("clearances", revision, records) { }
        }

        private sealed class ImmediateCoordinator : IApplicationTransactionCoordinator
        {
            public int ExecutionCount { get; private set; }
            public int LastStageCount { get; private set; }

            public string Execute(Action<IRepositoryTransactionScope> stageMutations)
            {
                ExecutionCount++;
                var scope = new ImmediateScope();
                stageMutations(scope);
                LastStageCount = scope.StageCount;
                return "TXN-2026-000001";
            }

            private sealed class ImmediateScope : IRepositoryTransactionScope
            {
                public int StageCount { get; private set; }

                public void Stage<T>(
                    IVersionedRepository<T> repository,
                    IReadOnlyCollection<T> records,
                    long expectedRevision,
                    string updatedByUserId)
                    where T : class, IEntity
                {
                    StageCount++;
                    repository.Write(records, expectedRevision, updatedByUserId);
                }
            }
        }

        private sealed class FakeAllocator : IApplicationIdentifierAllocator
        {
            private readonly Dictionary<string, int> _counts =
                new Dictionary<string, int>(StringComparer.Ordinal);

            public string Allocate(string prefix, int year, string actorUserId)
            {
                int count;
                _counts.TryGetValue(prefix, out count);
                count++;
                _counts[prefix] = count;
                return prefix + "-" + year.ToString("0000") + "-" + count.ToString("000000");
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

        private sealed class FailAfterFirstMutation : ITransactionFailureInjector
        {
            public void OnStage(TransactionExecutionContext context)
            {
                if (context.Stage == TransactionExecutionStage.MutationApplied
                    && context.AppliedMutationCount == 1)
                {
                    throw new InvalidOperationException(
                        "Deterministic Pass 12 Clinic transaction failure.");
                }
            }
        }
    }
}
