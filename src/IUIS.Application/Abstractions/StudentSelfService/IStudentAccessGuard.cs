using IUIS.Application.StudentSelfService.Access;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentAccessGuard
    {
        StudentAccessContext RequireStudent(
            string sessionId,
            string requiredPermission);

        StudentAccessContext RequireStudent(
            string sessionId,
            params string[] requiredPermissions);
    }
}
