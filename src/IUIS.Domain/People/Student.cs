using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.People
{
    public sealed class Student : PersonRecordBase
    {
        private const int MinimumYearLevel = 1;
        private const int MaximumYearLevel = 12;

        private Student()
        {
            CourseId = string.Empty;
            DepartmentCode = string.Empty;
        }

        public Student(
            string id,
            PersonName name,
            ContactInformation contactInformation,
            PostalAddress postalAddress,
            InstitutionLocalDate? dateOfBirth,
            string courseId,
            string departmentCode,
            int yearLevel,
            InstitutionLocalDate admissionDate,
            StudentStatus status,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                IdentifierPolicy.Require(id, IdentifierPrefixes.Student, nameof(id)),
                PersonRecordKind.Student,
                name,
                contactInformation,
                postalAddress,
                dateOfBirth,
                createdAtUtc,
                createdByUserId)
        {
            CourseId = IdentifierPolicy.Require(
                courseId,
                IdentifierPrefixes.Course,
                nameof(courseId));
            DepartmentCode = RequireDepartmentCode(departmentCode, nameof(departmentCode));
            YearLevel = RequireYearLevel(yearLevel, nameof(yearLevel));
            AdmissionDate = admissionDate;
            Status = RequireStatus(status, nameof(status));
        }

        public string StudentNumber
        {
            get { return Id; }
        }

        public string CourseId { get; private set; }

        public string DepartmentCode { get; private set; }

        public int YearLevel { get; private set; }

        public InstitutionLocalDate AdmissionDate { get; private set; }

        public StudentStatus Status { get; private set; }

        public void ChangeAcademicPlacement(
            string courseId,
            string departmentCode,
            int yearLevel,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureMutableLifecycle();

            var normalizedCourseId = IdentifierPolicy.Require(
                courseId,
                IdentifierPrefixes.Course,
                nameof(courseId));
            var normalizedDepartmentCode = RequireDepartmentCode(
                departmentCode,
                nameof(departmentCode));
            var normalizedYearLevel = RequireYearLevel(yearLevel, nameof(yearLevel));

            RecordChange(changedAtUtc, changedByUserId);
            CourseId = normalizedCourseId;
            DepartmentCode = normalizedDepartmentCode;
            YearLevel = normalizedYearLevel;
        }

        public void ChangeStatus(
            StudentStatus newStatus,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(newStatus, nameof(newStatus));

            if (newStatus == Status)
            {
                throw new DomainValidationException("The Student already has the requested status.");
            }

            if (!CanTransition(Status, newStatus))
            {
                throw new DomainValidationException(
                    "The Student status cannot transition from " + Status + " to " + newStatus + ".");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = newStatus;
        }

        public static bool CanTransition(StudentStatus currentStatus, StudentStatus newStatus)
        {
            if (currentStatus == StudentStatus.PendingAdmission)
            {
                return newStatus == StudentStatus.Active
                    || newStatus == StudentStatus.Withdrawn;
            }

            if (currentStatus == StudentStatus.Active)
            {
                return newStatus == StudentStatus.OnLeave
                    || newStatus == StudentStatus.Inactive
                    || newStatus == StudentStatus.Graduated
                    || newStatus == StudentStatus.Withdrawn
                    || newStatus == StudentStatus.Dismissed;
            }

            if (currentStatus == StudentStatus.OnLeave)
            {
                return newStatus == StudentStatus.Active
                    || newStatus == StudentStatus.Inactive
                    || newStatus == StudentStatus.Withdrawn;
            }

            if (currentStatus == StudentStatus.Inactive)
            {
                return newStatus == StudentStatus.Active
                    || newStatus == StudentStatus.Withdrawn;
            }

            return false;
        }

        private static StudentStatus RequireStatus(StudentStatus status, string parameterName)
        {
            if (status == StudentStatus.Unspecified
                || !Enum.IsDefined(typeof(StudentStatus), status))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return status;
        }

        private static int RequireYearLevel(int value, string parameterName)
        {
            if (value < MinimumYearLevel || value > MaximumYearLevel)
            {
                throw new DomainValidationException(
                    parameterName + " must be between " + MinimumYearLevel + " and " + MaximumYearLevel + ".");
            }

            return value;
        }

        private void EnsureMutableLifecycle()
        {
            if (Status == StudentStatus.Graduated
                || Status == StudentStatus.Withdrawn
                || Status == StudentStatus.Dismissed)
            {
                throw new DomainValidationException(
                    "Academic placement cannot be changed after the Student reaches a terminal status.");
            }
        }
    }
}
