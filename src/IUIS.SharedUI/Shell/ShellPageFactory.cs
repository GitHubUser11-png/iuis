using System.Collections.Generic;
using System.Windows.Forms;

using IUIS.Application.Navigation;
using IUIS.SharedUI.Controls;
using IUIS.UserApp.Forms.Student.Pages;
using IUIS.UserApp.Forms.Employee.Library;
using IUIS.UserApp.Forms.Employee.Counseling;
using IUIS.UserApp.Forms.Employee.Discipline;
using IUIS.UserApp.Forms.Employee.Clinic;
using IUIS.AdminApp.Forms;

namespace IUIS.SharedUI.Shell
{
    public static class ShellPageFactory
    {
        public static DashboardPagePanel CreateDashboard(
            string greeting,
            params DashboardCardModel[] cards)
        {
            var dashboard = new DashboardPagePanel();
            dashboard.SetGreeting(greeting, cards);
            return dashboard;
        }

        public static void RegisterModulePages(
            ApplicationShellPanel shell,
            IEnumerable<NavigationItemDefinition> items,
            string dashboardPageKey,
            DashboardPagePanel dashboard)
        {
            shell.RegisterPage(dashboardPageKey, dashboard, "Dashboard");

            foreach (var item in items)
            {
                if (item.AlwaysVisible && string.Equals(item.PageKey, dashboardPageKey))
                    continue;

                var page = CreatePageByKey(item.PageKey, item.DisplayText);
                shell.RegisterPage(item.PageKey, page, item.DisplayText);
            }
        }

        private static UserControl CreatePageByKey(string pageKey, string displayText)
        {
            // Student Pages
            if (pageKey == "STU-PRO-01")
                return new StudentProfilePage("session-placeholder");
            if (pageKey == "STU-ENR-01")
                return new StudentEnrollmentPage("session-placeholder");
            if (pageKey == "STU-SUB-01")
                return new StudentSubjectsPage("session-placeholder");
            if (pageKey == "STU-TUI-01")
                return new StudentAssessmentPage("session-placeholder");
            if (pageKey == "STU-PAY-01")
                return new StudentPaymentHistoryPage("session-placeholder");
            if (pageKey == "STU-SCH-01")
                return new StudentScholarshipPage("session-placeholder");
            if (pageKey == "STU-NOT-01")
                return new StudentNotificationsPage("session-placeholder");

            // Employee Library Pages
            if (pageKey == "EMP-LIB-01")
                return new BookInventoryPage("session-placeholder");
            if (pageKey == "EMP-LIB-02")
                return new BorrowingOperationsPage("session-placeholder");

            // Employee Counseling Pages
            if (pageKey == "EMP-COU-01")
                return new CounselingSessionsPage("session-placeholder");

            // Employee Discipline Pages
            if (pageKey == "EMP-DIS-01")
                return new DisciplineDashboardPage("session-placeholder");

            // Employee Clinic Pages
            if (pageKey == "EMP-CLN-01")
                return new ClinicDashboardPage("session-placeholder");

            // Administrator Pages
            if (pageKey == "ADM-DASH-01")
                return new AdminDashboardPage("session-placeholder");
            if (pageKey == "ADM-USER-01")
                return new UserManagementPage("session-placeholder");
            if (pageKey == "ADM-SYS-01")
                return new SystemAdministrationPage("session-placeholder");
            if (pageKey == "ADM-RPT-01")
                return new ReportsPage("session-placeholder");

            // Default placeholder for unimplemented pages
            var placeholder = new ModulePagePanel();
            placeholder.SetPage(displayText, pageKey);
            return placeholder;
        }
    }
}
