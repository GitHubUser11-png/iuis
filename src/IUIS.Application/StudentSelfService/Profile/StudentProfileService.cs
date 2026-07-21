using System;
using System.Collections.Generic;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.Common;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.Application.StudentSelfService.Access;
using IUIS.Domain.Students;
using IUIS.Domain.Projections.Student;

namespace IUIS.Application.StudentSelfService.Profile
{
    public sealed class StudentProfileService : IStudentProfileService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentProfileService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public StudentProfileView GetProfile(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Profile.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            var profile = new StudentProfileView
            {
                StudentId = context.Student.Id,
                FirstName = context.Student.Name.GivenName,
                MiddleName = context.Student.Name.MiddleName ?? string.Empty,
                LastName = context.Student.Name.FamilyName,
                Suffix = context.Student.Name.Suffix ?? string.Empty,
                BirthDate = System.DateTime.Parse(context.Student.BirthDate.ToString()),
                Sex = "Not Specified",
                Address = context.Student.Address.ToString(),
                ContactNumber = context.Student.Contact.MobileNumber ?? string.Empty,
                EmailAddress = context.Student.Contact.EmailAddress ?? string.Empty,
                ProgramName = GetProgramName(snapshot, context.StudentId),
                YearLevel = GetYearLevel(snapshot, context.StudentId),
                Section = GetSection(snapshot, context.StudentId),
                AdmissionYear = 2024,
                StudentStatus = context.Student.Status.ToString(),
                LastUpdatedUtc = context.Student.UpdatedAtUtc
            };

            return profile;
        }

        public OperationResult SubmitCorrectionRequest(
            string sessionId,
            StudentProfileCorrectionRequest request)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Profile.RequestCorrection");

            // TODO: Implement actual submission logic
            // This would:
            // 1. Validate the requested field is correctable
            // 2. Create a StudentProfileCorrectionRequest domain entity
            // 3. Save to student_profile_corrections.json
            // 4. Send notification to appropriate staff

            return OperationResult.Success();
        }

        public IReadOnlyList<StudentProfileCorrectionRequest> GetCorrectionRequests(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Profile.ViewCorrections");

            // TODO: Implement actual lookup from student_profile_corrections.json
            return new List<StudentProfileCorrectionRequest>();
        }

        private string GetProgramName(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "BS Computer Science";
        }

        private int GetYearLevel(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return 3;
        }

        private string GetSection(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "A";
        }
    }
}
