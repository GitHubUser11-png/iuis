using System.Collections.Generic;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.Domain.Students;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentProfileService
    {
        StudentProfileView GetProfile(string sessionId);
        
        OperationResult SubmitCorrectionRequest(
            string sessionId,
            StudentSelfService.Profile.StudentProfileCorrectionRequest request);
        
        IReadOnlyList<StudentSelfService.Profile.StudentProfileCorrectionRequest> 
            GetCorrectionRequests(string sessionId);
    }
}
