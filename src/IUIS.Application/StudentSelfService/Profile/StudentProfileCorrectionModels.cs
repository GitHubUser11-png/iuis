using System;
using IUIS.Domain.Students;

namespace IUIS.Application.StudentSelfService.Profile
{
    public sealed class StudentProfileCorrectionRequest
    {
        public string RequestedField { get; set; }
        
        public string CurrentValue { get; set; }
        
        public string RequestedValue { get; set; }
        
        public string Reason { get; set; }
    }

    public sealed class CorrectionRequestView
    {
        public string RequestId { get; set; }
        
        public string RequestedField { get; set; }
        
        public string CurrentValue { get; set; }
        
        public string RequestedValue { get; set; }
        
        public string Reason { get; set; }
        
        public CorrectionStatus Status { get; set; }
        
        public DateTime RequestDateUtc { get; set; }
        
        public DateTime? ReviewedDateUtc { get; set; }
        
        public string RejectionReason { get; set; }
    }
}
