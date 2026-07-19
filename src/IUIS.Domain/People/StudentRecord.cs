using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.People
{
    public sealed class StudentRecord : EntityBase
    {
        private StudentRecord()
        {
            StudentNumber = string.Empty;
            CourseId = string.Empty;
        }

        public StudentRecord(
            string id,
            string studentNumber,
            PersonName name,
            ContactInformation contact,
            PostalAddress address,
            InstitutionLocalDate birthDate,
            string courseId,
            StudentStatus status,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(id, createdAtUtc, createdByUserId)
        {
            var parsedId = InstitutionIdentifier.Parse(id);
            var parsedStudentNumber = InstitutionIdentifier.Parse(studentNumber);
            if (!StringComparer.Ordinal.Equals(parsedId.Prefix, "STU")
                || !parsedId.Equals(parsedStudentNumber))
            {
                throw new DomainValidationException(
                    "Student ID and Student Number must be the same STU identifier.");
            }

            StudentNumber = parsedStudentNumber.Value;
            Name = name ?? throw new DomainValidationException("Student name is required.");
            Contact = contact ?? throw new DomainValidationException(
                "Student contact information is required.");
            Address = address ?? throw new DomainValidationException("Student address is required.");
            BirthDate = birthDate;
            CourseId = RequireCourseIdentifier(courseId);
            Status = RequireStatus(status);
        }

        public string StudentNumber { get; private set; }

        public PersonName Name { get; private set; }

        public ContactInformation Contact { get; private set; }

        public PostalAddress Address { get; private set; }

        public InstitutionLocalDate BirthDate { get; private set; }

        public string CourseId { get; private set; }

        public StudentStatus Status { get; private set; }

        public void UpdateContact(
            ContactInformation contact,
            PostalAddress address,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedContact = contact ?? throw new DomainValidationException(
                "Student contact information is required.");
            var normalizedAddress = address ?? throw new DomainValidationException(
                "Student address is required.");

            RecordChange(changedAtUtc, changedByUserId);
            Contact = normalizedContact;
            Address = normalizedAddress;
        }

        public void ChangeCourse(
            string courseId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedCourseId = RequireCourseIdentifier(courseId);
            RecordChange(changedAtUtc, changedByUserId);
            CourseId = normalizedCourseId;
        }

        public void SetStatus(
            StudentStatus status,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedStatus = RequireStatus(status);
            RecordChange(changedAtUtc, changedByUserId);
            Status = normalizedStatus;
        }

        private static string RequireCourseIdentifier(string value)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, "CRS"))
            {
                throw new DomainValidationException("Course IDs must use the CRS prefix.");
            }

            return identifier.Value;
        }

        private static StudentStatus RequireStatus(StudentStatus value)
        {
            if (!Enum.IsDefined(typeof(StudentStatus), value))
            {
                throw new DomainValidationException("Student status is invalid.");
            }

            return value;
        }
    }
}
