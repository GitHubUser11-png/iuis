using System.Collections.Generic;
using IUIS.Application.StudentSelfService.Enrollment;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentEnrollmentService
    {
        StudentEnrollmentSummaryView GetEnrollmentSummary(
            string sessionId);
        
        StudentEnrollmentDetailsView GetEnrollmentDetails(
            string sessionId,
            string enrollmentId);
        
        IReadOnlyList<StudentEnrollmentListItem> 
            GetEnrollmentHistory(string sessionId);
    }
}
