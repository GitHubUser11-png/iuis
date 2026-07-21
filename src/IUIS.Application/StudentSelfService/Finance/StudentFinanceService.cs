using System;
using System.Collections.Generic;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Finance;
using IUIS.Application.StudentSelfService.Access;
using IUIS.Infrastructure.Projections.Student;

namespace IUIS.Application.StudentSelfService.Finance
{
    public sealed class StudentFinanceService : IStudentFinanceService
    {
        private readonly IStudentAccessGuard _accessGuard;
        private readonly IStudentProjectionDataSource _projectionDataSource;

        public StudentFinanceService(
            IStudentAccessGuard accessGuard,
            IStudentProjectionDataSource projectionDataSource)
        {
            _accessGuard = accessGuard;
            _projectionDataSource = projectionDataSource;
        }

        public StudentFinanceSummaryView GetFinanceSummary(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Assessment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            return new StudentFinanceSummaryView
            {
                CurrentAssessmentId = GetCurrentAssessmentId(snapshot, context.StudentId),
                AcademicYear = GetCurrentAcademicYear(snapshot),
                Semester = GetCurrentSemester(snapshot),
                TotalAssessment = GetTotalAssessment(snapshot, context.StudentId),
                TuitionAmount = GetTuitionAmount(snapshot, context.StudentId),
                MiscellaneousFees = GetMiscellaneousFees(snapshot, context.StudentId),
                LaboratoryFees = GetLaboratoryFees(snapshot, context.StudentId),
                OtherFees = GetOtherFees(snapshot, context.StudentId),
                ScholarshipDeduction = GetScholarshipDeduction(snapshot, context.StudentId),
                TotalPaid = GetTotalPaid(snapshot, context.StudentId),
                OutstandingBalance = GetOutstandingBalance(snapshot, context.StudentId),
                AssessmentStatus = GetCurrentAssessmentStatus(snapshot, context.StudentId)
            };
        }

        public StudentAssessmentDetailsView GetAssessmentDetails(string sessionId, string assessmentId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Assessment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            return new StudentAssessmentDetailsView
            {
                AssessmentId = assessmentId,
                AcademicYear = GetAssessmentAcademicYear(snapshot, assessmentId),
                Semester = GetAssessmentSemester(snapshot, assessmentId),
                Charges = GetAssessmentCharges(snapshot, assessmentId),
                Payments = GetAssessmentPayments(snapshot, assessmentId),
                TotalAssessment = GetAssessmentTotal(snapshot, assessmentId),
                TotalPaid = GetAssessmentPaid(snapshot, assessmentId),
                OutstandingBalance = GetAssessmentBalance(snapshot, assessmentId),
                AssessmentStatus = GetAssessmentStatus(snapshot, assessmentId)
            };
        }

        public IReadOnlyList<StudentPaymentListItem> GetPaymentHistory(string sessionId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Payment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from payments.json
            return List<StudentPaymentListItem>.Empty;
        }

        public StudentPaymentReceiptView GetPaymentReceipt(string sessionId, string paymentId)
        {
            var context = _accessGuard.RequireStudent(
                sessionId,
                "Student.Payment.ViewOwn");

            var snapshot = _projectionDataSource.ReadStudentSources(context.StudentId);

            // TODO: Implement actual lookup from payments.json
            return new StudentPaymentReceiptView
            {
                PaymentId = paymentId,
                ReceiptNumber = "REC-2026-000001",
                StudentName = $"{context.Student.LastName}, {context.Student.FirstName}",
                StudentId = context.Student.Id,
                PaymentDate = System.DateTime.UtcNow,
                Amount = 5000m,
                PaymentMethod = "Cash",
                ReferenceNumber = "REF-123456",
                AssessmentId = "ASMT-2026-000001",
                AcademicYear = "2026-2027",
                Semester = "First Semester",
                ProcessedBy = "Cashier Smith",
                Remarks = "Tuition payment"
            };
        }

        private string GetCurrentAssessmentId(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "ASMT-2026-000001";
        }

        private string GetCurrentAcademicYear(StudentProjectionSnapshot snapshot)
        {
            // TODO: Implement actual lookup
            return "2026-2027";
        }

        private string GetCurrentSemester(StudentProjectionSnapshot snapshot)
        {
            // TODO: Implement actual lookup
            return "First Semester";
        }

        private decimal GetTotalAssessment(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 20000m;
        }

        private decimal GetTuitionAmount(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 15000m;
        }

        private decimal GetMiscellaneousFees(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 3000m;
        }

        private decimal GetLaboratoryFees(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 2000m;
        }

        private decimal GetOtherFees(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 0m;
        }

        private decimal GetScholarshipDeduction(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 5000m;
        }

        private decimal GetTotalPaid(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 5000m;
        }

        private decimal GetOutstandingBalance(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual calculation
            return 15000m;
        }

        private string GetCurrentAssessmentStatus(StudentProjectionSnapshot snapshot, string studentId)
        {
            // TODO: Implement actual lookup
            return "Partially Paid";
        }

        private string GetAssessmentAcademicYear(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual lookup
            return "2026-2027";
        }

        private string GetAssessmentSemester(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual lookup
            return "First Semester";
        }

        private IReadOnlyList<AssessmentChargeItem> GetAssessmentCharges(
            StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual lookup
            return List<AssessmentChargeItem>.Empty;
        }

        private IReadOnlyList<AssessmentPaymentItem> GetAssessmentPayments(
            StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual lookup
            return List<AssessmentPaymentItem>.Empty;
        }

        private decimal GetAssessmentTotal(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual calculation
            return 20000m;
        }

        private decimal GetAssessmentPaid(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual calculation
            return 5000m;
        }

        private decimal GetAssessmentBalance(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual calculation
            return 15000m;
        }

        private string GetAssessmentStatus(StudentProjectionSnapshot snapshot, string assessmentId)
        {
            // TODO: Implement actual lookup
            return "Partially Paid";
        }
    }
}
