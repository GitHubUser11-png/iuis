using System;
using System.Windows.Forms;

using IUIS.Infrastructure.Composition;
using IUIS.UserApp.Forms.Student.Pages;
using IUIS.UserApp.Forms.Employee;
using IUIS.UserApp.Forms.Employee.Clinic;
using IUIS.UserApp.Forms.Employee.Counseling;
using IUIS.UserApp.Forms.Employee.Discipline;
using IUIS.UserApp.Forms.Employee.Library;
using IUIS.SharedUI.Shell;

namespace IUIS.UserApp.Forms
{
    /// <summary>
    /// Factory for creating user portal pages (Student, Employee, Coordinator roles).
    /// Wires page controls to their corresponding application services.
    /// </summary>
    public sealed class UserPageFactory
    {
        private readonly IuisCompositionRoot _composition;
        private readonly string _sessionToken;

        public UserPageFactory(IuisCompositionRoot composition, string sessionToken)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _sessionToken = sessionToken ?? throw new ArgumentNullException(nameof(sessionToken));
        }

        /// <summary>
        /// Creates a page control for the given navigation key, wired with its service dependencies.
        /// </summary>
        public UserControl CreatePage(string pageKey, string displayText)
        {
            try
            {
                // Student Portal Pages (STU-*)
                if (pageKey == "STU-DASH-01")
                    return new StudentDashboardPage(null, _sessionToken);
                
                if (pageKey == "STU-PRO-01")
                    return new StudentProfilePage(null, _sessionToken);
                
                if (pageKey == "STU-ENR-01")
                    return new StudentEnrollmentPage(null, _sessionToken);
                
                if (pageKey == "STU-SUB-01")
                    return new StudentSubjectsPage(null, _sessionToken);
                
                if (pageKey == "STU-TUI-01")
                    return new StudentAssessmentPage(null, _sessionToken);
                
                if (pageKey == "STU-PAY-01")
                    return new StudentPaymentHistoryPage(null, _sessionToken);
                
                if (pageKey == "STU-SCH-01")
                    return new StudentScholarshipPage(null, _sessionToken);
                
                if (pageKey == "STU-NOT-01")
                    return new StudentNotificationsPage(null, _sessionToken);

                // Employee Library Module Pages (EMP-LIB-*)
                if (pageKey == "EMP-LIB-01")
                    return new BookInventoryPage(_sessionToken);
                
                if (pageKey == "EMP-LIB-02")
                    return new BorrowingOperationsPage(_sessionToken);

                // Employee Counseling Module Pages (EMP-COU-*)
                if (pageKey == "EMP-COU-01")
                    return new CounselingSessionsPage(_sessionToken);
                
                if (pageKey == "EMP-COU-DAS")
                    return new CounselingDashboardPage(_sessionToken);

                // Employee Clinic Module Pages (EMP-CLN-*)
                if (pageKey == "EMP-CLN-01")
                    return new ClinicDashboardPage(_sessionToken);

                // Employee Discipline Module Pages (EMP-DIS-*)
                if (pageKey == "EMP-DIS-01")
                    return new DisciplineDashboardPage(_sessionToken);

                // Unimplemented pages return graceful placeholder
                return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating page {pageKey}: {ex.Message}");
                return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText + " (Error)");
            }
        }
    }
}
