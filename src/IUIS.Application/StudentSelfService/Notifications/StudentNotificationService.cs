using System.Collections.Generic;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.Common;
using IUIS.Application.StudentSelfService.Notifications;
using IUIS.Application.StudentSelfService.Access;

namespace IUIS.Application.StudentSelfService.Notifications
{
    public sealed class StudentNotificationService : IStudentNotificationService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentNotificationService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public IReadOnlyList<StudentNotificationView> GetNotifications(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Notification.Own.View");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from notifications.json
            return List<StudentNotificationView>.Empty;
        }

        public OperationResult MarkAsRead(string sessionId, string notificationId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Notification.MarkRead");

            // TODO: Implement actual update logic
            // This would:
            // 1. Load the notification
            // 2. Update IsRead to true
            // 3. Save to notifications.json

            return OperationResult.Success();
        }

        public OperationResult ArchiveNotification(string sessionId, string notificationId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Notification.Archive");

            // TODO: Implement actual update logic
            // This would:
            // 1. Load the notification
            // 2. Update IsArchived to true
            // 3. Save to notifications.json

            return OperationResult.Success();
        }
    }
}
