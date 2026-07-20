using System;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Repositories;
using IUIS.Domain.Identity;
using IUIS.Domain.People;

namespace IUIS.Application.Orchestration
{
    public sealed class ContactUpdateRequest
    {
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string AlternatePhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Barangay { get; set; }
        public string CityMunicipality { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }

    public sealed class ContactUpdateResult
    {
        public string TransactionId { get; set; }
        public string RepositoryName { get; set; }
        public string RecordId { get; set; }
        public long RepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
    }

    public sealed class StudentContactUpdateService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IStudentRecordRepository _students;
        private readonly IApplicationTransactionCoordinator _transactions;

        public StudentContactUpdateService(
            SessionAwareRequestExecutor executor,
            IStudentRecordRepository students,
            IApplicationTransactionCoordinator transactions)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _students = students ?? throw new ArgumentNullException(nameof(students));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public ContactUpdateResult UpdateOwnContact(
            string sessionId,
            string sessionToken,
            ContactUpdateRequest request,
            DateTime utcNow)
        {
            var contact = CreateContact(request);
            var address = CreateAddress(request);
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.profile.contact.update",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal =>
                {
                    var snapshot = _students.Read();
                    var record = snapshot.Records.SingleOrDefault(
                        item => string.Equals(
                            item.Id,
                            principal.PersonRecordId,
                            StringComparison.Ordinal));
                    if (record == null)
                    {
                        throw new InvalidOperationException(
                            "The Student record is unavailable.");
                    }

                    record.UpdateContact(contact, address, utcNow, principal.UserAccountId);
                    var transactionId = _transactions.Execute(scope => scope.Stage(
                        _students,
                        snapshot.Records,
                        snapshot.Revision,
                        principal.UserAccountId));
                    return Result(
                        transactionId,
                        _students.RepositoryName,
                        record.Id,
                        checked(snapshot.Revision + 1L),
                        record.Version,
                        record.UpdatedAtUtc,
                        record.UpdatedByUserId);
                });
        }

        private static ContactInformation CreateContact(ContactUpdateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new ContactInformation(
                request.EmailAddress,
                request.MobileNumber,
                request.AlternatePhoneNumber);
        }

        private static PostalAddress CreateAddress(ContactUpdateRequest request)
        {
            return new PostalAddress(
                request.AddressLine1,
                request.AddressLine2,
                request.Barangay,
                request.CityMunicipality,
                request.Province,
                request.PostalCode,
                request.CountryCode);
        }

        private static ContactUpdateResult Result(
            string transactionId,
            string repositoryName,
            string recordId,
            long repositoryRevision,
            long entityVersion,
            DateTime updatedAtUtc,
            string updatedByUserId)
        {
            return new ContactUpdateResult
            {
                TransactionId = transactionId,
                RepositoryName = repositoryName,
                RecordId = recordId,
                RepositoryRevision = repositoryRevision,
                EntityVersion = entityVersion,
                UpdatedAtUtc = updatedAtUtc,
                UpdatedByUserId = updatedByUserId
            };
        }
    }

    public sealed class EmployeeContactUpdateService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly IEmployeeRecordRepository _employees;
        private readonly IApplicationTransactionCoordinator _transactions;

        public EmployeeContactUpdateService(
            SessionAwareRequestExecutor executor,
            IEmployeeRecordRepository employees,
            IApplicationTransactionCoordinator transactions)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _employees = employees ?? throw new ArgumentNullException(nameof(employees));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public ContactUpdateResult UpdateOwnContact(
            string sessionId,
            string sessionToken,
            ContactUpdateRequest request,
            DateTime utcNow)
        {
            var contact = CreateContact(request);
            var address = CreateAddress(request);
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "employee.profile.contact.update",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.EmployeeFaculty }),
                principal =>
                {
                    var snapshot = _employees.Read();
                    var record = snapshot.Records.SingleOrDefault(
                        item => string.Equals(
                            item.Id,
                            principal.PersonRecordId,
                            StringComparison.Ordinal));
                    if (record == null)
                    {
                        throw new InvalidOperationException(
                            "The Employee record is unavailable.");
                    }

                    record.UpdateContact(contact, address, utcNow, principal.UserAccountId);
                    var transactionId = _transactions.Execute(scope => scope.Stage(
                        _employees,
                        snapshot.Records,
                        snapshot.Revision,
                        principal.UserAccountId));
                    return Result(
                        transactionId,
                        _employees.RepositoryName,
                        record.Id,
                        checked(snapshot.Revision + 1L),
                        record.Version,
                        record.UpdatedAtUtc,
                        record.UpdatedByUserId);
                });
        }

        private static ContactInformation CreateContact(ContactUpdateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new ContactInformation(
                request.EmailAddress,
                request.MobileNumber,
                request.AlternatePhoneNumber);
        }

        private static PostalAddress CreateAddress(ContactUpdateRequest request)
        {
            return new PostalAddress(
                request.AddressLine1,
                request.AddressLine2,
                request.Barangay,
                request.CityMunicipality,
                request.Province,
                request.PostalCode,
                request.CountryCode);
        }

        private static ContactUpdateResult Result(
            string transactionId,
            string repositoryName,
            string recordId,
            long repositoryRevision,
            long entityVersion,
            DateTime updatedAtUtc,
            string updatedByUserId)
        {
            return new ContactUpdateResult
            {
                TransactionId = transactionId,
                RepositoryName = repositoryName,
                RecordId = recordId,
                RepositoryRevision = repositoryRevision,
                EntityVersion = entityVersion,
                UpdatedAtUtc = updatedAtUtc,
                UpdatedByUserId = updatedByUserId
            };
        }
    }
}
