using System.Collections.Generic;

using IUIS.Application.Security;

namespace IUIS.Application.Navigation
{
    public static class EmployeeNavigationCatalog
    {
        public static IReadOnlyList<NavigationItemDefinition> GetAll()
        {
            return new List<NavigationItemDefinition>
            {
                Item("employee.dashboard", "Home", "Dashboard", "EMP-DASH-01", string.Empty, 10, true),
                Item("registrar.students", "Registrar", "Student Records", "EMP-STU-01", PermissionCatalog.Keys.RegistrarStudentView, 100),
                Item("registrar.enrollment", "Registrar", "Enrollment Review", "EMP-ENR-01", PermissionCatalog.Keys.RegistrarEnrollmentReview, 110),
                Item("registrar.courses", "Registrar", "Courses", "EMP-CRS-01", PermissionCatalog.Keys.RegistrarCourseManage, 120),
                Item("registrar.subjects", "Registrar", "Subjects", "EMP-SUB-01", PermissionCatalog.Keys.RegistrarSubjectManage, 130),
                Item("accounting.assessments", "Finance", "Tuition Assessments", "EMP-ASMT-01", PermissionCatalog.Keys.AccountingAssessmentManage, 200),
                Item("cashier.payments", "Finance", "Payments", "EMP-PAY-01", PermissionCatalog.Keys.CashierPaymentPost, 210),
                Item("scholarship.office", "Finance", "Scholarships", "EMP-SCH-01", PermissionCatalog.Keys.ScholarshipManage, 220),
                Item("library.inventory", "Library", "Book Inventory", "EMP-LIB-01", PermissionCatalog.Keys.LibraryBookManage, 300),
                Item("library.borrowings", "Library", "Borrowings", "EMP-LIB-02", PermissionCatalog.Keys.LibraryCirculationIssue, 310),
                Item("counseling.cases", "Student Services", "Counseling Cases", "EMP-COU-01", PermissionCatalog.Keys.CounselingCaseManage, 400),
                Item("discipline.cases", "Student Services", "Discipline Cases", "EMP-DIS-01", PermissionCatalog.Keys.DisciplineCaseManage, 410),
                Item("clinic.appointments", "Clinic", "Appointments", "EMP-CLN-01", PermissionCatalog.Keys.ClinicAppointmentManage, 500),
                Item("clinic.records", "Clinic", "Medical Records", "EMP-CLN-02", PermissionCatalog.Keys.ClinicMedicalRestrictedRead, 510),
                Item("clinic.clearances", "Clinic", "Clearances", "EMP-CLN-03", PermissionCatalog.Keys.ClinicClearanceIssue, 520),
                Item("hr.employees", "Human Resources", "Employee Records", "EMP-HR-01", PermissionCatalog.Keys.HrEmployeeManage, 600),
                Item("hr.attendance", "Human Resources", "Attendance", "EMP-HR-02", PermissionCatalog.Keys.HrAttendanceManage, 610),
                Item("faculty.assignments", "Faculty", "My Teaching Assignments", "FAC-ASG-01", PermissionCatalog.Keys.FacultyAssignmentViewOwn, 700),
                Item("coordinator.assignments", "Coordination", "Subject Assignments", "COORD-ASG-01", PermissionCatalog.Keys.CoordinatorAssignmentManage, 800),
                Item("employee.account", "Account", "My Account", "ACCOUNT-SELF-01", PermissionCatalog.Keys.AccountSelfView, 900)
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
