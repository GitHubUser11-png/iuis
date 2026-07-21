using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Scholarship;
using IUIS.Application.StudentSelfService.Access;

namespace IUIS.Application.StudentSelfService.Scholarship
{
    public sealed class StudentScholarshipService : IStudentScholarshipService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentScholarshipService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public System.Collections.Generic.IReadOnlyList<ScholarshipProgramView> GetAvailablePrograms(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Scholarship.ViewPrograms");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from scholarships.json
            return System.Collections.Generic.List<ScholarshipProgramView>.Empty;
        }

        public System.Collections.Generic.IReadOnlyList<StudentScholarshipApplicationView> GetMyApplications(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Scholarship.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from scholarship_applications.json
            return System.Collections.Generic.List<StudentScholarshipApplicationView>.Empty;
        }

        public OperationResult SubmitApplication(
            string sessionId,
            StudentScholarshipApplicationRequest request)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Scholarship.Apply");

            // TODO: Implement actual submission logic
            // This would:
            // 1. Validate the request
            // 2. Create a scholarship application record
            // 3. Save to scholarship_applications.json
            // 4. Send notification to scholarship committee

            return OperationResult.Success();
        }
    }
}
