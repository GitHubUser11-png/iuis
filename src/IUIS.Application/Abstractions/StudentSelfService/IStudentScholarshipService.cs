using System.Collections.Generic;
using IUIS.Application.Common;
using IUIS.Application.StudentSelfService.Scholarship;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentScholarshipService
    {
        IReadOnlyList<ScholarshipProgramView> 
            GetAvailablePrograms(string sessionId);
        
        IReadOnlyList<StudentScholarshipApplicationView> 
            GetMyApplications(string sessionId);
        
        OperationResult SubmitApplication(
            string sessionId,
            StudentScholarshipApplicationRequest request);
    }
}
