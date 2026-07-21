using System.Collections.Generic;
using IUIS.Application.Common;
using IUIS.Application.StudentSelfService.Notifications;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentNotificationService
    {
        IReadOnlyList<StudentNotificationView> 
            GetNotifications(string sessionId);
        
        OperationResult MarkAsRead(
            string sessionId,
            string notificationId);
        
        OperationResult ArchiveNotification(
            string sessionId,
            string notificationId);
    }
}
