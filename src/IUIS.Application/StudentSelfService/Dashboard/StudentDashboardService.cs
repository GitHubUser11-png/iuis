using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Dashboard;
using IUIS.Application.StudentSelfService.Access;

namespace IUIS.Application.StudentSelfService.Dashboard
{
    public sealed class StudentDashboardService : IStudentDashboardService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentDashboardService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public StudentDashboardView GetDashboard(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Dashboard.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            var dashboard = new StudentDashboardView
            {
                StudentName = $"{context.Student.LastName}, {context.Student.FirstName}",
                ProgramName = GetProgramName(snapshot, context.StudentId),
                YearLevel = GetYearLevel(snapshot, context.StudentId),
                Section = GetSection(snapshot, context.StudentId),
                EnrollmentStatus = GetEnrollmentStatus(snapshot, context.StudentId),
                CurrentTerm = GetCurrentTerm(snapshot),
                OutstandingBalance = GetOutstandingBalance(snapshot, context.StudentId),
                TotalPaid = GetTotalPaid(snapshot, context.StudentId),
                ActiveBorrowings = GetActiveBorrowings(snapshot, context.StudentId),
                OverdueBooks = GetOverdueBooks(snapshot, context.StudentId),
                PendingApplications = GetPendingApplications(snapshot, context.StudentId),
                ActiveScholarships = GetActiveScholarships(snapshot, context.StudentId),
                UpcomingAppointments = GetUpcomingAppointments(snapshot, context.StudentId),
                RecentNotifications = GetRecentNotifications(snapshot, context.UserId)
            };

            return dashboard;
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

        private string GetEnrollmentStatus(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "Enrolled";
        }

        private string GetCurrentTerm(StudentProjectionSnapshot snapshot)
        {
            // TODO: Implement actual lookup
            return "2026-2027 First Semester";
        }

        private decimal GetOutstandingBalance(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 15000m;
        }

        private decimal GetTotalPaid(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 5000m;
        }

        private int GetActiveBorrowings(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual count
            return 2;
        }

        private int GetOverdueBooks(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual count
            return 0;
        }

        private int GetPendingApplications(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual count
            return 1;
        }

        private int GetActiveScholarships(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual count
            return 1;
        }

        private System.Collections.Generic.IReadOnlyList<DashboardAppointmentItem> GetUpcomingAppointments(
            StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return System.Collections.Generic.List<DashboardAppointmentItem>.Empty;
        }

        private System.Collections.Generic.IReadOnlyList<DashboardNotificationItem> GetRecentNotifications(
            StudentProjectionSnapshot snapshot, string userId)
        {
            // TODO: Implement actual lookup
            return System.Collections.Generic.List<DashboardNotificationItem>.Empty;
        }
    }
}
