using System.Collections.Generic;
using IUIS.Application.Common;
using IUIS.Application.StudentSelfService.Profile;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentProfileService
    {
        StudentProfileView GetProfile(string sessionId);
        
        OperationResult SubmitCorrectionRequest(
            string sessionId,
            StudentProfileCorrectionRequest request);
        
        IReadOnlyList<StudentProfileCorrectionRequest> 
            GetCorrectionRequests(string sessionId);
    }
}
