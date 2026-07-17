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
            if (!StringComparer.Ordinal.Equals(parsedId.Prefix, "STU") || !parsedId.Equals(parsedStudentNumber))
            {
                throw new DomainValidationException("Student ID and Student Number must be the same STU identifier.");
            }

            StudentNumber = parsedStudentNumber.Value;
            Name = name ?? throw new DomainValidationException("Student name is required.");
            Contact = contact ?? throw new DomainValidationException("Student contact information is required.");
            Address = address ?? throw new DomainValidationException("Student address is required.");
            BirthDate = birthDate ?? throw new DomainValidationException("Student birth date is required.");
            CourseId = DomainGuard.RequiredIdentifier(courseId, nameof(courseId));
            Status = status;
        }

        public string StudentNumber { get; private set; }
        public PersonName Name { get; private set; }
        public ContactInformation Contact { get; private set; }
        public PostalAddress Address { get; private set; }
        public InstitutionLocalDate BirthDate { get; private set; }
        public string CourseId { get; private set; }
        public StudentStatus Status { get; private set; }

        public void UpdateContact(ContactInformation contact, PostalAddress address, DateTime changedAtUtc, string changedByUserId)
        {
            Contact = contact ?? throw new DomainValidationException("Student contact information is required.");
            Address = address ?? throw new DomainValidationException("Student address is required.");
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void ChangeCourse(string courseId, DateTime changedAtUtc, string changedByUserId)
        {
            CourseId = DomainGuard.RequiredIdentifier(courseId, nameof(courseId));
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void SetStatus(StudentStatus status, DateTime changedAtUtc, string changedByUserId)
        {
            Status = status;
            RecordChange(changedAtUtc, changedByUserId);
        }
    }
}
