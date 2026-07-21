using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Clinic;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Application.Orchestration
{
    public sealed class ClinicAppointmentRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public DateTime RequestedAppointmentAtUtc { get; set; }
        public string ReleasedReasonSummary { get; set; }
    }

    public sealed class ClinicAppointmentScheduleRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string AppointmentId { get; set; }
        public DateTime ScheduledAtUtc { get; set; }
        public string ClinicianEmployeeId { get; set; }
    }

    public sealed class ClinicAppointmentTransitionRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string AppointmentId { get; set; }
    }

    public sealed class MedicalConsultationCompletionRequest
    {
        public long ExpectedAppointmentRepositoryRevision { get; set; }
        public long ExpectedMedicalRecordRepositoryRevision { get; set; }
        public long ExpectedAppointmentEntityVersion { get; set; }
        public long ExpectedMedicalRecordEntityVersion { get; set; }
        public string AppointmentId { get; set; }
        public string ClinicianEmployeeId { get; set; }
        public string InternalClinicalNotes { get; set; }
        public string InternalAssessment { get; set; }
        public string InternalTreatmentPlan { get; set; }
    }

    public sealed class MedicalSummaryReleaseRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string MedicalRecordId { get; set; }
        public string ConsultationId { get; set; }
        public string ReleasedSummary { get; set; }
    }

    public sealed class MedicalClearanceRequest
    {
        public long ExpectedMedicalRecordRepositoryRevision { get; set; }
        public long ExpectedClearanceRepositoryRevision { get; set; }
        public long ExpectedMedicalRecordEntityVersion { get; set; }
        public string MedicalRecordId { get; set; }
        public string RequestReason { get; set; }
    }

    public sealed class MedicalClearanceReviewRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string ClearanceId { get; set; }
        public string ClinicianEmployeeId { get; set; }
    }

    public sealed class MedicalClearanceIssueRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string ClearanceId { get; set; }
        public InstitutionLocalDate ValidFrom { get; set; }
        public InstitutionLocalDate? ValidUntil { get; set; }
        public string ReleasedSummary { get; set; }
    }

    public sealed class MedicalClearanceReasonRequest
    {
        public long ExpectedRepositoryRevision { get; set; }
        public long ExpectedEntityVersion { get; set; }
        public string ClearanceId { get; set; }
        public string ReleasedReason { get; set; }
    }

    internal static class MedicalCommandGuard
    {
        public static SessionApplicationKind ApplicationKind(AuthorizationPrincipal principal)
        {
            return principal.PrimaryRole == PrimaryRole.Administrator
                ? SessionApplicationKind.AdministratorApplication
                : SessionApplicationKind.UserApplication;
        }

        public static void RequireRevision(long expected, long actual, string repositoryName)
        {
            if (expected < 0L) throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    "The request is based on a stale " + repositoryName + " repository revision.");
            }
        }

        public static void RequireVersion(long expected, long actual, string entityName)
        {
            if (expected < 1L) throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    "The request is based on a stale " + entityName + " entity version.");
            }
        }

        public static T Find<T>(IEnumerable<T> records, string id, string entityName)
            where T : class, IEntity
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(entityName + " ID is required.", nameof(id));
            var value = records.SingleOrDefault(item =>
                StringComparer.Ordinal.Equals(item.Id, id.Trim()));
            if (value == null)
                throw new InvalidOperationException(entityName + " is unavailable.");
            return value;
        }

        public static AuthorizationRequest Internal(
            AuthorizationPrincipal principal,
            string permission,
            ConfidentialityClassification confidentiality)
        {
            return new AuthorizationRequest(
                permission,
                ApplicationKind(principal),
                confidentiality,
                null,
                new[] { PrimaryRole.EmployeeFaculty, PrimaryRole.Administrator });
        }
    }

    public sealed class StudentMedicalServicesQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IClinicAppointmentRepository _appointments;
        private readonly IMedicalRecordRepository _medicalRecords;
        private readonly IMedicalClearanceRepository _clearances;

        public StudentMedicalServicesQueryService(
            SessionAwareRequestExecutor executor,
            IClinicAppointmentRepository appointments,
            IMedicalRecordRepository medicalRecords,
            IMedicalClearanceRepository clearances)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
            _medicalRecords = medicalRecords ?? throw new ArgumentNullException(nameof(medicalRecords));
            _clearances = clearances ?? throw new ArgumentNullException(nameof(clearances));
        }

        public StudentMedicalOverviewDto GetOwnOverview(
            string sessionId,
            string sessionToken,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.medical.read",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal => Build(principal.PersonRecordId));
        }

        private StudentMedicalOverviewDto Build(string studentId)
        {
            var appointmentSnapshot = _appointments.Read();
            var recordSnapshot = _medicalRecords.Read();
            var clearanceSnapshot = _clearances.Read();
            var appointments = appointmentSnapshot.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(item => new StudentClinicAppointmentDto
                {
                    AppointmentId = item.Id,
                    RequestedAppointmentAtUtc = item.RequestedAppointmentAtUtc,
                    ReleasedReasonSummary = item.ReleasedReasonSummary,
                    Status = item.Status.ToString(),
                    ScheduledAtUtc = item.ScheduledAtUtc,
                    CheckedInAtUtc = item.CheckedInAtUtc,
                    CompletedAtUtc = item.CompletedAtUtc,
                    CancellationReason = item.CancellationReason,
                    EntityVersion = item.Version
                })
                .ToList();
            var summaries = recordSnapshot.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .SelectMany(item => item.ReleasedSummaries)
                .OrderByDescending(item => item.ReleasedAtUtc)
                .Select(item => new StudentMedicalReleasedSummaryDto
                {
                    SummaryId = item.SummaryId,
                    ReleasedSummary = item.ReleasedSummary,
                    ReleasedAtUtc = item.ReleasedAtUtc
                })
                .ToList();
            var clearances = clearanceSnapshot.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(item.StudentId, studentId))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(item => new StudentMedicalClearanceDto
                {
                    ClearanceId = item.Id,
                    RequestReason = item.RequestReason,
                    Status = item.Status.ToString(),
                    ClearanceNumber = item.ClearanceNumber,
                    ValidFrom = item.ValidFrom.HasValue ? item.ValidFrom.Value.ToString() : null,
                    ValidUntil = item.ValidUntil.HasValue ? item.ValidUntil.Value.ToString() : null,
                    ReleasedSummary = item.ReleasedSummary,
                    EntityVersion = item.Version
                })
                .ToList();
            return new StudentMedicalOverviewDto
            {
                StudentId = studentId,
                AppointmentRepositoryRevision = appointmentSnapshot.Revision,
                MedicalRecordRepositoryRevision = recordSnapshot.Revision,
                MedicalClearanceRepositoryRevision = clearanceSnapshot.Revision,
                Appointments = appointments.AsReadOnly(),
                ReleasedSummaries = summaries.AsReadOnly(),
                Clearances = clearances.AsReadOnly()
            };
        }
    }

    public sealed class RestrictedMedicalRecordQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IMedicalRecordRepository _records;

        public RestrictedMedicalRecordQueryService(
            SessionAwareRequestExecutor executor,
            IMedicalRecordRepository records)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _records = records ?? throw new ArgumentNullException(nameof(records));
        }

        public RestrictedMedicalRecordDto Get(
            string sessionId,
            string sessionToken,
            string medicalRecordId,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "clinic.medical.restricted.read",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _records.Read();
                    var record = MedicalCommandGuard.Find(
                        snapshot.Records,
                        medicalRecordId,
                        "Medical Record");
                    return new RestrictedMedicalRecordDto
                    {
                        MedicalRecordId = record.Id,
                        StudentId = record.StudentId,
                        Status = record.Status.ToString(),
                        ClosedAtUtc = record.ClosedAtUtc,
                        RepositoryRevision = snapshot.Revision,
                        EntityVersion = record.Version,
                        ConfidentialConsultations = record.ConfidentialConsultations
                            .Select(item => new RestrictedMedicalConsultationDto
                            {
                                ConsultationId = item.ConsultationId,
                                AppointmentId = item.AppointmentId,
                                ClinicianEmployeeId = item.ClinicianEmployeeId,
                                OccurredAtUtc = item.OccurredAtUtc,
                                InternalClinicalNotes = item.InternalClinicalNotes,
                                InternalAssessment = item.InternalAssessment,
                                InternalTreatmentPlan = item.InternalTreatmentPlan
                            }).ToList().AsReadOnly()
                    };
                });
        }
    }

    public sealed class ClinicAppointmentCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IClinicAppointmentRepository _appointments;
        private readonly IMedicalRecordRepository _records;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public ClinicAppointmentCommandService(
            SessionAwareRequestExecutor executor,
            IClinicAppointmentRepository appointments,
            IMedicalRecordRepository records,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _appointments = appointments ?? throw new ArgumentNullException(nameof(appointments));
            _records = records ?? throw new ArgumentNullException(nameof(records));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public ClinicMedicalCommandResult Request(
            string sessionId,
            string sessionToken,
            ClinicAppointmentRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.clinic.appointment.request",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var snapshot = _appointments.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _appointments.RepositoryName);
                    var appointment = new ClinicAppointment(
                        _ids.Allocate("CAP", utcNow.Year, principal.UserId),
                        principal.PersonRecordId,
                        request.RequestedAppointmentAtUtc,
                        request.ReleasedReasonSummary,
                        utcNow,
                        principal.UserId);
                    var records = snapshot.Records.ToList();
                    records.Add(appointment);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _appointments,
                            records,
                            snapshot.Revision,
                            principal.UserId));
                    return AppointmentResult(
                        transactionId,
                        appointment,
                        checked(snapshot.Revision + 1L));
                });
        }

        public ClinicMedicalCommandResult Schedule(
            string sessionId,
            string sessionToken,
            ClinicAppointmentScheduleRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return MutateAppointment(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.appointment.schedule",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.AppointmentId,
                (appointment, principal) => appointment.Schedule(
                    request.ScheduledAtUtc,
                    request.ClinicianEmployeeId,
                    utcNow,
                    principal.UserId));
        }

        public ClinicMedicalCommandResult Confirm(
            string sessionId,
            string sessionToken,
            ClinicAppointmentTransitionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return MutateAppointment(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.appointment.confirm",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.AppointmentId,
                (appointment, principal) => appointment.Confirm(utcNow, principal.UserId));
        }

        public ClinicMedicalCommandResult CheckIn(
            string sessionId,
            string sessionToken,
            ClinicAppointmentTransitionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return MutateAppointment(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.appointment.checkin",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.AppointmentId,
                (appointment, principal) => appointment.CheckIn(utcNow, principal.UserId));
        }

        public ClinicMedicalCommandResult MarkNoShow(
            string sessionId,
            string sessionToken,
            ClinicAppointmentTransitionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return MutateAppointment(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.appointment.noshow",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.AppointmentId,
                (appointment, principal) => appointment.MarkNoShow(utcNow, principal.UserId));
        }

        public ClinicMedicalCommandResult CompleteWithConsultation(
            string sessionId,
            string sessionToken,
            MedicalConsultationCompletionRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "clinic.consultation.record",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var appointmentSnapshot = _appointments.Read();
                    var recordSnapshot = _records.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedAppointmentRepositoryRevision,
                        appointmentSnapshot.Revision,
                        _appointments.RepositoryName);
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedMedicalRecordRepositoryRevision,
                        recordSnapshot.Revision,
                        _records.RepositoryName);
                    var appointment = MedicalCommandGuard.Find(
                        appointmentSnapshot.Records,
                        request.AppointmentId,
                        "Clinic Appointment");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedAppointmentEntityVersion,
                        appointment.Version,
                        "Clinic Appointment");
                    if (appointment.Status != ClinicAppointmentStatus.CheckedIn)
                        throw new InvalidOperationException("Only a checked-in Appointment can be completed.");

                    var activeRecords = recordSnapshot.Records
                        .Where(item => !item.IsArchived)
                        .Where(item => StringComparer.Ordinal.Equals(
                            item.StudentId,
                            appointment.StudentId))
                        .Where(item => item.Status == MedicalRecordStatus.Active)
                        .ToList();
                    if (activeRecords.Count > 1)
                        throw new InvalidOperationException("The Student has multiple active Medical Records.");
                    MedicalRecord medicalRecord;
                    var medicalRecords = recordSnapshot.Records.ToList();
                    if (activeRecords.Count == 0)
                    {
                        if (request.ExpectedMedicalRecordEntityVersion != 0L)
                            throw new InvalidOperationException("The expected Medical Record version is invalid for a new record.");
                        medicalRecord = new MedicalRecord(
                            _ids.Allocate("MDR", utcNow.Year, principal.UserId),
                            appointment.StudentId,
                            utcNow,
                            principal.UserId);
                        medicalRecords.Add(medicalRecord);
                    }
                    else
                    {
                        medicalRecord = activeRecords[0];
                        MedicalCommandGuard.RequireVersion(
                            request.ExpectedMedicalRecordEntityVersion,
                            medicalRecord.Version,
                            "Medical Record");
                    }

                    var consultationId = _ids.Allocate("CON", utcNow.Year, principal.UserId);
                    medicalRecord.AddConsultation(
                        consultationId,
                        appointment.Id,
                        request.ClinicianEmployeeId,
                        utcNow,
                        request.InternalClinicalNotes,
                        request.InternalAssessment,
                        request.InternalTreatmentPlan,
                        utcNow,
                        principal.UserId);
                    appointment.Complete(consultationId, utcNow, principal.UserId);
                    var transactionId = _transactions.Execute(scope =>
                    {
                        scope.Stage(
                            _appointments,
                            appointmentSnapshot.Records.ToList(),
                            appointmentSnapshot.Revision,
                            principal.UserId);
                        scope.Stage(
                            _records,
                            medicalRecords,
                            recordSnapshot.Revision,
                            principal.UserId);
                    });
                    return new ClinicMedicalCommandResult
                    {
                        TransactionId = transactionId,
                        AppointmentId = appointment.Id,
                        MedicalRecordId = medicalRecord.Id,
                        AppointmentRepositoryRevision = checked(appointmentSnapshot.Revision + 1L),
                        MedicalRecordRepositoryRevision = checked(recordSnapshot.Revision + 1L),
                        AppointmentEntityVersion = appointment.Version,
                        MedicalRecordEntityVersion = medicalRecord.Version,
                        UpdatedAtUtc = utcNow,
                        UpdatedByUserId = principal.UserId
                    };
                });
        }

        private ClinicMedicalCommandResult MutateAppointment(
            string sessionId,
            string sessionToken,
            DateTime utcNow,
            string permission,
            long expectedRevision,
            long expectedVersion,
            string appointmentId,
            Action<ClinicAppointment, AuthorizationPrincipal> mutation)
        {
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    permission,
                    ConfidentialityClassification.Internal),
                principal =>
                {
                    var snapshot = _appointments.Read();
                    MedicalCommandGuard.RequireRevision(
                        expectedRevision,
                        snapshot.Revision,
                        _appointments.RepositoryName);
                    var appointment = MedicalCommandGuard.Find(
                        snapshot.Records,
                        appointmentId,
                        "Clinic Appointment");
                    MedicalCommandGuard.RequireVersion(
                        expectedVersion,
                        appointment.Version,
                        "Clinic Appointment");
                    mutation(appointment, principal);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _appointments,
                            snapshot.Records.ToList(),
                            snapshot.Revision,
                            principal.UserId));
                    return AppointmentResult(
                        transactionId,
                        appointment,
                        checked(snapshot.Revision + 1L));
                });
        }

        private static ClinicMedicalCommandResult AppointmentResult(
            string transactionId,
            ClinicAppointment appointment,
            long repositoryRevision)
        {
            return new ClinicMedicalCommandResult
            {
                TransactionId = transactionId,
                AppointmentId = appointment.Id,
                AppointmentRepositoryRevision = repositoryRevision,
                AppointmentEntityVersion = appointment.Version,
                UpdatedAtUtc = appointment.UpdatedAtUtc,
                UpdatedByUserId = appointment.UpdatedByUserId
            };
        }
    }

    public sealed class MedicalRecordCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IMedicalRecordRepository _records;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public MedicalRecordCommandService(
            SessionAwareRequestExecutor executor,
            IMedicalRecordRepository records,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _records = records ?? throw new ArgumentNullException(nameof(records));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public ClinicMedicalCommandResult ReleaseSummary(
            string sessionId,
            string sessionToken,
            MedicalSummaryReleaseRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    "clinic.medical.summary.release",
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _records.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedRepositoryRevision,
                        snapshot.Revision,
                        _records.RepositoryName);
                    var record = MedicalCommandGuard.Find(
                        snapshot.Records,
                        request.MedicalRecordId,
                        "Medical Record");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedEntityVersion,
                        record.Version,
                        "Medical Record");
                    record.ReleaseConsultationSummary(
                        _ids.Allocate("MRS", utcNow.Year, principal.UserId),
                        request.ConsultationId,
                        request.ReleasedSummary,
                        utcNow,
                        principal.UserId);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _records,
                            snapshot.Records.ToList(),
                            snapshot.Revision,
                            principal.UserId));
                    return new ClinicMedicalCommandResult
                    {
                        TransactionId = transactionId,
                        MedicalRecordId = record.Id,
                        MedicalRecordRepositoryRevision = checked(snapshot.Revision + 1L),
                        MedicalRecordEntityVersion = record.Version,
                        UpdatedAtUtc = record.UpdatedAtUtc,
                        UpdatedByUserId = record.UpdatedByUserId
                    };
                });
        }
    }

    public sealed class MedicalClearanceCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IMedicalRecordRepository _records;
        private readonly IMedicalClearanceRepository _clearances;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public MedicalClearanceCommandService(
            SessionAwareRequestExecutor executor,
            IMedicalRecordRepository records,
            IMedicalClearanceRepository clearances,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _records = records ?? throw new ArgumentNullException(nameof(records));
            _clearances = clearances ?? throw new ArgumentNullException(nameof(clearances));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public ClinicMedicalCommandResult Request(
            string sessionId,
            string sessionToken,
            MedicalClearanceRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.medical.clearance.request",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var recordSnapshot = _records.Read();
                    var clearanceSnapshot = _clearances.Read();
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedMedicalRecordRepositoryRevision,
                        recordSnapshot.Revision,
                        _records.RepositoryName);
                    MedicalCommandGuard.RequireRevision(
                        request.ExpectedClearanceRepositoryRevision,
                        clearanceSnapshot.Revision,
                        _clearances.RepositoryName);
                    var medicalRecord = MedicalCommandGuard.Find(
                        recordSnapshot.Records,
                        request.MedicalRecordId,
                        "Medical Record");
                    MedicalCommandGuard.RequireVersion(
                        request.ExpectedMedicalRecordEntityVersion,
                        medicalRecord.Version,
                        "Medical Record");
                    if (!StringComparer.Ordinal.Equals(
                        medicalRecord.StudentId,
                        principal.PersonRecordId))
                    {
                        throw new AuthorizationDeniedException("record-ownership-mismatch");
                    }
                    var clearance = new MedicalClearance(
                        _ids.Allocate("MCL", utcNow.Year, principal.UserId),
                        _ids.Allocate("MCH", utcNow.Year, principal.UserId),
                        principal.PersonRecordId,
                        medicalRecord.Id,
                        request.RequestReason,
                        utcNow,
                        principal.UserId);
                    var clearances = clearanceSnapshot.Records.ToList();
                    clearances.Add(clearance);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _clearances,
                            clearances,
                            clearanceSnapshot.Revision,
                            principal.UserId));
                    return ClearanceResult(
                        transactionId,
                        clearance,
                        checked(clearanceSnapshot.Revision + 1L));
                });
        }

        public ClinicMedicalCommandResult BeginReview(
            string sessionId,
            string sessionToken,
            MedicalClearanceReviewRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Mutate(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.medical.clearance.review",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.ClearanceId,
                (clearance, principal) => clearance.BeginReview(
                    _ids.Allocate("MCH", utcNow.Year, principal.UserId),
                    request.ClinicianEmployeeId,
                    utcNow,
                    principal.UserId));
        }

        public ClinicMedicalCommandResult Issue(
            string sessionId,
            string sessionToken,
            MedicalClearanceIssueRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Mutate(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.medical.clearance.issue",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.ClearanceId,
                (clearance, principal) => clearance.Issue(
                    _ids.Allocate("MCH", utcNow.Year, principal.UserId),
                    _ids.Allocate("MCN", utcNow.Year, principal.UserId),
                    request.ValidFrom,
                    request.ValidUntil,
                    request.ReleasedSummary,
                    utcNow,
                    principal.UserId));
        }

        public ClinicMedicalCommandResult Deny(
            string sessionId,
            string sessionToken,
            MedicalClearanceReasonRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Mutate(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.medical.clearance.deny",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.ClearanceId,
                (clearance, principal) => clearance.Deny(
                    _ids.Allocate("MCH", utcNow.Year, principal.UserId),
                    request.ReleasedReason,
                    utcNow,
                    principal.UserId));
        }

        public ClinicMedicalCommandResult Revoke(
            string sessionId,
            string sessionToken,
            MedicalClearanceReasonRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return Mutate(
                sessionId,
                sessionToken,
                utcNow,
                "clinic.medical.clearance.revoke",
                request.ExpectedRepositoryRevision,
                request.ExpectedEntityVersion,
                request.ClearanceId,
                (clearance, principal) => clearance.Revoke(
                    _ids.Allocate("MCH", utcNow.Year, principal.UserId),
                    request.ReleasedReason,
                    utcNow,
                    principal.UserId));
        }

        private ClinicMedicalCommandResult Mutate(
            string sessionId,
            string sessionToken,
            DateTime utcNow,
            string permission,
            long expectedRevision,
            long expectedVersion,
            string clearanceId,
            Action<MedicalClearance, AuthorizationPrincipal> mutation)
        {
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => MedicalCommandGuard.Internal(
                    principal,
                    permission,
                    ConfidentialityClassification.Restricted),
                principal =>
                {
                    var snapshot = _clearances.Read();
                    MedicalCommandGuard.RequireRevision(
                        expectedRevision,
                        snapshot.Revision,
                        _clearances.RepositoryName);
                    var clearance = MedicalCommandGuard.Find(
                        snapshot.Records,
                        clearanceId,
                        "Medical Clearance");
                    MedicalCommandGuard.RequireVersion(
                        expectedVersion,
                        clearance.Version,
                        "Medical Clearance");
                    mutation(clearance, principal);
                    var transactionId = _transactions.Execute(scope =>
                        scope.Stage(
                            _clearances,
                            snapshot.Records.ToList(),
                            snapshot.Revision,
                            principal.UserId));
                    return ClearanceResult(
                        transactionId,
                        clearance,
                        checked(snapshot.Revision + 1L));
                });
        }

        private static ClinicMedicalCommandResult ClearanceResult(
            string transactionId,
            MedicalClearance clearance,
            long repositoryRevision)
        {
            return new ClinicMedicalCommandResult
            {
                TransactionId = transactionId,
                MedicalClearanceId = clearance.Id,
                MedicalClearanceRepositoryRevision = repositoryRevision,
                MedicalClearanceEntityVersion = clearance.Version,
                UpdatedAtUtc = clearance.UpdatedAtUtc,
                UpdatedByUserId = clearance.UpdatedByUserId
            };
        }
    }
}
