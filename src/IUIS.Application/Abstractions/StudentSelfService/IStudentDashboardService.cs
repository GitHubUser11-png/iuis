using IUIS.Application.StudentSelfService.Dashboard;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentDashboardService
    {
        StudentDashboardView GetDashboard(string sessionId);
    }
}
