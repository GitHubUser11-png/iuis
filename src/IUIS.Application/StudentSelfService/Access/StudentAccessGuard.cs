using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Domain.Security;
using IUIS.Domain.Students;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Application.StudentSelfService.Access
{
    public sealed class StudentAccessGuard : IStudentAccessGuard
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;

        public StudentAccessGuard(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            IStudentRepository studentRepository)
        {
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
        }

        public StudentAccessContext RequireStudent(
            string sessionId,
            string requiredPermission)
        {
            return RequireStudent(sessionId, 
                new[] { requiredPermission });
        }

        public StudentAccessContext RequireStudent(
            string sessionId,
            params string[] requiredPermissions)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException(
                    "Session ID is required.", nameof(sessionId));
            }

            UserSession session = _sessionRepository
                .GetActiveSession(sessionId);

            if (session == null)
            {
                throw new UnauthorizedAccessException(
                    "Session not found or expired.");
            }

            if (session.PrimaryRole != PrimaryRole.Student)
            {
                throw new UnauthorizedAccessException(
                    "Access denied. Student role required.");
            }

            UserAccount user = _userRepository
                .GetById(session.UserId);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "User account not found or inactive.");
            }

            if (session.SecurityStamp != user.SecurityStamp)
            {
                throw new UnauthorizedAccessException(
                    "Session invalidated. Please sign in again.");
            }

            Student student = _studentRepository
                .GetById(user.PersonReferenceId);

            if (student == null)
            {
                throw new UnauthorizedAccessException(
                    "Student record not found.");
            }

            foreach (string permission in requiredPermissions)
            {
                if (!user.PermissionKeys.Contains(permission))
                {
                    throw new UnauthorizedAccessException(
                        $"Permission denied: {permission}");
                }
            }

            return new StudentAccessContext
            {
                SessionId = sessionId,
                UserId = user.Id,
                StudentId = student.Id,
                LoginId = user.LoginId,
                Session = session,
                Student = student,
                Permissions = user.PermissionKeys
            };
        }
    }
}
