using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.People
{
    public sealed class EmployeeRecord : EntityBase
    {
        private EmployeeRecord()
        {
            EmployeeNumber = string.Empty;
            DepartmentId = string.Empty;
            PositionTitle = string.Empty;
        }

        public EmployeeRecord(
            string id,
            string employeeNumber,
            PersonName name,
            ContactInformation contact,
            PostalAddress address,
            InstitutionLocalDate birthDate,
            string departmentId,
            string positionTitle,
            EmploymentStatus status,
            bool isFaculty,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(id, createdAtUtc, createdByUserId)
        {
            var parsedId = InstitutionIdentifier.Parse(id);
            var parsedEmployeeNumber = InstitutionIdentifier.Parse(employeeNumber);
            if (!StringComparer.Ordinal.Equals(parsedId.Prefix, "EMP") || !parsedId.Equals(parsedEmployeeNumber))
            {
                throw new DomainValidationException("Employee ID and Employee Number must be the same EMP identifier.");
            }

            EmployeeNumber = parsedEmployeeNumber.Value;
            Name = name ?? throw new DomainValidationException("Employee name is required.");
            Contact = contact ?? throw new DomainValidationException("Employee contact information is required.");
            Address = address ?? throw new DomainValidationException("Employee address is required.");
            BirthDate = birthDate;
            DepartmentId = DomainGuard.RequiredIdentifier(departmentId, nameof(departmentId));
            PositionTitle = TextNormalizer.Required(positionTitle, nameof(positionTitle), 150);
            Status = RequireStatus(status);
            IsFaculty = isFaculty;
        }

        public string EmployeeNumber { get; private set; }
        public PersonName Name { get; private set; }
        public ContactInformation Contact { get; private set; }
        public PostalAddress Address { get; private set; }
        public InstitutionLocalDate BirthDate { get; private set; }
        public string DepartmentId { get; private set; }
        public string PositionTitle { get; private set; }
        public EmploymentStatus Status { get; private set; }
        public bool IsFaculty { get; private set; }

        public static EmployeeRecord Rehydrate(
            string id,
            string employeeNumber,
            PersonName name,
            ContactInformation contact,
            PostalAddress address,
            InstitutionLocalDate birthDate,
            string departmentId,
            string positionTitle,
            EmploymentStatus status,
            bool isFaculty,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            var record = new EmployeeRecord(
                id,
                employeeNumber,
                name,
                contact,
                address,
                birthDate,
                departmentId,
                positionTitle,
                status,
                isFaculty,
                createdAtUtc,
                createdByUserId);
            record.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return record;
        }

        public void UpdateContact(ContactInformation contact, PostalAddress address, DateTime changedAtUtc, string changedByUserId)
        {
            var normalizedContact = contact ?? throw new DomainValidationException("Employee contact information is required.");
            var normalizedAddress = address ?? throw new DomainValidationException("Employee address is required.");
            RecordChange(changedAtUtc, changedByUserId);
            Contact = normalizedContact;
            Address = normalizedAddress;
        }

        public void ChangeAssignment(string departmentId, string positionTitle, bool isFaculty, DateTime changedAtUtc, string changedByUserId)
        {
            var normalizedDepartmentId = DomainGuard.RequiredIdentifier(departmentId, nameof(departmentId));
            var normalizedPositionTitle = TextNormalizer.Required(positionTitle, nameof(positionTitle), 150);
            RecordChange(changedAtUtc, changedByUserId);
            DepartmentId = normalizedDepartmentId;
            PositionTitle = normalizedPositionTitle;
            IsFaculty = isFaculty;
        }

        public void SetStatus(EmploymentStatus status, DateTime changedAtUtc, string changedByUserId)
        {
            var normalizedStatus = RequireStatus(status);
            RecordChange(changedAtUtc, changedByUserId);
            Status = normalizedStatus;
        }

        private static EmploymentStatus RequireStatus(EmploymentStatus value)
        {
            if (!Enum.IsDefined(typeof(EmploymentStatus), value))
            {
                throw new DomainValidationException("Employment status is invalid.");
            }

            return value;
        }
    }
}
