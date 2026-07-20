using System;
using System.Collections.Generic;

namespace IUIS.Infrastructure.Persistence
{
    public abstract class PersistedEntityRecord
    {
        public int RecordSchemaVersion { get; set; }
        public string Id { get; set; }
        public long Version { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }
        public string ArchivedByUserId { get; set; }
    }

    public sealed class PersistedPersonName
    {
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }
        public string Suffix { get; set; }
    }

    public sealed class PersistedContactInformation
    {
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string AlternatePhoneNumber { get; set; }
    }

    public sealed class PersistedPostalAddress
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Barangay { get; set; }
        public string CityMunicipality { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }

    public sealed class PersistedStudentRecord : PersistedEntityRecord
    {
        public string StudentNumber { get; set; }
        public PersistedPersonName Name { get; set; }
        public PersistedContactInformation Contact { get; set; }
        public PersistedPostalAddress Address { get; set; }
        public string BirthDate { get; set; }
        public string CourseId { get; set; }
        public string Status { get; set; }
    }

    public sealed class PersistedEmployeeMasterRecord : PersistedEntityRecord
    {
        public string EmployeeNumber { get; set; }
        public PersistedPersonName Name { get; set; }
        public PersistedContactInformation Contact { get; set; }
        public PersistedPostalAddress Address { get; set; }
        public string BirthDate { get; set; }
        public string DepartmentId { get; set; }
        public string PositionTitle { get; set; }
        public string Status { get; set; }
        public bool IsFaculty { get; set; }
    }

    public sealed class PersistedCourseRecord : PersistedEntityRecord
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DepartmentId { get; set; }
        public int DurationYears { get; set; }
        public string Status { get; set; }
    }

    public sealed class PersistedSubjectRecord : PersistedEntityRecord
    {
        public PersistedSubjectRecord()
        {
            PrerequisiteSubjectIds = new List<string>();
        }

        public string Code { get; set; }
        public string Title { get; set; }
        public decimal Units { get; set; }
        public string Status { get; set; }
        public List<string> PrerequisiteSubjectIds { get; set; }
    }

    public sealed class PersistedAcademicPeriodRecord : PersistedEntityRecord
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string EnrollmentOpenDate { get; set; }
        public string EnrollmentCloseDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
    }

    public sealed class PersistedAssessmentChargeRuleRecord : PersistedEntityRecord
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string CalculationKind { get; set; }
        public decimal RateAmount { get; set; }
        public string RateCurrencyCode { get; set; }
        public string Status { get; set; }
    }
}
