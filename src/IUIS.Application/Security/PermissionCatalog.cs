namespace IUIS.Application.Security
{
    public static class PermissionCatalog
    {
        public static class Keys
        {
            public const string StudentProfileViewOwn = "student.profile.read";
            public const string StudentProfileContactUpdate = "student.profile.contact.update";
            public const string StudentEnrollmentViewOwn = "student.enrollment.read";
            public const string StudentEnrollmentSubmit = "student.enrollment.submit";
            public const string StudentFinanceViewOwn = "student.finance.read";
            public const string StudentLibraryViewOwn = "student.library.read";
            public const string StudentMedicalViewOwn = "student.medical.read";
            public const string StudentCounselingViewOwn = "student.counseling.released.read";
            public const string StudentDisciplineViewOwn = "student.discipline.released.read";
            public const string StudentServicesViewOwn = "student.services.released.read";
            public const string NotificationOwnView = "notification.own.read";
            public const string AccountSelfView = "account.self.read";

            public const string RegistrarStudentView = "registrar.student.read";
            public const string RegistrarStudentManage = "registrar.student.manage";
            public const string RegistrarEnrollmentReview = "registrar.enrollment.review";
            public const string RegistrarCourseManage = "registrar.course.manage";
            public const string RegistrarSubjectManage = "registrar.subject.manage";

            public const string CashierPaymentPost = "cashier.payment.post";
            public const string CashierPaymentVoid = "cashier.payment.void";
            public const string AccountingAssessmentManage = "accounting.assessment.manage";
            public const string ScholarshipManage = "scholarship.manage";

            public const string LibraryBookManage = "library.book.manage";
            public const string LibraryCirculationIssue = "library.circulation.issue";
            public const string LibraryCirculationReturn = "library.circulation.return";

            public const string CounselingCaseManage = "counseling.case.manage";
            public const string DisciplineCaseManage = "discipline.case.manage";

            public const string ClinicAppointmentManage = "clinic.appointment.manage";
            public const string ClinicMedicalRestrictedRead = "clinic.medical.restricted.read";
            public const string ClinicClearanceIssue = "clinic.medical.clearance.issue";

            public const string HrEmployeeManage = "hr.employee.manage";
            public const string HrAttendanceManage = "hr.attendance.manage";
            public const string FacultyAssignmentViewOwn = "faculty.assignment.read.own";
            public const string CoordinatorAssignmentManage = "coordinator.assignment.manage";

            public const string AdminAccountManage = "admin.account.manage";
            public const string AdminApplicationReview = "admin.application.review";
            public const string AdminSecurityManage = "admin.security.manage";
            public const string AdminRepositoryManage = "admin.repository.manage";
            public const string AdminAuditView = "admin.audit.read";
            public const string AdminSettingsManage = "admin.settings.manage";
            public const string AdminReportRun = "admin.report.run";
        }
    }
}
