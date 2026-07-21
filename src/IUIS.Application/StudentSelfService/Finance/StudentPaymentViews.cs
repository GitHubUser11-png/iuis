using System;
using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Finance
{
    public sealed class StudentPaymentListItem
    {
        public string PaymentId { get; set; }
        
        public DateTime PaymentDate { get; set; }
        
        public decimal Amount { get; set; }
        
        public string PaymentMethod { get; set; }
        
        public string ReceiptNumber { get; set; }
        
        public string Status { get; set; }
    }

    public sealed class StudentPaymentReceiptView
    {
        public StudentPaymentReceiptView()
        {
            PaymentId = string.Empty;
            ReceiptNumber = string.Empty;
            StudentName = string.Empty;
            StudentId = string.Empty;
            
            PaymentDate = DateTime.MinValue;
            
            Amount = 0m;
            PaymentMethod = string.Empty;
            ReferenceNumber = string.Empty;
            
            AssessmentId = string.Empty;
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            ProcessedBy = string.Empty;
            
            Remarks = string.Empty;
        }

        public string PaymentId { get; set; }
        
        public string ReceiptNumber { get; set; }
        
        public string StudentName { get; set; }
        
        public string StudentId { get; set; }
        
        public DateTime PaymentDate { get; set; }
        
        public decimal Amount { get; set; }
        
        public string PaymentMethod { get; set; }
        
        public string ReferenceNumber { get; set; }
        
        public string AssessmentId { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string ProcessedBy { get; set; }
        
        public string Remarks { get; set; }
    }
}
