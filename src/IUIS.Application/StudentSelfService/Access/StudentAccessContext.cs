using System.Collections.Generic;
using IUIS.Domain.Security;
using IUIS.Domain.Students;

namespace IUIS.Application.StudentSelfService.Access
{
    public sealed class StudentAccessContext
    {
        public StudentAccessContext()
        {
            SessionId = string.Empty;
            UserId = string.Empty;
            StudentId = string.Empty;
            LoginId = string.Empty;
            Permissions = new List<string>();
        }

        public string SessionId { get; set; }

        public string UserId { get; set; }

        public string StudentId { get; set; }

        public string LoginId { get; set; }

        public UserSession Session { get; set; }

        public Student Student { get; set; }

        public IReadOnlyCollection<string>
            Permissions { get; set; }

        public bool HasPermission(
            string permissionKey)
        {
            return Permissions != null &&
                Permissions.Contains(
                    permissionKey);
        }
    }
}
