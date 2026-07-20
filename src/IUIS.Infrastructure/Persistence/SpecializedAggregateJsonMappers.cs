using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using IUIS.Domain.Academic;
using IUIS.Domain.Common;
using IUIS.Domain.Finance;
using IUIS.Domain.People;
using IUIS.Domain.Time;

namespace IUIS.Infrastructure.Persistence
{
    internal static class PersistedRecordMapperGuard
    {
        public const int CurrentRecordSchemaVersion = 1;

        public static TRecord Read<TRecord>(
            JsonElement element,
            JsonSerializerOptions options,
            string aggregateName)
            where TRecord : PersistedEntityRecord
        {
            TRecord record;
            try
            {
                record = JsonSerializer.Deserialize<TRecord>(element.GetRawText(), options);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    aggregateName + " persisted record JSON is invalid.",
                    exception);
            }

            if (record == null)
            {
                throw new InvalidOperationException(
                    aggregateName + " persisted record is missing.");
            }

            if (record.RecordSchemaVersion != 0
                && record.RecordSchemaVersion != CurrentRecordSchemaVersion)
            {
                throw new InvalidOperationException(
                    aggregateName + " persisted record schema version is unsupported.");
            }

            return record;
        }

        public static JsonElement Write<TRecord>(
            TRecord record,
            JsonSerializerOptions options)
            where TRecord : PersistedEntityRecord
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.RecordSchemaVersion = CurrentRecordSchemaVersion;
            return JsonSerializer.SerializeToElement(record, options);
        }

        public static TEnum ParseEnum<TEnum>(
            string value,
            string propertyName,
            bool allowZero)
            where TEnum : struct
        {
            TEnum parsed;
            if (string.IsNullOrWhiteSpace(value)
                || !Enum.TryParse(value.Trim(), true, out parsed)
                || !Enum.IsDefined(typeof(TEnum), parsed)
                || (!allowZero && Convert.ToInt32(parsed) == 0))
            {
                throw new InvalidOperationException(
                    propertyName + " contains an unsupported persisted value.");
            }

            return parsed;
        }

        public static PersonName Name(PersistedPersonName value)
        {
            if (value == null)
                throw new InvalidOperationException("Persisted PersonName is required.");
            return new PersonName(
                value.GivenName,
                value.MiddleName,
                value.FamilyName,
                value.Suffix);
        }

        public static PersistedPersonName Name(PersonName value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new PersistedPersonName
            {
                GivenName = value.GivenName,
                MiddleName = value.MiddleName,
                FamilyName = value.FamilyName,
                Suffix = value.Suffix
            };
        }

        public static ContactInformation Contact(PersistedContactInformation value)
        {
            if (value == null)
                throw new InvalidOperationException("Persisted ContactInformation is required.");
            return new ContactInformation(
                value.EmailAddress,
                value.MobileNumber,
                value.AlternatePhoneNumber);
        }

        public static PersistedContactInformation Contact(ContactInformation value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new PersistedContactInformation
            {
                EmailAddress = value.EmailAddress,
                MobileNumber = value.MobileNumber,
                AlternatePhoneNumber = value.AlternatePhoneNumber
            };
        }

        public static PostalAddress Address(PersistedPostalAddress value)
        {
            if (value == null)
                throw new InvalidOperationException("Persisted PostalAddress is required.");
            return new PostalAddress(
                value.AddressLine1,
                value.AddressLine2,
                value.Barangay,
                value.CityMunicipality,
                value.Province,
                value.PostalCode,
                value.CountryCode);
        }

        public static PersistedPostalAddress Address(PostalAddress value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new PersistedPostalAddress
            {
                AddressLine1 = value.AddressLine1,
                AddressLine2 = value.AddressLine2,
                Barangay = value.Barangay,
                CityMunicipality = value.CityMunicipality,
                Province = value.Province,
                PostalCode = value.PostalCode,
                CountryCode = value.CountryCode
            };
        }
    }

    public sealed class StudentRecordJsonMapper : IJsonRecordMapper<StudentRecord>
    {
        public StudentRecord FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedStudentRecord>(
                element,
                options,
                "StudentRecord");
            return StudentRecord.Rehydrate(
                record.Id,
                record.StudentNumber,
                PersistedRecordMapperGuard.Name(record.Name),
                PersistedRecordMapperGuard.Contact(record.Contact),
                PersistedRecordMapperGuard.Address(record.Address),
                InstitutionLocalDate.Parse(record.BirthDate),
                record.CourseId,
                PersistedRecordMapperGuard.ParseEnum<StudentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(StudentRecord value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedStudentRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentNumber = value.StudentNumber,
                    Name = PersistedRecordMapperGuard.Name(value.Name),
                    Contact = PersistedRecordMapperGuard.Contact(value.Contact),
                    Address = PersistedRecordMapperGuard.Address(value.Address),
                    BirthDate = value.BirthDate.ToString(),
                    CourseId = value.CourseId,
                    Status = value.Status.ToString()
                },
                options);
        }
    }

    public sealed class EmployeeRecordJsonMapper : IJsonRecordMapper<EmployeeRecord>
    {
        public EmployeeRecord FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedEmployeeMasterRecord>(
                element,
                options,
                "EmployeeRecord");
            return EmployeeRecord.Rehydrate(
                record.Id,
                record.EmployeeNumber,
                PersistedRecordMapperGuard.Name(record.Name),
                PersistedRecordMapperGuard.Contact(record.Contact),
                PersistedRecordMapperGuard.Address(record.Address),
                InstitutionLocalDate.Parse(record.BirthDate),
                record.DepartmentId,
                record.PositionTitle,
                PersistedRecordMapperGuard.ParseEnum<EmploymentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.IsFaculty,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(EmployeeRecord value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedEmployeeMasterRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    EmployeeNumber = value.EmployeeNumber,
                    Name = PersistedRecordMapperGuard.Name(value.Name),
                    Contact = PersistedRecordMapperGuard.Contact(value.Contact),
                    Address = PersistedRecordMapperGuard.Address(value.Address),
                    BirthDate = value.BirthDate.ToString(),
                    DepartmentId = value.DepartmentId,
                    PositionTitle = value.PositionTitle,
                    Status = value.Status.ToString(),
                    IsFaculty = value.IsFaculty
                },
                options);
        }
    }

    public sealed class CourseJsonMapper : IJsonRecordMapper<Course>
    {
        public Course FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedCourseRecord>(
                element,
                options,
                "Course");
            return Course.Rehydrate(
                record.Id,
                record.Code,
                record.Name,
                record.DepartmentId,
                record.DurationYears,
                PersistedRecordMapperGuard.ParseEnum<CourseStatus>(
                    record.Status,
                    nameof(record.Status),
                    false),
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(Course value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedCourseRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    Code = value.Code,
                    Name = value.Name,
                    DepartmentId = value.DepartmentId,
                    DurationYears = value.DurationYears,
                    Status = value.Status.ToString()
                },
                options);
        }
    }

    public sealed class SubjectJsonMapper : IJsonRecordMapper<Subject>
    {
        public Subject FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedSubjectRecord>(
                element,
                options,
                "Subject");
            return Subject.Rehydrate(
                record.Id,
                record.Code,
                record.Title,
                record.Units,
                PersistedRecordMapperGuard.ParseEnum<SubjectStatus>(
                    record.Status,
                    nameof(record.Status),
                    false),
                record.PrerequisiteSubjectIds ?? new List<string>(),
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(Subject value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedSubjectRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    Code = value.Code,
                    Title = value.Title,
                    Units = value.Units,
                    Status = value.Status.ToString(),
                    PrerequisiteSubjectIds = value.Prerequisites
                        .Select(item => item.PrerequisiteSubjectId)
                        .ToList()
                },
                options);
        }
    }

    public sealed class AcademicPeriodJsonMapper : IJsonRecordMapper<AcademicPeriod>
    {
        public AcademicPeriod FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedAcademicPeriodRecord>(
                element,
                options,
                "AcademicPeriod");
            return AcademicPeriod.Rehydrate(
                record.Id,
                record.Code,
                record.Name,
                InstitutionLocalDate.Parse(record.EnrollmentOpenDate),
                InstitutionLocalDate.Parse(record.EnrollmentCloseDate),
                InstitutionLocalDate.Parse(record.StartDate),
                InstitutionLocalDate.Parse(record.EndDate),
                PersistedRecordMapperGuard.ParseEnum<AcademicPeriodStatus>(
                    record.Status,
                    nameof(record.Status),
                    false),
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(AcademicPeriod value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedAcademicPeriodRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    Code = value.Code,
                    Name = value.Name,
                    EnrollmentOpenDate = value.EnrollmentOpenDate.ToString(),
                    EnrollmentCloseDate = value.EnrollmentCloseDate.ToString(),
                    StartDate = value.StartDate.ToString(),
                    EndDate = value.EndDate.ToString(),
                    Status = value.Status.ToString()
                },
                options);
        }
    }

    public sealed class AssessmentChargeRuleJsonMapper : IJsonRecordMapper<AssessmentChargeRule>
    {
        public AssessmentChargeRule FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedAssessmentChargeRuleRecord>(
                element,
                options,
                "AssessmentChargeRule");
            return AssessmentChargeRule.Rehydrate(
                record.Id,
                record.Code,
                record.Description,
                PersistedRecordMapperGuard.ParseEnum<AssessmentChargeCategory>(
                    record.Category,
                    nameof(record.Category),
                    false),
                PersistedRecordMapperGuard.ParseEnum<ChargeCalculationKind>(
                    record.CalculationKind,
                    nameof(record.CalculationKind),
                    false),
                new Money(record.RateAmount, record.RateCurrencyCode),
                PersistedRecordMapperGuard.ParseEnum<ChargeRuleStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(AssessmentChargeRule value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedAssessmentChargeRuleRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    Code = value.Code,
                    Description = value.Description,
                    Category = value.Category.ToString(),
                    CalculationKind = value.CalculationKind.ToString(),
                    RateAmount = value.Rate.Amount,
                    RateCurrencyCode = value.Rate.CurrencyCode,
                    Status = value.Status.ToString()
                },
                options);
        }
    }
}
