using System;
using System.Windows.Forms;

using IUIS.Infrastructure.Composition;
using IUIS.UserApp.Forms.Student.Pages;
using IUIS.UserApp.Forms.Employee;
using IUIS.SharedUI.Shell;

namespace IUIS.UserApp.Forms
{
    public sealed class UserPageFactory
    {
        private readonly IuisCompositionRoot _composition;
        private readonly string _sessionToken;

        public UserPageFactory(IuisCompositionRoot composition, string sessionToken)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _sessionToken = sessionToken ?? throw new ArgumentNullException(nameof(sessionToken));
        }

        public UserControl CreatePage(string pageKey, string displayText)
        {
            // Student Portal Pages (STU-*)
            if (pageKey == "STU-DASH-01")
                return new StudentDashboardPage(_composition.StudentOwnRecords, _sessionToken);
            
            if (pageKey == "STU-PRO-01")
                return new StudentProfilePage(_composition.StudentOwnRecords, _sessionToken);
            
            if (pageKey == "STU-ENR-01")
                return new StudentEnrollmentPage(_composition.EnrollmentSubmissions, _sessionToken);
            
            if (pageKey == "STU-SUB-01")
                return new StudentSubjectsPage(_composition.StudentOwnRecords, _sessionToken);
            
            if (pageKey == "STU-TUI-01")
                return new StudentAssessmentPage(_composition.StudentFinance, _sessionToken);
            
            if (pageKey == "STU-PAY-01")
                return new StudentPaymentHistoryPage(_composition.StudentFinance, _sessionToken);
            
            if (pageKey == "STU-SCH-01")
                return new StudentScholarshipPage(_composition.StudentFinance, _sessionToken);
            
            if (pageKey == "STU-NOT-01")
                return new StudentNotificationsPage(_composition.StudentOwnRecords, _sessionToken);

            // Employee Library Portal Pages (EMP-LIB-*)
            if (pageKey == "EMP-LIB-01")
                return new BookInventoryPage(_composition.LibraryBooks, _sessionToken);
            
            if (pageKey == "EMP-LIB-02")
                return new BorrowingOperationsPage(_composition.LibraryCirculation, _sessionToken);

            // Employee Counseling Portal Pages (EMP-COU-*)
            if (pageKey == "EMP-COU-01")
                return new CounselingSessionsPage(_composition.CounselingCommands, _sessionToken);

            // Employee Clinic Portal Pages (EMP-CLN-*)
            if (pageKey == "EMP-CLN-01")
                return new ClinicDashboardPage(_composition.ClinicAppointmentCommands, _sessionToken);

            // Employee Discipline Portal Pages (EMP-DIS-*)
            if (pageKey == "EMP-DIS-01")
                return new DisciplineDashboardPage(_composition.DisciplineCommands, _sessionToken);

            // Employee Counseling Dashboard
            if (pageKey == "EMP-COU-DAS")
                return new CounselingDashboardPage(_composition.CounselingCommands, _sessionToken);

            // All unimplemented pages return placeholder for graceful degradation
            return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText);
        }
    }
}
