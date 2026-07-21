using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Enrollment;
using IUIS.Application.StudentSelfService.Access;

namespace IUIS.Application.StudentSelfService.Enrollment
{
    public sealed class StudentEnrollmentService : IStudentEnrollmentService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentEnrollmentService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public StudentEnrollmentSummaryView GetEnrollmentSummary(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Enrollment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            return new StudentEnrollmentSummaryView
            {
                CurrentEnrollmentId = GetCurrentEnrollmentId(snapshot, context.StudentId),
                ProgramName = GetProgramName(snapshot, context.StudentId),
                YearLevel = GetYearLevel(snapshot, context.StudentId),
                Section = GetSection(snapshot, context.StudentId),
                AcademicYear = GetCurrentAcademicYear(snapshot),
                Semester = GetCurrentSemester(snapshot),
                EnrollmentStatus = GetEnrollmentStatus(snapshot, context.StudentId),
                EnrollmentDate = GetEnrollmentDate(snapshot, context.StudentId),
                TotalUnits = GetTotalUnits(snapshot, context.StudentId),
                SubjectCount = GetSubjectCount(snapshot, context.StudentId),
                OutstandingBalance = GetOutstandingBalance(snapshot, context.StudentId)
            };
        }

        public StudentEnrollmentDetailsView GetEnrollmentDetails(string sessionId, string enrollmentId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Enrollment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            return new StudentEnrollmentDetailsView
            {
                EnrollmentId = enrollmentId,
                ProgramName = GetProgramName(snapshot, context.StudentId),
                YearLevel = GetYearLevel(snapshot, context.StudentId),
                Section = GetSection(snapshot, context.StudentId),
                AcademicYear = GetAcademicYear(snapshot, enrollmentId),
                Semester = GetSemester(snapshot, enrollmentId),
                EnrollmentStatus = GetEnrollmentStatus(snapshot, enrollmentId),
                EnrollmentDate = GetEnrollmentDate(snapshot, enrollmentId),
                Subjects = GetEnrolledSubjects(snapshot, enrollmentId)
            };
        }

        public System.Collections.Generic.IReadOnlyList<StudentEnrollmentListItem> GetEnrollmentHistory(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Enrollment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from enrollments.json
            return System.Collections.Generic.List<StudentEnrollmentListItem>.Empty;
        }

        private string GetCurrentEnrollmentId(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "ENR-2026-000001";
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

        private string GetCurrentAcademicYear(StudentProjectionSnapshot snapshot)
        {
            // TODO: Implement actual lookup
            return "2026-2027";
        }

        private string GetCurrentSemester(StudentProjectionSnapshot snapshot)
        {
            // TODO: Implement actual lookup
            return "First Semester";
        }

        private string GetEnrollmentStatus(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "Enrolled";
        }

        private System.DateTime GetEnrollmentDate(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return new System.DateTime(2026, 8, 15);
        }

        private int GetTotalUnits(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 21;
        }

        private int GetSubjectCount(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual count
            return 7;
        }

        private decimal GetOutstandingBalance(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 15000m;
        }

        private string GetAcademicYear(StudentProjectionSnapshot snapshot, string enrollmentId)
        {
            // TODO: Implement actual lookup
            return "2026-2027";
        }

        private string GetSemester(StudentProjectionSnapshot snapshot, string enrollmentId)
        {
            // TODO: Implement actual lookup
            return "First Semester";
        }

        private string GetEnrollmentStatus(StudentProjectionSnapshot snapshot, string enrollmentId)
        {
            // TODO: Implement actual lookup
            return "Enrolled";
        }

        private System.DateTime GetEnrollmentDate(StudentProjectionSnapshot snapshot, string enrollmentId)
        {
            // TODO: Implement actual lookup
            return new System.DateTime(2026, 8, 15);
        }

        private System.Collections.Generic.IReadOnlyList<EnrolledSubjectView> GetEnrolledSubjects(
            StudentProjectionSnapshot snapshot, string enrollmentId)
        {
            // TODO: Implement actual lookup
            return System.Collections.Generic.List<EnrolledSubjectView>.Empty;
        }
    }
}
