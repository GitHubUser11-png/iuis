using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.Application.StudentSelfService.Access;
using IUIS.Domain.Students;

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
                FirstName = context.Student.FirstName,
                MiddleName = context.Student.MiddleName ?? string.Empty,
                LastName = context.Student.LastName,
                Suffix = context.Student.Suffix ?? string.Empty,
                BirthDate = context.Student.BirthDate,
                Sex = context.Student.Sex,
                Address = context.Student.Address,
                ContactNumber = context.Student.ContactNumber,
                EmailAddress = context.Student.EmailAddress,
                ProgramName = GetProgramName(snapshot, context.StudentId),
                YearLevel = GetYearLevel(snapshot, context.StudentId),
                Section = GetSection(snapshot, context.StudentId),
                AdmissionYear = context.Student.AdmissionYear,
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

        public System.Collections.Generic.IReadOnlyList<CorrectionRequestView> GetCorrectionRequests(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Profile.ViewCorrections");

            // TODO: Implement actual lookup from student_profile_corrections.json
            return System.Collections.Generic.List<CorrectionRequestView>.Empty;
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
