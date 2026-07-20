using System;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Identity;

namespace IUIS.Application.Orchestration
{
    public sealed class StudentOwnRecordQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IStudentRecordRepository _students;
        private readonly RestrictedProjectionService _projections;

        public StudentOwnRecordQueryService(SessionAwareRequestExecutor executor,
            IStudentRecordRepository students, RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _students = students ?? throw new ArgumentNullException(nameof(students));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public StudentOwnRecordDto GetOwnRecord(string sessionId, string sessionToken, DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "student.profile.read", SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord, principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var record = _students.FindById(principal.PersonRecordId);
                    if (record == null) throw new InvalidOperationException("The Student record is unavailable.");
                    return _projections.ToStudentOwnRecord(record);
                });
        }
    }

    public sealed class EmployeeRecordQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IEmployeeRecordRepository _employees;
        private readonly RestrictedProjectionService _projections;

        public EmployeeRecordQueryService(SessionAwareRequestExecutor executor,
            IEmployeeRecordRepository employees, RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _employees = employees ?? throw new ArgumentNullException(nameof(employees));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public EmployeeSelfServiceDto GetOwnRecord(
            string sessionId,
            string sessionToken,
            DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "employee.profile.read",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.EmployeeFaculty }),
                principal =>
                {
                    var record = _employees.FindById(principal.PersonRecordId);
                    if (record == null)
                        throw new InvalidOperationException("The Employee record is unavailable.");
                    return _projections.ToEmployeeSelfService(record);
                });
        }

        public EmployeeSelfServiceDto GetEmployee(string sessionId, string sessionToken,
            string employeeId, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("An Employee ID is required.", nameof(employeeId));
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "hr.employee.read",
                    principal.PrimaryRole == PrimaryRole.Administrator
                        ? SessionApplicationKind.AdministratorApplication
                        : SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.Internal, employeeId,
                    new[] { PrimaryRole.EmployeeFaculty, PrimaryRole.Administrator }),
                principal =>
                {
                    var record = _employees.FindById(employeeId.Trim());
                    if (record == null) throw new InvalidOperationException("The Employee record is unavailable.");
                    return _projections.ToEmployeeSelfService(record);
                });
        }
    }

    public sealed class ConfidentialServiceQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ICounselingCaseRepository _counseling;
        private readonly IDisciplineCaseRepository _discipline;
        private readonly IMedicalRecordRepository _medical;
        private readonly RestrictedProjectionService _projections;

        public ConfidentialServiceQueryService(SessionAwareRequestExecutor executor,
            ICounselingCaseRepository counseling, IDisciplineCaseRepository discipline,
            IMedicalRecordRepository medical, RestrictedProjectionService projections)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _counseling = counseling ?? throw new ArgumentNullException(nameof(counseling));
            _discipline = discipline ?? throw new ArgumentNullException(nameof(discipline));
            _medical = medical ?? throw new ArgumentNullException(nameof(medical));
            _projections = projections ?? throw new ArgumentNullException(nameof(projections));
        }

        public CounselingReleasedCaseDto GetStudentCounselingRelease(
            string sessionId, string sessionToken, string caseId, DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "student.counseling.released.read", SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord, principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var value = _counseling.FindById(caseId);
                    if (value == null || !string.Equals(value.StudentId, principal.PersonRecordId, StringComparison.Ordinal))
                        throw new AuthorizationDeniedException("record-ownership-mismatch");
                    return _projections.ToCounselingReleased(value);
                });
        }

        public CounselingInternalCaseDto GetCounselingInternal(
            string sessionId, string sessionToken, string caseId, DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "counseling.case.internal.read",
                    principal.PrimaryRole == PrimaryRole.Administrator
                        ? SessionApplicationKind.AdministratorApplication
                        : SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.Restricted, null,
                    new[] { PrimaryRole.EmployeeFaculty, PrimaryRole.Administrator }),
                principal => _projections.ToCounselingInternal(
                    RequireRecord(_counseling.FindById(caseId), "Counseling Case")));
        }

        public DisciplineReleasedCaseDto GetStudentDisciplineRelease(
            string sessionId, string sessionToken, string caseId, DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "student.discipline.released.read", SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord, principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var value = _discipline.FindById(caseId);
                    if (value == null || !string.Equals(value.StudentId, principal.PersonRecordId, StringComparison.Ordinal))
                        throw new AuthorizationDeniedException("record-ownership-mismatch");
                    return _projections.ToDisciplineReleased(value);
                });
        }

        public MedicalReleasedRecordDto GetStudentMedicalRelease(
            string sessionId, string sessionToken, string medicalRecordId, DateTime utcNow)
        {
            return _executor.Query(sessionId, sessionToken, utcNow,
                principal => new AuthorizationRequest(
                    "student.medical.released.read", SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord, principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var value = _medical.FindById(medicalRecordId);
                    if (value == null || !string.Equals(value.StudentId, principal.PersonRecordId, StringComparison.Ordinal))
                        throw new AuthorizationDeniedException("record-ownership-mismatch");
                    return _projections.ToMedicalReleased(value);
                });
        }

        private static T RequireRecord<T>(T value, string entityName) where T : class
        {
            if (value == null) throw new InvalidOperationException(entityName + " is unavailable.");
            return value;
        }
    }
}
