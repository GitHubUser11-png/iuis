using System.Collections.Generic;
using IUIS.Application.StudentSelfService.Finance;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentFinanceService
    {
        StudentFinanceSummaryView GetFinanceSummary(
            string sessionId);
        
        StudentAssessmentDetailsView GetAssessmentDetails(
            string sessionId,
            string assessmentId);
        
        IReadOnlyList<StudentPaymentListItem> 
            GetPaymentHistory(string sessionId);
        
        StudentPaymentReceiptView GetPaymentReceipt(
            string sessionId,
            string paymentId);
    }
}
