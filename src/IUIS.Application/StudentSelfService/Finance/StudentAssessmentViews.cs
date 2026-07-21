using System;
using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Finance
{
    public sealed class StudentFinanceSummaryView
    {
        public StudentFinanceSummaryView()
        {
            CurrentAssessmentId = string.Empty;
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            TotalAssessment = 0m;
            TuitionAmount = 0m;
            MiscellaneousFees = 0m;
            LaboratoryFees = 0m;
            OtherFees = 0m;
            
            ScholarshipDeduction = 0m;
            
            TotalPaid = 0m;
            OutstandingBalance = 0m;
            
            AssessmentStatus = string.Empty;
        }

        public string CurrentAssessmentId { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public decimal TotalAssessment { get; set; }
        
        public decimal TuitionAmount { get; set; }
        
        public decimal MiscellaneousFees { get; set; }
        
        public decimal LaboratoryFees { get; set; }
        
        public decimal OtherFees { get; set; }
        
        public decimal ScholarshipDeduction { get; set; }
        
        public decimal TotalPaid { get; set; }
        
        public decimal OutstandingBalance { get; set; }
        
        public string AssessmentStatus { get; set; }
    }

    public sealed class StudentAssessmentDetailsView
    {
        public StudentAssessmentDetailsView()
        {
            AssessmentId = string.Empty;
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            Charges = new List<AssessmentChargeItem>();
            Payments = new List<AssessmentPaymentItem>();
            
            TotalAssessment = 0m;
            TotalPaid = 0m;
            OutstandingBalance = 0m;
            
            AssessmentStatus = string.Empty;
        }

        public string AssessmentId { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public IReadOnlyList<AssessmentChargeItem> Charges { get; set; }
        
        public IReadOnlyList<AssessmentPaymentItem> Payments { get; set; }
        
        public decimal TotalAssessment { get; set; }
        
        public decimal TotalPaid { get; set; }
        
        public decimal OutstandingBalance { get; set; }
        
        public string AssessmentStatus { get; set; }
    }

    public sealed class AssessmentChargeItem
    {
        public string ChargeType { get; set; }
        
        public string Description { get; set; }
        
        public decimal Amount { get; set; }
    }

    public sealed class AssessmentPaymentItem
    {
        public string PaymentId { get; set; }
        
        public DateTime PaymentDate { get; set; }
        
        public decimal Amount { get; set; }
        
        public string PaymentMethod { get; set; }
        
        public string ReceiptNumber { get; set; }
    }
}
