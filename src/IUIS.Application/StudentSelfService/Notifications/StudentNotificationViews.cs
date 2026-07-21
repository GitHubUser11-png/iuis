using System;

namespace IUIS.Application.StudentSelfService.Notifications
{
    public sealed class StudentNotificationView
    {
        public StudentNotificationView()
        {
            NotificationId = string.Empty;
            Title = string.Empty;
            Message = string.Empty;
            
            Category = string.Empty;
            Priority = string.Empty;
            
            CreatedAtUtc = DateTime.MinValue;
            
            IsRead = false;
            IsArchived = false;
            
            ActionLink = string.Empty;
            ActionLabel = string.Empty;
        }

        public string NotificationId { get; set; }
        
        public string Title { get; set; }
        
        public string Message { get; set; }
        
        public string Category { get; set; }
        
        public string Priority { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        
        public bool IsRead { get; set; }
        
        public bool IsArchived { get; set; }
        
        public string ActionLink { get; set; }
        
        public string ActionLabel { get; set; }
    }
}
