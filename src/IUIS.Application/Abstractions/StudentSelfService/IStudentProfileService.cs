using IUIS.Application.StudentSelfService.Profile;
using IUIS.Domain.Students;

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
