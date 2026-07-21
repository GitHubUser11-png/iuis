using System.Collections.Generic;

using IUIS.Application.Security;

namespace IUIS.Application.Navigation
{
    public static class StudentNavigationCatalog
    {
        public static IReadOnlyList<NavigationItemDefinition> GetAll()
        {
            return new List<NavigationItemDefinition>
            {
                Item("student.dashboard", "Home", "Dashboard", "STU-DASH-01", string.Empty, 10, true),
                Item("student.profile", "Academic", "My Profile", "STU-PRO-01", PermissionCatalog.Keys.StudentProfileViewOwn, 20),
                Item("student.enrollment", "Academic", "Enrollment", "STU-ENR-01", PermissionCatalog.Keys.StudentEnrollmentViewOwn, 30),
                Item("student.subjects", "Academic", "My Subjects", "STU-SUB-01", PermissionCatalog.Keys.StudentEnrollmentViewOwn, 40),
                Item("student.assessments", "Finance", "Tuition Assessment", "STU-TUI-01", PermissionCatalog.Keys.StudentFinanceViewOwn, 50),
                Item("student.payments", "Finance", "Payments", "STU-PAY-01", PermissionCatalog.Keys.StudentFinanceViewOwn, 60),
                Item("student.scholarship", "Finance", "Scholarships", "STU-SCH-01", PermissionCatalog.Keys.StudentFinanceViewOwn, 70),
                Item("student.library", "Services", "Library", "STU-LIB-01", PermissionCatalog.Keys.StudentLibraryViewOwn, 100),
                Item("student.appointments", "Services", "Appointments", "STU-APT-01", PermissionCatalog.Keys.StudentMedicalViewOwn, 110),
                Item("student.counseling", "Services", "Counseling", "STU-COU-01", PermissionCatalog.Keys.StudentCounselingViewOwn, 120),
                Item("student.medical", "Services", "Medical Records", "STU-MED-01", PermissionCatalog.Keys.StudentMedicalViewOwn, 130),
                Item("student.clearances", "Services", "Medical Clearances", "STU-CLR-01", PermissionCatalog.Keys.StudentMedicalViewOwn, 140),
                Item("student.violations", "Services", "Violation Notices", "STU-VIO-01", PermissionCatalog.Keys.StudentDisciplineViewOwn, 150),
                Item("student.notifications", "Account", "Notifications", "STU-NOT-01", PermissionCatalog.Keys.NotificationOwnView, 200),
                Item("student.account", "Account", "My Account", "ACCOUNT-SELF-01", PermissionCatalog.Keys.AccountSelfView, 210)
            };
        }

        private static NavigationItemDefinition Item(
            string key,
            string group,
            string text,
            string pageKey,
            string permission,
            int order,
            bool alwaysVisible = false)
        {
            return new NavigationItemDefinition
            {
                Key = key,
                GroupKey = group,
                DisplayText = text,
                PageKey = pageKey,
                RequiredPermission = permission,
                DisplayOrder = order,
                AlwaysVisible = alwaysVisible
            };
        }
    }
}
