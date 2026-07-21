using System;
using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Dashboard
{
    public sealed class StudentDashboardView
    {
        public StudentDashboardView()
        {
            StudentName = string.Empty;
            ProgramName = string.Empty;
            YearLevel = 0;
            Section = string.Empty;
            
            EnrollmentStatus = string.Empty;
            CurrentTerm = string.Empty;
            
            OutstandingBalance = 0m;
            TotalPaid = 0m;
            
            ActiveBorrowings = 0;
            OverdueBooks = 0;
            
            PendingApplications = 0;
            ActiveScholarships = 0;
            
            UpcomingAppointments = new List<DashboardAppointmentItem>();
            RecentNotifications = new List<DashboardNotificationItem>();
            
            LastUpdatedUtc = DateTime.UtcNow;
        }

        public string StudentName { get; set; }
        
        public string ProgramName { get; set; }
        
        public int YearLevel { get; set; }
        
        public string Section { get; set; }
        
        public string EnrollmentStatus { get; set; }
        
        public string CurrentTerm { get; set; }
        
        public decimal OutstandingBalance { get; set; }
        
        public decimal TotalPaid { get; set; }
        
        public int ActiveBorrowings { get; set; }
        
        public int OverdueBooks { get; set; }
        
        public int PendingApplications { get; set; }
        
        public int ActiveScholarships { get; set; }
        
        public IReadOnlyList<DashboardAppointmentItem> 
            UpcomingAppointments { get; set; }
        
        public IReadOnlyList<DashboardNotificationItem> 
            RecentNotifications { get; set; }
        
        public DateTime LastUpdatedUtc { get; set; }
    }

    public sealed class DashboardAppointmentItem
    {
        public string AppointmentId { get; set; }
        
        public string AppointmentType { get; set; }
        
        public DateTime ScheduledDateUtc { get; set; }
        
        public string Location { get; set; }
    }

    public sealed class DashboardNotificationItem
    {
        public string NotificationId { get; set; }
        
        public string Title { get; set; }
        
        public string Message { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        
        public bool IsRead { get; set; }
    }
}
